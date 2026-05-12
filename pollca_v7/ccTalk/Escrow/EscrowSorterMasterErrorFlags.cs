using System;

namespace ccTalk
{
    /// <summary>
    /// Error flags for timeouts in escrow sorter master mode. When setting up master mode these flags can be used to enable which errors should cancel the transaction.
    /// </summary>
    [Flags]
    public enum EscrowSorterMasterErrorFlags
    {
        /// <summary>No errors.</summary>
        None = 0x00,
        /// <summary>Insert timeout.</summary>
        Insert = 0x01,
        /// <summary>Coin selector timeout.</summary>
        CoinSelector = 0x02,
        /// <summary>Coin accelerator timeout.</summary>
        Accelerator = 0x04,
        /// <summary>Sorter timeout.</summary>
        Sorter = 0x08,
        /// <summary>Coin accelerator repeat error.</summary>
        AccRepeat = 0x10,
        /// <summary>Reserved for future use.</summary>
        Reserved = 0x20,
        /// <summary>Masks or sets all flags.</summary>
        All = 0x1f,
    }
}