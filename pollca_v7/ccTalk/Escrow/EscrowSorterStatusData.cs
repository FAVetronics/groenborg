using System;

namespace ccTalk
{
    /// <summary>
    /// Escrow sorter status data.
    /// </summary>
    [Serializable]
    public struct EscrowSorterStatusData
    {
        /// <summary>Current status of the sorter(see: <see cref="EscrowSorterStatus"/>).</summary>        
        public EscrowSorterStatus Status;
        /// <summary>Status flags(see: <see cref="EscrowSorterStatusFlags"/>).</summary>        
        public EscrowSorterStatusFlags Flags;
        /// <summary>Error flags(see: <see cref="EscrowSorterErrorFlags"/>).</summary>        
        public EscrowSorterErrorFlags ErrorFlags;
        /// <summary>Status of the Anti Pin System (see: <see cref="ccTalk.AntiPinStatus"/>).</summary>
        public TWSAntiPinStatus AntiPinStatus;
        /// <summary>Status of the Coin Accelerator (see: <see cref="ccTalk.CoinAcceleratorStatus"/>).</summary>
        public CoinAcceleratorStatus CoinAcceleratorStatus;
    }
}