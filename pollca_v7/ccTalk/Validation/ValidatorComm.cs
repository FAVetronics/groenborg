using System;
using System.Threading;
using ccTalk.Bill;

namespace ccTalk.Validation
{
    /// <summary>
    /// wh Bill Validator Communication class.
    /// A MDB bill validator connected via CCT 900 will appear at special ccTalk address <see cref="CcTalkComm.MdbBillValidatorAddress"/>
    /// and will behave like a normal ccTalk validator. 
    /// </summary>
    [Serializable]
    public class ValidatorComm : CcTalkComm
    {
        #region Constructor/Destructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// Sets some default values.
        /// </remarks>
        public ValidatorComm()
        {
            Address = VALIDATOR_ADR;
            EncryptionMode = CcTalkEncryption.None;
            ChecksumType = CcTalkChecksumTypes.CRC16;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// <param name="basedevice">Instance of the base class<see cref="CcTalkComm"/> were some settings are taken from:</param>
        /// Address, Port and ChecksumType.
        /// </remarks>
        public ValidatorComm(CcTalkComm basedevice)
        {
            Address = basedevice.Address;
            Port = basedevice.Port;
            ChecksumType = basedevice.ChecksumType;
        }
        #endregion

        /// <summary>
        /// Opens the com port and sets some default values.
        /// </summary>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public override CcTalkErrors OpenComm()
        {
            for (int i = 0; i < 16; i++)
            {
                scalfactors[i].ID = "XX";
                scalfactors[i].factor = 1;
                scalfactors[i].decimals = 2;
            }

            rctype = BillRecyclerType.None;
            rcsupported = false;
            rcconnected = false;
            masterinhibit = true;
            evtctr = -1;
            int rcbill;

            ExtendedEBDS = false;

            lasterr = base.OpenComm();
            billinescrow = false;
            if (lasterr == CcTalkErrors.Ok && !IsSupported(Manufacturer))
            {
                CloseComm();
                lasterr = CcTalkErrors.UnSupported;
            }
            if (lasterr == CcTalkErrors.Ok)
            {
                if (!nospecialaddresses)
                {
                    switch (Address)
                    {
#if !WindowsCE
                        case MdbAddresses.CcBillValidator:
                            rcsupported = false;
                            break;
#endif
                        default:
                            string prstr = GetStringResponse(244);
                            if (prstr.ToLower().IndexOf("vega") > -1)
                            {
                                rcsupported = true;
                                rcconnected = true;
                                rctype = BillRecyclerType.JCMVegaCcTalk;
                                rccount = 1;
                                BillRecyclerStatus rcstat = new BillRecyclerStatus();
                                GetRecyclerStatus(ref rcstat);
                                if (lasterr == CcTalkErrors.Ok)
                                {
                                    if (rcconnected = (rcstat.Status & RecyclerFlags.NotConnected) != RecyclerFlags.NotConnected)
                                    {
                                        rctype = BillRecyclerType.JCMVegaCcTalk;
                                        rcbill = RecyclerBill;
                                    }
                                    else
                                    {
                                        rcconnected = false;
                                        rctype = BillRecyclerType.None;
                                        rccount = 0;
                                    }
                                }
                                else
                                {
                                    rcconnected = false;
                                    rctype = BillRecyclerType.None;
                                    rccount = 0;
                                }
                            }
                            break;
                    }
                }
                else
                {
                    string prstr = GetStringResponse(244);
                }
            }
            return lasterr;
        }
        /// <summary>Maximum number of poll events.</summary>
        public const int MaxPollEvents = 16;
        /// <summary>
        /// Master Inhibit status.
        /// </summary>
        /// <remarks>
        /// Setting it to "true" inhibits acceptance of all bills.
        /// Setting it to "false" enables only acceptance of bills enabled.
        /// </remarks>
        public bool MasterInhibit
        {
            get { return GetMasterInhibit(); }
            set { SetMasterInhibit(value); }
        }
        /// <summary>
        /// Escrow enabled status.
        /// </summary>
        /// <remarks>
        /// Setting it to "true" activates the escrow feature.
        /// </remarks>
        public bool EscrowEnabled
        {
            get { return GetEscrowEnabled(); }
            set { SetEscrowEnabled(value); }
        }
        /// <summary>
        /// Is there a bill in the escrow position?
        /// </summary>
        public bool BillInEscrow
        {
            get { return billinescrow; }
        }
        /// <summary>
        /// Automatically reset escrow time-out while bill is in position.
        /// </summary>
        /// <remarks>
        /// This refers only to ccTalk bill validators.
        /// </remarks>
        public bool AutoResetEscrowTimeout
        {
            get { return autoresetescrow; }
            set { autoresetescrow = value; }
        }

        /// <summary>Device supports bill recycler.</summary>
        public bool RecyclerSupported
        {
            get { return rcsupported; }
        }
        /// <summary>The supported recycler is connected to the device.</summary>
        public bool RecyclerConnected
        {
            get
            {
                return rcconnected && rcidx < rccount;
            }
        }
        /// <summary>Type of the bill recycler <see cref="BillRecyclerType"/>. Supported methodes and properties depend on the actual recycler.</summary>
        public BillRecyclerType RecyclerType
        {
            get { return rctype; }
        }

        #region Notenwerte für EBDS-Geräte von MEI verwalten
        // Erweiterter Modus aktiv?
        internal bool ExtendedEBDS = false;

        #region Non-421717 IDs übersetzen
        internal string[,] CurrIDMapping = {
            { "EC", "GB" },
        };
        internal string TranslateCurrID(string OrgID)
        {
            string newid = OrgID;
            for (int i = 0; i < CurrIDMapping.GetUpperBound(0) + 1; i++)
            {
                if (CurrIDMapping[i, 0] == OrgID.ToUpper())
                {
                    newid = CurrIDMapping[i, 1];
                    break;
                }
            }
            return newid;
        }
        #endregion

        #region Tabelle aller originalen Notenwerte
        internal struct whEBDSBillEntry
        {
            public string ValueStr;
            public BillValue BillValue;
            public bool Enabled;

            public int CurrValsIdx;
        }

        internal whEBDSBillEntry[] EBDSBillEntries = new whEBDSBillEntry[0];
        #endregion

        #endregion

        /// <summary>
        /// Retrieves values and currency IDs of the 16 Bills.
        /// </summary>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetBillValues(ref BillValue[] currvals)
        {
            MdbDataBlock smdb = new MdbDataBlock(true);
            MdbDataBlock rmdb = new MdbDataBlock(true);
            string ccid = "XX";

            if (currvals.Length < 16) return CcTalkErrors.WrongParameter;

            switch (Address)
            {
#if !WindowsCE
                case MdbAddresses.CcBillValidator:
                    #region MDB Device
                    BillValidatorSetupInstance.InitStructure();
                    smdb.DataLength = 1;
                    smdb.Data[0] = MdbAddresses.MdbBillValidator | MdbCommands.Setup;
                    lasterr = TalkMdb(smdb, ref rmdb);
                    if (!BillValidatorSetupInstance.GetFromBuffer(rmdb)) lasterr = CcTalkErrors.DataFormat;
                    if (lasterr != CcTalkErrors.Ok) return lasterr;

                    double bvfac = (BillValidatorSetupInstance.ScalingHi * 256 + BillValidatorSetupInstance.ScalingLo) / Math.Pow(10, BillValidatorSetupInstance.Decimals);
                    ccid = GetCcTalkID(BillValidatorSetupInstance.CountryHi * 256 + BillValidatorSetupInstance.CountryLo);
                    for (int i = 0; i < 16; i++)
                    {
                        currvals[i].Value = BillValidatorSetupInstance.BillCredit[i] * bvfac;
                        currvals[i].ID = ccid;
                        currvals[i].Decimals = BillValidatorSetupInstance.Decimals;
                    }
                    #endregion
                    break;
#endif
                default:
                    for (int i = 0; i < currvals.Length; i++)
                    {
                        currvals[i] = GetBillValue(i);
                        if (lasterr != CcTalkErrors.Ok) return lasterr;
                    }
                    break;
            }
            return lasterr;
        }
        /// <summary>
        /// Retrieves the current inhibit status, sorter path and security status of the 16 bills.
        /// </summary>
        /// <param name="currvals">Array of <see cref="ValBillStatus"/> - must have at least 16 elements.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetBillStates(ref ValBillStatus[] currvals)
        {
            ushort inh = 0x0000, esc = 0x0000, msk;

            switch (Address)
            {
#if !WindowsCE
                case MdbAddresses.CcBillValidator:
                    inh = BillInhibit;
                    break;
#endif
                default:
                    if (currvals.Length < 16) return CcTalkErrors.WrongParameter;

                    inh = (ushort)GetLongResponse(230);
                    if (lasterr != CcTalkErrors.Ok) return lasterr;
                    esc = (ushort)GetLongResponse(152);
                    if (lasterr != CcTalkErrors.Ok) return lasterr;

                    break;
            }
            msk = 0x0001;
            for (int i = 0; i < 16; i++)
            {
                currvals[i].Inhibit = (inh & msk) != 0;
                msk <<= 1;
                if (lasterr != CcTalkErrors.Ok) return lasterr;
            }
            return lasterr;
        }

        /// <summary>
        /// Sets the inhibit status of the 16 Bills.
        /// </summary>
        /// <remarks>
        /// On power on all Bills are inhibited. 
        /// Use <see cref="GetBillValues"/> to identify available Bill values and currencies
        /// and <see cref="GetBillStates"/> to determine current inhibit status.
        /// </remarks>
        /// <param name="currvals">
        /// Array of <see cref="ValBillStatus"/> - must have at least 16 elements.
        /// Only then <see cref="ValBillStatus.Inhibit"/> field will be used.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetBillInhibit(ValBillStatus[] currvals)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            if (currvals.Length < 16) return CcTalkErrors.WrongParameter;

            ushort msk = 0x0001;
            ushort inh = 0x0000;
            for (int i = 0; i < 16; i++)
            {
                if (currvals[i].Inhibit) inh |= msk;
                msk <<= 1;
            }

            inh = 0x0000;
            for (int i = 0; i < 16; i++)
                if (currvals[i].Inhibit) inh |= (ushort)(0x0001 << i);

            switch (Address)
            {
#if !WindowsCE
                case MdbAddresses.CcBillValidator:
                    BillInhibit = inh;
                    SetupMdbBillValidator();
                    break;
#endif
                default:
                    sdta.DataLength = 2;
                    sdta.Header = 231;
                    sdta.Data[0] = (byte)(inh & 0x00ff);
                    sdta.Data[1] = (byte)(inh >> 8 & 0x00ff);
                    lasterr = TalkCc(sdta, ref rdta);
                    break;
            }
            return lasterr;
        }
        /// <summary>
        /// Sets the same inhibit status for all 16 bills.
        /// </summary>
        /// <remarks>
        /// On power on all bills are inhibited. 
        /// </remarks>
        /// <param name="billenable">
        /// The status that will be set for all coins. If true all bills will be enabled.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetBillInhibit(bool billenable)
        {
            ValBillStatus[] currvals = new ValBillStatus[16];

            for (int i = 0; i < 16; i++) currvals[i].Inhibit = billenable;
            return SetBillInhibit(currvals);
        }

        /// <summary>
        /// Routes a bill held in escrow.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="route">
        /// The destination <see cref="ValBillRoute"/> of the bill.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors RouteBill(ValBillRoute route)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);
            MdbDataBlock smdb = new MdbDataBlock(true);
            MdbDataBlock rmdb = new MdbDataBlock(true);

            switch (Address)
            {
#if !WindowsCE
                case MdbAddresses.CcBillValidator:
                    smdb.DataLength = 2;
                    smdb.Data[0] = MdbAddresses.MdbBillValidator | MdbCommands.Escrow;
                    switch (route)
                    {
                        case ValBillRoute.Return:
                            smdb.Data[1] = 0x00;
                            lasterr = TalkMdb(smdb, ref rmdb);
                            billinescrow = false;
                            break;
                        case ValBillRoute.Stack:
                            smdb.Data[1] = 0x01;
                            lasterr = TalkMdb(smdb, ref rmdb);
                            billinescrow = false;
                            break;
                        case ValBillRoute.Hold:
                            lasterr = CcTalkErrors.Ok;
                            break;
                    }

                    break;
#endif
                default:
                    sdta.DataLength = 1;
                    sdta.Header = 154;
                    sdta.Data[0] = (byte)route;
                    switch (route)
                    {
                        case ValBillRoute.Return:
                        case ValBillRoute.Stack:
                            billinescrow = false;
                            break;
                    }
                    lasterr = TalkCc(sdta, ref rdta);
                    if (lasterr != CcTalkErrors.Ok || rdta.DataLength == 0)
                    {
                        return lasterr;
                    }
                    else
                    {
                        switch (rdta.Data[0])
                        {
                            case 254: return CcTalkErrors.BillEscrowEmpty;
                            case 255: return CcTalkErrors.BillRouteFailed;
                            default: return CcTalkErrors.Unknown;
                        }
                    }
            }
            return lasterr;
        }

        /// <summary>
        /// Polls the device to retrieve current events.
        /// </summary>
        /// <remarks>
        /// Up to <see cref="MaxPollEvents"/> events can be retrieved. Poll should be performed app. every
        /// 200msecs otherwise events especially credit may be lost.
        /// If <see cref="ValPollResponse.Status"/> == <see cref="ValPollEvent.Bill"/> 
        /// use <see cref="ValPollResponse.BillIndex"/>
        /// to retrieve further information from the arrays returned by <see cref="GetBillValues"/> and
        /// <see cref="GetBillStates"/>.
        /// </remarks>
        /// <param name="pollresps">
        /// Array of <see cref="ValPollResponse"/>, must have at least <see cref="MaxPollEvents"/> elements.
        /// </param>
        /// <param name="evts">
        /// Number of events since last poll, the first "evts" elements of pollresps are valid.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors PollValidator(ref ValPollResponse[] pollresps, out int evts)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);
            MdbDataBlock smdb = new MdbDataBlock(true);
            MdbDataBlock rmdb = new MdbDataBlock(true);

            evts = 0;
            if (pollresps.Length < MaxPollEvents) return CcTalkErrors.InvalidParameter;

            switch (Address)
            {
#if !WindowsCE
                case MdbAddresses.CcBillValidator:
                    #region MDB Bill Validator
                    smdb.DataLength = 1;
                    smdb.Data[0] = MdbAddresses.MdbBillValidator | MdbCommands.Poll;
                    lasterr = TalkMdb(smdb, ref rmdb);
                    if (lasterr != CcTalkErrors.Ok) return lasterr;

                    int respidx = 0;
                    while (respidx < rmdb.DataLength)
                    {
                        pollresps[evts].Status = ValPollEvent.Unknown;
                        pollresps[evts].BillPosition = ValBillPosition.Unknown;

                        if ((rmdb.Data[respidx] & 0x80) == 0x80)  // Bill
                        {
                            pollresps[evts].Status = ValPollEvent.Bill;
                            pollresps[evts].BillIndex = rmdb.Data[respidx] & 0x0f;
                            switch (rmdb.Data[respidx] & 0x70)
                            {
                                case 0x00:
                                    pollresps[evts].BillPosition = ValBillPosition.Stacked;
                                    billinescrow = false;
                                    break;
                                case 0x10:
                                    pollresps[evts].BillPosition = ValBillPosition.Escrow;
                                    billinescrow = true;
                                    break;
                                case 0x20:
                                    pollresps[evts].Status = ValPollEvent.Returned;
                                    billinescrow = false;
                                    break;
                                case 0x40:
                                    pollresps[evts].Status = ValPollEvent.ValidationFailed;
                                    billinescrow = false;
                                    break;
                            }
                            respidx++;
                        }
                        else
                        {
                            if ((rmdb.Data[respidx] & 0xe0) == 0x40)    // Slug
                            {
                                pollresps[evts].Status = ValPollEvent.ValidationFailed;
                                billinescrow = false;
                            }
                            else
                            {
                                pollresps[evts].Status = TranslateBillValidatorPoll(rmdb.Data[respidx]);
                            }
                        }
                        evts++;
                        respidx++;
                    }
                    break;
                #endregion
#endif
                default:
                    #region Normal ccTalk Device
                    if (pollresps.Length < MaxPollEvents) return CcTalkErrors.WrongParameter;
                    sdta.DataLength = 0;
                    sdta.Header = 159;
                    lasterr = TalkCc(sdta, ref rdta);
                    if (lasterr != CcTalkErrors.Ok) return lasterr;
                    if (rdta.DataLength == 0) return lasterr;
                    if (rdta.DataLength != 11)  // Wrong data format
                    {
                        lasterr = CcTalkErrors.DataFormat;
                        return lasterr;
                    }
                    if (rdta.Data[0] == 0 && evtctr < 0)      // Just reset
                    {
                        evtctr = rdta.Data[0];
                        pollresps[0].Status = ValPollEvent.Reset;
                        pollresps[0].BillIndex = -1;
                        pollresps[0].BillPosition = ValBillPosition.Unknown;
                        evtctr = 1;
                        return lasterr;
                    }
                    if (rdta.Data[0] >= evtctr) // Get number of events since last poll
                        evts = rdta.Data[0] - evtctr;
                    else evts = 255 - evtctr + rdta.Data[0];
                    evtctr = rdta.Data[0];
                    if (evts > 5)               // More than 5 events since last poll
                    {
                        lasterr = CcTalkErrors.EventsLost;
                        evts = 5;
                    }
                    for (int i = 0; i < evts && i < pollresps.GetUpperBound(0); i++)
                    {
                        if (rdta.Data[i * 2 + 1] == 0)
                        {
                            try
                            {
                                pollresps[i].Status = (ValPollEvent)rdta.Data[i * 2 + 2];
                            }
                            catch (Exception) { pollresps[i].Status = ValPollEvent.Unknown; }
                            pollresps[i].BillIndex = -1;
                            pollresps[i].BillPosition = ValBillPosition.Unknown;
                            if (pollresps[i].Status == ValPollEvent.Returned) billinescrow = false;
                            if (pollresps[i].Status == ValPollEvent.AFD_Locked) masterinhibit = true;
                            if (pollresps[i].Status == ValPollEvent.AFD_Unlocked) masterinhibit = false;
                        }
                        else
                        {
                            if (rdta.Data[i * 2 + 1] == (byte)ValPollEvent.AFD_SpecificError)
                            {
                                pollresps[i].Status = (ValPollEvent)((byte)ValPollEvent.AFD_SpecificError << 8 | rdta.Data[i * 2 + 2]);
                                pollresps[i].BillIndex = -1;
                                pollresps[i].BillPosition = ValBillPosition.Unknown;
                            }
                            else
                            {
                                pollresps[i].Status = ValPollEvent.Bill;
                                pollresps[i].BillIndex = rdta.Data[i * 2 + 1] - 1 & 0x001f;
                                try
                                {
                                    pollresps[i].BillPosition = (ValBillPosition)rdta.Data[i * 2 + 2];
                                    switch (pollresps[i].BillPosition)
                                    {
                                        case ValBillPosition.Escrow:
                                            billinescrow = true;
                                            break;
                                        case ValBillPosition.Stacked:
                                        case ValBillPosition.AFD_DispenserSS1:
                                        case ValBillPosition.AFD_DispenserSS2:
                                        case ValBillPosition.AFD_DispenserSS3:
                                            billinescrow = false;
                                            break;
                                    }
                                }
                                catch (Exception)
                                {
                                    pollresps[i].BillPosition = ValBillPosition.Unknown;
                                    billinescrow = false;
                                };
                            }
                        }
                    }
                    if (billinescrow && escrowenabled && autoresetescrow)
                    {
                        RouteBill(ValBillRoute.Hold);
                    }
                    break;
                    #endregion
            }
            return lasterr;
        }

        #region Support for various bill recyclers
        /// <summary>Number of available recycler boxes.</summary>
        public int RecyclerBoxCount
        {
            get { return rccount; }
        }
        /// <summary>Number of the active recycler box.</summary>
        public int RecyclerBox
        {
            get { return rcbox; }
            set
            {
                rcbox = value;
                rcidx = rcbox - 1;
            }
        }
        /// <summary>The bill index of the selcted recycler. Will be -1 if recycler is not supported or selection is not valid. Only with the JCM iPRO recycler the value can be changed. To set the value validator must be in "Master Inhibit" state.</summary>
        public int RecyclerBill
        {
            get
            {
                CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

                int billidx = -1;
                switch (rctype)
                {
                    case BillRecyclerType.JCMVegaCcTalk:
                        #region JCM VEGA
                        sdta.DataLength = 1;
                        sdta.Header = 24;
                        sdta.Data[0] = 0;
                        lasterr = TalkCc(sdta, ref rdta);
                        if (lasterr == CcTalkErrors.Ok)
                        {
                            if (rdta.DataLength >= 5)
                            {
                                billidx = rdta.Data[rcidx * 2 + 1] + 256 * rdta.Data[rcidx * 2 + 2] - 1;
                                rccount = (rdta.DataLength - 1) / 2;
                            }
                            else
                            {
                                billidx = rdta.Data[1] - 1;
                                rccount = 1;
                            }
                        }
                        #endregion
                        break;
                }
                return billidx;
            }
            set { }
        }
        /// <summary>The bill count of the selected recycler. Will be -1 if recycler is not supported or selection is not valid. Only with the JCM iPRO recycler the value can be changed. To set the value validator must be in "Master Inhibit" state.</summary>
        public int RecyclerBillCount
        {
            get
            {
                if (!rcconnected)
                {
                    lasterr = CcTalkErrors.InvalidCommand;
                    return -1;
                }
                CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

                int recyclercount = -1;
                switch (rctype)
                {
                    case BillRecyclerType.JCMVegaCcTalk:
                        if (rccount < 2)
                        {
                            lasterr = CcTalkErrors.UnSupported;
                        }
                        else
                        {
                            sdta.DataLength = 1;
                            sdta.Header = 0x24;
                            sdta.Data[0] = 0x00;
                            lasterr = TalkCc(sdta, ref rdta);
                            if (lasterr == CcTalkErrors.Ok)
                            {
                                recyclercount = rdta.Data[2 * rcidx] + 256 * rdta.Data[2 * rcidx + 1];
                            }
                        }
                        break;
                    default:
                        lasterr = CcTalkErrors.UnSupported;
                        break;
                }
                return recyclercount;
            }
            set
            {
                CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

                switch (rctype)
                {
                    case BillRecyclerType.JCMVegaCcTalk:
                        if (rccount < 2)
                        {
                            lasterr = CcTalkErrors.UnSupported;
                        }
                        else
                        {
                            sdta.DataLength = 4;
                            sdta.Header = 0x14;
                            sdta.Data[0] = 0x00;
                            sdta.Data[1] = (byte)value;
                            sdta.Data[2] = (byte)(value >> 8);
                            sdta.Data[3] = (byte)(rcidx + 1);
                            lasterr = TalkCc(sdta, ref rdta);
                        }
                        break;
                    default:
                        lasterr = CcTalkErrors.UnSupported;
                        break;
                }
            }
        }

        /// <summary>Enable operation button. Will always be false if recycler is not supported. Applies to JCM VEGA with recycler only.</summary>
        public bool RecyclerEnabled
        {
            get
            {
                bool rcenab = false;
                switch (rctype)
                {
                    case BillRecyclerType.JCMVegaCcTalk:
                        BillRecyclerStatus rcstat = new BillRecyclerStatus();
                        GetRecyclerStatus(ref rcstat);
                        if (lasterr == CcTalkErrors.Ok)
                            rcenab = (rcstat.Status & RecyclerFlags.Disabled) != RecyclerFlags.Disabled;
                        break;
                }
                return rcenab;
            }
            set
            {
                CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

                switch (rctype)
                {
                    case BillRecyclerType.JCMVegaCcTalk:
                        sdta.DataLength = 1;
                        sdta.Header = 27;
                        sdta.Data[0] = value ? (byte)0xa5 : (byte)0x00;
                        lasterr = TalkCc(sdta, ref rdta);
                        break;

                }
            }
        }
        /// <summary>Enable operation button. Will always be false if recycler is not supported. Applies to JCM VEGA with recycler only.</summary>
        public bool ButtonsEnabled
        {
            get
            {
                CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

                btnenab = false;
                switch (rctype)
                {
                    case BillRecyclerType.JCMVegaCcTalk:
                        sdta.DataLength = 1;
                        sdta.Header = 24;
                        sdta.Data[0] = 0;
                        lasterr = TalkCc(sdta, ref rdta);
                        if (lasterr == CcTalkErrors.Ok) btnenab = rdta.Data[0] != 0;
                        break;
                }
                return btnenab;
            }
            set
            {
                CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

                switch (rctype)
                {
                    case BillRecyclerType.JCMVegaCcTalk:
                        sdta.DataLength = 2;
                        sdta.Header = 25;
                        sdta.Data[0] = 0;
                        btnenab = value;
                        sdta.Data[1] = value ? (byte)0x01 : (byte)0x00;
                        lasterr = TalkCc(sdta, ref rdta);
                        break;
                }
            }
        }

        ///<summary>Maximum number of bills in the selected recycler. Will be -1 if recycler is not supported or selection is not valid. Only with the JCM iPRO recycler the value can be changed. To set the value validator must be in "Master Inhibit" state.</summary>
        public int MaximumRecyclerBills
        {
            get
            {
                CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

                int maxcount = -1;
                switch (rctype)
                {
                    case BillRecyclerType.JCMVegaCcTalk:
                        lasterr = CcTalkErrors.UnSupported;
                        break;
                }
                return maxcount;
            }
            set
            {
                CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

                switch (rctype)
                {
                    case BillRecyclerType.JCMVegaCcTalk:
                        sdta.DataLength = 4;
                        sdta.Header = 0x23;
                        sdta.Data[0] = 0;
                        sdta.Data[1] = (byte)value;
                        sdta.Data[2] = (byte)(value >> 8);
                        sdta.Data[3] = (byte)(rcidx + 1);
                        lasterr = TalkCc(sdta, ref rdta);
                        break;
                }
            }
        }

        /// <summary>
        /// Retrieves the current status of a JCM recycler. Other recycler types will return a <see cref="CcTalkErrors.UnSupported"/> error.
        /// </summary>
        /// <param name="currstat">Struct <see cref="BillRecyclerStatus"/> for recycler status information.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetRecyclerStatus(ref BillRecyclerStatus currstat)
        {
            if (!rcconnected)
            {
                lasterr = CcTalkErrors.InvalidCommand;
                return lasterr;
            }
            int[] stidx = new int[rccount];
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            switch (rctype)
            {
                case BillRecyclerType.JCMVegaCcTalk:
                    #region JCM VEGA RC
                    sdta.DataLength = 1;
                    sdta.Header = 29;
                    sdta.Data[0] = 0;
                    lasterr = TalkCc(sdta, ref rdta);
                    if (lasterr == CcTalkErrors.Ok)
                    {
                        if (rdta.DataLength > 0)
                        {
                            if (rccount < 2)
                            {
                                currstat.Status = (RecyclerFlags)rdta.Data[0];
                                currstat.PayOutReject = BillPayoutRejectCode.Normal;
                                currstat.Remaining = rdta.Data[2];
                                currstat.LastDispensed = rdta.Data[3];
                                currstat.LastUndispensed = rdta.Data[4];
                                currstat.Stored = rdta.Data[5];
                                currstat.Storing = rdta.Data[6];
                                currstat.PayRejectCount = 0;
                                currstat.PayRejectedCount = 0;
                            }
                            else
                            {
                                currstat.Status = (RecyclerFlags)(rdta.Data[0] & 0xf9 | rdta.Data[rcidx + 2] & 0x06);
                                currstat.PayOutReject = (BillPayoutRejectCode)(rdta.Data[1] & 0x03);
                                currstat.Remaining = rdta.Data[rccount + 3];
                                currstat.LastDispensed = rdta.Data[rccount + 4];
                                currstat.LastUndispensed = rdta.Data[rccount + 5];
                                currstat.Stored = rdta.Data[rccount + 6];
                                currstat.Storing = rdta.Data[rccount + 7];
                                currstat.PayRejectCount = rdta.Data[rccount + 8];
                                currstat.PayRejectedCount = rdta.Data[rccount + 9];
                            }
                        }
                        else
                        {
                            lasterr = CcTalkErrors.DataFormat;
                        }
                        Thread.Sleep(10);
                    }
                    #endregion
                    break;
                default:
                    lasterr = CcTalkErrors.UnSupported;
                    break;
            }
            return lasterr;
        }

        /// <summary>
        /// Retrieves the total counts of a JCM recycler. Other recycler types will return a <see cref="CcTalkErrors.UnSupported"/> error.
        /// </summary>
        /// <param name="counts">Struct <see cref="BillRecyclerTotalCounts"/> for recycler total counts.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetRecyclerTotalCounts(ref BillRecyclerTotalCounts counts)
        {
            if (!rcconnected)
            {
                lasterr = CcTalkErrors.InvalidCommand;
                return lasterr;
            }
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            int[] stackercounts = new int[16];
            int[] recyclercounts = new int[3];
            counts = new BillRecyclerTotalCounts();
            switch (rctype)
            {
                case BillRecyclerType.JCMVegaCcTalk:
                    #region JCM VEGA RC and iPRO ccTalk
                    sdta.DataLength = 1;
                    sdta.Header = 26;
                    sdta.Data[0] = 0;
                    lasterr = TalkCc(sdta, ref rdta);
                    if (lasterr == CcTalkErrors.Ok)
                    {
                        if (rdta.DataLength > 0)
                        {
                            counts.Filled = rdta.Data[0] + 256 * rdta.Data[1] + 65536 * rdta.Data[2];
                            counts.Dispensed = rdta.Data[3] + 256 * rdta.Data[4] + 65536 * rdta.Data[5];
                            counts.Collected = rdta.Data[6] + 256 * rdta.Data[7] + 65536 * rdta.Data[8];
                        }
                        else
                        {
                            lasterr = CcTalkErrors.DataFormat;
                        }
                        Thread.Sleep(10);
                    }
                    #endregion
                    break;
                default:
                    lasterr = CcTalkErrors.UnSupported;
                    break;
            }
            return lasterr;
        }

        private byte bitscramble(byte value)
        {
            byte tempbt = value;
            value = (byte)(value << 2);
            value |= (byte)(tempbt >> 6 & 0x03);
            return value;
        }

        /// <summary>
        /// Dispenses up to 255 bills from the selected recycler. For the adp AFD MD-100 this value must always be 1! JCM bill validators must be "Master Inhibit" state to operate the recycler.
        /// </summary>
        /// <param name="count">Number of bills which will be to be dispensed.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors DispenseBills(int count)
        {
            if (!rcconnected)
            {
                lasterr = CcTalkErrors.InvalidCommand;
                return lasterr;
            }
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            switch (rctype)
            {
                case BillRecyclerType.JCMVegaCcTalk:
                    #region JCM VEGA
                    if (count > -1 && count < 256)
                    {
                        lasterr = CcTalkErrors.UnSupported;
                        // Dispense
                        if (lasterr == CcTalkErrors.Ok)
                        {
                            if (rccount > 1)
                            {
                                sdta.DataLength = 11;
                                sdta.Header = 28;
                                sdta.Data[0] = 0;
                                sdta.Data[9] = (byte)count;
                                sdta.Data[10] = (byte)(rcidx + 1);
                                lasterr = TalkCc(sdta, ref rdta);
                            }
                            else
                            {
                                sdta.DataLength = 10;
                                sdta.Header = 0x1c;
                                sdta.Data[0] = 0;
                                sdta.Data[9] = (byte)count;
                                lasterr = TalkCc(sdta, ref rdta);
                            }
                        }
                    }
                    else
                    {
                        lasterr = CcTalkErrors.WrongParameter;
                    }
                    #endregion
                    break;
                default:
                    lasterr = CcTalkErrors.UnSupported;
                    break;
            }
            return lasterr;
        }

        /// <summary>
        /// Stops the dispense procedure of a JCM recycler immediately. Stacks the current bill back to the box.
        /// Works for JCM recyclers only. Other types will return a  <see cref="CcTalkErrors.UnSupported"/> error.
        /// </summary>
        /// <param name="count">Number of bills which failed to be dispensed.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors EmergencyStop(ref int count)
        {
            if (!rcconnected)
            {
                lasterr = CcTalkErrors.InvalidCommand;
                return lasterr;
            }
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);
            switch (rctype)
            {
                case BillRecyclerType.JCMVegaCcTalk:
                    count = 0;
                    sdta.DataLength = 2;
                    sdta.Header = 30;
                    sdta.Data[0] = 0;
                    sdta.Data[1] = 1;
                    lasterr = TalkCc(sdta, ref rdta);
                    if (lasterr == CcTalkErrors.Ok && rdta.DataLength > 0) count = rdta.Data[0];
                    break;
                default:
                    lasterr = CcTalkErrors.UnSupported;
                    break;
            }
            return lasterr;
        }

        /// <summary>
        /// Clears all total counts of a JCM recycler. Other recycler types will return a <see cref="CcTalkErrors.UnSupported"/> error.
        /// </summary>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors ClearCounts()
        {
            if (!rcconnected)
            {
                lasterr = CcTalkErrors.InvalidCommand;
                return lasterr;
            }
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            switch (rctype)
            {
                case BillRecyclerType.JCMVegaCcTalk:
                    sdta.DataLength = 1;
                    sdta.Header = 21;
                    sdta.Data[0] = 0;
                    lasterr = TalkCc(sdta, ref rdta);
                    break;
                default:
                    lasterr = CcTalkErrors.UnSupported;
                    break;
            }
            return lasterr;
        }

        /// <summary>
        /// Transfers on bill from recycler to stacker.
        /// Works for JCM recyclers only. Other types will return a  <see cref="CcTalkErrors.UnSupported"/> error. The validator must be "Master Inhibit" state to operate the recycler.
        /// </summary>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors RequestStore()
        {
            if (!rcconnected)
            {
                lasterr = CcTalkErrors.InvalidCommand;
                return lasterr;
            }
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            switch (rctype)
            {
                case BillRecyclerType.JCMVegaCcTalk:
                    if (rccount < 2)
                    {
                        sdta.DataLength = 1;
                        sdta.Header = 31;
                        sdta.Data[0] = 0;
                        lasterr = TalkCc(sdta, ref rdta);
                    }
                    else
                    {
                        sdta.DataLength = 2;
                        sdta.Header = 31;
                        sdta.Data[0] = 0;
                        sdta.Data[1] = (byte)(rcidx + 1);
                        lasterr = TalkCc(sdta, ref rdta);
                    }
                    break;
                default:
                    lasterr = CcTalkErrors.UnSupported;
                    break;
            }
            return lasterr;
        }
        #endregion

        #region Private methodes and variables
        // Constants
        private const byte VALIDATOR_ADR = 40;
        private bool rcsupported = false;
        private bool rcconnected = false;
        private int rcbox = 1;
        private int rcidx = 0, rccount = -1;
        private bool billinescrow = false;
        private bool btnenab = false;
        private bool escrowenabled = true;
        private bool autoresetescrow = true;
        private bool masterinhibit = false;
        private BillRecyclerType rctype = BillRecyclerType.None;
        // Others
#if !WindowsCE
        private CcTalkErrors SetupMdbBillValidator()
        {
            MdbDataBlock smdb = new MdbDataBlock(true);
            MdbDataBlock rmdb = new MdbDataBlock(true);

            smdb.DataLength = 5;
            smdb.Data[0] = MdbAddresses.MdbBillValidator | MdbCommands.BillType;
            if (BillMasterInhibit)
            {
                smdb.Data[1] = 0x00;
                smdb.Data[2] = 0x00;
            }
            else
            {
                smdb.Data[1] = (byte)(BillInhibit >> 8 & 0x00ff);
                smdb.Data[2] = (byte)(BillInhibit & 0x00ff);
            }
            if (BillEscrowEnable)
            {
                smdb.Data[3] = 0xff;
                smdb.Data[4] = 0xff;
            }
            else
            {
                smdb.Data[3] = 0x00;
                smdb.Data[4] = 0x00;
            }
            lasterr = TalkMdb(smdb, ref rmdb);
            return lasterr;
        }
#endif
        private void SetMasterInhibit(bool mival)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            switch (Address)
            {
#if !WindowsCE
                case MdbAddresses.CcBillValidator:
                    BillMasterInhibit = mival;
                    SetupMdbBillValidator();
                    break;
#endif
                default:
                    sdta.DataLength = 1;
                    sdta.Header = 228;
                    // sdta.Data[0] = (byte)(mival ? 0x00 : 0x01);
                    if (mival) sdta.Data[0] = 0; else sdta.Data[0] = 1;
                    masterinhibit = mival;
                    lasterr = TalkCc(sdta, ref rdta);
                    break;
            }
        }
        private bool GetMasterInhibit()
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            switch (Address)
            {
#if !WindowsCE
                case MdbAddresses.CcBillValidator:
                    return BillMasterInhibit;
#endif
                default:
                    switch (rctype)
                    {
                        default:
                            sdta.DataLength = 0;
                            sdta.Header = 227;
                            lasterr = TalkCc(sdta, ref rdta);
                            if (lasterr != CcTalkErrors.Ok || rdta.DataLength < 1)
                            {
                                return masterinhibit;
                            }
                            return (rdta.Data[0] & 0x01) == 0;
                    }
            }
        }
        private void SetEscrowEnabled(bool mival)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            switch (Address)
            {
#if !WindowsCE
                case MdbAddresses.CcBillValidator:
                    BillEscrowEnable = mival;
                    lasterr = SetupMdbBillValidator();
                    break;
#endif
                default:
                    sdta.DataLength = 1;
                    sdta.Header = 153;
                    if (mival)
                        sdta.Data[0] = 2;
                    else sdta.Data[0] = 0;
                    lasterr = TalkCc(sdta, ref rdta);
                    break;
            }
            escrowenabled = mival && lasterr == CcTalkErrors.Ok;
        }
        private bool GetEscrowEnabled()
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            switch (Address)
            {
#if !WindowsCE
                case MdbAddresses.CcBillValidator:
                    escrowenabled = BillEscrowEnable;
                    break;
#endif
                default:
                    sdta.DataLength = 0;
                    sdta.Header = 152;
                    lasterr = TalkCc(sdta, ref rdta);
                    if (lasterr != CcTalkErrors.Ok) return false;
                    if (rdta.DataLength < 1) return false;
                    escrowenabled = (rdta.Data[0] & 0x02) != 0;
                    break;
            }
            return escrowenabled;
        }
        #endregion
    }
}