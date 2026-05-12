using System;

namespace ccTalk
{
    /// <summary>
    /// Control flags for MDB file transfer.
    /// </summary>
    [Flags]
    public enum MDBFileControl
    {
        /// <summary>No flags.</summary>
        None = 0x00,
        /// <summary>Reset device after transfer.</summary>
        Reset = 0x01,
        /// <summary>End of file: the last block of the current FTL session contains the end of this file.</summary>
        EndOfFile = 0x02,
    }
}