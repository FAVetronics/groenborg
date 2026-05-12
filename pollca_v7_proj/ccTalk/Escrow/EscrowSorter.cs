using System;

namespace ccTalk
{
    /// <summary>
    /// wh Escrow Sorter TWS 100 Communication class.
    /// </summary>
    [Serializable]
    public class EscrowSorter : CcTalkComm
    {
        #region Constructor/Destructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// Sets some default values.
        /// </remarks>
        public EscrowSorter()
        {
            Address = SORTER_ADR;
            AntiPinFeatures = ccTalk.AntiPinFeatures.Standard;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// <param name="basedevice">Instance of the base class<see cref="CcTalkComm"/> were some settings are taken from:</param>
        /// Address, Port and ChecksumType.
        /// </remarks>
        public EscrowSorter(CcTalkComm basedevice)
        {
            Address = basedevice.Address;
            Port = basedevice.Port;
            ChecksumType = basedevice.ChecksumType;
        }
        #endregion

        /// <summary>The currently active master mode.</summary>
        public EscrowSorterMasterMode MasterMode
        {
            get { return mastermode; }
        }
        /// <summary>The currently active sorting mode.</summary>
        public EscrowSortingMode SortingMode
        {
            get { return sortingmode; }
        }

        /// <summary>
        /// Opens the com port and sets some default values.
        /// </summary>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public override CcTalkErrors OpenComm()
        {
            lasterr = base.OpenComm();

            if ((lasterr == CcTalkErrors.Ok) && !this.IsSupported(this.Manufacturer))
            {
                CloseComm();
                lasterr = CcTalkErrors.UnSupported;
            }
            return lasterr;

        }

        /// <summary>
        /// Sends the reset command to reset the device the soft way. If successful the master mode will be to <see cref="EscrowSorterMasterMode.Slave"/>.
        /// </summary>
        /// <param name="wt">If wt != 0 the function will wait this number of milliseconds before returning.</param>        
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public override CcTalkErrors ResetDevice(int wt)
        {
            CcTalkErrors res = base.ResetDevice(wt);
            if (res == CcTalkErrors.Ok)
            {
                mastermode = EscrowSorterMasterMode.Slave;
                sortingmode = EscrowSortingMode.Escrow;
            }
            else
            {
                mastermode = EscrowSorterMasterMode.Unknown;
                sortingmode = EscrowSortingMode.Escrow;
            }
            cashreference = 0;
            return res;
        }

        /// <summary>The actual features of the connected ant-pin-system. Will be set to just standard on creation.
        /// </summary>
        public AntiPinFeatures AntiPinFeatures;

        #region Master Mode Commands
        /// <summary>
        /// Sets the master or slave mode for the escrow sorter. The timing parameters for this mode need to be set by the application.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="mode">
        /// The desired mode. Returns the active mode or an error code.
        /// </param>
        /// <param name="errorflags">
        /// Selects which errors will cancel the transaction.
        /// </param>
        /// <param name="insertmode">
        /// Determines the handling of the peripheral devices.
        /// </param>
        /// <param name="timing">
        /// Timing parameters.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetMasterMode(EscrowSorterMasterMode mode, EscrowSorterMasterErrorFlags errorflags, EscrowSorterMasterInsertMode insertmode, EscrowSorterMasterModeTiming timing)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            bool fnd = false;
            for (int i = 0; i < EscrowMasterSettings.Length; i++)
            {
                if (EscrowMasterSettings[i].MasterMode == mode)
                {
                    sdta.Data[0] = (byte)EscrowMasterSettings[i].MasterMode;
                    sdta.Data[1] = (byte)EscrowMasterSettings[i].SorterMode;
                    sdta.Data[2] = timing.InsertTimeout;
                    sdta.Data[3] = timing.EmpTimeout;
                    sdta.Data[4] = timing.AccTimeout;
                    sdta.Data[5] = timing.AccRepeat;
                    sdta.Data[6] = timing.EscrowTimeout;
                    sdta.Data[7] = timing.EscrowDelay;
                    sdta.Data[8] = timing.ShutterDelay;
                    sdta.Data[9] = (byte)((byte)errorflags | (byte)insertmode);
                    fnd = true;
                    break;
                }
            }
            this.mastermode = EscrowSorterMasterMode.Unknown;
            this.sortingmode = EscrowSortingMode.Escrow;
            if (fnd)
            {
                sdta.DataLength = 10;
                sdta.Header = 54;
                lasterr = TalkCc(sdta, ref rdta);
                if (lasterr == CcTalkErrors.Ok)
                {
                    if (rdta.DataLength > 0)
                    {
                        switch (mode)
                        {
                            case EscrowSorterMasterMode.MasterErrorActive:
                            case EscrowSorterMasterMode.MasterErrorInvalid:
                            case EscrowSorterMasterMode.MasterErrorMissing:
                                break;
                            default:
                                this.mastermode = mode;
                                this.sortingmode = sortingmode;
                                break;
                        }
                    }
                    else
                    {
                        lasterr = CcTalkErrors.DataFormat;
                    }
                }
            }
            else
            {
                lasterr = CcTalkErrors.InvalidParameter;
            }

            return lasterr;
        }
        /// <summary>
        /// Sets the master or slave mode for the escrow sorter. The recommended parameters for this mode will be set automatically. The sorting mode will
        /// be set to whEscrowSortingMode.Escrow.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="mode">
        /// The desired mode. Returns the active mode or an error code.
        /// </param>
        /// <param name="errorflags"</param>
        /// Selects which errors will cancel the transaction.
        /// <returns>
        /// <see cref="CcTalkErrors.Ok"/> if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetMasterMode(EscrowSorterMasterMode mode, EscrowSorterMasterErrorFlags errorflags)
        {
            return SetMasterMode(mode, errorflags, EscrowSortingMode.Escrow);
        }
        /// <summary>
        /// Sets the master or slave mode for the escrow sorter. The recommended parameters for this mode will be set automatically. The sorting mode can be selected.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="mode">
        /// The desired mode. Returns the active mode or an error code.
        /// </param>
        /// <param name="errorflags">
        /// Selects which errors will cancel the transaction.
        /// </param>
        /// <param name="sortingmode">
        /// The active sorting mode: escrow, direct sorter or multi escrow.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors.Ok"/> if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetMasterMode(EscrowSorterMasterMode mode, EscrowSorterMasterErrorFlags errorflags, EscrowSortingMode sortingmode)
        {
            return SetMasterMode(mode, errorflags, sortingmode, EscrowSorterAdvancedControlFlags.None);
        }
        /// <summary>
        /// Sets the master or slave mode for the escrow sorter. The recommended parameters for this mode will be set automatically. The sorting mode can be selected.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="mode">
        /// The desired mode. Returns the active mode or an error code.
        /// </param>
        /// <param name="errorflags">
        /// Selects which errors will cancel the transaction.
        /// </param>
        /// <param name="sortingmode">
        /// The active sorting mode: escrow, direct sorter or multi escrow.
        /// </param>
        /// <param name="controlflags">
        /// Advanced flags to control the chamber monitorint and the accelerator.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors.Ok"/> if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetMasterMode(EscrowSorterMasterMode mode, EscrowSorterMasterErrorFlags errorflags, EscrowSortingMode sortingmode, EscrowSorterAdvancedControlFlags controlflags)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            bool fnd = false;
            for (int i = 0; i < EscrowMasterSettings.Length; i++)
            {
                if (EscrowMasterSettings[i].MasterMode == mode)
                {
                    sdta.Data[0] = (byte)EscrowMasterSettings[i].MasterMode;
                    sdta.Data[1] = (byte)sortingmode;
                    sdta.Data[2] = EscrowMasterSettings[i].InsertTimeout;
                    sdta.Data[3] = EscrowMasterSettings[i].EmpTimeout;
                    sdta.Data[4] = EscrowMasterSettings[i].AccTimeout;
                    sdta.Data[5] = EscrowMasterSettings[i].AccRepeat;
                    sdta.Data[6] = EscrowMasterSettings[i].EscrowTimeout;
                    sdta.Data[7] = EscrowMasterSettings[i].EscrowDelay;
                    sdta.Data[8] = EscrowMasterSettings[i].ShutterDelay;
                    sdta.Data[9] = (byte)((byte)errorflags | (byte)EscrowSorterMasterInsertMode.Fast);
                    fnd = true;
                    break;
                }
            }
            this.mastermode = EscrowSorterMasterMode.Unknown;
            this.sortingmode = EscrowSortingMode.Escrow;
            if (fnd)
            {
                if (controlflags == EscrowSorterAdvancedControlFlags.None)
                {
                    sdta.DataLength = 10;
                }
                else
                {
                    sdta.DataLength = 11;
                    sdta.Data[10] = (byte)controlflags;
                }
                sdta.Header = 54;
                lasterr = TalkCc(sdta, ref rdta);
                if (lasterr == CcTalkErrors.Ok)
                {
                    if (rdta.DataLength > 0)
                    {
                        switch (mode)
                        {
                            case EscrowSorterMasterMode.MasterErrorActive:
                            case EscrowSorterMasterMode.MasterErrorInvalid:
                            case EscrowSorterMasterMode.MasterErrorMissing:
                                break;
                            default:
                                this.mastermode = mode;
                                this.sortingmode = sortingmode;
                                break;
                        }
                    }
                    else
                    {
                        lasterr = CcTalkErrors.DataFormat;
                    }
                }
            }
            else
            {
                lasterr = CcTalkErrors.InvalidParameter;
            }

            return lasterr;
        }
        /// <summary>
        /// Sets the master or slave mode for the escrow sorter. The recommended parameters for this mode will be set automatically.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="mode">
        /// The desired mode. Returns the active mode or an error code.
        /// </param>
        /// <param name="errorflags">
        /// Selects which errors will cancel the transaction.
        /// </param>
        /// <param name="insertmode">
        /// Determines the handling of the peripheral devices.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetMasterMode(EscrowSorterMasterMode mode, EscrowSorterMasterErrorFlags errorflags, EscrowSorterMasterInsertMode insertmode)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            bool fnd = false;
            for (int i = 0; i < EscrowMasterSettings.Length; i++)
            {
                if (EscrowMasterSettings[i].MasterMode == mode)
                {
                    sdta.Data[0] = (byte)EscrowMasterSettings[i].MasterMode;
                    sdta.Data[1] = (byte)EscrowMasterSettings[i].SorterMode;
                    sdta.Data[2] = EscrowMasterSettings[i].InsertTimeout;
                    sdta.Data[3] = EscrowMasterSettings[i].EmpTimeout;
                    sdta.Data[4] = EscrowMasterSettings[i].AccTimeout;
                    sdta.Data[5] = EscrowMasterSettings[i].AccRepeat;
                    sdta.Data[6] = EscrowMasterSettings[i].EscrowTimeout;
                    sdta.Data[7] = EscrowMasterSettings[i].EscrowDelay;
                    sdta.Data[8] = EscrowMasterSettings[i].ShutterDelay;
                    sdta.Data[9] = (byte)((byte)errorflags | (byte)insertmode);
                    fnd = true;
                    break;
                }
            }
            this.mastermode = EscrowSorterMasterMode.Unknown;
            this.sortingmode = EscrowSortingMode.Escrow;
            if (fnd)
            {
                sdta.DataLength = 10;
                sdta.Header = 54;
                lasterr = TalkCc(sdta, ref rdta);
                if (lasterr == CcTalkErrors.Ok)
                {
                    if (rdta.DataLength > 0)
                    {
                        switch (mode)
                        {
                            case EscrowSorterMasterMode.MasterErrorActive:
                            case EscrowSorterMasterMode.MasterErrorInvalid:
                            case EscrowSorterMasterMode.MasterErrorMissing:
                                break;
                            default:
                                this.mastermode = mode;
                                this.sortingmode = sortingmode;
                                break;
                        }
                    }
                    else
                    {
                        lasterr = CcTalkErrors.DataFormat;
                    }
                }
            }
            else
            {
                lasterr = CcTalkErrors.InvalidParameter;
            }

            return lasterr;
        }
        /// <summary>
        /// Sets the master or slave mode for the escrow sorter. The recommended parameters for this mode will be set automatically.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="mode">
        /// The desired mode. Returns the active mode or an error code.
        /// </param>
        /// <param name="errorflags">
        /// Selects which errors will cancel the transaction.
        /// </param>
        /// <param name="sortingmode">
        /// The active sorting mode: escrow, direct sorter or multi escrow.
        /// </param>
        /// <param name="insertmode">
        /// Determines the handling of the peripheral devices.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetMasterMode(EscrowSorterMasterMode mode, EscrowSorterMasterErrorFlags errorflags, EscrowSortingMode sortingmode, EscrowSorterMasterInsertMode insertmode)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            bool fnd = false;
            for (int i = 0; i < EscrowMasterSettings.Length; i++)
            {
                if (EscrowMasterSettings[i].MasterMode == mode)
                {
                    sdta.Data[0] = (byte)EscrowMasterSettings[i].MasterMode;
                    sdta.Data[1] = (byte)sortingmode;
                    sdta.Data[2] = EscrowMasterSettings[i].InsertTimeout;
                    sdta.Data[3] = EscrowMasterSettings[i].EmpTimeout;
                    sdta.Data[4] = EscrowMasterSettings[i].AccTimeout;
                    sdta.Data[5] = EscrowMasterSettings[i].AccRepeat;
                    sdta.Data[6] = EscrowMasterSettings[i].EscrowTimeout;
                    sdta.Data[7] = EscrowMasterSettings[i].EscrowDelay;
                    sdta.Data[8] = EscrowMasterSettings[i].ShutterDelay;
                    sdta.Data[9] = (byte)((byte)errorflags | (byte)insertmode);
                    fnd = true;
                    break;
                }
            }
            this.mastermode = EscrowSorterMasterMode.Unknown;
            this.sortingmode = EscrowSortingMode.Escrow;
            if (fnd)
            {
                sdta.DataLength = 10;
                sdta.Header = 54;
                lasterr = TalkCc(sdta, ref rdta);
                if (lasterr == CcTalkErrors.Ok)
                {
                    if (rdta.DataLength > 0)
                    {
                        switch (mode)
                        {
                            case EscrowSorterMasterMode.MasterErrorActive:
                            case EscrowSorterMasterMode.MasterErrorInvalid:
                            case EscrowSorterMasterMode.MasterErrorMissing:
                                break;
                            default:
                                this.mastermode = mode;
                                this.sortingmode = sortingmode;
                                break;
                        }
                    }
                    else
                    {
                        lasterr = CcTalkErrors.DataFormat;
                    }
                }
            }
            else
            {
                lasterr = CcTalkErrors.InvalidParameter;
            }

            return lasterr;
        }

        /// <summary>
        /// Starts a cash transaction.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="value">
        /// The maximum value that can be paid. If this is zero paying won't be stopped until a appropriate command is received.
        /// Currently always two decimal places are assumed for the currency when processing the value.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors StartCash(double value)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            UInt16 pvalue = (UInt16)Math.Round(value * 100);

            sdta.DataLength = 2;
            sdta.Data[0] = (byte)pvalue;
            sdta.Data[1] = (byte)(pvalue >> 8);
            sdta.Header = 46;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                if (rdta.DataLength > 0)
                {
                    cashreference = rdta.Data[0];
                    if (cashreference == 0)
                    {
                        lasterr = CcTalkErrors.CommandRejected;
                    }
                }
                else
                {
                    lasterr = CcTalkErrors.DataFormat;
                }
            }

            return lasterr;
        }
        /// <summary>
        /// Pauses an aactive cash transaction.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors PauseCash()
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 1;
            sdta.Header = 47;
            sdta.Data[0] = cashreference;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                if (rdta.DataLength > 0)
                {
                    if (cashreference != rdta.Data[0])
                    {
                        lasterr = CcTalkErrors.CommandSequence;
                    }
                }
                else
                {
                    lasterr = CcTalkErrors.DataFormat;
                }
            }

            return lasterr;
        }
        /// <summary>
        /// Continues a paused cash transaction.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors ContinueCash()
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 1;
            sdta.Header = 48;
            sdta.Data[0] = cashreference;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                if (rdta.DataLength > 0)
                {
                    if (cashreference != rdta.Data[0])
                    {
                        lasterr = CcTalkErrors.CommandSequence;
                    }
                }
                else
                {
                    lasterr = CcTalkErrors.DataFormat;
                }
            }

            return lasterr;
        }
        /// <summary>
        /// Aborts an active cash transaction. All coins will be returned.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="mode">
        /// Determines the cref="whEsrcowSorterAbortMode"/> for aborting the transaction.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors AbortCash(EsrcowSorterAbortMode mode)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 2;
            sdta.Header = 49;
            sdta.Data[0] = cashreference;
            sdta.Data[1] = (byte)mode;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                if (rdta.DataLength > 0)
                {
                    if (cashreference != rdta.Data[0])
                    {
                        lasterr = CcTalkErrors.CommandSequence;
                    }
                }
                else
                {
                    lasterr = CcTalkErrors.DataFormat;
                }
            }

            return lasterr;
        }
        /// <summary>
        /// Aborts an active cash transaction. All coins will be returned.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="mode">
        /// If set all positions will be emptied regardless of their status.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors AbortCash(bool purge)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 2;
            sdta.Header = 49;
            sdta.Data[0] = cashreference;
            sdta.Data[1] = (byte)(purge ? 0x01 : 0x00);
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                if (rdta.DataLength > 0)
                {
                    if (cashreference != rdta.Data[0])
                    {
                        lasterr = CcTalkErrors.CommandSequence;
                    }
                }
                else
                {
                    lasterr = CcTalkErrors.DataFormat;
                }
            }

            return lasterr;
        }
        /// <summary>
        /// Ends an active cash transaction.
        /// </summary>
        /// <remarks>
        /// All coins will be routed to the assigned paths.
        /// </remarks>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors EndCash()
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 1;
            sdta.Header = 50;
            sdta.Data[0] = cashreference;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                if (rdta.DataLength > 0)
                {
                    if (cashreference != rdta.Data[0])
                    {
                        lasterr = CcTalkErrors.CommandSequence;
                    }
                }
                else
                {
                    lasterr = CcTalkErrors.DataFormat;
                }
            }

            return lasterr;
        }
        /// <summary>
        /// Ends an active cash transaction ind the selected mode.
        /// </summary>
        /// <remarks>
        /// All coins will be routed to the assigned paths - either immediately or delayed.
        /// </remarks>
        /// <param name="mode">
        /// Selects if the coin s will be hold or sorted. This parameter is only valid for the MultEscrow sorting mode.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors EndCash(EscrowSorterEndCashMode mode)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 2;
            sdta.Header = 50;
            sdta.Data[0] = cashreference;
            sdta.Data[1] = (byte)mode;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                if (rdta.DataLength > 0)
                {
                    if (cashreference != rdta.Data[0])
                    {
                        lasterr = CcTalkErrors.CommandSequence;
                    }
                }
                else
                {
                    lasterr = CcTalkErrors.DataFormat;
                }
            }

            return lasterr;
        }

        /// <summary>
        /// Retrieves the status of an active cash transaction.
        /// </summary>
        /// <param name="statusdata">
        /// <see cref="EscrowSorterCashStatus"/> structure will be filled with the recent status information.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetCashStatus(ref EscrowSorterCashStatus statusdata)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            statusdata = new EscrowSorterCashStatus(true);

            sdta.DataLength = 1;
            sdta.Header = 52;
            sdta.Data[0] = cashreference;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                if (rdta.DataLength > 3)
                {
                    statusdata.Status = (EscrowSorterMasterStatus)rdta.Data[1];
                    statusdata.Value = ((double)rdta.Data[2] + (double)(rdta.Data[3] * 256)) / 100;
                }
                else
                {
                    lasterr = CcTalkErrors.DataFormat;
                }
            }

            return lasterr;
        }
        /// <summary>Retrieves the error details of the active transaction if necessary.</summary>
        /// <param name="statusdata">
        /// <see cref="whCcTalkCommunication.whEscrowSorterCashStatus"/> structure will be filled with the recent status information.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors.Ok"/> if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetCashErrors(ref EscrowSorterCashErrors errordata)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            errordata = new EscrowSorterCashErrors(true);

            sdta.DataLength = 2;
            sdta.Header = 52;
            sdta.Data[0] = cashreference;
            sdta.Data[1] = 0x10;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                if (rdta.DataLength > 11)
                {
                    errordata.Status = (EscrowSorterMasterStatus)rdta.Data[6];
                    errordata.SorterStatus = (EscrowSorterStatus)rdta.Data[7];
                    errordata.SubError = rdta.Data[8];
                    errordata.Extended0 = 0x0000;
                    errordata.Extended1 = 0x0000;
                    errordata.Extended2 = 0x00000000;
                    for (int i = 0; i < 2; i++)
                    {
                        errordata.Extended0 += (UInt16)((UInt16)rdta.Data[9 + i] << (8 * i));
                        errordata.Extended1 += (UInt16)((UInt16)rdta.Data[11 + i] << (8 * i));
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        errordata.Extended2 += (UInt16)((UInt16)rdta.Data[13 + i] << (8 * i));
                    }
                }
                else
                {
                    lasterr = CcTalkErrors.DataFormat;
                }
            }

            return lasterr;
        }
        /// <summary>Retrieves the extended error details of the active transaction if necessary.</summary>
        /// <param name="statusdata">
        /// <see cref="whCcTalkCommunication.whEscrowSorterCashStatusEx"/> structure will be filled with the recent status information.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors.Ok"/> if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetCashErrors(ref EscrowSorterCashErrorsEx errordata)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            errordata = new EscrowSorterCashErrorsEx(true);

            sdta.DataLength = 2;
            sdta.Header = 52;
            sdta.Data[0] = cashreference;
            sdta.Data[1] = 0x10;
            lasterr = TalkCc(sdta, ref rdta);
            errordata.CcTalkError = lasterr;
            if (lasterr == CcTalkErrors.Ok)
            {
                if (rdta.DataLength > 11)
                {
                    errordata.Status = (EscrowSorterMasterStatus)rdta.Data[6];
                    errordata.SorterStatus = (EscrowSorterStatus)rdta.Data[7];
                    errordata.SubError = rdta.Data[8];
                    errordata.Extended0 = 0x0000;
                    errordata.Extended1 = 0x0000;
                    errordata.Extended2 = 0x00000000;
                    for (int i = 0; i < 2; i++)
                    {
                        errordata.Extended0 += (UInt16)((UInt16)rdta.Data[9 + i] << (8 * i));
                        errordata.Extended1 += (UInt16)((UInt16)rdta.Data[11 + i] << (8 * i));
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        errordata.Extended2 += (UInt16)((UInt16)rdta.Data[13 + i] << (8 * i));
                    }
                }
                else
                {
                    lasterr = CcTalkErrors.DataFormat;
                }
            }
            errordata.CcTalkError = lasterr;
            return lasterr;
        }

        /// <summary>
        /// Retrieves recent statistics of an active cash transaction.
        /// </summary>
        /// <param name="coindata">
        /// <see cref="EscrowSorterMasterCoins"/> structure will be filled with the recent coin statistics.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetCashCoins(ref EscrowSorterMasterCoins coindata)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            coindata = new EscrowSorterMasterCoins(true);

            sdta.DataLength = 2;
            sdta.Header = 51;
            sdta.Data[0] = cashreference;
            sdta.Data[1] = 1; // extended Info
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                if (rdta.DataLength > 13)
                {
                    if (cashreference != rdta.Data[0])
                    {
                        lasterr = CcTalkErrors.CommandSequence;
                    }
                    coindata.Value = ((double)rdta.Data[1] + (double)(rdta.Data[2] * 256)) / 100;
                    coindata.Rejected = rdta.Data[3];
                    for (int i = 0; i < 10; i++)
                    {
                        coindata.Paths[i] = rdta.Data[i + 4];
                    }
                    coindata.Unidentified = rdta.Data[14];
                    coindata.Disabled = rdta.Data[15];
                    coindata.CoinJam = rdta.Data[16];
                    coindata.FollowUp = rdta.Data[17];
                    coindata.CoinOnString = rdta.Data[18];
                    coindata.NotReady = rdta.Data[19];
                    coindata.MotorReject = rdta.Data[20];
                    coindata.UndueAcc = rdta.Data[21];
                    coindata.RepeatAcc = rdta.Data[22];
                    coindata.Unknown = rdta.Data[23];
                }
                else
                {
                    lasterr = CcTalkErrors.DataFormat;
                }
            }

            return lasterr;
        }

        /// <summary>
        /// Retrieves the coin path history of an active cash transaction.
        /// </summary>
        /// <param name="statusdata">
        /// <see cref="whCcTalkCommunication.whEscrowSorterCoinHistory"/> structure will be filled with the recent coin history information.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetCashHistory(ref EscrowSorterCoinHistory coinhistory)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            coinhistory = new EscrowSorterCoinHistory(true);

            sdta.DataLength = 0;
            sdta.Header = 44;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                // if ((rdta.DataLength < 2) || ((rdta.DataLength % 2) != 0) || ((rdta.DataLength > 3) && ((rdta.Data[2] == 0x00) && (rdta.Data[3] == 0x00))))
                if ((rdta.DataLength < 2) || ((rdta.DataLength % 2) != 0))
                {
                    lasterr = CcTalkErrors.DataFormat;
                }
                else
                {
                    try
                    {
                        coinhistory.CoinsInEscrow = rdta.Data[0];
                        int histlen = 0;
                        for (int i = 0; i < ((rdta.DataLength - 2) / 2); i++)
                        {
                            if ((rdta.Data[i * 2 + 2] != 0x00) && (rdta.Data[i * 2 + 3] != 0x00))
                            {
                                histlen++;
                            }
                        }
                        coinhistory.History = new EscrowSorterCoinHistoryEntry[histlen];
                        int histidx = 0;
                        for (int i = 0; i < ((rdta.DataLength - 2) / 2); i++)
                        {
                            if ((rdta.Data[i * 2 + 2] != 0x00) && (rdta.Data[i * 2 + 3] != 0x00))
                            {
                                coinhistory.History[histidx].CoinNo = rdta.Data[i * 2 + 2] - 1;
                                coinhistory.History[histidx].InsertMode = (EscrowSorterMasterInsertMode)rdta.Data[i * 2 + 3];
                                histidx++;
                            }
                        }
                    }
                    catch { lasterr = CcTalkErrors.Internal; }
                }
            }
            return lasterr;
        }
        #endregion

        #region Common Commands
        /// <summary>
        /// Retrieves the current inhibit status.
        /// </summary>
        /// <param name="currinhibit">Array of bool - must have at least 16 elements.</param>
        /// <returns>
        /// <see cref="CcTalkErrors.Ok"/> if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetCoinInhibit(ref bool[] currinhibit)
        {
            int i;
            ushort inh, msk;

            if (currinhibit.Length < 16) return CcTalkErrors.WrongParameter;

            inh = (ushort)GetLongResponse(230);
            if (lasterr != CcTalkErrors.Ok) return lasterr;
            //ovr = (ushort)GetLongResponse(221);
            //if (lasterr != whCcTalkErrors.Ok) return lasterr;

            msk = 0x0001;
            for (i = 0; i < 16; i++)
            {
                currinhibit[i] = (inh & (msk << i)) != 0;
            }
            return lasterr;
        }

        /// <summary>
        /// Sets the inhibit status of the 16 coins - only valid in MasterMode.
        /// </summary>
        /// <remarks>
        /// On power on all coins are inhibited. 
        /// </remarks>
        /// <param name="currinhibit">
        /// Array of bool - must have at least 16 elements.
        /// If an element is set to true the related coin will be enabled. This inconsistency is due to ccTalk terminology.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors.Ok"/> if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetCoinInhibit(bool[] currinhibit)
        {
            int i;
            uint inh, msk;
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            if (currinhibit.Length < 16) return CcTalkErrors.WrongParameter;
            msk = 0x0001;
            inh = 0x0000;
            for (i = 0; i < 16; i++)
            {
                if (currinhibit[i]) inh |= msk;
                msk <<= 1;
            }
            sdta.DataLength = 2;
            sdta.Header = 231;
            sdta.Data[0] = (byte)(inh & 0x00ff);
            sdta.Data[1] = (byte)((inh >> 8) & 0x00ff);
            lasterr = TalkCc(sdta, ref rdta);
            return lasterr;
        }
        /// <summary>
        /// Sets the same inhibit status for all 16 coins - only valid in MasterMode.
        /// </summary>
        /// <remarks>
        /// On power on all coins are inhibited. 
        /// </remarks>
        /// <param name="coinenable">
        /// The status that will be set for all coins. If true all coins will be enabled.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors.Ok"/> if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetCoinInhibit(bool coinenable)
        {
            bool[] currvals = new bool[16];
            for (int i = 0; i < 16; i++) currvals[i] = coinenable;
            return SetCoinInhibit(currvals);
        }
        /// <summary>
        /// Retrieves values and currency IDs of the 16 coins.
        /// </summary>
        /// <param name="currvals">Array of <see cref="CoinValue"/>, must have at least 16 elements.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetCoinValues(ref CoinValue[] currvals)
        {
            int i;
            if (currvals.Length < 16) return CcTalkErrors.WrongParameter;
            for (i = 0; i < 16; i++)
            {
                currvals[i] = GetCoinValue(i);
                if (lasterr != CcTalkErrors.Ok) return lasterr;
            }
            return lasterr;
        }

        /// <summary>
        /// Retrieves the current sorter path of the 16 coins.
        /// </summary>
        /// Byte array holding the path numbers, must have at least 16 elements.
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetSorterPaths(ref byte[] coinpaths)
        {
            if (coinpaths.Length < 16) return CcTalkErrors.WrongParameter;
            for (int i = 0; i < 16; i++)
            {
                coinpaths[i] = GetSorterPath(i);
                if (lasterr != CcTalkErrors.Ok) return lasterr;
            }
            return lasterr;
        }
        /// <summary>
        /// Sets the sorter path of the 16 coins.
        /// </summary>
        /// <remarks>
        /// On power on the sorter path for all coins is set to a default value.
        /// Use <see cref="GetCoinValues"/> to identify available coin values and currencies
        /// and <see cref="GetSorterPaths"/> to determine current sorter paths.
        /// </remarks>
        /// <param name="coinpaths">
        /// Byte array holding the path numbers, must have at least 16 elements.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetSorterPaths(byte[] coinpaths)
        {
            int i;
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);
            for (i = 0; i < 16; i++)
            {
                sdta.DataLength = 2;
                sdta.Header = 210;
                sdta.Data[0] = (byte)(i + 1);
                sdta.Data[1] = coinpaths[i];
                lasterr = TalkCc(sdta, ref rdta);
                if (lasterr != CcTalkErrors.Ok) break;
            };
            return lasterr;
        }

        /// <summary>
        /// Tells the sorter which coin is inserted and how to route it.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="coinidx">
        /// The number of the coin which is inserted.
        /// </param>
        /// <param name="coinctrl">
        /// The routing mode for this coin.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors InsertCoin(byte coinidx, EscrowSorterCoinControl coinctrl)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 2;
            sdta.Header = 22;
            sdta.Data[0] = (byte)(coinidx + 1);
            sdta.Data[1] = (byte)coinctrl;
            lasterr = TalkCc(sdta, ref rdta);

            return lasterr;
        }
        /// <summary>
        /// Routes all coins in the sorter to the predefined paths.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SortCoins()
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 0;
            sdta.Header = 23;
            lasterr = TalkCc(sdta, ref rdta);

            return lasterr;
        }
        /// <summary>
        /// Routes all coins in the sorter to the same path.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="path">
        /// The path all coins will go to: 0 - cash box, 1...8 - payouts, 9 - reject.
        /// </param>
        /// <param name="ejectempty">
        /// Try to eject coins from slots listed as empty.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors EjectCoins(byte path, bool ejectempty)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 2;
            sdta.Header = 24;
            sdta.Data[0] = path;
            sdta.Data[1] = (byte)(ejectempty ? 0x01 : 0x00);
            lasterr = TalkCc(sdta, ref rdta);

            return lasterr;
        }

        /// <summary>
        /// Restarts the device completely.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors Restart()
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 0;
            sdta.Header = 20;
            lasterr = TalkCc(sdta, ref rdta);

            return lasterr;
        }
        /// <summary>
        /// Stops any running ejection of coins.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors Stop()
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 0;
            sdta.Header = 25;
            lasterr = TalkCc(sdta, ref rdta);

            return lasterr;
        }
        /// <summary>
        /// Resumes a previously paused ejection.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors Continue()
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 0;
            sdta.Header = 26;
            lasterr = TalkCc(sdta, ref rdta);

            return lasterr;
        }
        /// <summary>
        /// Cancels a running or stopped ejection.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors Cancel()
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 0;
            sdta.Header = 27;
            lasterr = TalkCc(sdta, ref rdta);

            return lasterr;
        }

        /// <summary>
        /// Starts one revolution of the motor reject.
        /// </summary>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors StartMotorReject()
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 0;
            sdta.Header = 32;
            lasterr = TalkCc(sdta, ref rdta);
            return lasterr;
        }

        /*  Fehlerhafte Implementierung, daher jetzt auskommentiert
        /// <summary>
        /// Sets the behaviour of the anti pin system (shutter).
        /// </summary>
        /// <param name="setting">Set up <see cref="whAntiPinSetting"/> for the behaviour of the anti pin system.</param>
        /// <returns>
        /// <see cref="whCcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public whCcTalkErrors SetupAntiPin(whAntiPinSetting setting)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 1;
            sdta.Header = 33;
            switch (setting)
            {
                case whAntiPinSetting.Disabled:
                    sdta.Data[0] = 0x00;
                    break;
                case whAntiPinSetting.Auto:
                    sdta.Data[0] = 0x01;
                    break;
                case whAntiPinSetting.Enable:
                    sdta.Data[0] = 0x02;
                    break;
                case whAntiPinSetting.Open:
                    sdta.Data[0] = 0x03;
                    break;
            }
            lasterr = TalkCc(sdta, ref rdta);
            return lasterr;
        }
        */

        /// <summary>
        /// Send a command for anti pin system (shutter).
        /// </summary>
        /// <param name="command">Command <see cref="TWSAntiPinCommand"/>.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors AntiPinControl(TWSAntiPinCommand command)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 1;
            sdta.Header = 33;
            if ((AntiPinFeatures & AntiPinFeatures.Hold) == AntiPinFeatures.Hold)
            {
                sdta.Data[0] = (byte)((int)command + 10);
            }
            else
            {
                sdta.Data[0] = (byte)command;
            }
            lasterr = TalkCc(sdta, ref rdta);
            return lasterr;
        }

        /// <summary>
        /// Sets the behaviour of the coin accelerator.
        /// </summary>
        /// <param name="setting">Set up <see cref="whCoinAcceleratorSetting"/> for the behaviour of the coin accelerator.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetupCoinAccelerator(whCoinAcceleratorSetting setting)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 1;
            sdta.Header = 35;
            sdta.Data[0] = (byte)setting;
            lasterr = TalkCc(sdta, ref rdta);
            return lasterr;
        }

        /// <summary>
        /// Polls the device to retrieve current status.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="statusdata">
        /// Returns the current status of the escrow sorter.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors PollSorter(ref EscrowSorterStatusData statusdata)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            // Sorter Status
            sdta.DataLength = 0;
            sdta.Header = 21;
            lasterr = TalkCc(sdta, ref rdta);
            if (rdta.DataLength < 2)  // Wrong data format
            {
                lasterr = CcTalkErrors.DataFormat;
                return lasterr;
            }
            statusdata.Status = (EscrowSorterStatus)rdta.Data[0];
            statusdata.Flags = EscrowSorterStatusFlags.Nothing;
            statusdata.ErrorFlags = EscrowSorterErrorFlags.NoErrors;
            // Anti Pin Status
            sdta.DataLength = 0;
            sdta.Header = 34;
            lasterr = TalkCc(sdta, ref rdta);
            statusdata.AntiPinStatus = TWSAntiPinStatus.Disabled;
            if (rdta.DataLength < 2)
            {
                lasterr = CcTalkErrors.DataFormat;
                return lasterr;
            }
            statusdata.AntiPinStatus = (TWSAntiPinStatus)((int)(rdta.Data[0] & 0x0f) + (int)rdta.Data[1] * 256);
            // Coin Accelerator Status
            sdta.DataLength = 0;
            sdta.Header = 36;
            lasterr = TalkCc(sdta, ref rdta);
            if (rdta.DataLength < 2)
            {
                lasterr = CcTalkErrors.DataFormat;
                return lasterr;
            }
            statusdata.CoinAcceleratorStatus = (CoinAcceleratorStatus)rdta.Data[0];

            return lasterr;
        }

        /// <summary>
        /// Enables the insertion sensor and retrieves its status(if applicable).
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="insertionstatus">
        /// Returns the current status of the insertion sensor.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetInsertionStatus(ref EscrowSorterInsertionStatus insertionstatus)
        {

            insertionstatus = EscrowSorterInsertionStatus.Unknown;

            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            // Scharf schalten
            sdta.DataLength = 7;
            sdta.Header = 60;
            sdta.Data[0] = 0x01;
            for (int i = 1; i < 7; i++)
            {
                sdta.Data[i] = 0x00;
            }
            lasterr = TalkCc(sdta, ref rdta);

            switch (lasterr)
            {
                case CcTalkErrors.NoAck:
                    insertionstatus = EscrowSorterInsertionStatus.Failure;
                    break;
                case CcTalkErrors.Ok:
                    sdta.DataLength = 0;
                    sdta.Header = 64;
                    lasterr = TalkCc(sdta, ref rdta);
                    if (lasterr == CcTalkErrors.Ok)
                    {
                        if (rdta.DataLength > 0)
                        {
                            switch (rdta.Data[0])
                            {
                                case 0:
                                    insertionstatus = EscrowSorterInsertionStatus.Empty;
                                    break;
                                case 1:
                                    insertionstatus = EscrowSorterInsertionStatus.Coin;
                                    break;
                                case 11:
                                case 101:
                                    insertionstatus = EscrowSorterInsertionStatus.NotActive;
                                    break;
                                case 12:
                                case 102:
                                    insertionstatus = EscrowSorterInsertionStatus.NotReady;
                                    break;
                                default:
                                    insertionstatus = EscrowSorterInsertionStatus.Unknown;
                                    break;
                            }
                            /*
                            if (rdta.Data[0] == 0x00)
                            {
                                insertionstatus = whEscrowSorterInsertionStatus.Empty;
                            }
                            else
                            {
                                insertionstatus = whEscrowSorterInsertionStatus.Coin;
                            }
                            */
                        }
                        else
                        {
                            lasterr = CcTalkErrors.DataFormat;
                        }
                    }
                    break;
            }

            return lasterr;
        }

        /// <summary>
        /// Enables or disables the insertion sensor (if applicable).
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="enable">
        /// The desired status of the insertion sensor.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetupInsertionSensor(bool enable)
        {

            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            // Scharf schalten
            sdta.DataLength = 7;
            sdta.Header = 60;
            sdta.Data[0] = (byte)(enable ? 0x01 : 0x00);
            for (int i = 1; i < 7; i++)
            {
                sdta.Data[i] = 0x00;
            }
            lasterr = TalkCc(sdta, ref rdta);

            return lasterr;
        }
        #endregion

        #region Interne Kommandos - nur für direkte Einbindung
        // Signale paralleler Müzprüfer
        internal CcTalkErrors GetEmpSignals(ref bool[] signals)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            signals = new bool[8];

            sdta.DataLength = 1;
            sdta.Header = 99;
            sdta.Data[0] = 33;
            lasterr = TalkCc(sdta, ref rdta);

            signals[0] = (rdta.Data[0] & 0x02) == 0;
            signals[1] = (rdta.Data[0] & 0x04) == 0;

            return lasterr;
        }

        // Mainblocking paralleler Münzprüfer
        internal CcTalkErrors SetEmpMainBlocking(bool state)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);


            sdta.DataLength = 2;
            sdta.Header = 99;
            sdta.Data[0] = 34;
            sdta.Data[1] = (byte)(state ? 0x01 : 0x00);
            lasterr = TalkCc(sdta, ref rdta);

            return lasterr;
        }

        internal CcTalkErrors GetLightBarriers(ref int lightbarriers)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 1;
            sdta.Header = 99;
            sdta.Data[0] = 9;
            lasterr = TalkCc(sdta, ref rdta);

            if ((lasterr == CcTalkErrors.Ok) && (rdta.DataLength >= 2))
            {
                lightbarriers = rdta.Data[0] + rdta.Data[1] * 256;
            }

            return lasterr;
        }
        internal CcTalkErrors GetLightBarriers(ref bool[] lightbarriers)
        {
            int lbs = 0x0000;

            lightbarriers = new bool[0];

            if (GetLightBarriers(ref lbs) == CcTalkErrors.Ok)
            {
                lightbarriers = new bool[16];
                for (int i = 0; i < 16; i++)
                {
                    lightbarriers[i] = (lbs & (0x0001 << i)) != 0;
                }
            }

            return lasterr;
        }

        internal CcTalkErrors GetInsertionStatus(ref bool occupied)
        {
            EscrowSorterInsertionStatus insertionstatus = EscrowSorterInsertionStatus.Unknown;

            GetInsertionStatus(ref insertionstatus);

            occupied = insertionstatus == EscrowSorterInsertionStatus.Coin;

            return lasterr;
        }
        #endregion

        #region Private variables
        // Constants
        private const byte SORTER_ADR = 160;

        private EscrowSorterMasterMode mastermode = EscrowSorterMasterMode.Slave;
        private EscrowSortingMode sortingmode = EscrowSortingMode.Escrow;
        private byte cashreference = 0;

        // Voreinstellungen für Mastermodes
        private struct whEscrowSorterSetupEntry
        {
            public EscrowSorterMasterMode MasterMode;
            public EscrowSorterMode SorterMode;
            public byte InsertTimeout;
            public byte EmpTimeout;
            public byte AccTimeout;
            public byte AccRepeat;
            public byte EscrowTimeout;
            public byte EscrowDelay;
            public byte ShutterDelay;

            public whEscrowSorterSetupEntry(EscrowSorterMasterMode md, EscrowSorterMode sm, byte itot, byte etot, byte atot, byte arep, byte stot, byte sdel, byte rdel)
            {
                MasterMode = md;
                SorterMode = sm;
                InsertTimeout = itot;
                EmpTimeout = etot;
                AccTimeout = atot;
                AccRepeat = arep;
                EscrowTimeout = stot;
                EscrowDelay = sdel;
                ShutterDelay = rdel;
            }
        }

        private whEscrowSorterSetupEntry[] EscrowMasterSettings = {
            new whEscrowSorterSetupEntry(EscrowSorterMasterMode.Slave,   EscrowSorterMode.Escrow,  0,  0,  0, 0,   0,  0, 0),
            new whEscrowSorterSetupEntry(EscrowSorterMasterMode.Master1, EscrowSorterMode.Escrow,  0,  0,  0, 0, 100, 15, 0),
            new whEscrowSorterSetupEntry(EscrowSorterMasterMode.Master2, EscrowSorterMode.Escrow,  0, 20,  0, 0, 100, 15, 0),
            new whEscrowSorterSetupEntry(EscrowSorterMasterMode.Master3, EscrowSorterMode.Escrow,  0, 20, 50, 3, 100, 25, 0),
            new whEscrowSorterSetupEntry(EscrowSorterMasterMode.Master4, EscrowSorterMode.Escrow,  0, 20, 50, 0, 100, 15, 0),
            new whEscrowSorterSetupEntry(EscrowSorterMasterMode.Master5, EscrowSorterMode.Escrow,  0, 20,  0, 0, 100, 15, 0),
            new whEscrowSorterSetupEntry(EscrowSorterMasterMode.Master6, EscrowSorterMode.Escrow,  0, 20, 50, 3, 100, 25, 0),
        };
        #endregion
    }
}