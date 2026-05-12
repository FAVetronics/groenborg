using System;

namespace ccTalk
{
    /// <summary>
    /// Possible states of the LED drivers.
    /// </summary>
    [Flags]
    public enum LEDStatus
    {
        /// <summary>LED is permanently off.</summary>
        Off = 0,
        /// <summary>LED is permanently on.</summary>
        On = 1,
        /// <summary>LED is flashing.</summary>
        Flashing = 2,
    }
}