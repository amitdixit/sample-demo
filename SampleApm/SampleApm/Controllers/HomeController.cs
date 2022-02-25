using Microsoft.AspNetCore.Mvc;
using SampleApm.Models;
using System.Diagnostics;
using ILogger = Serilog.ILogger;

namespace SampleApm.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger _logger;

        public HomeController(ILogger logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.Information("This is Index");
            return View();
        }

        public IActionResult Privacy()
        {
            _logger.Warning("This is a suspicios request");
            return BadRequest();
           // return View();
        }

        public IActionResult About()
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                _logger.Error(ex,"This is my beautiful error");
                //return new StatusCodeResult(500);
                throw new FileNotFoundException();
            }

            //return View();
            // throw new System.Exception();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    internal record NewRecord(string Error1);
}
