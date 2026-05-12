namespace ccTalk
{
    /// <summary>
    /// Slave mode and master modes and error codes for escrow sorter.
    /// </summary>
    public enum EscrowSorterMasterMode
    {
        /// <summary>Mode unknown.</summary>
        Unknown = -1,
        /// <summary>Slave mode.</summary>
        Slave = 0,
        /// <summary>Master mode: just coin selector attached.</summary>
        Master1 = 1,
        /// <summary>Master mode: shutter -> coin selector.</summary>
        Master2 = 2,
        /// <summary>Master mode: shutter -> coin selector -> coin accelerator.</summary>
        Master3 = 3,
        /// <summary>Master mode: shutter -> coin accelerator -> coin selector.</summary>
        Master4 = 4,
        /// <summary>Master mode: coin feeder -> coin selector.</summary>
        Master5 = 5,
        /// <summary>Master mode: coin feeder -> coin selector -> coin accelerator.</summary>
        Master6 = 6,

        /// <summary>Master mode return code: invalid parameter.</summary>
        MasterErrorInvalid = 150,
        /// <summary>Master mode return code: missing deivice.</summary>
        MasterErrorMissing = 151,
        /// <summary>Master mode return code: master already active.</summary>
        MasterErrorActive = 152,
    }
}