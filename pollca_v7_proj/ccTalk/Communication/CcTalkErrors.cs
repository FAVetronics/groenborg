namespace ccTalk
{
    /// <summary>
    /// ccTalk cash devices error codes.
    /// </summary>
    /// <remarks>
    /// Most methods in this namespace return a value of this type.
    /// </remarks>
    public enum CcTalkErrors
    {
        ///<summary>Everything is fine.</summary>
        Ok,
        /// <summary>Invalid com port.</summary>
        WrongCom,
        /// <summary>Error opening com port.</summary>
        OpenErr,
        /// <summary>No device found while opening connection.</summary>
        NoDevice,
        /// <summary>Can't setup com port.</summary>
        SetupErr,
        /// <summary>Error closing com port.</summary>
        CloseErr,
        /// <summary>Error sending data block.</summary>
        SendErr,
        /// <summary>Port is already opened.</summary>
        AlreadyOpen,
        /// <summary>Port not open.</summary>
        NotOpen,
        /// <summary>Missing local echo - most likely a hardware problem.</summary>
        BadLine,
        /// <summary>Receive timeout.</summary>
        RcvTimout,
        /// <summary>Internal library error.</summary>
        Internal,
        /// <summary>Faulty block format.</summary>
        BlockFormat,
        /// <summary>Data length error.</summary>
        DataLen,
        /// <summary>Common receive error.</summary>
        ReceiveError,
        /// <summary>Negative acknowledge received.</summary>
        NoAck,
        /// <summary>Wrong checksum.</summary>
        ChSumErr,
        /// <summary>Poll too slow - buffered events lost.</summary>
        EventsLost,
        /// <summary>Wrong data format in response</summary>
        DataFormat,
        /// <summary>Wrong destination address.</summary>
        WrongAddr,
        /// <summary>Bad ref parameter.</summary>
        WrongParameter,
        /// <summary>Invalid parameter value.</summary>
        InvalidParameter,
        /// <summary>Invalid command.</summary>
        InvalidCommand,
        /// <summary>Bill route: Escrow was empty.</summary>
        BillEscrowEmpty,
        /// <summary>Bill route: Failed to route bill.</summary>
        BillRouteFailed,
        /// <summary>Command not valid for this device</summary>
        WrongCommand,
        /// <summary>Device not supported</summary>
        UnSupported,
        /// <summary>Payout time exceeded.</summary>
        PayoutExceeded,
        /// <summary>A clone file was not loaded.</summary>
        FileNotLoaded,
        /// <summary>The clone file wasn't comptible with the actual device.</summary>
        FileNotCompatible,
        /// <summary>A lengthy operation was cancelled by the main application</summary>
        OperationCancelled,
        /// <summary>Short circuit on serial port.</summary>
        SerialShortCircuit,
        /// <summary>Failed to initialise CCT 910 for serial communication.</summary>
        SerialInitFailure,
        /// <summary>Exception while initialising CCT 910 for serial communication.</summary>
        SerialInitException,
        /// <summary>Error while initialising DES encryption.</summary>
        InitEncryption,
        /// <summary>Wrong DES key length.</summary>
        DESKeyLength,
        /// <summary>Error while decrypting DES receive data.</summary>
        Decryption,
        /// <summary>Error while encrypting DES send data.</summary>
        Encryption,
        /// <summary>Unsupported encryption method.</summary>
        UnsupportedEncryption,
        ///<summary>A ID003 comm error was reported.</summary>
        CommError,
        ///<summary>The TWS 100 Escrow Sorter has rejected the cash command.</summary>
        CommandRejected,
        ///<summary>The TWS 100 Escrow Sorter has returned a wrong cash reference number.</summary>
        CommandSequence,
        /// <summary>The command cannot be applied to this device.</summary>
        WrongDevice,
        /// <summary>Nobody knows...</summary>
        Unknown,
    }
}