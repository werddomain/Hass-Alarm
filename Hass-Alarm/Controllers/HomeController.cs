using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Hass_Alarm.Models;
using Microsoft.Extensions.Configuration;
using HADotNet.Core.Clients;
using Hass_Alarm.Views.Home;
using HADotNet.Core;
using System.Net;

namespace Hass_Alarm.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        string Entity_Arm = "";
        string Entity_Disarm = "";
        string Entity_ArmHome = "";

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            Entity_Arm = configuration.GetValue<string>("Ha:Entities:Arm");
            Entity_Disarm = configuration.GetValue<string>("Ha:Entities:Disarm");
            Entity_ArmHome = configuration.GetValue<string>("Ha:Entities:ArmHome");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Panel()
        {
            var haHost = _configuration.GetValue<string>("Ha:Host");
            var haApi = _configuration.GetValue<string>("Ha:ApiKey");
            ClientFactory.Initialize(haHost, haApi);
            WebClient client = new WebClient();
            string reply = client.DownloadString("https://google.ca");

            var model = new PanelModel();
            var statesClient = ClientFactory.GetClient<StatesClient>();
            var armT = statesClient.GetState(Entity_Arm);
            var servicesClient = ClientFactory.GetClient<ServiceClient>();
          
            //var disarmT = statesClient.GetState(Entity_Disarm);
            var armHomeT = !string.IsNullOrEmpty(Entity_ArmHome) ? statesClient.GetState(Entity_ArmHome) : null;

            if (armHomeT != null)
                await Task.WhenAll(armT, armHomeT);
            else
                await armT;
            bool arm = false;
            bool arm_home = false;
            if (armT?.Result?.State?.ToLower() == "on")
            {
                arm = true;
            }
            if (armHomeT != null && armHomeT?.Result?.State?.ToLower() == "on")
            {
                arm_home = true;
            }
            model.State = (arm, arm_home) switch
            {
                (true, false) => "arm",
                (false, true) => "arm_home",
                (true, true) => "arm",
                (false, false) => "disarm"
            };
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
