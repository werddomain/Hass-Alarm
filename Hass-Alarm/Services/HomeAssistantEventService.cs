using System;
using System.Threading.Tasks;
using HADotNet.Core;
using HADotNet.Core.Clients;
using Microsoft.Extensions.Configuration;

namespace Hass_Alarm.Services
{
    public class HomeAssistantEventService : IHomeAssistantEventService
    {
        private readonly IConfiguration _configuration;
        private readonly string _haHost;
        private readonly string _haApiKey;

        public HomeAssistantEventService(IConfiguration configuration)
        {
            _configuration = configuration;
            _haHost = _configuration.GetValue<string>("Ha:Host");
            _haApiKey = _configuration.GetValue<string>("Ha:ApiKey");
        }

        public async Task FireAlarmStateChangeEventAsync(
            string userName,
            string friendlyName,
            string pinName,
            string newState,
            string deviceName,
            string ipAddress)
        {
            try
            {
                ClientFactory.Initialize(_haHost, _haApiKey);
                var eventClient = ClientFactory.GetClient<EventClient>();

                var eventData = new
                {
                    timestamp = DateTime.UtcNow.ToString("o"),
                    user_name = userName,
                    friendly_name = friendlyName,
                    pin_name = pinName,
                    new_state = newState,
                    device_name = deviceName,
                    device_ip = ipAddress,
                    event_type = "alarm_state_change"
                };

                await eventClient.FireEvent("hass_alarm_state_change", eventData);
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the operation
                Console.WriteLine($"Error firing Home Assistant event: {ex.Message}");
            }
        }
    }
}
