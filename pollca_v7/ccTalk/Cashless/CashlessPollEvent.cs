namespace ccTalk
{
    /// <summary>
    /// Poll response data.
    /// </summary>
    public enum CashlessPollEvent
    {
        /// <summary>Nothing to report.</summary>
        Null = 0xff,
        /// <summary>Unknown response.</summary>
        Unknown = 0xfe,
        /// <summary>Just reset.</summary>
        Reset = 0x00,
        /// <summary>Reader Configuration Data.</summary>
        Config = 0x01,
        /// <summary>Display request.</summary>
        Display = 0x02,
        /// <summary>begin session.</summary>
        BeginSession = 0x03,
        /// <summary>Session cancel request.</summary>
        CancelRequest = 0x04,
        /// <summary>Vend approved.</summary>
        VendApproved = 0x05,
        /// <summary>Vend denied.</summary>
        VendDenied = 0x06,
        /// <summary>Session cancelled.</summary>
        EndSession = 0x07,
        /// <summary>Reader Config Data.</summary>
        Cancelled = 0x08,
        /// <summary>Reader Identify Data.</summary>
        Identify = 0x09,
        /// <summary>Malfunction or error.</summary>
        Malfunction = 0x0a,
        /// <summary>Command out of sequence.</summary>
        OutOfSequence = 0x0b,
        /// <summary>Unknown response code 0x0C.</summary>
        Unknown0C = 0x0c,
        /// <summary>Revalue approved.</summary>
        RevalueApproved = 0x0d,
        /// <summary>Revalue denied.</summary>
        RevalueDenied = 0x0e,
        /// <summary>Revalue limit amount.</summary>
        RevalueLimit = 0x0f,
        /// <summary>Obsolete request: number of user file.</summary>
        NumberOfUserFile = 0x10,
        /// <summary>
        /// Request to synchronize the real time clock of the reader.
        /// Use <see cref="CashlessComm.SetDateTime"/> to react on this.
        /// </summary>
        TimeDateRequest = 0x11,
        /// <summary>
        /// If Data Entry option is enabled: the reader is making a data entry request.
        /// There's currently no support for this feature.
        /// </summary>
        DataEntryRequestResponse = 0x12,
        /// <summary>
        /// If Data Entry option is enabled: cancel all data entry activities.
        /// There's currently no support for this feature.
        /// </summary>
        DataEntryCancel = 0x13,
        /// <summary>
        /// If File Transfer Layer option is enabled: the reader is requesting to receive data.
        /// There's currently no support for this feature.
        /// </summary>
        RequestToReceive = 0x1b,
        /// <summary>
        /// If File Transfer Layer option is enabled: the reader is requesting to retry or deny last FTL command.
        /// There's currently no support for this feature.
        /// </summary>
        RetryDeny = 0x1c,
        /// <summary>
        /// If File Transfer Layer option is enabled: the reader is sending a block of data.
        /// This will be handled internally by the library.
        /// </summary>
        SendBlock = 0x1d,
        /// <summary>
        /// If File Transfer Layer option is enabled: the reader is indicating that is OK for the device to send data.
        /// There's currently no support for this feature.
        /// </summary>
        OkToSend = 0x1e,
        /// <summary>
        /// If File Transfer Layer option is enabled: the reader is requesting send data.
        /// </summary>
        RequestToSend = 0x1f,
    }
}