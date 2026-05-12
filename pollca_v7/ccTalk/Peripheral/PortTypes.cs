namespace ccTalk
{
    /// <summary>
    /// Types of serial port supported by this class library.
    /// </summary>
    public enum PortTypes
    {
        /// <summary>Matches all types.</summary>
        Any = 0,
        /// <summary>Standard serial port.</summary>
        Serial = 1,
        /// <summary>Virtual USB.</summary>
        USB = 2,
        /// <summary>Virtual Bluetooth.</summary>
        Bluetooth = 3,
        /// <summary>Virtual IrDA.</summary>
        IrDA = 4,    // 
        /// <summary>Who knows?</summary>
        Other = 5,
    }
}