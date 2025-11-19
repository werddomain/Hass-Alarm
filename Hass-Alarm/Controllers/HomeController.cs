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
using Microsoft.EntityFrameworkCore;

namespace Hass_Alarm.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;
        private readonly IAlarmState _alarmState;

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
                // CRITICAL FIX: Use async query and check if PIN is enabled
                var pin = await _dbContext.PinCodes
                    .Where(o => o.Pin == model.code && o.Enabled)
                    .FirstOrDefaultAsync();

                if (pin != null)
                {
                    model.code_invalid = false;
                    _logger.LogInformation("Valid PIN entered: {PinName}", pin.Name);

                    try
                    {
                        switch (model.action)
                        {
                            case "arm":
                                await _alarmState.SetArmState(AlarmState.Armed);
                                _logger.LogInformation("Alarm armed by PIN: {PinName}", pin.Name);
                                break;

                            case "disarm":
                                await _alarmState.SetArmState(AlarmState.Disarmed);
                                _logger.LogInformation("Alarm disarmed by PIN: {PinName}", pin.Name);
                                break;

                            case "arm_home":
                                await _alarmState.SetArmState(AlarmState.ArmedHome);
                                _logger.LogInformation("Alarm armed (home) by PIN: {PinName}", pin.Name);
                                break;

                            case "unlock":
                                // Unlock action - could be used for other purposes
                                _logger.LogInformation("Unlock requested by PIN: {PinName}", pin.Name);
                                break;

                            default:
                                _logger.LogWarning("Unknown action requested: {Action}", model.action);
                                break;
                        }

                        // Refresh the alarm state after action
                        model.ArmState = await _alarmState.GetArmState();
                        model.State = model.ArmState.ToString().ToLower();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing alarm action {Action}", model.action);
                        model.code_invalid = true;
                    }
                }
                else
                {
                    model.code_invalid = true;
                    _logger.LogWarning("Invalid or disabled PIN attempted: {Code}", model.code);
                }
            }

            return Json(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
