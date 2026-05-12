using System;

namespace ccTalk
{
    /// <summary>
    /// Struct holding only the relevant information of whCcTalkComm for saving a list
    /// </summary>
    [Serializable]
    public struct CcTalkListDevice
    {
        /// <summary>Address of the com port.</summary>
        public string Port;
        /// <summary>Address of the device.</summary>
        public byte Address;
        /// <summary>Checksum type of the device.</summary>
        public CcTalkChecksumTypes ChecksumType;
        /// <summary>Encryption type of the device</summary>
        public CcTalkEncryption EncryptionMode;
        /// <summary>Encryption support level.</summary>
        public CcEncryptionSupport EncryptionSupport;

        /// <summary>
        /// Create a new instance and initialise it with the data of a ccTalk device.
        /// </summary>
        /// <param name="device">Device providing the core information.</param>
        public CcTalkListDevice(CcTalkComm device)
        {
            this.Port = device.Port;
            this.Address = device.Address;
            this.ChecksumType = device.ChecksumType;
            this.EncryptionMode = device.EncryptionMode;
            this.EncryptionSupport = device.EncryptionSupport.Clone();
        }
    }
}