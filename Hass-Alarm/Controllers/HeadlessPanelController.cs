using System;
using System.Linq;
using System.Threading.Tasks;
using Hass_Alarm.Data;
using Hass_Alarm.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hass_Alarm.Controllers
{
    public class HeadlessPanelController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAlarmState _alarmState;
        private readonly IPinHashingService _pinHashingService;
        private readonly IRateLimitService _rateLimitService;
        private readonly ISessionService _sessionService;
        private readonly IHomeAssistantEventService _eventService;

        public HeadlessPanelController(
            ApplicationDbContext context,
            IAlarmState alarmState,
            IPinHashingService pinHashingService,
            IRateLimitService rateLimitService,
            ISessionService sessionService,
            IHomeAssistantEventService eventService)
        {
            _context = context;
            _alarmState = alarmState;
            _pinHashingService = pinHashingService;
            _rateLimitService = rateLimitService;
            _sessionService = sessionService;
            _eventService = eventService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Authenticate([FromBody] AuthRequest request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (!_rateLimitService.IsAllowed(ipAddress))
            {
                return Json(new
                {
                    success = false,
                    error = "Too many failed attempts",
                    rate_limited = true
                });
            }

            if (string.IsNullOrWhiteSpace(request.Pin) || string.IsNullOrWhiteSpace(request.DeviceName))
            {
                _rateLimitService.RecordFailedAttempt(ipAddress);
                return Json(new { success = false, error = "PIN and device name required" });
            }

            // Get or create device
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Name == request.DeviceName);

            if (device == null)
            {
                device = new Data.Models.Device
                {
                    Name = request.DeviceName,
                    UniqueIdentifier = Guid.NewGuid().ToString(),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Devices.Add(device);
                await _context.SaveChangesAsync();
            }

            // Verify PIN
            var pinCode = await _context.PinCodes
                .Include(p => p.User)
                .FirstOrDefaultAsync(p =>
                    p.Enabled &&
                    (p.Code == request.Pin || _pinHashingService.VerifyPin(request.Pin, p.Code)));

            if (pinCode == null)
            {
                _rateLimitService.RecordFailedAttempt(ipAddress);
                return Json(new
                {
                    success = false,
                    error = "Invalid PIN",
                    remaining_attempts = _rateLimitService.GetRemainingAttempts(ipAddress)
                });
            }

            _rateLimitService.ResetAttempts(ipAddress);

            var session = await _sessionService.CreateSessionAsync(
                pinCode.UserId,
                device.Id,
                ipAddress);

            return Json(new
            {
                success = true,
                session_token = session.SessionToken,
                expires_at = session.ExpiresAt
            });
        }

        [HttpPost]
        public async Task<IActionResult> SetState([FromBody] SetStateRequest request)
        {
            var session = await _sessionService.ValidateSessionAsync(request.SessionToken);
            if (session == null)
                return Json(new { success = false, error = "Invalid or expired session" });

            if (!Enum.TryParse<ArmState>(request.Action, true, out var requestedState))
            {
                return Json(new { success = false, error = "Invalid action" });
            }

            await _alarmState.SetArmState(requestedState);
            var newState = await _alarmState.GetArmState();

            await _sessionService.RefreshSessionAsync(request.SessionToken);

            var pinCode = await _context.PinCodes
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == session.UserId);

            var userName = session.User?.UserName ?? "Unknown";
            var friendlyName = session.User?.Email ?? userName;
            var pinName = pinCode?.Name ?? userName;

            await _eventService.FireAlarmStateChangeEventAsync(
                userName: userName,
                friendlyName: friendlyName,
                pinName: pinName,
                newState: newState.ToString().ToLower(),
                deviceName: session.Device.Name,
                ipAddress: session.IpAddress);

            return Json(new
            {
                success = true,
                new_state = newState.ToString().ToLower()
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetState([FromQuery] string sessionToken)
        {
            var session = await _sessionService.ValidateSessionAsync(sessionToken);
            if (session == null)
                return Json(new { success = false, error = "Invalid or expired session" });

            var currentState = await _alarmState.GetArmState();

            return Json(new
            {
                success = true,
                arm_state = (int)currentState,
                state_name = currentState.ToString().ToLower()
            });
        }
    }

    public class AuthRequest
    {
        public string Pin { get; set; }
        public string DeviceName { get; set; }
    }

    public class SetStateRequest
    {
        public string SessionToken { get; set; }
        public string Action { get; set; }
    }
}
