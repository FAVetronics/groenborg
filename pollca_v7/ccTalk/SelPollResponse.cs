using System;

namespace ccTalk
{
    /// <summary>
    /// Poll response data for coin selector.
    /// </summary>
    [Serializable]
    public struct SelPollResponse
    {
        /// <summary>A coin was inserted.</summary>
        public bool CoinInserted;
        /// <summary>Poll event  (see: <see cref="SelPollEvent"/>).</summary>
        public SelPollEvent Status;
        /// <summary>Index of accepted coin - only valid if Status == whSelPollEvent.Coin.</summary>
        public int CoinIndex;
        /// <summary>Sorter path of accepted coin.</summary>        
        public int CoinPath;
    }
}