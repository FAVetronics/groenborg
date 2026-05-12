using System;

namespace ccTalk
{
    /// <summary>
    /// Poll response data for cashles payment device.
    /// </summary>
    [Serializable]
    public struct CashlessPollResponse
    {
        /// <summary>Poll event  (see: <see cref="CashlessPollEvent"/>)</summary>
        public CashlessPollEvent Status;
        /// <summary>Malfunction error code - applies only if Status == CashlessPollEvent.Malfunction.</summary>
        public CashlessError Error;
        /// <summary>Display data - valid only if  Status == CashlessPollEvent.Display.</summary> 
        public CashlessDisplay Display;
        /// <summary>Amount of currency - meaning depends on Status.</summary>
        public double Amount;
        /// <summary>The 32 bit ID of the payment media.</summary>
        public long MediaID;
    }
}