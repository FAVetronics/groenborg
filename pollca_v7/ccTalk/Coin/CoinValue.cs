using System;

namespace ccTalk
{
    /// <summary>
    /// Properties of a coin in a device.
    /// </summary>
    [Serializable]
    public struct CoinValue
    {
        /// <summary>Two letter currency ID.</summary>
        public string ID;
        /// <summary>Value of the coin.</summary>
        public double Value;
        /// <summary>Integer value of the coin.</summary>
        public int IntValue;
        /// <summary>Number of decimal places for the coins currency.</summary>
        public int Decimals;

        /// <summary>Creates a new instance and initialises the fields with default values.</summary>
        public CoinValue(bool dummy)
        {
            ID = "";
            Value = 0.0;
            IntValue = 0;
            Decimals = 2;
        }
        /// <summary>Creates a new instance and initialises the fields with the given values..</summary>
        public CoinValue(string id, double value, int decimals)
        {
            ID = id;
            Value = value;
            IntValue = (int)(value * Math.Pow(10, decimals));
            Decimals = decimals;
        }
        /// <summary>Compare two coin values including the currency.</summary>
        public bool Compare(CoinValue compval)
        {
            return ((this.ID == compval.ID)) && (Math.Abs(this.Value - compval.Value) < 0.00001);
        }
    }
}