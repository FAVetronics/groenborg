using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ccTalk
{
    /// <summary>
    /// Information about encryption support of ccTalk devices.
    /// </summary>
#if !WindowsCE
    [Serializable]
#endif
    public struct CcEncryptionSupport
    {
        /// <summary>Protocol layer encryption method.</summary>
        public CcTalkEncryption ProtocolLevel;
        /// <summary>Command layer encryption method.</summary>
        public CcTalkCryptography CommandLevel;
        /// <summary>Key size in bits for protocol level.</summary>
        public int ProtocolKeySize;
        /// <summary>Key size in bits for command level.</summary>
        public int CommandKeySize;
        /// <summary>Block size in bits for command level.</summary>
        public int BlockSize;
        /// <summary>Current operating mode.</summary>
        public OperatingMode OperatingMode;
        /// <summary>Key bytes for DES encryption.</summary>
        public byte[] DESKey;

        /// <summary>Constructor for setting default values.</summary>
        /// <param name="init">Dummy parameter.</param>
        public CcEncryptionSupport(bool init)
        {
            ProtocolLevel = CcTalkEncryption.None;
            CommandLevel = CcTalkCryptography.None;
            ProtocolKeySize = 0;
            CommandKeySize = 0;
            BlockSize = 0;
            OperatingMode = OperatingMode.Normal;
            DESKey = new byte[8];
            for (int i = 0; i < DESKey.Length; i++) DESKey[i] = 0x00;
        }

#if !WindowsCE
        /// <summary>
        /// Creates a copy of the struct.
        /// </summary>
        /// <returns></returns>
        public CcEncryptionSupport Clone()
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return (CcEncryptionSupport)formatter.Deserialize(ms);
            }
            catch { return this; }
        }
#endif
    }
}