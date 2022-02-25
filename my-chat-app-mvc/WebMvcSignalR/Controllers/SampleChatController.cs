using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using WebMvcSignalR.Hubs;

namespace WebMvcSignalR.Controllers
{
    public class SampleChatController : Controller
    {
        private readonly IHubContext<ProgressHub> _progressHubContext;

        public SampleChatController(IHubContext<ProgressHub> progressHubContext)
        {
            _progressHubContext = progressHubContext;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult LengthyTask()
        {
            return View();
        }




        [HttpGet("lengthy")]
        public async Task<IActionResult> Lengthy([Bind(Prefix = "id")] string connectionId)
        {
            /*
            await _progressHubContext
                .Clients
                .Client(connectionId)
                .SendAsync("taskStarted");

            for (int i = 0; i < 100; i++)
            {
                Thread.Sleep(500);
                Debug.WriteLine($"progress={i}");
                await _progressHubContext
                    .Clients
                    .Client(connectionId)
                    .SendAsync("taskProgressChanged", i);
            }

            await _progressHubContext
                .Clients
                .Client(connectionId)
                .SendAsync("taskEnded");

            return Ok();

            */


            await _progressHubContext
                .Clients
                .Group(ProgressHub.GROUP_NAME)
                .SendAsync("taskStarted");

            for (int i = 0; i < 100; i++)
            {
                Thread.Sleep(200);
                Debug.WriteLine($"progress={i + 1}");
                await _progressHubContext
                    .Clients
                    .Group(ProgressHub.GROUP_NAME)
                    .SendAsync("taskProgressChanged", i + 1);
            }

            await _progressHubContext
                .Clients
                .Group(ProgressHub.GROUP_NAME)
                .SendAsync("taskEnded");

            return Ok();

        }




    }
}
