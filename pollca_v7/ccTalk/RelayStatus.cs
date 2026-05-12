using System;

namespace ccTalk
{
    /// <summary>
    /// Possible states of the relay.
    /// </summary>
    [Flags]
    public enum RelayStatus
    {
        /// <summary>Relay is off.</summary>
        Off = 0,
        /// <summary>Realy is on.</summary>
        On = 1,
    }
}