using System;
using System.Linq;
using System.Threading.Tasks;
using Hass_Alarm.Data;
using Hass_Alarm.Data.Models;
using Hass_Alarm.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hass_Alarm.Controllers
{
    [Route("api/plugin")]
    [ApiController]
    public class PluginController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAlarmState _alarmState;
        private readonly IPinHashingService _pinHashingService;
        private readonly IRateLimitService _rateLimitService;
        private readonly ISessionService _sessionService;
        private readonly IHomeAssistantEventService _eventService;
        private readonly ILogger<PluginController> _logger;

        public PluginController(
            ApplicationDbContext context,
            IAlarmState alarmState,
            IPinHashingService pinHashingService,
            IRateLimitService rateLimitService,
            ISessionService sessionService,
            IHomeAssistantEventService eventService,
            ILogger<PluginController> logger)
        {
            _context = context;
            _alarmState = alarmState;
            _pinHashingService = pinHashingService;
            _rateLimitService = rateLimitService;
            _sessionService = sessionService;
            _eventService = eventService;
            _logger = logger;
        }

        [HttpPost("device/register")]
        public async Task<IActionResult> RegisterDevice([FromBody] DeviceRegistrationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DeviceName))
                return BadRequest(new { error = "Device name is required" });

            var deviceIdentifier = request.UniqueIdentifier ?? Guid.NewGuid().ToString();

            var existingDevice = await _context.Devices
                .FirstOrDefaultAsync(d => d.UniqueIdentifier == deviceIdentifier);

            if (existingDevice != null)
            {
                existingDevice.Name = request.DeviceName;
                existingDevice.Description = request.Description;
                existingDevice.LastAccessedAt = DateTime.UtcNow;
                existingDevice.IsActive = true;
            }
            else
            {
                existingDevice = new Device
                {
                    Name = request.DeviceName,
                    Description = request.Description,
                    UniqueIdentifier = deviceIdentifier,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastAccessedAt = DateTime.UtcNow
                };
                _context.Devices.Add(existingDevice);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                device_id = existingDevice.Id,
                unique_identifier = existingDevice.UniqueIdentifier,
                message = "Device registered successfully"
            });
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticationRequest request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Check rate limiting
            if (!_rateLimitService.IsAllowed(ipAddress))
            {
                return StatusCode(429, new
                {
                    error = "Too many failed attempts",
                    rate_limited = true,
                    remaining_attempts = 0
                });
            }

            if (string.IsNullOrWhiteSpace(request.Pin) || request.DeviceId <= 0)
            {
                _rateLimitService.RecordFailedAttempt(ipAddress);
                return BadRequest(new
                {
                    error = "PIN and Device ID are required",
                    code_invalid = true,
                    remaining_attempts = _rateLimitService.GetRemainingAttempts(ipAddress)
                });
            }

            // Verify device exists and is active
            var device = await _context.Devices.FindAsync(request.DeviceId);
            if (device == null || !device.IsActive)
            {
                _rateLimitService.RecordFailedAttempt(ipAddress);
                return BadRequest(new
                {
                    error = "Invalid device",
                    code_invalid = true,
                    remaining_attempts = _rateLimitService.GetRemainingAttempts(ipAddress)
                });
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
                return Unauthorized(new
                {
                    error = "Invalid PIN",
                    code_invalid = true,
                    remaining_attempts = _rateLimitService.GetRemainingAttempts(ipAddress)
                });
            }

            // Reset rate limiting on successful authentication
            _rateLimitService.ResetAttempts(ipAddress);

            // Create new session (this resets the 30-day expiration)
            var session = await _sessionService.CreateSessionAsync(
                pinCode.UserId,
                device.Id,
                ipAddress);

            // Update device last accessed time
            device.LastAccessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                session_token = session.SessionToken,
                expires_at = session.ExpiresAt,
                user_name = pinCode.User.UserName,
                device_name = device.Name,
                message = "Authentication successful"
            });
        }

        [HttpGet("state")]
        public async Task<IActionResult> GetState([FromHeader(Name = "X-Session-Token")] string sessionToken)
        {
            var session = await ValidateSession(sessionToken);
            if (session == null)
                return Unauthorized(new { error = "Invalid or expired session" });

            var currentState = await _alarmState.GetArmState();

            return Ok(new
            {
                arm_state = (int)currentState,
                state_name = currentState.ToString().ToLower(),
                timestamp = DateTime.UtcNow
            });
        }

        [HttpPost("state")]
        public async Task<IActionResult> SetState(
            [FromHeader(Name = "X-Session-Token")] string sessionToken,
            [FromBody] StateChangeRequest request)
        {
            var session = await ValidateSession(sessionToken);
            if (session == null)
                return Unauthorized(new { error = "Invalid or expired session" });

            if (!Enum.TryParse<ArmState>(request.Action, true, out var requestedState))
            {
                return BadRequest(new { error = "Invalid action. Use: disarm, arm, or arm_home" });
            }

            var currentState = await _alarmState.GetArmState();
            await _alarmState.SetArmState(requestedState);
            var newState = await _alarmState.GetArmState();

            // Refresh session on successful state change (resets 30-day expiration)
            await _sessionService.RefreshSessionAsync(sessionToken);

            // Get user and PIN information for the event
            var pinCode = await _context.PinCodes
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == session.UserId);

            var userName = session.User?.UserName ?? "Unknown";
            var friendlyName = session.User?.Email ?? userName;
            var pinName = pinCode?.Name ?? userName;

            // Fire Home Assistant event
            await _eventService.FireAlarmStateChangeEventAsync(
                userName: userName,
                friendlyName: friendlyName,
                pinName: pinName,
                newState: newState.ToString().ToLower(),
                deviceName: session.Device.Name,
                ipAddress: session.IpAddress);

            return Ok(new
            {
                previous_state = currentState.ToString().ToLower(),
                new_state = newState.ToString().ToLower(),
                timestamp = DateTime.UtcNow,
                session_refreshed = true
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromHeader(Name = "X-Session-Token")] string sessionToken)
        {
            if (string.IsNullOrWhiteSpace(sessionToken))
                return BadRequest(new { error = "Session token required" });

            await _sessionService.RevokeSessionAsync(sessionToken);

            return Ok(new { message = "Logged out successfully" });
        }

        private async Task<PanelSession> ValidateSession(string sessionToken)
        {
            if (string.IsNullOrWhiteSpace(sessionToken))
                return null;

            return await _sessionService.ValidateSessionAsync(sessionToken);
        }
    }

    public class DeviceRegistrationRequest
    {
        public string DeviceName { get; set; }
        public string Description { get; set; }
        public string UniqueIdentifier { get; set; }
    }

    public class AuthenticationRequest
    {
        public string Pin { get; set; }
        public int DeviceId { get; set; }
    }

    public class StateChangeRequest
    {
        public string Action { get; set; }
    }
}
