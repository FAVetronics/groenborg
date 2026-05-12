using System;

namespace ccTalk
{
    /// <summary>
    /// Possible states of the anti pin system.
    /// </summary>
    [Flags]
    public enum AntiPinStatus
    {
        /// <summary>Inactive.</summary>
        Inactive = 0x00,
        /// <summary>Coin detected.</summary>
        Coin = 0x01,
        /// <summary>Shutter open.</summary>
        Open = 0x02,
        /// <summary>String detected.</summary>
        String = 0x04,
        /// <summary>Shutter ready.</summary>
        Ready = 0x10,
        /// <summary>Auto mode active.</summary>
        Auto = 0x20,
        /// <summary>Error detected.</summary>
        Error = 0x40,
    }
}