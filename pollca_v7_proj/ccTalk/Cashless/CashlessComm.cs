using System;

namespace ccTalk
{
    /// <summary>
    /// wh Cashless Payment System class 
    /// This a MDB device connected via CCT 900. 
    /// It will appear at special ccTalk address <see cref="CcTalkComm.MdbCashlessAddress"/>
    /// </summary>
    [Serializable]
    public class CashlessComm : CcTalkComm
    {
        #region Constructor/Destructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// Sets some default values.
        /// </remarks>
        public CashlessComm()
        {
            Address = MdbAddresses.CcCashless;
            displaydata = new CashlessDisplay();
            displaydata.Duration = 0;
            displaydata.Data = new string[2];
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// <param name="basedevice">Instance of the base class<see cref="CcTalkComm"/> were some settings are taken from:</param>
        /// Address, Port and ChecksumType.
        /// </remarks>
        public CashlessComm(CcTalkComm basedevice)
        {
            Address = MdbAddresses.CcCashless;
            Port = basedevice.Port;
            ChecksumType = CcTalkChecksumTypes.Simple8;
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
#if SUPPORT_CASHLESS
            lasterr = base.OpenComm();
            if (Address == MdbAddresses.CcCashless)
            {
                if (lasterr == whCcTalkErrors.Ok)
                {
                    decimals = CashlessSetup.Decimals;
                    sclfac = CashlessSetup.Scaling / Math.Pow(10, CashlessSetup.Decimals);
                    ccid = GetCcTalkID(CashlessSetup.CountryHi * 256 + CashlessSetup.CountryLo);
                    devstatus = whCashlessStatus.Inactive;
                    ReceivedFile = new whMDBFile(true);
                }
            }
            else
            {
            }
#else
            lasterr = CcTalkErrors.UnSupported;
#endif
            return lasterr;
        }

        /// <summary>
        /// Available options for the current device.
        /// </summary>
        public CashlessOptions Options
        {
            get
            {
                if (Address == MdbAddresses.CcCashless)
                {
                    return (CashlessOptions)CashlessSetupInstance.Options;
                }
                else
                {
                    CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                    CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);
                    UInt16 options = 0x0000;

                    sdta.DataLength = 0;
                    sdta.Header = 235;
                    if (((lasterr = TalkCc(sdta, ref rdta)) == CcTalkErrors.Ok) && (rdta.DataLength > 3))
                    {
                        options = (UInt16)((UInt16)rdta.Data[0] + (UInt16)rdta.Data[1] * 256);
                    }
                    return (CashlessOptions)options;
                }
            }
        }
        /// <summary>
        /// Available features of the current device.
        /// </summary>
        public CashlessFeatures Features
        {
            get
            {
                UInt32 ifeatures = 0x00000000;
                if (Address == MdbAddresses.CcCashless)
                {

                    for (int i = 0; i < 4; i++)
                    {
                        try
                        {
                            ifeatures = ifeatures << 8;
                            ifeatures |= CashlessIdentifyInstance.FeatureBytes[i];
                        }
                        catch { };
                    }
                }
                else
                {
                    CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                    CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);
                    UInt16 features = 0x0000;

                    sdta.DataLength = 0;
                    sdta.Header = 235;
                    if (((lasterr = TalkCc(sdta, ref rdta)) == CcTalkErrors.Ok) && (rdta.DataLength > 3))
                    {
                        features = (UInt16)((UInt16)rdta.Data[2] + (UInt16)rdta.Data[3] * 256);
                    }
                    return (CashlessFeatures)features;
                }
                return (CashlessFeatures)ifeatures;
            }
        }
        /// <summary>
        /// Currency code of the cashless payment device.
        /// </summary>
        public string Currency
        {
            get
            {
                if (Address == MdbAddresses.CcCashless)
                {
                    ccid = GetCcTalkID(CashlessSetupInstance.CountryHi * 256 + CashlessSetupInstance.CountryLo);
                }
                else
                {
                    CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                    CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);
                    ccid = "XX";

                    sdta.DataLength = 0;
                    sdta.Header = 234;
                    if (((lasterr = TalkCc(sdta, ref rdta)) == CcTalkErrors.Ok) && (rdta.DataLength > 3))
                    {
                        ccid = "" + (char)rdta.Data[0] + (char)rdta.Data[1];
                    }
                }
                return ccid;
            }
        }
        /// <summary>
        /// Number of decimal places.
        /// </summary>
        public int Decimals
        {
            get
            {
                if (Address != MdbAddresses.CcCashless)
                {
                    CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                    CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);
                    decimals = 0;

                    sdta.DataLength = 0;
                    sdta.Header = 234;
                    if (((lasterr = TalkCc(sdta, ref rdta)) == CcTalkErrors.Ok) && (rdta.DataLength > 3))
                    {
                        decimals = rdta.Data[2];
#if SUPPORT_CASHLESS
                        sclfac = rdta.Data[3] / Math.Pow(10, rdta.Data[2]);
#endif
                    }
                }
                return decimals;
            }
        }
        /// <summary>
        /// Current statusof the device.
        /// </summary>
        public CashlessStatus Status
        {
            get
            {
                if (Address != MdbAddresses.CcCashless)
                {
                }
                return devstatus;
            }
        }
        /// <summary>A data file received from the reader</summary>
        public MDBFile ReceivedFile;

        /// <summary>
        /// Enables cashless payment device and sets the minimum and maximum price. 
        /// Afterwards system should be ready to accept payment media.
        /// </summary>
        /// <param name="minprice">Minimum price.</param>
        /// <param name="maxprice">Maximum price.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetupCashless(double minprice, double maxprice)
        {
#if SUPPORT_CASHLESS
            whMdbDataBlock smdblk = new whMdbDataBlock(true);
            whMdbDataBlock rmdblk = new whMdbDataBlock(true);

            if (Address == MdbAddresses.CcCashless)
            {
                ResetDevice(200);
                if (lasterr == whCcTalkErrors.Ok) devstatus = whCashlessStatus.Disabled;

                #region Setup Reader
                CashlessConfig.InitStructure();
                smdblk.DataLength = Marshal.SizeOf(CashlessConfig) + 1;
                smdblk.Data[0] = (byte)(cashlessaddr | whMdbCommands.CPSetup);
                CashlessConfig.SetToBuffer(smdblk, 1);
                lasterr = TalkCPMdb(smdblk, ref rmdblk, CashlessPollEvent.Config);
                #endregion
                #region Set prices
                smdblk.DataLength = 6;
                smdblk.Data[0] = (byte)(cashlessaddr | whMdbCommands.CPSetup);
                smdblk.Data[1] = 0x01;  // Setup Prices
                UInt16 imaxpr = (UInt16)Math.Round(maxprice / sclfac);
                UInt16 iminpr = (UInt16)Math.Round(minprice / sclfac);
                smdblk.Data[2] = (byte)(imaxpr >> 8);
                smdblk.Data[3] = (byte)(imaxpr & 0x00ff);
                smdblk.Data[4] = (byte)(iminpr >> 8);
                smdblk.Data[5] = (byte)(iminpr & 0x00ff);
                lasterr = TalkMdb(smdblk, ref rmdblk);
                #endregion
            }
            else
            {
                lasterr = whCcTalkErrors.Ok;
            }
            if (lasterr == whCcTalkErrors.Ok)
            {
                SetState(whCashlessState.Enable);
                if (lasterr == whCcTalkErrors.Ok) devstatus = whCashlessStatus.Enabled;
            }
#else
            lasterr = CcTalkErrors.UnSupported;
#endif
            return lasterr;
        }

        /// <summary>
        /// Sets date and time of the readers real time clock. 
        /// </summary>
        /// <param name="settime">The date and time to be set - usually <see cref="DateTime.Now"/></param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetDateTime(DateTime settime)
        {
#if SUPPORT_CASHLESS

            if (Address == MdbAddresses.CcCashless)
            {
                whMdbDataBlock smdblk = new whMdbDataBlock(true);
                whMdbDataBlock rmdblk = new whMdbDataBlock(true);

                smdblk.DataLength = 12;
                smdblk.Data[0] = (byte)(cashlessaddr | whMdbCommands.Expansion);
                smdblk.Data[1] = (byte)(whMdbCommands.CPSetDateTime);
                smdblk.Data[2] = IntToBCD(settime.Year - 2000);
                smdblk.Data[3] = IntToBCD(settime.Month);
                smdblk.Data[4] = IntToBCD(settime.Day);
                smdblk.Data[5] = IntToBCD(settime.Hour);
                smdblk.Data[6] = IntToBCD(settime.Minute);
                smdblk.Data[7] = IntToBCD(settime.Second);
                smdblk.Data[8] = IntToBCD((int)settime.DayOfWeek);
                smdblk.Data[9] = 0xff;      // Week Number
                smdblk.Data[10] = 0xff;     // Summertime
                smdblk.Data[11] = 0xff;     // Holiday
                lasterr = TalkMdb(smdblk, ref rmdblk);
            }
#else
            lasterr = CcTalkErrors.UnSupported;
#endif
            return lasterr;
        }

        /// <summary>
        /// Sets the cashless payment device to the desired state.
        /// </summary>
        /// <param name="state">Desired state of the device.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetState(CashlessState state)
        {
#if SUPPORT_CASHLESS
            if (Address == MdbAddresses.CcCashless)
            {
                whMdbDataBlock smdblk = new whMdbDataBlock(true);
                whMdbDataBlock rmdblk = new whMdbDataBlock(true);
                smdblk.DataLength = 2;
                smdblk.Data[0] = (byte)(cashlessaddr | whMdbCommands.CPEnable);
                smdblk.Data[1] = (byte)state;
                lasterr = TalkMdb(smdblk, ref rmdblk);
            }
            else
            {
                CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);
                decimals = 0;

                sdta.DataLength = 1;
                sdta.Header = 224;
                sdta.Data[0] = (byte)state;
                lasterr = TalkCc(sdta, ref rdta);
            }
            if (lasterr == whCcTalkErrors.Ok)
            {
                switch (state)
                {
                    case whCashlessState.Cancel:
                        switch (devstatus)
                        {
                            case whCashlessStatus.Enabled:
                            case whCashlessStatus.NegativeVend:
                            case whCashlessStatus.Revalue:
                            case whCashlessStatus.SessionIdle:
                            case whCashlessStatus.Vend:
                                devstatus = whCashlessStatus.Enabled;
                                break;
                        }
                        break;
                    case whCashlessState.Disable:
                        switch (devstatus)
                        {
                            case whCashlessStatus.Enabled:
                            case whCashlessStatus.NegativeVend:
                            case whCashlessStatus.Revalue:
                            case whCashlessStatus.SessionIdle:
                            case whCashlessStatus.Vend:
                                devstatus = whCashlessStatus.Disabled;
                                break;
                        }
                        break;
                    case whCashlessState.Enable:
                        switch (devstatus)
                        {
                            case whCashlessStatus.Disabled:
                                devstatus = whCashlessStatus.Enabled;
                                break;
                        }
                        break;
                }
            }
#else
            lasterr = CcTalkErrors.UnSupported;
#endif
            return lasterr;
        }

        /// <summary>
        /// Enables features at the cashless payment device.
        /// </summary>
        /// <param name="features">Features that will be enabled. 
        /// Read <see cref="CashlessComm.Features"/> to determine which features are available></param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors EnableFeatures(CashlessFeatures features)
        {
#if SUPPORT_CASHLESS
            if (Address == MdbAddresses.CcCashless)
            {
                whMdbDataBlock smdblk = new whMdbDataBlock(true);
                whMdbDataBlock rmdblk = new whMdbDataBlock(true);
                UInt32 ifeatures = (UInt32)features;

                smdblk.DataLength = 6;
                smdblk.Data[0] = (byte)(cashlessaddr | whMdbCommands.Expansion);
                smdblk.Data[1] = (byte)(whMdbCommands.CPEnableFeatures);
                for (int i = 3; i >= 0; i--)
                {
                    smdblk.Data[2 + i] = (byte)ifeatures;
                    ifeatures = ifeatures >> 8;
                }
                lasterr = TalkMdb(smdblk, ref rmdblk);
            }
            else
            {
            }
#else
            lasterr = CcTalkErrors.UnSupported;
#endif
            return lasterr;
        }

        /// <summary>
        /// Initiate a transaction.
        /// </summary>
        /// <param name="itemprice">Price of the item to be sold.</param>
        /// <param name="itemnumber">Number of the item to be sold (optional).</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors VendRequest(double itemprice, int itemnumber)
        {
#if SUPPORT_CASHLESS
            if (Address == MdbAddresses.CcCashless)
            {
                whMdbDataBlock smdblk = new whMdbDataBlock(true);
                whMdbDataBlock rmdblk = new whMdbDataBlock(true);

                smdblk.DataLength = 6;
                smdblk.Data[0] = (byte)(cashlessaddr | whMdbCommands.CPVend);
                smdblk.Data[1] = 0x00;  // Vend Request
                UInt16 iprice = (UInt16)Math.Round(itemprice / sclfac);
                smdblk.Data[2] = (byte)(iprice >> 8);
                smdblk.Data[3] = (byte)(iprice & 0x00ff);
                smdblk.Data[4] = (byte)(itemnumber >> 8);
                smdblk.Data[5] = (byte)(itemnumber & 0x00ff);
                lasterr = TalkMdb(smdblk, ref rmdblk);
                if (lasterr == whCcTalkErrors.Ok) devstatus = whCashlessStatus.Vend;
            }
            else
            {
                CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

                sdta.DataLength = 7;
                sdta.Header = 206;
                UInt32 iprice = (UInt32)Math.Round(itemprice / sclfac);
                sdta.Data[0] = 0x00;
                sdta.Data[1] = (byte)(iprice);
                sdta.Data[2] = (byte)(iprice >> 8);
                sdta.Data[3] = (byte)(iprice >> 16);
                sdta.Data[4] = (byte)(iprice >> 24);
                sdta.Data[5] = (byte)(itemnumber >> 8);
                sdta.Data[6] = (byte)(itemnumber & 0x00ff);

                if ((lasterr = TalkCc(sdta, ref rdta)) == whCcTalkErrors.Ok) devstatus = whCashlessStatus.Vend;
            }
#else
            lasterr = CcTalkErrors.UnSupported;
#endif
            return lasterr;
        }

        /// <summary>
        /// Retrieves the limit for revalue operations.
        /// </summary>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetRevalueLimit()
        {
            if (Address == MdbAddresses.CcCashless)
            {
                #region MDB Card Reader
                MdbDataBlock smdblk = new MdbDataBlock(true);
                MdbDataBlock rmdblk = new MdbDataBlock(true);
                smdblk.Data[0] = (byte)(cashlessaddr | MdbCommands.CPRevalue);
                smdblk.Data[1] = MdbCommands.CPRevalueLimit;
                smdblk.DataLength = 1;
                lasterr = TalkMdb(smdblk, ref rmdblk);
                #endregion
            }
            else
            {
                CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

                sdta.DataLength = 1;
                sdta.Header = 205;
                sdta.Data[0] = 0x01;
                lasterr = TalkCc(sdta, ref rdta);
            }
            return lasterr;
        }

        /// <summary>
        /// Transfer a value to the payment media.
        /// </summary>
        /// <param name="balance">Value to be transferred.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors RevalueRequest(double balance)
        {
#if SUPPORT_CASHLESS
            if (Address == MdbAddresses.CcCashless)
            {
                #region MDB Card Reader
                whMdbDataBlock smdblk = new whMdbDataBlock(true);
                whMdbDataBlock rmdblk = new whMdbDataBlock(true);
                smdblk.Data[0] = (byte)(cashlessaddr | whMdbCommands.CPRevalue);
                smdblk.Data[1] = whMdbCommands.CPRevalueRequest;
                if (CashlessSetup.FeatureLevel < 3)
                {
                    smdblk.DataLength = 4;
                    UInt16 ibalance = (UInt16)Math.Round(balance / sclfac);
                    smdblk.Data[2] = (byte)(ibalance >> 8);
                    smdblk.Data[3] = (byte)(ibalance & 0x00ff);
                }
                else
                {
                    smdblk.DataLength = 6;
                    UInt32 ibalance = (UInt16)Math.Round(balance / sclfac);
                    smdblk.Data[2] = (byte)(ibalance >> 24);
                    smdblk.Data[3] = (byte)(ibalance >> 16);
                    smdblk.Data[4] = (byte)(ibalance >> 8);
                    smdblk.Data[5] = (byte)(ibalance & 0x00ff);
                }
                if ((lasterr = TalkMdb(smdblk, ref rmdblk)) == whCcTalkErrors.Ok)
                {
                }
                #endregion
            }
            else
            {
                CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

                if (CashlessSetup.FeatureLevel < 3)
                {
                    sdta.DataLength = 5;
                    sdta.Header = 205;
                    UInt32 iprice = (UInt32)Math.Round(balance / sclfac);
                    sdta.Data[0] = (byte)0;
                    sdta.Data[1] = (byte)(iprice);
                    sdta.Data[2] = (byte)(iprice >> 8);
                    sdta.Data[3] = (byte)(iprice >> 16);
                    sdta.Data[4] = (byte)(iprice >> 24);
                }

                if ((lasterr = TalkCc(sdta, ref rdta)) == whCcTalkErrors.Ok) devstatus = whCashlessStatus.Revalue;
            }
#else
            lasterr = CcTalkErrors.UnSupported;
#endif
            return lasterr;
        }

        /// <summary>
        /// Cancels or completes a transaction. for Vend Success the item number will be set to -1 (0xffff).
        /// </summary>
        /// <param name="state">Desired state of the current vend <see  cref="VendState"/>.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetVendState(VendState state)
        {
            return SetVendState(state, -1);
        }
        /// <summary>
        /// Cancels or completes a transaction.
        /// </summary>
        /// <param name="state">Desired state of the current vend <see  cref="VendState"/>.</param>
        /// <param name="itemnumber">The item number of the selected product. Applies only to Vend Success.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetVendState(VendState state, int itemnumber)
        {
#if SUPPORT_CASHLESS
            if (Address == MdbAddresses.CcCashless)
            {

                whMdbDataBlock smdblk = new whMdbDataBlock(true);
                whMdbDataBlock rmdblk = new whMdbDataBlock(true);

                switch (state)
                {
                    case whVendState.Success:
                        smdblk.DataLength = 4;
                        smdblk.Data[0] = (byte)(cashlessaddr | whMdbCommands.CPVend);
                        smdblk.Data[1] = (byte)state;
                        smdblk.Data[2] = (byte)(itemnumber >> 8);
                        smdblk.Data[3] = (byte)(itemnumber & 0x00ff);
                        break;
                    default:
                        smdblk.DataLength = 2;
                        smdblk.Data[0] = (byte)(cashlessaddr | whMdbCommands.CPVend);
                        smdblk.Data[1] = (byte)state;
                        break;
                }
                lasterr = TalkMdb(smdblk, ref rmdblk);
            }
            else
            {
                CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

                sdta.DataLength = 1;
                sdta.Header = 206;
                sdta.Data[0] = (byte)state;

                if ((lasterr = TalkCc(sdta, ref rdta)) == whCcTalkErrors.Ok) devstatus = whCashlessStatus.Vend;
            }
#else
            lasterr = CcTalkErrors.UnSupported;
#endif
            return lasterr;
        }

        /// <summary>
        /// Polls the device to retrieve current events.
        /// </summary>
        /// <remarks>
        /// Poll should be performed app. every 200msecs otherwise events may be lost.
        /// </remarks>
        /// <param name="pollresp">Current state of the cashless payment device<see  cref="CashlessPollResponse"/>.
        /// The meaning of Amount depends on the event: on BeginSession it holds the available fund of the payment media, on VendApproved it holds the credit withdrawn.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors PollCashless(ref CashlessPollResponse pollresp)
        {
#if SUPPORT_CASHLESS
            pollresp.Status = CashlessPollEvent.Unknown;
            pollresp.Error = whCashlessError.Ok;

            if (Address == MdbAddresses.CcCashless)
            {
                #region MDB Card Reader
                whMdbDataBlock smdblk = new whMdbDataBlock(true);
                whMdbDataBlock rmdblk = new whMdbDataBlock(true);
                smdblk.DataLength = 1;
                smdblk.Data[0] = (byte)(cashlessaddr | whMdbCommands.CPPoll);

                if ((lasterr = TalkMdb(smdblk, ref rmdblk)) == whCcTalkErrors.Ok)
                {
                    if (rmdblk.DataLength > 0)
                    {
                        pollresp.Status = (CashlessPollEvent)rmdblk.Data[0];
                        UInt16 iamount = (UInt16)(rmdblk.Data[2] + 256 * rmdblk.Data[1]);
                        pollresp.MediaID = 0x00000000;
                        switch (pollresp.Status)
                        {
                            case CashlessPollEvent.Display:
                                #region Retrieve display data
                                displaydata.Duration = rmdblk.Data[1] * 100;
                                displaydata.Data[0] = "";
                                displaydata.Data[1] = "";
                                for (int i = 0; i < 16; i++)
                                {
                                    displaydata.Data[0] += (char)rmdblk.Data[i + 2];
                                    displaydata.Data[1] += (char)rmdblk.Data[i + 18];
                                }
                                pollresp.Display = displaydata.Clone();
                                #endregion
                                break;
                            case CashlessPollEvent.Malfunction:
                                pollresp.Error = (whCashlessError)rmdblk.Data[1];
                                break;
                            case CashlessPollEvent.BeginSession:
                                // Available fund
                                if (iamount == 0xffff)
                                    pollresp.Amount = -1;
                                else
                                    pollresp.Amount = (double)iamount * sclfac;
                                // Media ID
                                if (rmdblk.DataLength > 6)
                                {
                                    for (int i = 0; i < 4; i++)
                                        pollresp.MediaID += (long)rmdblk.Data[3 + i] << (8 * (3 - i));
                                }
                                // Hier gibt's ggf. noch was zu tun!
                                devstatus = whCashlessStatus.SessionIdle;
                                break;
                            case CashlessPollEvent.VendApproved:
                                pollresp.Amount = (double)iamount * sclfac;
                                devstatus = whCashlessStatus.SessionIdle;
                                break;
                            case CashlessPollEvent.RevalueLimit:
                                pollresp.Amount = (double)iamount * sclfac;
                                break;
                            case CashlessPollEvent.EndSession:
                                devstatus = whCashlessStatus.Enabled;
                                break;
                            case CashlessPollEvent.CancelRequest:
                                devstatus = whCashlessStatus.Enabled;
                                break;
                            case CashlessPollEvent.VendDenied:
                                devstatus = whCashlessStatus.SessionIdle;
                                break;
                            case CashlessPollEvent.OutOfSequence:
                                devstatus = (whCashlessStatus)rmdblk.Data[1];
                                break;
                            case CashlessPollEvent.SendBlock:
                                if (rmdblk.Data[1] == 0x00)         // Destionation is host
                                {
                                    if (rmdblk.DataLength > 3)
                                    {
                                        int blockno = rmdblk.Data[2];
                                        int dtalen = rmdblk.DataLength - 3;
                                        ReceivedFile.Length += dtalen;
                                        for (int i = 0; i < dtalen; i++)
                                            ReceivedFile.Data[blockno * 31 + i] = rmdblk.Data[i + 3];
                                        ReceivedFile.Complete = (blockno + 1) == ReceivedFile.Info.MaxBlocks;
                                    }
                                    else
                                    {
                                        ReceivedFile.Complete = true;       // Done!
                                    }
                                    if (!ReceivedFile.Complete) OkToSend();
                                }
                                break;
                            case CashlessPollEvent.RetryDeny:
                                pollresp.Amount = rmdblk.Data[3];
                                OkToSend();
                                break;
                            case CashlessPollEvent.RequestToSend:
                                if (rmdblk.Data[1] == 0x00)         // Destionation is host
                                {
                                    ReceivedFile.Info.Destination = rmdblk.Data[1];
                                    ReceivedFile.Info.Source = rmdblk.Data[2];
                                    ReceivedFile.Info.FileID = rmdblk.Data[3];
                                    ReceivedFile.Info.MaxBlocks = rmdblk.Data[4];
                                    ReceivedFile.Info.Control = (whMDBFileControl)rmdblk.Data[5];
                                    ReceivedFile.Length = 0;
                                    ReceivedFile.Complete = false;
                                    OkToSend();
                                }
                                break;
                            case CashlessPollEvent.RequestToReceive:
                                break;
                            case CashlessPollEvent.OkToSend:
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        pollresp.Status = CashlessPollEvent.Null;
                    }
                }
                #endregion
            }
            else
            {
                #region ccTalk Card Reader
                CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

                sdta.DataLength = 0;
                sdta.Header = 190;
                if (((lasterr = TalkCc(sdta, ref rdta)) == whCcTalkErrors.Ok) && (rdta.DataLength > 5))
                {
                    pollresp.Status = (CashlessPollEvent)rdta.Data[0];

                    UInt32 iamount = 0;
                    for (int i = 0; i < 4; i++)
                        iamount += (UInt32)rdta.Data[2 + i] << (8 * i);
                    switch (pollresp.Status)
                    {
                        case CashlessPollEvent.Display:
                            #region Retrieve display data
                            displaydata.Duration = rdta.Data[1] * 100;
                            displaydata.Data[0] = "";
                            displaydata.Data[1] = "";
                            for (int i = 0; i < 16; i++)
                            {
                                displaydata.Data[0] += (char)rdta.Data[i + 2];
                                displaydata.Data[1] += (char)rdta.Data[i + 18];
                            }
                            pollresp.Display = displaydata.Clone();
                            #endregion
                            break;
                        case CashlessPollEvent.Malfunction:
                            pollresp.Error = (whCashlessError)rdta.Data[1];
                            break;
                        case CashlessPollEvent.BeginSession:
                            if (iamount == 0xffffffff)
                                pollresp.Amount = -1;
                            else
                                pollresp.Amount = (double)iamount * sclfac;
                            // Hier gibt's ggf. noch was zu tun!
                            devstatus = whCashlessStatus.SessionIdle;
                            break;
                        case CashlessPollEvent.VendApproved:
                            pollresp.Amount = (double)iamount * sclfac;
                            devstatus = whCashlessStatus.SessionIdle;
                            break;
                        case CashlessPollEvent.RevalueLimit:
                            pollresp.Amount = (double)iamount * sclfac;
                            break;
                        case CashlessPollEvent.EndSession:
                            devstatus = whCashlessStatus.Enabled;
                            break;
                        case CashlessPollEvent.CancelRequest:
                            devstatus = whCashlessStatus.Enabled;
                            break;
                        case CashlessPollEvent.VendDenied:
                            devstatus = whCashlessStatus.SessionIdle;
                            break;
                        case CashlessPollEvent.OutOfSequence:
                            devstatus = (whCashlessStatus)rdta.Data[1];
                            break;
                        case CashlessPollEvent.RevalueDenied:
                            devstatus = whCashlessStatus.SessionIdle;
                            break;
                        case CashlessPollEvent.RevalueApproved:
                            pollresp.Amount = (double)iamount * sclfac;
                            devstatus = whCashlessStatus.SessionIdle;
                            break;
                        case CashlessPollEvent.SendBlock:
                        case CashlessPollEvent.RetryDeny:
                        case CashlessPollEvent.RequestToSend:
                        case CashlessPollEvent.RequestToReceive:
                        case CashlessPollEvent.OkToSend:
                            break;
                        default:
                            break;
                    }
                }
                #endregion
            }
#else
            lasterr = CcTalkErrors.UnSupported;
#endif
            return lasterr;
        }

        /// <summary>
        /// Log a successfully completed cash sales. 
        /// </summary>
        /// <remarks>
        /// Once a cash sale has been successfully completed by the VMC it can be logged in the cash terminal.
        /// </remarks>
        /// <param name="itemprice">Price of the selected product.</param>
        /// <param name="itemnumber">The item number of the selected product. Set to -1 for undefined or not implemented.</param>
        /// <param name="itemcurrency">The currency for the item during the vend. The currency is passed using the numeric code as defined in ISO 4217.
        /// The value is configured as packed BCD with the leading digit 1. This parameter will be ignored for level 01/02 readers.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors LogCashSale(double itemprice, int itemnumber, int itemcurrency)
        {
#if SUPPORT_CASHLESS
            if (Address == MdbAddresses.CcCashless)
            {
                #region MDB Card Reader
                whMdbDataBlock smdblk = new whMdbDataBlock(true);
                whMdbDataBlock rmdblk = new whMdbDataBlock(true);
                smdblk.Data[0] = (byte)(cashlessaddr | whMdbCommands.CPVend);
                smdblk.Data[1] = whMdbCommands.CPCashSale;
                if (CashlessSetup.FeatureLevel < 3)
                {
                    smdblk.DataLength = 6;
                    UInt16 iprice = (UInt16)Math.Round(itemprice / sclfac);
                    smdblk.Data[2] = (byte)(iprice >> 8);
                    smdblk.Data[3] = (byte)(iprice & 0x00ff);
                    smdblk.Data[4] = (byte)(itemnumber >> 8);
                    smdblk.Data[5] = (byte)(itemnumber & 0x00ff);
                }
                else
                {
                    smdblk.DataLength = 10;
                    UInt32 iprice = (UInt16)Math.Round(itemprice / sclfac);
                    smdblk.Data[2] = (byte)(iprice >> 24);
                    smdblk.Data[3] = (byte)(iprice >> 16);
                    smdblk.Data[4] = (byte)(iprice >> 8);
                    smdblk.Data[5] = (byte)(iprice & 0x00ff);
                    smdblk.Data[6] = (byte)(itemnumber >> 8);
                    smdblk.Data[7] = (byte)(itemnumber & 0x00ff);
                    smdblk.Data[8] = (byte)(itemcurrency >> 8);
                    smdblk.Data[9] = (byte)(itemcurrency & 0x00ff);
                }
                if ((lasterr = TalkMdb(smdblk, ref rmdblk)) == whCcTalkErrors.Ok)
                {
                }
                #endregion
            }
            else
            {
                #region ccTalk Card Reader
                lasterr = whCcTalkErrors.UnSupported;
                #endregion
            }
#else
            lasterr = CcTalkErrors.UnSupported;
#endif
            return lasterr;
        }
        /// <summary>
        /// Log a successfully completed cash sales. 
        /// </summary>
        /// <remarks>
        /// Once a cash sale has been successfully completed by the VMC it can be logged in the cash terminal. For level 03 readers the currency of the reader will be used.
        /// </remarks>
        /// <param name="itemprice">Price of the selected product.</param>
        /// <param name="itemnumber">The item number of the selected product. Set to -1 for undefined or not implemented.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors LogCashSale(double itemprice, int itemnumber)
        {
#if SUPPORT_CASHLESS
            lasterr = LogCashSale(itemprice, itemnumber, CashlessSetup.CountryHi * 256 + CashlessSetup.CountryLo);
#else
            lasterr = CcTalkErrors.UnSupported;
#endif
            return lasterr;
        }
        /// <summary>
        /// Log a successfully completed cash sales. 
        /// </summary>
        /// <remarks>
        /// Once a cash sale has been successfully completed by the VMC it can be logged in the cash terminal.
        /// </remarks>
        /// <param name="itemprice">Price of the selected product.</param>
        /// <param name="itemnumber">The item number of the selected product. Set to -1 for undefined or not implemented.</param>
        /// <param name="itemcurrency">The currency for the item during the vend. The currency is passed as string of at least two characters. 
        /// The first two characters represent the ccTalk currency ID.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors LogCashSale(double itemprice, int itemnumber, string itemcurrency)
        {
#if SUPPORT_CASHLESS
            int icurrency = GetMdbCode(itemcurrency);
            if (icurrency > 0)
            {
                lasterr = LogCashSale(itemprice, itemnumber, icurrency);
            }
            else
            {
                lasterr = whCcTalkErrors.WrongParameter;
            }
#else
            lasterr = CcTalkErrors.UnSupported;
#endif
            return lasterr;
        }


        #region Private and internal variables and methodes
#if SUPPORT_CASHLESS
        internal double sclfac = 0.01;
#endif
        internal string ccid = "XX";
        internal int decimals = 0;
        internal CashlessDisplay displaydata;
        internal CashlessStatus devstatus = CashlessStatus.Closed;

        internal byte IntToBCD(int ival)
        {
            byte bcdval = 0x00;
            for (int i = 0; i < 2; i++)
            {
                bcdval |= (byte)((ival % 10) << (i * 4));
                ival = ival / 10;
            }

            return bcdval;
        }

        internal CcTalkErrors OkToSend()
        {
            MdbDataBlock smdblk = new MdbDataBlock(true);
            MdbDataBlock rmdblk = new MdbDataBlock(true);

            smdblk.DataLength = 4;
            smdblk.Data[0] = (byte)(cashlessaddr | MdbCommands.Expansion);
            smdblk.Data[1] = (byte)MdbCommands.CPOkToSend;
            smdblk.Data[3] = ReceivedFile.Info.Destination;
            smdblk.Data[2] = ReceivedFile.Info.Source;
            lasterr = TalkMdb(smdblk, ref rmdblk);
            return lasterr;
        }
        #endregion
    }
}