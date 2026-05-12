namespace ccTalk
{
    /// <summary>
    /// Possible commands for anti-pin-system controlled via TWS 100.
    /// </summary>
    public enum TWSAntiPinCommand
    {
        /// <summary>Disable - always closed, no coins will be notified.</summary>
        Disable = 0,
        /// <summary>Opens automatically when a coin approaches.</summary>
        Automatic = 1,
        /// <summary>Enable - coins will be notified.</summary>
        Enable = 2,
        /// <summary>Accept a coin.</summary>
        Accept = 3,
    }
}