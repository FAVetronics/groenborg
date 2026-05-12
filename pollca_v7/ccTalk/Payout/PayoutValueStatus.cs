using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ccTalk
{
    /// <summary>
    /// Payout value status data.
    /// </summary>
    [Serializable]
    public struct whPayoutValueStatus
    {
        /// <summary>Payout status flags  (see: <see cref="PayoutStatusFlags"/>).</summary>
        public PayoutStatusFlags Status;
        /// <summary>Total value of remaining payout.</summary>
        public double Remaining;
        /// <summary>Value paid out since last payout commmand.</summary>
        public double LastPaidout;
        /// <summary>Value not yet paid out since last payout commmand.</summary>
        public double LastUnpaid;
        /// <summary>Status of the high level sensor (if present).</summary>
        public PayoutSensorStatus HighLevelSensor;
        /// <summary>Status of the low level sensor (if present).</summary>
        public PayoutSensorStatus LowLevelSensor;

        /// <summary>Returns a deep copy of this object.</summary>
        /// <returns>
        /// A new copy of the <see cref="whPayoutValueStatus"/> object.
        /// </returns>
        public whPayoutValueStatus Clone()
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return (whPayoutValueStatus)formatter.Deserialize(ms);
            }
            catch { return this; }
        }
    }
}