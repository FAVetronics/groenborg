namespace ccTalk
{
    /// <summary>
    /// States the device can be set to using <see cref="CashlessComm.SetState"/>.
    /// </summary>
    public enum CashlessState
    {
        /// <summary>Disable cashless payment device -  transaction in progress will not be affected.</summary>
        Disable = 0x00,
        /// <summary>Enable cashless payment device. It will now accept payment media</summary>
        Enable = 0x01,
        /// <summary>Abort cashless payment device activities.</summary>
        Cancel = 0x02,
    }
}