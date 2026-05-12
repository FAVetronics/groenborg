namespace ccTalk
{
    /// <summary>
    /// Escrow sorter handling of shutter and coin selector on coin insert for maximum safety or maximum speed.
    /// </summary>
    public enum EscrowSorterMasterInsertMode
    {
        /// <summary>Safest mode but slow.</summary>
        Safe = 0x00,
        /// <summary>Faster but still safe.</summary>
        Normal = 0x40,
        /// <summary>Fast mode, coins may be rejected.</summary>
        Fast = 0x80,
        /// <summary>Very fast, more rejections, coin jams possible.</summary>
        Fastest = 0xc0,
    }
}