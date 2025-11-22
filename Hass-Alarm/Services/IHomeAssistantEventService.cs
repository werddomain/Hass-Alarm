using System.Threading.Tasks;

namespace Hass_Alarm.Services
{
    public interface IHomeAssistantEventService
    {
        Task FireAlarmStateChangeEventAsync(
            string userName,
            string friendlyName,
            string pinName,
            string newState,
            string deviceName,
            string ipAddress);
    }
}
