namespace ccTalk
{
    /// <summary>
    /// Possible payout modes.
    /// </summary>
    public enum PayoutMode
    {
        /// <summary>Use serial number to verify payout command.</summary>
        SerialNumber,
        /// <summary>Use dummy key verify payout command.</summary>
        NoEncryption,
        /// <summary>Use encrypted key to verify payout command.</summary>
        Encrypted,          // Currently not supported
    }
}