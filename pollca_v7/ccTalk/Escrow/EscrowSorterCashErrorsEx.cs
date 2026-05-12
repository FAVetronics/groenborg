using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ccTalk
{
    /// <summary>
    /// Sorter master error data, extended structure. 
    /// </summary>
    [Serializable]
    public struct EscrowSorterCashErrorsEx
    {
        /// <summary>Result of the retrieve command</summary>
        public CcTalkErrors CcTalkError;
        /// <summary>Current status of the transaction.</summary>        
        public EscrowSorterMasterStatus Status;
        /// <summary>Current sorter status.</summary>
        public EscrowSorterStatus SorterStatus;
        /// <summary>Error source or sub error. The exact meaning depends an the status.</summary>
        public byte SubError;
        /// <summary>Extended error information. The meaning depends an the status.</summary>  
        public UInt16 Extended0;
        /// <summary>Extended error information. The meaning depends an the status.</summary>  
        public UInt16 Extended1;
        /// <summary>Extended error information. The meaning depends an the status.</summary>  
        public UInt32 Extended2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// Sets some default values.
        /// </remarks>
        public EscrowSorterCashErrorsEx(bool init)
        {
            CcTalkError = CcTalkErrors.Unknown;
            Status = EscrowSorterMasterStatus.Unknown;
            SorterStatus = EscrowSorterStatus.Unknown;
            SubError = 0x00;
            Extended0 = 0x0000;
            Extended1 = 0x0000;
            Extended2 = 0x00000000;
        }

        /// <summary>Returns a deep copy of this object.</summary>
        /// <returns>
        /// A new copy of the object.
        /// </returns>
        public EscrowSorterCashErrorsEx Clone()
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return (EscrowSorterCashErrorsEx)formatter.Deserialize(ms);
            }
            catch { return this; }
        }

    }
}