using System;

namespace ccTalk
{
    /// <summary>
    /// Implemented function of the I/O port.
    /// </summary>
    [Flags]
    public enum whDongleIOPortUsage
    {
        /// <summary>Standard configuration as programmable I/O port.</summary>
        Standard = 0,
        /// <summary>Support for Asahi Seiko Dispensers.</summary>
        Dispenser = 1,
    }
}