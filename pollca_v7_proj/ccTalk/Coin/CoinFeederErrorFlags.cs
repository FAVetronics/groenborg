using System;

namespace ccTalk
{
    /// <summary>
    /// Detailed Coin Feeder error flags.
    /// </summary>
    [Flags]
    public enum CoinFeederErrorFlags
    {
        /// <summary>Nothing to report.</summary>
        Nothing = 0x00,
        /// <summary>Internal watchdog has bitten</summary>
        Watchdog = 0x0001,
        /// <summary>Internal error.</summary>
        Internal = 0x0002,
        /// <summary>Emergency, motor stopped by external event</summary>
        Emergency = 0x0004,
        /// <summary>Defective position sensor.</summary>
        PositionSensor = 0x0008,
        /// <summary>Reserved for future use.</summary>
        Reserved1 = 0x0010,
        /// <summary>Coin jammed inside c CIS 100.</summary>
        InternalJam = 0x0020,
        /// <summary>Coin jammed in external path.</summary>
        ExternalJam = 0x0040,
        /// <summary>CcTalk communication error.</summary>
        CommError = 0x0080,
        /// <summary>Reserved for future use.</summary>
        Reserved2 = 0x0100,
        /// <summary>Can't fetch coin in due time.</summary>
        CoinFetch = 0x0200,
        /// <summary>Maximum motor current exceeded.</summary>
        MotorCurrent = 0x0400,
        /// <summary>Motor timed out.</summary>
        MotorTimeout = 0x0800,
        /// <summary>Can't eject coin in due time.</summary>
        CoinEject = 0x1000,
        /// <summary>Motor reject was automatically activated.</summary>
        MotorReject = 0x2000,
        /// <summary>Coin didn't reach eject position in due time.</summary>
        CoinTimeout = 0x4000,
        /// <summary>Reserved for future use.</summary>
        Reserved3 = 0x8000,
    }
}