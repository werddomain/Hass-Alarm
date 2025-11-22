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
using Hass_Alarm.Services;

namespace Hass_Alarm.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;
        private readonly IAlarmState _alarmState;
        private readonly IRateLimitService _rateLimitService;

        public HomeController(
            ILogger<HomeController> logger,
            IConfiguration configuration,
            Data.ApplicationDbContext dbContext,
            IAlarmState alarmState,
            IRateLimitService rateLimitService)
        {
            _logger = logger;
            _configuration = configuration;
            _dbContext = dbContext;
            _alarmState = alarmState;
            _rateLimitService = rateLimitService;
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
            // Get IP address for rate limiting
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // SECURITY: Check if this IP is rate limited
            if (_rateLimitService.IsBlocked(ipAddress))
            {
                model.rate_limited = true;
                model.code_invalid = true;
                model.remaining_attempts = 0;
                _logger.LogWarning("Rate limited PIN attempt from IP: {IpAddress}", ipAddress);
                return Json(model);
            }

            // Get remaining attempts for feedback
            model.remaining_attempts = _rateLimitService.GetRemainingAttempts(ipAddress);

            if (!string.IsNullOrEmpty(model.code))
            {
                // CRITICAL FIX: Use async query and check if PIN is enabled
                var pin = await _dbContext.PinCodes
                    .Where(o => o.Pin == model.code && o.Enabled)
                    .FirstOrDefaultAsync();

                if (pin != null)
                {
                    model.code_invalid = false;
                    _logger.LogInformation("Valid PIN entered: {PinName} from IP: {IpAddress}", pin.Name, ipAddress);

                    // SECURITY: Reset failed attempts on successful authentication
                    _rateLimitService.ResetAttempts(ipAddress);
                    model.remaining_attempts = _rateLimitService.GetRemainingAttempts(ipAddress);

                    try
                    {
                        switch (model.action)
                        {
                            case "arm":
                                await _alarmState.SetArmState(AlarmState.Armed);
                                _logger.LogInformation("Alarm armed by PIN: {PinName} from IP: {IpAddress}", pin.Name, ipAddress);
                                break;

                            case "disarm":
                                await _alarmState.SetArmState(AlarmState.Disarmed);
                                _logger.LogInformation("Alarm disarmed by PIN: {PinName} from IP: {IpAddress}", pin.Name, ipAddress);
                                break;

                            case "arm_home":
                                await _alarmState.SetArmState(AlarmState.ArmedHome);
                                _logger.LogInformation("Alarm armed (home) by PIN: {PinName} from IP: {IpAddress}", pin.Name, ipAddress);
                                break;

                            case "unlock":
                                // Unlock action - could be used for other purposes
                                _logger.LogInformation("Unlock requested by PIN: {PinName} from IP: {IpAddress}", pin.Name, ipAddress);
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
                        _logger.LogError(ex, "Error executing alarm action {Action} from IP: {IpAddress}", model.action, ipAddress);
                        model.code_invalid = true;
                    }
                }
                else
                {
                    model.code_invalid = true;

                    // SECURITY: Record failed attempt for rate limiting
                    _rateLimitService.RecordFailedAttempt(ipAddress);
                    model.remaining_attempts = _rateLimitService.GetRemainingAttempts(ipAddress);

                    // Check if now blocked after this attempt
                    if (_rateLimitService.IsBlocked(ipAddress))
                    {
                        model.rate_limited = true;
                        model.remaining_attempts = 0;
                        _logger.LogWarning("IP address {IpAddress} has been rate limited after failed PIN attempt", ipAddress);
                    }
                    else
                    {
                        _logger.LogWarning("Invalid or disabled PIN attempted from IP: {IpAddress}. Remaining attempts: {RemainingAttempts}",
                            ipAddress, model.remaining_attempts);
                    }
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
