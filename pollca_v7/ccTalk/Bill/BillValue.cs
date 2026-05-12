using System;

namespace ccTalk
{
    /// <summary>
    /// Properties of a bill in a device.
    /// </summary>
    [Serializable]
    public struct BillValue
    {
        /// <summary>Value of the Bill.</summary>
        public double Value;
        /// <summary>Two letter currency ID.</summary>
        public string ID;
        /// <summary>Number of decimal places.</summary>
        public int Decimals;

        /// <summary>
        /// Checks if currency and value is identical.
        /// </summary>
        /// <param name="cmpbill">The bill value to be compared with.</param>
        /// <returns></returns>
        public bool IsIdentical(BillValue cmpbill)
        {
            return ((this.ID.ToLower() == cmpbill.ID.ToLower()) && (Math.Abs(this.Value - cmpbill.Value) < 0.0001) && (this.Decimals == cmpbill.Decimals));
        }
    }
}