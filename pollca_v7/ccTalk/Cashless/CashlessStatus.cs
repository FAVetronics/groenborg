namespace ccTalk
{
    /// <summary>
    /// Current status of the cashless payment device.
    /// </summary>
    public enum CashlessStatus
    {
        /// <summary>Unknown status.</summary>
        Unknown = 0xff,
        /// <summary>Device not open.</summary>
        Closed = 0xfe,
        /// <summary>Inactive state.</summary>
        Inactive = 0x01,
        /// <summary>Disabled state.</summary>
        Disabled = 0x02,
        /// <summary>Enabled state.</summary>
        Enabled = 0x03,
        /// <summary>Session idle state.</summary>
        SessionIdle = 0x04,
        /// <summary>Vend state.</summary>
        Vend = 0x05,
        /// <summary>Revalue state.</summary>
        Revalue = 0x06,
        /// <summary>Negative vend state.</summary>
        NegativeVend = 0x07,
    }
}