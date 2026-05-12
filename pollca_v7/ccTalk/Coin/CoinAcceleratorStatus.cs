using System;

namespace ccTalk
{
    /// <summary>
    /// Possible states of the coin accelerator connected to the escrow sorter.
    /// </summary>
    [Flags]
    public enum CoinAcceleratorStatus
    {
        /// <summary>Inactive.</summary>
        Inactive = 0x00,
        /// <summary>Accelerator ready.</summary>
        Ready = 0x01,
        /// <summary>Auto mode engaged.</summary>
        Auto = 0x02,
        /// <summary>Coin inside.</summary>
        CoinInside = 0x04,
        /// <summary>Coin ejected.</summary>
        CoinEjected = 0x08,
        /// <summary>Eject error.</summary>
        EjectError = 0x10,
    }
}