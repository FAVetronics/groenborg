using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ccTalk
{
    /// <summary>
    /// Info about data file transmitted to or from a peripheral MDB device.
    /// </summary>
    [Serializable]
    public struct MDBFileInfo
    {
        /// <summary>Destination of data: 0x10/0x60 for reader, 0x00 for VMC.</summary>
        public byte Destination;
        /// <summary>Source of data: 0x10/0x60 for reader, 0x00 for VMC.</summary>
        public byte Source;
        /// <summary>The type of information - please refer to the manufacturer's manual for further information</summary>
        public byte FileID;
        /// <summary>Maximum number of data blocks - each up to 31 bytes.</summary>
        public byte MaxBlocks;
        /// <summary>Data transfer control flags.</summary>
        public MDBFileControl Control;

        /// <summary>Returns a deep copy of this object.</summary>
        /// <returns>
        /// A new copy of the <see cref="MDBFileInfo"/> object.
        /// </returns>
        public MDBFileInfo Clone()
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return (MDBFileInfo)formatter.Deserialize(ms);
            }
            catch { return this; }
        }
    }
}