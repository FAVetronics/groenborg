namespace ccTalk
{
    /// <summary>
    /// Possible states of the escrow sorter.
    /// </summary>
    public enum EscrowSorterStatus
    {
        /// <summary>Sorter was just powered on.</summary>
        PowerOn = 0,
        /// <summary>Sorter was restarted.</summary>
        Restart,
        /// <summary>Sorter is ready for action.</summary>
        Ready,
        /// <summary>Sorter is empty.</summary>
        Empty,
        /// <summary>Sorter is full, no more inserts allowed.</summary>
        Full,
        /// <summary>Injecting coin.</summary>
        Inject,
        /// <summary>Sorter is flushing stored coins into payouts.</summary>
        Flushing,
        /// <summary>Sorter is ejecting stored coins into defined slots.</summary>
        Ejecting,
        /// <summary>Sorter was stopped during payout procedure.</summary>
        Stopped,
        /// <summary>Sorter is busy in some way</summary>
        Busy = 17,
        /// <summary>Common error.</summary>
        Error = 100,
        /// <summary>Failure during power on.</summary>
        PowerOnError = 101,
        /// <summary>Failure during coin insert.</summary>
        InsertError = 102,
        /// <summary>Failure during coin eject.</summary>
        EjectError = 103,
        /// <summary>Failure initialising motor reject.</summary>
        RejectInitError = 104,
        /// <summary>Failure calibrating light barriers.</summary>
        LightBarrierInitError = 105,
        /// <summary>Failure initialising motor.</summary>
        MotorInitError = 106,
        /// <summary>Timeout coin exit during eject.</summary>
        EjectExitError = 107,
        /// <summary>Transport failure during eject.</summary>
        EjectMotorError = 108,
        /// <summary>Illegal coin error during eject.</summary>
        CoinFaultError = 109,
        /// <summary>Booster undervoltage failure.</summary>
        BoosterUnderVoltageError = 110,
        /// <summary>Booster overvoltage failure.</summary>
        BoosterOverVoltageError = 111,
        /// <summary>Power supply undervoltage failure.</summary>
        SupplyUnderVoltageError = 112,
        /// <summary>Power supply overvoltage failure.</summary>
        SupplyOverVoltageError = 113,
        /// <summary>Light barrier run-time error.</summary>
        LightBarrierCheckError = 114,
        ///<summary>Staus unknown.</summary>
        Unknown = 255,
    }
}