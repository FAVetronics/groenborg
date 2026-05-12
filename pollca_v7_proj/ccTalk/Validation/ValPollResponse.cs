using System;
using ccTalk.Bill;

namespace ccTalk.Validation
{
    /// <summary>
    /// Poll response data.
    /// </summary>
    [Serializable]
    public struct ValPollResponse
    {
        /// <summary>Poll event  (see: <see cref="ValPollEvent"/>).</summary>
        public ValPollEvent Status;
        /// <summary>Index of accepted coin - only valid if Status == whValPollEvent.Bill.</summary>
        public int BillIndex;
        /// <summary>Sorter path of accepted Bill - only valid if Status == whValPollEvent.Bill.</summary>        
        public ValBillPosition BillPosition;
    }
}