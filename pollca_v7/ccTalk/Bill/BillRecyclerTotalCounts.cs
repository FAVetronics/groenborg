using System;

namespace ccTalk
{
    /// <summary>
    /// Total counts of the bill recycler. For JCM recyclers only.
    /// </summary>
    [Serializable]
    public struct BillRecyclerTotalCounts
    {
        /// <summary>Total number of filled bills in the recycler.</summary>
        public int Filled;
        /// <summary>Total number of dispensed bills.</summary>
        public int Dispensed;
        /// <summary>Total number of collected bills.</summary>
        public int Collected;
    }
}