using System;

namespace ccTalk
{
    /// <summary>
    /// Advanced control flags for master mode.
    /// </summary>
    [Flags]
    public enum EscrowSorterAdvancedControlFlags
    {
        /// <summary>Nothing to control.</summary>
        None = 0x00,
        /// <summary>Activate chamber monitoring.</summary>
        ChamberMonitoring = 0x04,
        /// <summary>Activate chamber monitoring during Ready state.</summary>
        ChamberMonitoringReady = 0x08,
        /// <summary>Deactivate the delay for the recognition of a second coin in the coin accelerator.</summary>
        NoAcceleratorDelay = 0x80,
    }
}