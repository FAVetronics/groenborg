using System;

namespace ccTalk
{
    /// <summary>
    /// Poll response data for change giver.
    /// </summary>
    [Serializable]
    public struct ChgPollResponse
    {
        /// <summary>Poll event  (see: <see cref="SelPollEvent"/>)</summary>
        public SelPollEvent Status;
        /// <summary>Index of accepted coin - only valid if Status == whSelPollEvent.Coin.</summary>
        public int CoinIndex;
        /// <summary>Where was the coin going to?</summary>        
        public CoinRouting Routing;
        /// <summary>If coin was routed into a tube this the number of coins in the tube. If the coin was dispensed manually this is the number of coins dispensed.</summary>        
        public int Count;
    }
}