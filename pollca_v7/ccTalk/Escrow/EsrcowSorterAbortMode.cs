namespace ccTalk
{
    /// <summary>
    /// Detailled modes for AbortCash.
    /// </summary>    
    public enum EsrcowSorterAbortMode
    {
        /// <summary>Eject all inserted coins and check for errors.</summary>
        Default = 0x00,
        /// <summary>Empty all positions and ignore errors.</summary>
        EjectAllNoError = 0x01,
        /// <summary>Eject all inserted coins and ignore errors.</summary>
        EjectNoError = 0x02,
        /// <summary>Reserved for future use.</summary>
        EjectReserved = 0x03,
        /// <summary>Activate coin accelerator. Eject all inserted coins and check for errors.</summary>
        EjectWmb = 0x04,
        /// <summary>Activate coin accelerator. Empty all positions and ignore errors.</summary>
        EjectWmbAllNoError = 0x05,
        /// <summary>Activate coin accelerator. Eject all inserted coins and ignore errors.</summary>
        EjectWmbNoError = 0x06,
    };
}