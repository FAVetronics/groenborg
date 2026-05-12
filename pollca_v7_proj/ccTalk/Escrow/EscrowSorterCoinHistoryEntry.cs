using System;

namespace ccTalk
{
    /// <summary>
    /// Master mode history entry.
    /// </summary>
    [Serializable]
    public struct EscrowSorterCoinHistoryEntry
    {
        /// <summary>Number of coin.</summary>
        public int CoinNo;
        /// <summary>Insert mode of coin</summary>
        public EscrowSorterMasterInsertMode InsertMode;
    }
}