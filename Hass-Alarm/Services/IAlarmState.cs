using System.Threading.Tasks;

namespace Hass_Alarm
{
    public interface IAlarmState
    {
        string Entity_Arm { get; }
        string Entity_ArmHome { get; }

        Task<ArmState> GetArmState();
        Task<bool> SetArmState(ArmState state);
    }
}