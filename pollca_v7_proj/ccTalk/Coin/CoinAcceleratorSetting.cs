namespace ccTalk
{
    /// <summary>
    /// Possible settings for coin accelerator to the escrow sorter.
    /// </summary>
    public enum whCoinAcceleratorSetting
    {
        /// <summary>Setting unknown.</summary>
        Unknown,
        /// <summary>Disabled - inactive.</summary>
        Disabled = 0,
        /// <summary>Fires automatically when a coin drops.</summary>
        Auto = 1,
        /// <summary>Enables the coin accelerator for ejection.</summary>
        Enable = 2,
        /// <summary>Fires coin accelerator.</summary>
        Eject = 3,
    }
}