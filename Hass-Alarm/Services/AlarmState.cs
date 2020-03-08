using HADotNet.Core;
using HADotNet.Core.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hass_Alarm
{
    public class AlarmState : IAlarmState
    {
        public AlarmState(IConfiguration configuration, ILogger<AlarmState> logger)
        {
            Entity_Arm = configuration.GetValue<string>("Ha:Entities:Arm");
            Entity_ArmHome = configuration.GetValue<string>("Ha:Entities:ArmHome");
            haHost = configuration.GetValue<string>("Ha:Host");
            haApi = configuration.GetValue<string>("Ha:ApiKey");
            _logger = logger;
        }

        public string Entity_Arm { get; }
        public string Entity_ArmHome { get; }

        private readonly string haHost;
        private readonly string haApi;
        private readonly ILogger<AlarmState> _logger;

        public async Task<ArmState> GetArmState()
        {
            ClientFactory.Initialize(haHost, haApi);



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
            return (arm, arm_home) switch
            {
                (true, false) => ArmState.Arm,
                (false, true) => ArmState.Arm_Home,
                (true, true) => ArmState.Arm,
                (false, false) => ArmState.Disarm
            };
        }
        public async Task<bool> SetArmState(ArmState state)
        {
            try
            {
                var currentState = await GetArmState();

                if (currentState != state)
                {
                    ClientFactory.Initialize(haHost, haApi);
                    var serviceClient = ClientFactory.GetClient<ServiceClient>();
                    if (currentState == ArmState.Arm)
                    {
                        if (state == ArmState.Disarm || state == ArmState.Arm_Home)
                            await serviceClient.CallService("input_boolean.turn_off", new { entity_id = Entity_Arm });
                        if (state == ArmState.Arm_Home)
                            await serviceClient.CallService("input_boolean.turn_on", new { entity_id = Entity_ArmHome });
                    }
                    if (currentState == ArmState.Arm_Home)
                        await serviceClient.CallService("input_boolean.turn_off", new { entity_id = Entity_ArmHome });
                    if (currentState == ArmState.Disarm)
                    {
                        if (state == ArmState.Arm)
                            await serviceClient.CallService("input_boolean.turn_on", new { entity_id = Entity_Arm });
                        if (state == ArmState.Arm_Home)
                            await serviceClient.CallService("input_boolean.turn_on", new { entity_id = Entity_ArmHome });
                    }

                }


            }
            catch (Exception)
            {

                return false;
            }
            return true;
        }
    }
    public enum ArmState
    {
        Disarm,
        Arm,
        Arm_Home
    }
}
