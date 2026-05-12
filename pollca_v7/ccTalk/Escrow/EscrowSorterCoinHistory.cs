using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ccTalk
{
    /// <summary>
    /// Master mode history of inserted coins.
    /// </summary>
    [Serializable]
    public struct EscrowSorterCoinHistory
    {
        /// <summary></summary>
        public int CoinsInEscrow;
        /// <summary></summary>
        public EscrowSorterCoinHistoryEntry[] History;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// Sets some default values.
        /// </remarks>
        public EscrowSorterCoinHistory(bool init)
        {
            CoinsInEscrow = 0;
            History = new EscrowSorterCoinHistoryEntry[0];
        }

        /// <summary>Returns a deep copy of this object.</summary>
        /// <returns>
        /// A new copy of the object.
        /// </returns>
        public EscrowSorterCoinHistory Clone()
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return (EscrowSorterCoinHistory)formatter.Deserialize(ms);
            }
            catch { return this; }
        }

    }
}