using System;

namespace ccTalk
{
    /// <summary>
    /// Detailed Coin Feeder status flags.
    /// </summary>
    [Flags]
    public enum CoinFeederStatusFlags
    {
        /// <summary>Nothing to report.</summary>
        Nothing = 0x00,
        /// <summary>Powered on, ready for action.</summary>
        PowerOn = 0x01,
        /// <summary>At least one coin inside</summary>
        Coins = 0x02,
        /// <summary>Coin at position 1.</summary>
        CoinPos1 = 0x04,
        /// <summary>Coin at position 2.</summary>
        CoinPos2 = 0x08,
        /// <summary>External solenoid active.</summary>
        Solenoid = 0x10,
        /// <summary>EMR 100 is active.</summary>
        MotorReject = 0x20,
        /// <summary>Coin ejected.</summary>
        CoinEject = 0x40,
        /// <summary>Cap is closed.</summary>
        CapClosed = 0x80,
    }
}