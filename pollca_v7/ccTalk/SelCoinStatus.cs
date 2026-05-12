using System;

namespace ccTalk
{
    /// <summary>
    ///  Status of a coin in the selector or change giver.
    /// </summary>
    [Serializable]
    public struct SelCoinStatus
    {
        /// <summary>Inhibit status - set it to "true" to enable coin!</summary>
        public bool Inhibit;
        /// <summary>Sorter path - only valid for coin selector.</summary>
        public byte SorterPath;
        /// <summary>Overide sorting, route coin to cashbox - only valid for coin selector.</summary>
        public bool Override;
    }
}