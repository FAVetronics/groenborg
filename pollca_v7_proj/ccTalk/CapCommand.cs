namespace ccTalk
{
    /// <summary>
    /// Commands for the cap.
    /// </summary>
    public enum CapCommand
    {
        /// <summary>Retrieves the current Status, no further actions.</summary>
        GetState = 0,
        /// <summary>Open the cap.</summary>
        Open = 1,
        /// <summary>Close the cap.</summary>
        Close = 2,
        /// <summary>Open the cap, move feeder backwards for cleaning.</summary>
        Clean = 3,
        /// <summary>Find the next valid position.</summary>
        FindPos = 4,
        /// <summary>Reset cap sequence control.</summary>
        Reset = 9,

    }
}