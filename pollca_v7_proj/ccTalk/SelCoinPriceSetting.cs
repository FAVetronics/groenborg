using System;

namespace ccTalk
{
    /// <summary>
    ///  Setup of the price for cashless payment. (EMP with NFC module only)
    /// </summary>
    [Serializable]
    public struct SelCoinPriceSetting
    {
        /// <summary>Price to be withdrawn from the medium.</summary>
        public double Price;
        /// <summary>Disable cashless payment to allow coins only.</summary>
        public bool CashlessPaymentBlocking;
        /// <summary>Machine is currently in use.</summary>
        public bool MachineOccupied;
        /// <summary>Service mode is active.</summary>
        public bool ServiceModeActive;
    }
}