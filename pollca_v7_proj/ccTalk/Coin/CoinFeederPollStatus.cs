using System;

namespace ccTalk
{
    /// <summary>
    /// Complete status information for feeder and cap.
    /// </summary>
    [Serializable]
    public struct CoinFeederPollStatus
    {
        /// <summary>Common feeder status.</summary>
        public CoinFeederStatus Status;
        /// <summary>Feeder status flags.</summary>
        public CoinFeederStatusFlags StatusFlags;
        /// <summary>Feeder error flags.</summary>
        public CoinFeederErrorFlags ErrorFlags;
        /// <summary>Cap status.</summary>
        public CapStatus CapStatus;
    }
}