using System;

namespace ccTalk
{
    /// <summary>
    /// Dispense bills status data.
    /// </summary>
    [Serializable]
    public struct BillRecyclerStatus
    {
        /// <summary>Recycler status flags <see cref="RecyclerFlags"/>.</summary>
        public RecyclerFlags Status;
        ///<summary>Last pay out reject code.</summary>
        public BillPayoutRejectCode PayOutReject;
        /// <summary>Total number of remaining bills to be dispensed.</summary>
        public int Remaining;
        /// <summary>Number of bills dispensed out since last dispense commmand.</summary>
        public int LastDispensed;
        /// <summary>Number of bills not yet paid out since last dispense commmand.</summary>
        public int LastUndispensed;
        /// <summary>Number of bills already collected to the recycler.</summary>
        public int Stored;
        /// <summary>Number of bills under collection operation.</summary>
        public int Storing;
        ///<summary>Number of bills under collection due to dispense reject.</summary>
        public int PayRejectCount;
        ///<summary>Number of bills already collected due to dispense reject.</summary>
        public int PayRejectedCount;
    }
}