using System;

namespace ccTalk
{
    /// <summary>
    /// Properties of a coin tube in a change giver.
    /// </summary>
    [Serializable]
    public struct CoinTube
    {
        /// <summary>Is a tube present for this coin?</summary>
        public bool Present;
        /// <summary>Number of coins in the tube.</summary>
        public int Coins;
        /// <summary>Is the tube full?</summary>
        public bool Full;
        /// <summary>Is the tube faulty?</summary>
        public bool Error;

    }
}