namespace ccTalk
{
    /// <summary>
    /// Types of host data storage in the device.
    /// </summary>
    public enum DataStorageType
    {
        /// <summary>Volatile, lost on reset.</summary>
        VolatileReset = 0,
        /// <summary>Volatile, lost on power-down.</summary>
        VolatilePower = 1,
        /// <summary>Permanent, limited use.</summary>
        PermanentLimited = 2,
        /// <summary>Permanent, unlimited use.</summary>
        PermanentUnlimited = 3,
        /// <summary>No data storage available.</summary>
        None = 99,
    }
}