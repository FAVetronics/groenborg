namespace ccTalk.Validation
{
    /// <summary>
    /// Bill selector poll events.
    /// </summary>
    public enum ValPollEvent
    {
        /// <summary>A Bill was accepted.</summary>
        Bill = 512,
        /// <summary>Device was reset.</summary>
        Reset = 513,
        /// <summary>Unknown event.</summary>
        Unknown = 514,
        /// <summary>Nothing to report.</summary>
        Null = 515,
        /// <summary>Master inhibit active.</summary>
        MasterInhibit = 0,
        /// <summary>Bill returned from escrow.</summary>
        Returned = 1,
        /// <summary>Invalid bill (due to validation fail).</summary>
        ValidationFailed = 2,
        /// <summary>Invalid bill (due to transport problem)</summary>
        TransportProblem = 3,
        /// <summary>Inhibited bill (on protocoll).</summary>
        Inhibit = 4,
        /// <summary>Inhibited bill (on DIP switches).</summary>
        Switch = 5,
        /// <summary>Bill jammed in transport (unsafe mode).</summary>
        UnsafeJam = 6,
        /// <summary>Bill jammed in stacker.</summary>
        StackerJam = 7,
        /// <summary>Bill pulled backwards.</summary>
        PulledBackwards = 8,
        /// <summary>Bill tamper.</summary>
        Tamper = 9,
        /// <summary>Stacker OK.</summary>
        StackerOk = 10,
        /// <summary>Stacker removed.</summary>
        StackerRemoved = 11,
        /// <summary>Stacker inserted.</summary>
        StackerInserted = 12,
        /// <summary>Stacker faulty.</summary>
        StackerFaulty = 13,
        /// <summary>Stacker full.</summary>
        StackerFull = 14,
        /// <summary>Stacker jammed.</summary>
        StackerJammed = 15,
        /// <summary>Bill jammed in transport (safe mode).</summary>
        SafeJam = 16,
        /// <summary>Opto fraud detected.</summary>
        OptoFraud = 17,
        /// <summary>String fraud detected.</summary>
        StringFraud = 18,
        /// <summary>Validator is busy.</summary>
        Busy = 640,
        /// <summary>ROM Checksum error</summary>
        RomChecksum = 641,
        /// <summary>One of the motors failed.</summary>
        DefectiveMotor = 642,
        /// <summary>Invalid escrow request.</summary>
        InvalidEscrow = 643,
        /// <summary>Bill validator disabled by host</summary>
        Disabled = 644,
        ///<summary>A bill is being dispensed from the recycle box.</summary>
        Paying = 645,
        ///<summary>A bill is being collected from recycle box to cash box.</summary>
        Collecting = 646,
        ///<summary>A bill has beend collected from recycle box to cash box.</summary>
        Collected = 647,
        ///<summary>Dispense is complete.</summary>
        PayValid = 648,
        ///<summary>A bill stays at the note payout slot.</summary>
        PayStay = 649,
        ///<summary>Dispensing has been cancelled and bill was collected to cash box.</summary>
        ReturnToBox = 650,
        ///<summary>Bill is collected to cash box since an error was detected while dispensing.</summary>
        ReturnPayOutNote = 651,
        ///<summary>Error during the collection of dispensed bills after dispensing process has been cancelled.</summary>
        ReturnError = 652,
        #region adp AFD MD-100 specific
        /// <summary>For adp AFD-MONO: Reset.</summary>
        AFD_Reset = 0x20,
        /// <summary>For adp AFD-MONO: Connection to master.</summary>
        AFD_Connect = 0x21,
        /// <summary>For adp AFD-MONO: Initialisation of MD-100.</summary>
        AFD_Init = 0x22,
        /// <summary>For adp AFD-MONO: Married (encrypted version only).</summary>
        AFD_Married = 0x23,
        /// <summary>For adp AFD-MONO: Update AFD / MD-100 active.</summary>
        AFD_Update = 0x24,
        /// <summary>For adp AFD-MONO: Configuration of dispenser SS1...SS3.</summary>
        AFD_Config = 0x25,
        /// <summary>For adp AFD-MONO: Bill in transport.</summary>
        AFD_Busy = 0x26,
        /// <summary>For adp AFD-MONO: Idle - ready for work.</summary>
        AFD_Idle = 0x27,
        /// <summary>For adp AFD-MONO: Globally locked.</summary>
        AFD_Locked = 0x28,
        /// <summary>For adp AFD-MONO: Globally unlocked.</summary>
        AFD_Unlocked = 0x29,
        /// <summary>For adp AFD-MONO: Bill moves to cash box after an error.</summary>
        AFD_MoveCBox = 0x2a,
        /// <summary>For adp AFD-MONO: Bill paid out and staying in the bezel.</summary>
        AFD_MoveBill = 0x2b,
        /// <summary>For adp AFD-MONO: Operating mode changed to Sys_S_Fill.</summary>
        AFD_ChangeToFill = 0x2c,
        /// <summary>For adp AFD-MONO: Operating mode changed to Sys_S-Unload.</summary>
        AFD_ChangeToUnload = 0x2d,
        /// <summary>For adp AFD-MONO: Operating mode changed to Sys_Game.</summary>
        AFD_ChangeToGame = 0x2e,
        /// <summary>For adp AFD-MONO: Modifying bill types.</summary>
        AFD_ModeType = 0x2f,
        /// <summary>For adp AFD-MONO: AFD tries to start pay out of one bill.</summary>
        AFD_Payout = 0x30,
        /// <summary>For adp AFD-MONO: Specific error.</summary>
        AFD_SpecificError = 0x65,
        #endregion
        #region adp AFD MD-100 extended
        /// <summary>For adp AFD-MONO extended: Lifting failure.</summary>
        AFD_LiftFailure = 0x65d0,
        /// <summary>For adp AFD-MONO extended: Drum rotation failure.</summary>
        AFD_DrumRotateFailure = 0x65d1,
        /// <summary>For adp AFD-MONO extended: Tape moving failure.</summary>
        AFD_TapeMoveFailure = 0x65d2,
        /// <summary>For adp AFD-MONO extended: Common failure dispenser 1.</summary>
        AFD_Dispenser1Failure = 0x65d3,
        /// <summary>For adp AFD-MONO extended: Common failure dispenser 2.</summary>
        AFD_Dispenser2Failure = 0x65d4,
        /// <summary>For adp AFD-MONO extended: Common failure dispenser 3.</summary>
        AFD_Dispenser3Failure = 0x65d5,
        /// <summary>For adp AFD-MONO extended: Drum failure.</summary>
        AFD_DrumFailure = 0x65d6,
        /// <summary>For adp AFD-MONO extended: Payout failure. Bill cannot be moved to acceptor head and was stacked instead.</summary>
        AFD_DispenserBillSatcked = 0x65d7,
        /// <summary>For adp AFD-MONO extended: Dispenser light barrier faulty.</summary>
        AFD_LbNotchPosition = 0x65d8,
        /// <summary>For adp AFD-MONO extended: Dispenser light barrier faulty.</summary>
        AFD_LbTacho = 0x65d9,
        /// <summary>For adp AFD-MONO extended: Dispenser light barrier faulty.</summary>
        AFD_LbBillIn = 0x65da,
        /// <summary>For adp AFD-MONO extended: Dispenser light barrier faulty.</summary>
        AFD_LbBillOut = 0x65db,
        /// <summary>For adp AFD-MONO extended: Dispenser light barrier faulty.</summary>
        AFD_LbMoveForward = 0x65dc,
        /// <summary>For adp AFD-MONO extended: Dispenser light barrier faulty.</summary>
        AFD_LbLift = 0x65dd,
        /// <summary>For adp AFD-MONO extended: Dispenser light barrier faulty.</summary>
        AFD_LbLiftPos = 0x65f3,
        /// <summary>For adp AFD-MONO extended: Dispenser light barrier faulty.</summary>
        AFD_LbPosition = 0x65de,
        /// <summary>For adp AFD-MONO extended: Dispenser light barrier faulty.</summary>
        AFD_LbStacker = 0x65df,
        /// <summary>For adp AFD-MONO extended: Bill count of SS1 differs from count of AFD.</summary>
        AFD_BillCountDisp1 = 0x65e0,
        /// <summary>For adp AFD-MONO extended: Bill count of SS2 differs from count of AFD.</summary>
        AFD_BillCountDisp2 = 0x65e9,
        /// <summary>For adp AFD-MONO extended: Bill count of SS3 differs from count of AFD.</summary>
        AFD_BillCountDisp3 = 0x65ef,
        /// <summary>For adp AFD-MONO extended: Dispenser didn't respond to AFD's command.</summary>
        AFD_ResponseTimeout = 0x65e3,
        /// <summary>For adp AFD-MONO extended: AFD encryption error.</summary>
        AFD_MacError = 0x65e4,
        /// <summary>For adp AFD-MONO extended: Stacker removed.</summary>
        AFD_StackerRemoved = 0x65e6,
        /// <summary>For adp AFD-MONO extended: Stacker full.</summary>
        AFD_StackerFull = 0x65e7,
        /// <summary>For adp AFD-MONO extended: Stacker replaced.</summary>
        AFD_StackerReplaced = 0x65e8,
        /// <summary>For adp AFD-MONO extended: Configuration of dispenser differs from AFD.</summary>
        AFD_ConfigError = 0x65eb,
        /// <summary>For adp AFD-MONO extended: EEPROM databuffer is full.</summary>
        AFD_MemoryFull = 0x65ec,
        /// <summary>For adp AFD-MONO extended: Dispenser motor faulty.</summary>
        AFD_MotorLift = 0x65f0,
        /// <summary>For adp AFD-MONO extended: Dispenser motor faulty.</summary>
        AFD_MotorRotate = 0x65f1,
        /// <summary>For adp AFD-MONO extended: Dispenser motor faulty.</summary>
        AFD_MotorMove = 0x65f2,
        /// <summary>For adp AFD-MONO extended: Persistent error condition. Please contact technical support.</summary>
        AFD_PersistentError = 0x65f6,
        /// <summary>For adp AFD-MONO extended: Bill jammed in dispenser.</summary>
        AFD_DispenserJammed = 0x65f8,
        /// <summary>For adp AFD-MONO extended: Bill jammed during insertion.</summary>
        AFD_BillInJam = 0x65fb,
        /// <summary>For adp AFD-MONO extended: Bill jammed during payout.</summary>
        AFD_BillOutJam = 0x65fc,
        /// <summary>For adp AFD-MONO extended: Bill repeatedly jammed during payout.</summary>
        AFD_RepeatedBillOutJam = 0x65fd,
        /// <summary>For adp AFD-MONO extended: Unknown or incompatible combination.</summary>
        AFD_IndnentifyEroor = 0x65ff,
        #endregion
    }
}