using System.Threading.Tasks;
using Hass_Alarm.Data.Models;

namespace Hass_Alarm.Services
{
    public interface ISessionService
    {
        Task<PanelSession> CreateSessionAsync(string userId, int deviceId, string ipAddress);
        Task<PanelSession> ValidateSessionAsync(string sessionToken);
        Task<bool> RefreshSessionAsync(string sessionToken);
        Task RevokeSessionAsync(string sessionToken);
        Task CleanupExpiredSessionsAsync();
    }
}
