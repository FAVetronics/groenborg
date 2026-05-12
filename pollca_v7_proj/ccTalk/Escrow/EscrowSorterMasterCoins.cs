using System;

namespace ccTalk
{
    /// <summary>
    /// Escrow sorter master mode coin statistics for active transacttion.
    /// </summary>
    [Serializable]
    public struct EscrowSorterMasterCoins
    {
        /// <summary>value of inserted coins.</summary>
        public double Value;
        /// <summary>Total number of rejected coins.</summary>
        public int Rejected;
        /// <summary>Number of coins for path 0 to 9.</summary>
        public int[] Paths;
        /// <summary>Number of unidentified coins.</summary>
        public int Unidentified;
        /// <summary>Number of disabled coins rejected.</summary>
        public int Disabled;
        /// <summary>Number of coin jams.</summary>
        public int CoinJam;
        /// <summary>Number of follow-up coins.</summary>
        public int FollowUp;
        /// <summary>Number of coins-on-a-string.</summary>
        public int CoinOnString;
        /// <summary>Number of coin "selector not ready" events.</summary>
        public int NotReady;
        /// <summary>Number of motor reject activations.</summary>
        public int MotorReject;
        /// <summary>Number of undue coins in accelerator.</summary>
        public int UndueAcc;
        /// <summary>Number of accelerator repeats.</summary>
        public int RepeatAcc;
        /// <summary>Number of unknown errors.</summary>
        public int Unknown;

        /// <summary>Initialises the structure.</summary>
        /// <param name="init">Just a dummy parameter.</param>
        public EscrowSorterMasterCoins(bool init)
        {
            Value = 0.0;
            Rejected = 0;
            Paths = new int[10];
            Unidentified = 0;
            Disabled = 0;
            CoinJam = 0;
            FollowUp = 0;
            CoinOnString = 0;
            NotReady = 0;
            MotorReject = 0;
            UndueAcc = 0;
            RepeatAcc = 0;
            Unknown = 0;
        }
    }
}