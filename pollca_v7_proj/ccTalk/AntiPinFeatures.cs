namespace ccTalk
{
    /// <summary>
    /// The different features of ant-pin-systems.
    /// </summary>
    public enum AntiPinFeatures
    {
        /// <summary>A device with no features at all.</summary>
        None = 0x00,
        /// <summary>Standard variant.</summary>
        Standard = 0x01,
        /// <summary>With hold function.</summary>
        Hold = 0x02,
        /// <summary>With status signal.</summary>
        Status = 0x04,
    }
}