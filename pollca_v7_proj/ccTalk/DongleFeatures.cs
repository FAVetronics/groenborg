using System;

namespace ccTalk
{
    /// <summary>
    /// Implemented features of the dongle.
    /// </summary>
    [Flags]
    public enum DongleFeatures
    {
        /// <summary>Nothing.</summary>
        Nothing = 0x00000000,
        /// <summary>MDB interface.</summary>
        MDB = 0x00000001,
        /// <summary>Serial interface 0.</summary>
        Serial0 = 0x00000002,
        /// <summary>Serial interface 1.</summary>
        Serial1 = 0x00000004,
        /// <summary>JCM ID003 interface.</summary>
        ID003 = 0x00000008,
        /// <summary>LED driver.</summary>
        LEDs = 0x00000100,
        /// <summary>Switch input.</summary>
        Switches = 0x00000200,
        /// <summary>Mult-Purpose IO.</summary>
        MultiIO = 0x00000400,
        /// <summary>Light barriers.</summary>
        LightBarriers = 0x00000800,
        /// <summary>Something spurious.</summary>
        Something = 0x00001000,
        /// <summary>Dispenser support.</summary>
        Dispenser = 0x00010000,
    }
}