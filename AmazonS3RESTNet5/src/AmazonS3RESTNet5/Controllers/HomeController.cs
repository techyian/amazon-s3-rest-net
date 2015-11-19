using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using AmazonS3RESTNet5.Interfaces;
using AmazonS3RESTNet5.ViewModels.Home;

namespace AmazonS3RESTNet5.Controllers
{
    public class HomeController : Controller
    {

        private readonly IAmazonS3Service _amazonService;

        public HomeController(IAmazonS3Service amazonService)
        {
            _amazonService = amazonService;
        }

        public IActionResult Index()
        {
            List<KeyValuePair<string, string>> formInputs = _amazonService.GetS3Details("YOUR BUCKET NAME", "YOUR REGION");

            HomeViewModel vm = new HomeViewModel();
            vm.AmazonFormInputs = formInputs;

            return View("~/Views/Home/Index.cshtml", vm);
        }
    }
}
