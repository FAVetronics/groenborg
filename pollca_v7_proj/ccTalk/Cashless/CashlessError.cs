namespace ccTalk
{
    /// <summary>
    /// Malfunction error codes.
    /// </summary>
    public enum CashlessError
    {
        /// <summary>No problems.</summary>
        Ok = 0xff,
        /// <summary>Unknown status.</summary>
        Unknown = 0xfe,
        /// <summary>Payment media error.</summary>
        MediaError = 0x00,
        /// <summary>Invalid payment media.</summary>
        InvalidMedia = 0x01,
        /// <summary>Tamper error.</summary>
        TamperError = 0x02,
        /// <summary>Manufacturer defined error 1.</summary>
        ManufacturerDefined1 = 0x03,
        /// <summary>Communications error 1.</summary>
        Communication1 = 0x04,
        /// <summary>Reader requires service.</summary>
        RequiresService = 0x05,
        /// <summary>Unassigend.</summary>
        Unassigned = 0x06,
        /// <summary>Manufacturer defined error 2.</summary>
        ManufacturerDefined2 = 0x07,
        /// <summary>Reader failure.</summary>
        ReaderFailure = 0x08,
        /// <summary>Communications error 2.</summary>
        Communication2 = 0x09,
        /// <summary>Payment media jammed.</summary>
        MediaJammed = 0x0a,
        /// <summary>Manufacturer defined error 3.</summary>
        ManufacturerDefined3 = 0x0b,
        /// <summary>Refund error - internal reader credit lost.</summary>
        RefundError = 0x04,
    }
}