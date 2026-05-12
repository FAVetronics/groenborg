using System;

namespace ccTalk
{
    /// <summary>
    /// A file that is received from  a peripheral MDB device or transmitted to it.
    /// </summary>
    [Serializable]
    public struct MDBFile
    {
        /// <summary>Common information about the file.</summary>
        public MDBFileInfo Info;
        /// <summary>File was completely transmitted.</summary>
        public bool Complete;
        /// <summary>Actual Length of data.</summary>
        public int Length;
        /// <summary>Received or transmitted file data.</summary>
        public byte[] Data;

        /// <summary>
        /// Creates a new instance and sets default values.
        /// </summary>
        /// <param name="init">Just a dummy parameter</param>
        public MDBFile(bool init)
        {
            Info = new MDBFileInfo();
            Complete = false;
            Length = 0;
            Data = new byte[8192];
        }
    }
}