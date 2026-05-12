using System;

namespace ccTalk
{
    /// <summary>
    /// Escrow sorter master and transaction status data.
    /// </summary>
    [Serializable]
    public struct EscrowSorterCashStatus
    {
        /// <summary>Current status of the transaction(see: <see cref="EscrowSorterMasterStatus"/>).</summary>        
        public EscrowSorterMasterStatus Status;
        /// <summary>Value of inserted coins.</summary>
        public double Value;

        /// <summary>Initialises the structure.</summary>
        /// <param name="init">Just a dummy parameter.</param>
        public EscrowSorterCashStatus(bool init)
        {
            Status = EscrowSorterMasterStatus.Unknown;
            Value = 0.0;
        }
    }
}