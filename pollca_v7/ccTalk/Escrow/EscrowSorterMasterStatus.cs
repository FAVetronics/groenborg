namespace ccTalk
{
    /// <summary>
    /// States and error codes for master mode of the escrow sorter.
    /// </summary>
    public enum EscrowSorterMasterStatus
    {
        /// <summary>Status unknown.</summary>
        Unknown = -1,
        /// <summary>Slave mode.</summary>
        Disabled = 0,
        /// <summary>Checking connected devices.</summary>
        CheckDevices = 1,
        /// <summary>Initialising connected devices.</summary>
        InitDevices = 2,
        /// <summary>Ready for cash transaction.</summary>
        Ready = 3,
        /// <summary>Starting transaction.</summary>
        StartCash = 16,
        /// <summary>Transaction paused.</summary>
        PauseCash = 17,
        /// <summary>Transaction ended.</summary>
        EndCashIn = 18,
        /// <summary>Transaction aborted.</summary>
        AbortCash = 19,
        /// <summary>Transaction complete.</summary>
        CashComplete = 20,
        /// <summary>Sorter full - all 50 positions occupied. Transaction stopped.</summary>
        SorterFull = 21,
        /// <summary>Transaction ended, waiting for sorter ready.</summary>
        WaitEndCashIn = 22,
        /// <summary>Transaction aborted, waiting for sorter ready.</summary>
        WaitAbortCashIn = 23,
        /// <summary>Initialising next coin insert.</summary>
        NextCoinInsert = 32,
        /// <summary>Waiting for next coin insert.</summary>
        WaitCoinInsert = 33,
        /// <summary>Waiting for coin in accelearator.</summary>
        WaitCoinAcc = 34,
        /// <summary>Waiting for coin entering the light barrier next to accelerator.</summary>
        WaitCoinAccLbIn = 35,
        /// <summary>Waiting for coin passing the light barrier next to accelerator.</summary>
        WaitCoinAccLbPass = 36,
        /// <summary>Waiting for coin exiting the light barrier next to accelerator.</summary>
        WaitCoinAccLbOut = 37,
        /// <summary>Waiting for coin entering the coin selector.</summary>
        WaitCoinEmp = 38,
        /// <summary>Delay after coin insert.</summary>
        WaitCoinSorterDelay = 39,
        /// <summary>Waiting for sorter ready after coin insert.</summary>
        WaitCoinSorterReady = 40,
        /// <summary>Waiting for possible second coin.</summary>
        WaitDoubleCoin = 42,
        /// <summary>Coin selector is waiting for coin after pause command.</summary>
        WaitStopEmp = 48,
        /// <summary>waiting for motor reject activation.</summary>
        WaitEmr = 49,
        /// <summary>Waiting for coin selector ready after motor reejct activation.</summary>
        WaitEmpEmr = 50,
        /// <summary>Undue coin in accelerator.</summary>
        AccCoinFault = 51,
        /// <summary>Undue coin in accelerator. Waiting for coin entering the light barrier next to accelerator.</summary>
        AccCoinFaultLbIn = 52,
        /// <summary>Undue coin in accelerator. Waiting for coin passing the light barrier next to accelerator.</summary>
        AccCoinFaultLbPass = 53,
        /// <summary>Undue coin in accelerator. Waiting for coin exiting the light barrier next to accelerator.</summary>
        AccCoinFaultLbOut = 54,
        /// <summary>Accelerator waiting for coin settling in the slot.</summary>
        WaitAccCoinFaultSorterDelay = 55,
        /// <summary>Accelerator checking if slot is empty.</summary>
        WaitAccCheckCoinFault = 56,
        /// <summary>Debounce time for WaitAccCheckCoinFault.</summary>
        WaitAccFaultCoin = 57,
        /// <summary>Debounce time for WaitAccCheckCoinFault.</summary>
        WaitAccWaitFaultTwsDelay = 58,
        /// <summary>Waiting until accelerator ha moved coin.</summary>
        WaitAccWaitFaultTwsReady = 59,

        /// <summary>Error handling coin selector.</summary>
        EmpErrorHandler = 60,
        /// <summary>Error handling coin feeder insert.</summary>
        CisErrorInsert = 61,
        /// <summary>Error handling coin feeder waiting for coin selector.</summary>
        CisErrorEmp = 62,
        /// <summary>Error handling coin for cancel.</summary>
        CisErrorAbort = 63,
        /// <summary>Error handling coin feeder for CashIn end.</summary>
        CisErrorEnd = 64,

        /// <summary>Invalid configuration parameter.</summary>
        ErrorParameter = 150,
        /// <summary>Invalid master mode.</summary>
        ErrorMode = 151,
        /// <summary>Device not ready for coins or coins in sorter.</summary>
        ErrorNotReady = 152,
        /// <summary>Slave device missing.</summary>
        ErrorMissingEmp = 153,
        /// <summary>coin feeder missing.</summary>
        ErrorMisisngFeeder = 154,
        /// <summary>Failure initialising coin feeder.</summary>
        ErrorInitEmp = 155,
        /// <summary>Failure initialising coin feeder.</summary>
        ErrorInitFeeder = 156,
        /// <summary>External light barrier blocked.</summary>
        ErrorInitLightBarrier = 157,
        /// <summary>System failure on power on.</summary>
        ErrorSystemPowerOn = 158,
        /// <summary>Coin insert timeout.</summary>
        ErrorTimeoutCoinInsert = 160,
        /// <summary>Timeout during processing coin in selector.</summary>
        ErrorTimeoutCoinEmp = 161,
        /// <summary>Timeout processing coin in accelerator.</summary>
        ErrorTimeoutCoinAcc = 162,
        /// <summary>Timeout for coin entering light barrier.</summary>
        ErrorTimeoutCoinAccLbIn = 163,
        /// <summary>Timeout for coin exiting light barrier.</summary>
        ErrorTimeoutCoinAccLbOut = 164,
        /// <summary>Accelerator couldn't process coin.</summary>
        ErrorTimeoutCoinAccRepeat = 165,
        /// <summary>Timeout for coin entering sorter.</summary>
        ErrorTimeoutCoinSorterInsert = 166,
        /// <summary>Timeout for routing coin to its destination.</summary>
        ErrorTimeoutCoinSorterSorting = 167,
        /// <summary>Timeout for routing a coin to the rekject path.</summary>
        ErrorTimeoutCoinSorterReject = 168,
        /// <summary>Undue coin in accelerator.</summary>
        ErrorAccCoinFault = 169,
        /// <summary>Connection to coin selector is lost.</summary>
        ErrorEmpLost = 170,
        /// <summary>Coin selector reported reset.</summary>
        ErrorEmpReset = 171,
        /// <summary>Irrecoverable coin selector error.</summary>
        ErrorEmpCoinError = 172,
        /// <summary>Connection to coin feeder is lost.</summary>
        ErrorFeederLost = 173,
        /// <summary>Unexpected coin reported by selector.</summary>
        ErrorEmpUnexpected = 174,
        /// <summary>Permanent coin selector fault.</summary>
        ErrorEmpFault = 175,
        /// <summary>Coin jammed inside selector.</summary>
        ErrorCoinJamSelector = 187,
        /// <summary>Coin jammed in selector exit.</summary>
        ErrorCoinJamExitSelector = 188,
        /// <summary>Coin jammed in selector measuring system.</summary>
        ErrorCoinJamMeasureSelector = 189,
        /// <summary>Selector reports reject during idle.</summary>
        ErrorRejectIdleSelector = 190,
        /// <summary>Coin in escrow insert insert position.</summary>
        ErrorCoinInInsertPosition = 211,
        /// <summary>Error during Full Coin Check command.</summary>
        ErrorFullCoinCheck = 212,
        /// <summary>Unexpected coin in accelerator during <see cref="EscrowSorterMasterStatus.WaitDoubleCoin"/>.</summary>
        ErrorUnexpectedCoinInAccelerator = 213,
    }
}