namespace ccTalk.Bill
{
    /// <summary>
    /// Bill positions.
    /// </summary>
    public enum ValBillPosition
    {
        /// <summary>Position unknown.</summary>
        Unknown = -1,
        /// <summary>Bill was sent to the cash box.</summary>
        Stacked = 0,
        /// <summary>Holding bill in escrow position.</summary>
        Escrow = 1,
        #region adp AFD MD-100 specific
        /// <summary>For adp AFD-MONO: bill moved to dispenser SS1.</summary>
        AFD_DispenserSS1 = 0x12,
        /// <summary>For adp AFD-MONO: bill moved to dispenser SS2.</summary>
        AFD_DispenserSS2 = 0x13,
        /// <summary>For adp AFD-MONO: bill moved to dispenser SS3.</summary>
        AFD_DispenserSS3 = 0x14,
        /// <summary>Bill form recycler stored in cash box.</summary>
        AFD_Stored = 0x20,
        #endregion
    }
}