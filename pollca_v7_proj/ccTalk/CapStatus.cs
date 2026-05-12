namespace ccTalk
{
    /// <summary>
    /// Status of the coin feeder's cap.
    /// </summary>
    public enum CapStatus
    {
        /// <summary>Cap control disabled by software.</summary>
        Disabled = 0,
        /// <summary>Cap control was resetted.</summary>
        Reset = 1,
        /// <summary>Searching for next valid position</summary>
        FindPos = 2,
        /// <summary>Cap is closed.</summary>
        IsClosed = 3,
        /// <summary>Cap is open.</summary>
        IsOpen = 4,
        /// <summary>Open command received.</summary>
        OpenCommand = 5,
        /// <summary>Cap is opening</summary>
        Opening = 6,
        /// <summary>Cap is open and cleaning.</summary>
        Cleaning = 7,
        /// <summary>Close command received.</summary>
        CloseCommand = 8,
        /// <summary>Cap is closing</summary>
        Closing = 9,
        /// <summary>General cap failure.</summary>
        Error = 100,
        /// <summary>Open procedure timed out.</summary>
        OpenTimeOut = 101,
        /// <summary>Cap in wrong position for open.</summary>
        OpenError = 102,
        /// <summary>Close procedure timed out.</summary>
        CloseTimeOut = 103,
        /// <summary>Cap in wrong position for close.</summary>
        CloseError = 104,
        /// <summary>Position sensor faulty.</summary>
        SensorError = 105,
        /// <summary>Nobody knows...</summary>
        Unknown,
    }
}