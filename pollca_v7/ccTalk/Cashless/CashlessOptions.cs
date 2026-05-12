using System;

namespace ccTalk
{
    /// <summary>
    /// Available options for cashless payment system.
    /// </summary>
    [Flags]
    public enum CashlessOptions
    {
        /// <summary>No special options.</summary>
        None = 0x00,
        /// <summary>Device is capable of restoring funds.</summary>
        Revalue = 0x01,
        /// <summary>Device is multivend capable.</summary>
        Multivend = 0x02,
        /// <summary>Device does have its own display.</summary>
        Display = 0x04,
        /// <summary>Device supports the Vend/Cash Sale subcommand</summary>
        CashSale,
    }
}