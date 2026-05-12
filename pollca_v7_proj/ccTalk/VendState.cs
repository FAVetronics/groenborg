namespace ccTalk
{
    /// <summary>
    /// Possible values for SetVendState()>.
    /// </summary>
    public enum VendState
    {
        /// <summary>Cancel a vend request.</summary>
        Cancel = 0x01,
        /// <summary>Vend succesfully completed.</summary>
        Success = 0x02,
        /// <summary>Vend failed. Fund should be refunded.</summary>
        Failure = 0x03,
        /// <summary>Session complete. Cashless payment device will return to Enabled state.</summary>
        Complete = 0x04,
    }
}