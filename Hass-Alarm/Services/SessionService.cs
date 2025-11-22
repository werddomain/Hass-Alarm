using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Hass_Alarm.Data;
using Hass_Alarm.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hass_Alarm.Services
{
    public class SessionService : ISessionService
    {
        private readonly ApplicationDbContext _context;
        private const int SessionExpirationDays = 30;

        public SessionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PanelSession> CreateSessionAsync(string userId, int deviceId, string ipAddress)
        {
            var sessionToken = GenerateSecureToken();

            var session = new PanelSession
            {
                SessionToken = sessionToken,
                UserId = userId,
                DeviceId = deviceId,
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(SessionExpirationDays),
                LastActivityAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.PanelSessions.Add(session);
            await _context.SaveChangesAsync();

            return session;
        }

        public async Task<PanelSession> ValidateSessionAsync(string sessionToken)
        {
            var session = await _context.PanelSessions
                .Include(s => s.User)
                .Include(s => s.Device)
                .FirstOrDefaultAsync(s =>
                    s.SessionToken == sessionToken &&
                    s.IsActive &&
                    s.ExpiresAt > DateTime.UtcNow);

            if (session != null)
            {
                session.LastActivityAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return session;
        }

        public async Task<bool> RefreshSessionAsync(string sessionToken)
        {
            var session = await _context.PanelSessions
                .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && s.IsActive);

            if (session == null)
                return false;

            session.ExpiresAt = DateTime.UtcNow.AddDays(SessionExpirationDays);
            session.LastActivityAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task RevokeSessionAsync(string sessionToken)
        {
            var session = await _context.PanelSessions
                .FirstOrDefaultAsync(s => s.SessionToken == sessionToken);

            if (session != null)
            {
                session.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task CleanupExpiredSessionsAsync()
        {
            var expiredSessions = await _context.PanelSessions
                .Where(s => s.ExpiresAt < DateTime.UtcNow || !s.IsActive)
                .ToListAsync();

            _context.PanelSessions.RemoveRange(expiredSessions);
            await _context.SaveChangesAsync();
        }

        private string GenerateSecureToken()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var tokenData = new byte[64];
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData);
            }
        }
    }
}
