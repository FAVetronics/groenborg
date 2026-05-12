using System;

namespace ccTalk
{
    /// <summary>
    /// Timing parameters for the escrow sorter master modes.
    /// </summary>
    [Serializable]
    public struct EscrowSorterMasterModeTiming
    {
        /// <summary>Currently not used. should be set to 0.</summary>
        public byte InsertTimeout;
        /// <summary>The maximum time for a coin from insertion to being reported by the coin selector</summary>
        public byte EmpTimeout;
        /// <summary>the maximum time for coin from leaving the coin selector to entering the coin accelerator.</summary>
        public byte AccTimeout;
        /// <summary>Maximum number of retries for ejecting a coin from the accelerator.</summary>
        public byte AccRepeat;
        /// <summary>The timeout for successfully carrying out a move inside the escrow sorter.</summary>
        public byte EscrowTimeout;
        /// <summary>The time the escrow sorter waits after the coin has left the coin selector before carrying out a move.</summary>
        public byte EscrowDelay;
        /// <summary>Currently not used. should be set to 0.</summary>
        public byte ShutterDelay;
    }
}