using System;

namespace ccTalk
{
    /// <summary>
    /// Peripheral status data.
    /// </summary>
    [Serializable]
    public struct PeripheralStatus
    {
        /// <summary>State of the escrow flaps (see: <see cref="EscrowState"/>).</summary>
        public EscrowState EscrowFlaps;
        /// <summary>True if the position switch of the escrow is closed.</summary>
        public bool EscrowSwitch;
        /// <summary>Setting of the Anti Pin System (see: <see cref="AntiPinSetting"/>).</summary>
        public AntiPinSetting AntiPinSetup;
        /// <summary>Status of the Anti Pin System (see: <see cref="ccTalk.AntiPinStatus"/>).</summary>
        public AntiPinStatus AntiPinStatus;
        /// <summary>True if the switch of the motor reject is closed.</summary>
        public bool MotorRejectSwitch;
        /// <summary>Initialises the structure.</summary>
        /// <param name="init">Just a dummy parameter.</param>
        public PeripheralStatus(bool init)
        {
            EscrowFlaps = EscrowState.Unknown;
            EscrowSwitch = false;
            AntiPinSetup = AntiPinSetting.Unknown;
            AntiPinStatus = AntiPinStatus.Inactive;
            MotorRejectSwitch = false;
        }
    }
}