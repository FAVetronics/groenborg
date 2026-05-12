using System;

namespace ccTalk
{
    /// <summary>
    /// Available features of the cashless payment system and flags to enable these features.
    /// </summary>
    [Flags]
    public enum CashlessFeatures
    {
        /// <summary>No special features are supported.</summary>
        None = 0x00,
        /// <summary>File transport layer is supported.</summary>
        FTLSupport = 0x00000001,
        /// <summary>32 bit monetary format.</summary>
        Monetary32 = 0x00000002,
        /// <summary>Multi currency, multi lingual support.</summary>
        MultiCurrency = 0x00000004,
        /// <summary>Negative vend is supported.</summary>
        NegativeVend = 0x00000008,
        /// <summary>Data entry - reader is able to send data entered via its keypad.</summary>
        DataEntry = 0x00000010,
        /// <summary>The device allows the "Always Idle" state.</summary>
        AlwaysIdle = 0x00000020,
        /// <summary>The device allows “Remote Vend” feature to initiate dispensing process without the vending machine user interface.</summary>
        RemoteVend = 0x00000040,
        /// <summary>The device allows “Basket / Partial Refund / Options Price” feature to perform multiple vends with single vend request and multiple vend success / vend failures.</summary>
        Basket = 0x00000080,
        /// <summary>he device allows the "Ask Begin Session" feature in selection first.</summary>
        AskBeginSession = 0x00000200,
    }
}