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
using Hass_Alarm.Data;

namespace Hass_Alarm.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;
        private readonly IAlarmState _alarmState;
        string Entity_Arm = "";
        string Entity_ArmHome = "";

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, Data.ApplicationDbContext dbContext, IAlarmState alarmState)
        {
            _logger = logger;
            _configuration = configuration;
            _dbContext = dbContext;
            _alarmState = alarmState;

        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            var model = new PanelModel();

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Panel()
        {
            var model = new PanelModel();
            model.ArmState = await _alarmState.GetArmState();
            model.State = model.ArmState.ToString().ToLower();

            return View(model);
        }

        [ValidateAntiForgeryToken, HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Panel(PanelModel model)
        {

            if (!string.IsNullOrEmpty(model.code))
            {
                var pin = _dbContext.PinCodes.Where(o => o.Pin.ToString() == model.code);
                if (pin.Any())
                {
                    model.code_invalid = false;
                    
                    switch (model.action)
                    {
                        case "arm":
                            break;
                        case "disarm":
                            break;
                        case "arm_home":
                            break;
                        case "unlock":
                            break;

                        default:
                            break;
                    }
                }
                else
                    model.code_invalid = true;
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
