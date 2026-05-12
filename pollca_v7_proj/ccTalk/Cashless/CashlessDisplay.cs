using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ccTalk
{
    /// <summary>
    /// Display request data.
    /// </summary>
    [Serializable]
    public struct CashlessDisplay
    {
        /// <summary> Display time in milliseconds.</summary>
        public int Duration;
        /// <summary> Display data formatted for 2 rows with 16 columns.</summary>
        public string[] Data;

        /// <summary>Returns a deep copy of this object.</summary>
        /// <returns>
        /// A new copy of the <see cref="CashlessDisplay"/> object.
        /// </returns>
        public CashlessDisplay Clone()
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return (CashlessDisplay)formatter.Deserialize(ms);
            }
            catch { return this; }
        }
    }
}