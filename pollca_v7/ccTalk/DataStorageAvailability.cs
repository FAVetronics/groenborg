namespace ccTalk
{
    /// <summary>
    /// Data storage availability info.
    /// </summary>
    public struct DataStorageAvailability
    {
        public DataStorageType StorageType;
        public int ReadBlockCount, ReadBlockSize;
        public int WriteBlockCount, WriteBlockSize;
    }
}