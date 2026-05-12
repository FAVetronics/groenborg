using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ccTalk
{
    /// <summary>
    /// Extended payout coins status data: extra field Events.
    /// </summary>
    [Serializable]
    public struct PayoutStatusEx
    {
        /// <summary>Total number of payout events. 
        /// When the event counter is at 255 the next event causes the counter to change to 1. 
        /// The only way for the counter to be 0 is at power-up or reset.</summary>
        public int Events;
        /// <summary>Payout status flags  (see: <see cref="PayoutStatusFlags"/>).</summary>
        public PayoutStatusFlags Status;
        /// <summary>Total number of remaining payout coins.</summary>
        public int Remaining;
        /// <summary>Number of coins paid out since last payout commmand.</summary>
        public int LastPaidout;
        /// <summary>Number of coins not yet paid out since last payout commmand.</summary>
        public int LastUnpaid;
        /// <summary>Status of the high level sensor (if present).</summary>
        public PayoutSensorStatus HighLevelSensor;
        /// <summary>Status of the low level sensor (if present).</summary>
        public PayoutSensorStatus LowLevelSensor;

        /// <summary>Returns a deep copy of this object.</summary>
        /// <returns>
        /// A new copy of the <see cref="PayoutStatusEx"/> object.
        /// </returns>
        public PayoutStatusEx Clone()
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return (PayoutStatusEx)formatter.Deserialize(ms);
            }
            catch { return this; }
        }
    }
}