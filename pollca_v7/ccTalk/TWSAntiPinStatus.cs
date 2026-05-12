using System;

namespace ccTalk
{
    /// <summary>
    /// Possible states of the anti pin system controlled via TWS 100.
    /// </summary>
    [Flags]
    public enum TWSAntiPinStatus
    {
        /// <summary>Disabled.</summary>
        Disabled = 0x0000,
        /// <summary>Enabled.</summary>
        Enabled = 0x0001,
        /// <summary>Automatic mode.</summary>
        Automatic = 0x0002,
        /// <summary>Coin detected.</summary>
        CoinDetected = 0x0004,
        /// <summary>Coin accepted.</summary>
        CoinAccepted = 0x0008,
        /// <summary>Error detected.</summary>
        Error = 0x0100,
    }
}