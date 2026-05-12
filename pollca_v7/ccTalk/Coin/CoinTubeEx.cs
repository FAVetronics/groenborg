using System;

namespace ccTalk
{
    /// <summary>
    /// Extended properties of a coin tube in a change giver.
    /// Includes the "secure coins" of a change giver. Not supported by all brands
    /// </summary>
    [Serializable]
    public struct CoinTubeEx
    {
        /// <summary>Is a tube present for this coin?</summary>
        public bool Present;
        /// <summary>Number of coins in the tube.</summary>
        public int Coins;
        /// <summary>Number of secure coins in the tube.</summary>
        public int SecureCoins;
        /// <summary>Is the tube full?</summary>
        public bool Full;
        /// <summary>Is the tube faulty?</summary>
        public bool Error;

    }
}