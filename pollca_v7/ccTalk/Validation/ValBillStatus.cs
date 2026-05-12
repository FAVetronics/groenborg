using System;

namespace ccTalk.Bill
{
    /// <summary>
    ///  Status of a Bill in the selector.
    /// </summary>
    [Serializable]
    public struct ValBillStatus
    {
        /// <summary>Inhibit status.</summary>
        public bool Inhibit;
    }
}