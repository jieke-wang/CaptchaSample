using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using BotDetect.Web.Mvc;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Sample3.Models;

namespace Sample3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(new HomeViewModel());
        }

        [HttpPost]
        [CaptchaValidationActionFilter("CaptchaCode", "ExampleCaptcha", "Wrong Captcha!")]
        public ActionResult Index(HomeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                //return Content("no");
                return View(model);
            }
            else
            {
                MvcCaptcha.ResetCaptcha("ExampleCaptcha");
                //return Content("ok");
                return View(model);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
