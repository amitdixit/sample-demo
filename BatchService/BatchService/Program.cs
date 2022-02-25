using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BatchService
{
    class Program
    {

        // Batch account credentials
        private const string _batchAccountName = "";
        private const string _batchAccountKey = "";
        private const string _batchAccountUrl = "";

        // Storage account credentials
        private const string _storageAccountName = "";
        private const string _storageAccountKey = "";

        // Pool and Job constants
        private const string _poolId = "WinAmlPool";
        private const int _dedicatedNodeCount = 0;
        private const int _lowPriorityNodeCount = 2;
        //private const string _poolVMSize = "STANDARD_A1_v2";
        private const string _poolVMSize = "STANDARD_A8_v2";
        private const string _jobId = "WinAmlTestAppJob1";


        const string _appPackageId = "TestAppapp";
        const string _appPackageVersion = "1.0";


        static void Main(string[] args)
        {
            if (string.IsNullOrEmpty(_batchAccountName) ||
                string.IsNullOrEmpty(_batchAccountKey) ||
                string.IsNullOrEmpty(_batchAccountUrl) ||
                string.IsNullOrEmpty(_storageAccountName) ||
                string.IsNullOrEmpty(_storageAccountKey))
            {
                throw new InvalidOperationException("One or more account credential strings have not been populated. Please ensure that your Batch and Storage account credentials have been specified.");
            }

            try
            {
                // Call the asynchronous version of the Main() method. This is done so that we can await various
                // calls to async methods within the "Main" method of this console application.
                MainAsync().Wait();
            }
            catch (AggregateException)
            {
                Console.WriteLine();
                Console.WriteLine("One or more exceptions occurred.");
                Console.WriteLine();
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("Sample complete, hit ENTER to exit...");
                Console.ReadLine();
            }
        }

        private static async Task MainAsync()
        {
            Console.WriteLine("Sample start: {0}", DateTime.Now);
            Console.WriteLine();
            var timer = new Stopwatch();
            timer.Start();




            // CREATE BATCH CLIENT / CREATE POOL / CREATE JOB / ADD TASKS

            // Create a Batch client and authenticate with shared key credentials.
            // The Batch client allows the app to interact with the Batch service.
            var sharedKeyCredentials = new BatchSharedKeyCredentials(_batchAccountUrl, _batchAccountName, _batchAccountKey);

            using (var batchClient = BatchClient.Open(sharedKeyCredentials))
            {
                // Create the Batch pool, which contains the compute nodes that execute the tasks.
                await CreatePoolIfNotExistAsync(batchClient, _poolId);

                // Create the job that runs the tasks.
                await CreateJobAsync(batchClient, _jobId, _poolId);

                // Create a collection of tasks and add them to the Batch job. 
                await AddTasksAsync(batchClient, _jobId, new List<int> { 1 });

                // Monitor task success or failure, specifying a maximum amount of time to wait for the tasks to complete.
                await MonitorTasks(batchClient, _jobId, TimeSpan.FromMinutes(30));



                // Print out timing info
                timer.Stop();
                Console.WriteLine();
                Console.WriteLine("Sample end: {0}", DateTime.Now);
                Console.WriteLine("Elapsed time: {0}", timer.Elapsed);

                // Clean up Batch resources (if the user so chooses)
                Console.WriteLine();
                Console.Write("Delete job? [yes] no: ");
                string response = Console.ReadLine().ToLower();
                if (response != "n" && response != "no")
                {
                    await batchClient.JobOperations.DeleteJobAsync(_jobId);
                }

                Console.Write("Delete pool? [yes] no: ");
                response = Console.ReadLine().ToLower();
                if (response != "n" && response != "no")
                {
                    await batchClient.PoolOperations.DeletePoolAsync(_poolId);
                }
            }
        }

        /// <summary>
        /// Creates the Batch pool.
        /// </summary>
        /// <param name="batchClient">A BatchClient object</param>
        /// <param name="poolId">ID of the CloudPool object to create.</param>
        private static async Task CreatePoolIfNotExistAsync(BatchClient batchClient, string poolId)
        {
            try
            {
                Console.WriteLine("Creating pool [{0}]...", poolId);

                var imageReference = new ImageReference(
                        publisher: "MicrosoftWindowsServer",
                        offer: "WindowsServer",
                        sku: "2012-R2-Datacenter-smalldisk",
                        version: "latest");

                var virtualMachineConfiguration = new VirtualMachineConfiguration(imageReference: imageReference, nodeAgentSkuId: "batch.node.windows amd64");

                // Create an unbound pool. No pool is actually created in the Batch service until we call
                // CloudPool.Commit(). This CloudPool instance is therefore considered "unbound," and we can
                // modify its properties.
                var pool = batchClient.PoolOperations.CreatePool(
                    poolId: poolId,
                    targetDedicatedComputeNodes: _dedicatedNodeCount,
                    targetLowPriorityComputeNodes: _lowPriorityNodeCount,
                    virtualMachineSize: _poolVMSize,
                    virtualMachineConfiguration: virtualMachineConfiguration);

                // Specify the application and version to install on the compute nodes
                // This assumes that a Windows 64-bit zipfile of "TestAppServiceUtiity" has been added to Batch account
                // with Application Id of "TestAppapp" and Version of "1.0".
                pool.ApplicationPackageReferences = new List<ApplicationPackageReference>
                {
                    new ApplicationPackageReference
                    {
                        ApplicationId = _appPackageId,
                        Version = _appPackageVersion
                    }
                };

                await pool.CommitAsync();
            }
            catch (BatchException be)
            {
                // Accept the specific error code PoolExists as that is expected if the pool already exists
                if (be.RequestInformation?.BatchError?.Code == BatchErrorCodeStrings.PoolExists)
                {
                    Console.WriteLine("The pool {0} already existed when we tried to create it", poolId);
                }
                else
                {
                    throw; // Any other exception is unexpected
                }
            }
        }


        /// <summary>
        /// Creates a job in the specified pool.
        /// </summary>
        /// <param name="batchClient">A BatchClient object.</param>
        /// <param name="jobId">ID of the job to create.</param>
        /// <param name="poolId">ID of the CloudPool object in which to create the job.</param>
        private static async Task CreateJobAsync(BatchClient batchClient, string jobId, string poolId)
        {
            Console.WriteLine("Creating job [{0}]...", jobId);

            CloudJob job = batchClient.JobOperations.CreateJob();
            job.Id = jobId;
            job.PoolInformation = new PoolInformation { PoolId = poolId };

            await job.CommitAsync();
        }


        // <summary>
        /// Creates tasks to process each of the specified input files, and submits them
        ///  to the specified job for execution.
        /// <param name="batchClient">A BatchClient object.</param>
        /// <param name="jobId">ID of the job to which the tasks are added.</param>
        /// <param name="inputSources">A collection of Source representing the FileUpload Source UNSC/OFAC
        /// to be processed by the tasks executed on the compute nodes.</param>        
        /// <returns>A collection of the submitted cloud tasks.</returns>
        /// </summary>
        private static async Task<List<CloudTask>> AddTasksAsync(BatchClient batchClient, string jobId, List<int> inputSources)
        {
            Console.WriteLine("Adding {0} tasks to job [{1}]...", inputSources.Count, jobId);

            // Create a collection to hold the tasks added to the job:
            var tasks = new List<CloudTask>();

            for (int i = 0; i < inputSources.Count; i++)
            {
                // Assign a task ID for each iteration
                string taskId = string.Format("Task{0}", i);

                // Define task command line to convert the video format from MP4 to MP3 using ffmpeg.
                // Note that ffmpeg syntax specifies the format as the file extension of the input file
                // and the output file respectively. In this case inputs are MP4.
                
                //  string appPath = string.Format("%AZ_BATCH_APP_PACKAGE_{0}#{1}%", _appPackageId, _appPackageVersion);
                //string taskCommandLine = string.Format("cmd /c {0}\\MyTestApp.exe {1}", appPath, i + 1);
                string appPath = $"%AZ_BATCH_APP_PACKAGE_{_appPackageId}#{_appPackageVersion}%";
                string taskCommandLine = $"cmd /c {appPath}\\MyTestApp.exe {i + 1}";

                /*
                  string appPath = $"%AZ_BATCH_APP_PACKAGE_{appId}#{appVersion}%";

                string taskId = $"Task1";
                string taskCommandLine = $"cmd /c {appPath}\\Multiply.exe 6 4";
                 */


                // Create a cloud task (with the task ID and command line) and add it to the task list
                var task = new CloudTask(taskId, taskCommandLine);
                tasks.Add(task);
            }

            // Call BatchClient.JobOperations.AddTask() to add the tasks as a collection rather than making a
            // separate call for each. Bulk task submission helps to ensure efficient underlying API
            // calls to the Batch service. 
            await batchClient.JobOperations.AddTaskAsync(jobId, tasks);

            return tasks;
        }

        /// <summary>
        /// Monitors the specified tasks for completion and whether errors occurred.
        /// </summary>
        /// <param name="batchClient">A BatchClient object.</param>
        /// <param name="jobId">ID of the job containing the tasks to be monitored.</param>
        /// <param name="timeout">The period of time to wait for the tasks to reach the completed state.</param>
        private static async Task<bool> MonitorTasks(BatchClient batchClient, string jobId, TimeSpan timeout)
        {
            bool allTasksSuccessful = true;
            const string completeMessage = "All tasks reached state Completed.";
            const string incompleteMessage = "One or more tasks failed to reach the Completed state within the timeout period.";
            const string successMessage = "Success! All tasks completed successfully. TestApp done for the customers.";
            const string failureMessage = "One or more tasks failed.";

            // Obtain the collection of tasks currently managed by the job. 
            // Use a detail level to specify that only the "id" property of each task should be populated. 
            // See https://docs.microsoft.com/en-us/azure/batch/batch-efficient-list-queries

            var detail = new ODATADetailLevel(selectClause: "id");

            var addedTasks = await batchClient.JobOperations.ListTasks(jobId, detail).ToListAsync();

            Console.WriteLine("Monitoring all tasks for 'Completed' state, timeout in {0}...", timeout.ToString());

            // We use a TaskStateMonitor to monitor the state of our tasks. In this case, we will wait for all tasks to
            // reach the Completed state.

            var taskStateMonitor = batchClient.Utilities.CreateTaskStateMonitor();
            try
            {
                await taskStateMonitor.WhenAll(addedTasks, TaskState.Completed, timeout);

                var completedtasks = batchClient.JobOperations.ListTasks(_jobId);

                foreach (var task in completedtasks)
                {
                    string nodeId = string.Format(task.ComputeNodeInformation.ComputeNodeId);
                    Console.WriteLine("Task: {0}", task.Id);
                    Console.WriteLine("Node: {0}", nodeId);
                    Console.WriteLine("Standard out:");
                    Console.WriteLine(task.GetNodeFile(Constants.StandardOutFileName).ReadAsString());
                }
            }
            catch (TimeoutException)
            {
                await batchClient.JobOperations.TerminateJobAsync(jobId);
                Console.WriteLine(incompleteMessage);
                return false;
            }
            await batchClient.JobOperations.TerminateJobAsync(jobId);
            Console.WriteLine(completeMessage);

            // All tasks have reached the "Completed" state, however, this does not guarantee all tasks completed successfully.
            // Here we further check for any tasks with an execution result of "Failure".

            // Update the detail level to populate only the executionInfo property.
            detail.SelectClause = "executionInfo";
            // Filter for tasks with 'Failure' result.
            detail.FilterClause = "executionInfo/result eq 'Failure'";

            List<CloudTask> failedTasks = await batchClient.JobOperations.ListTasks(jobId, detail).ToListAsync();

            if (failedTasks.Any())
            {
                allTasksSuccessful = false;
                Console.WriteLine(failureMessage);
            }
            else
            {
                Console.WriteLine(successMessage);
            }

            return allTasksSuccessful;
        }



        public static IEnumerable<int> DistributeInteger(int total, int divider)
        {
            if (divider == 0)
            {
                yield return 0;
            }
            else
            {
                int rest = total % divider;
                double result = total / (double)divider;

                for (int i = 0; i < divider; i++)
                {
                    if (rest-- > 0)
                        yield return (int)Math.Ceiling(result);
                    else
                        yield return (int)Math.Floor(result);
                }
            }
        }
    }
}
