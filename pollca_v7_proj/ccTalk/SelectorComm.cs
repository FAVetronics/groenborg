using System;

namespace ccTalk
{
    /// <summary>
    /// wh Coin Selector Communication class.
    /// </summary>
    [Serializable]
    public class SelectorComm : CcTalkComm
    {
        #region Constructor/Destructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// Sets some default values.
        /// </remarks>
        public SelectorComm()
        {
            Address = ACCEPTOR_ADR;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// <param name="basedevice">Instance of the base class<see cref="CcTalkComm"/> were some settings are taken from:</param>
        /// Address, Port and ChecksumType.
        /// </remarks>
        public SelectorComm(CcTalkComm basedevice)
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
            lasterr = base.OpenComm();
            evtctr = -1;
            if (lasterr != CcTalkErrors.Ok)
            {
                CloseComm();
                lasterr = CcTalkErrors.OpenErr;
            }
            return lasterr;

        }

        /// <summary>Maximum number of poll events.</summary>
        public const int MaxPollEvents = 5;

        /// <summary>Customer ID of the device as long integer (if applicable).</summary>
        public long CustomerID
        {
            get { return GetReverseLongResponse(247); }
        }
        /// <summary>
        /// Master Inhibit status
        /// </summary>
        /// <remarks>
        /// Setting it to "true" inhibits acceptance of all coins.
        /// Setting it to "false" enables only acceptance of coins enabled.
        /// </remarks>
        public bool MasterInhibit
        {
            get { return GetMasterInhibit(); }
            set { SetMasterInhibit(value); }
        }

        /// <summary>
        /// Default sorter pat (i.e. cash box)
        /// </summary>
        public int DefaulPath
        {
            get { return GetDefaultPath(); }
            set { SetDefaultPath(value); }
        }

        #region Private variables
        // Constants
        internal const byte ACCEPTOR_ADR = 2;
        #endregion

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
                //if (lasterr != whCcTalkErrors.Ok) return lasterr;
            }
            return lasterr;
        }
        /// <summary>
        /// Retrieves the current inhibit status, sorter path and override status of the 16 coins.
        /// </summary>
        /// <param name="currvals">Array of <see cref="SelCoinStatus"/> - must have at least 16 elements.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetCoinStates(ref SelCoinStatus[] currvals)
        {
            int i;
            ushort inh, msk;

            if (currvals.Length < 16) return CcTalkErrors.WrongParameter;

            inh = (ushort)GetLongResponse(230);
            if (lasterr != CcTalkErrors.Ok) return lasterr;
            //ovr = (ushort)GetLongResponse(221);
            //if (lasterr != whCcTalkErrors.Ok) return lasterr;

            msk = 0x0001;
            for (i = 0; i < 16; i++)
            {
                currvals[i].Inhibit = (inh & msk) != 0;
                currvals[i].Override = false; // (ovr & msk) == 0;
                msk <<= 1;
                currvals[i].SorterPath = GetSorterPath(i);
                if (lasterr != CcTalkErrors.Ok) return lasterr;
            }
            return lasterr;
        }

        /// <summary>
        /// Retrieves override status for all sorter path.
        /// </summary>
        /// <param name="sorteroverrides">Array of bool, must have at least 8 elements.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetSorterOverride(ref bool[] sorteroverrides)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            if (sorteroverrides.Length < 8) return CcTalkErrors.WrongParameter;
            sdta.DataLength = 0;
            sdta.Header = 221;
            lasterr = TalkCc(sdta, ref rdta);
            for (int i = 0; i < 8; i++)
            {
                if ((lasterr == CcTalkErrors.Ok) && (rdta.DataLength > 0))
                    sorteroverrides[i] = (rdta.Data[0] & (0x01 << i)) == 0;
                else
                    sorteroverrides[i] = false;
            }
            return lasterr;
        }
        /// <summary>
        /// Sets the override status of up to 8 paths.
        /// </summary>
        /// <remarks>
        /// On power on override status for all paths is set to "false". 
        /// </remarks>
        /// <param name="sorteroverrides">
        /// Array of bool - must have at least 8 elements.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetSorterOverride(bool[] sorteroverrides)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            if (sorteroverrides.Length < 8) return CcTalkErrors.WrongParameter;
            sdta.Data[0] = 0xff;
            for (int i = 0; i < 8; i++)
            {
                if (sorteroverrides[i]) sdta.Data[0] = (byte)(sdta.Data[0] & ~(0x01 << i));
            }
            sdta.DataLength = 1;
            sdta.Header = 222;
            lasterr = TalkCc(sdta, ref rdta);
            return lasterr;
        }

        /// <summary>
        /// Sets the inhibit status of the 16 coins.
        /// </summary>
        /// <remarks>
        /// On power on all coins are inhibited. 
        /// Use <see cref="GetCoinValues"/> to identify available coin values and currencies
        /// and <see cref="GetCoinStates"/> to determine current inhibit status.
        /// </remarks>
        /// <param name="currvals">
        /// Array of <see cref="SelCoinStatus"/> - must have at least 16 elements.
        /// Only then <see cref="SelCoinStatus.Inhibit"/> field will be used. 
        /// If it is set to true the coin will be enabled. This inconsistency is due to ccTalk terminology.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetCoinInhibit(SelCoinStatus[] currvals)
        {
            int i;
            uint inh, msk;
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            if (currvals.Length < 16) return CcTalkErrors.WrongParameter;
            msk = 0x0001;
            inh = 0x0000;
            for (i = 0; i < 16; i++)
            {
                if (currvals[i].Inhibit) inh |= msk;
                msk <<= 1;
            }
            sdta.DataLength = 0;        // für SR5 & Co.: Alle Bänke aktivieren.
            sdta.Header = 179;
            sdta.Data[0] = 1;
            TalkCc(sdta, ref rdta);     // Fehler wird nicht beachtet.

            sdta.DataLength = 2;
            sdta.Header = 231;
            sdta.Data[0] = (byte)(inh & 0x00ff);
            sdta.Data[1] = (byte)((inh >> 8) & 0x00ff);
            lasterr = TalkCc(sdta, ref rdta);
            return lasterr;
        }
        /// <summary>
        /// Sets the same inhibit status for all 16 coins.
        /// </summary>
        /// <remarks>
        /// On power on all coins are inhibited. 
        /// </remarks>
        /// <param name="coinenable">
        /// The status that will be set for all coins. If true all coins will be enabled.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors CoinInhibit(bool coinenable)
        {
            SelCoinStatus[] currvals = new SelCoinStatus[16];
            for (int i = 0; i < 16; i++) currvals[i].Inhibit = coinenable;
            return SetCoinInhibit(currvals);
        }

        /// <summary>
        /// Activates one or more solenoids for app. 500ms.
        /// </summary>
        /// <remarks>
        /// May be useful to free a coin trapped by the solenoid. 
        /// </remarks>
        /// <param name="solenoids">
        /// A bit pattern selecting the solenoids to be activated. Bit 0 allways represent the accept gate solenoid.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors TestSolenoids(byte solenoids)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 1;
            sdta.Header = 240;
            sdta.Data[0] = solenoids;
            rcvtot += 500;
            lasterr = TalkCc(sdta, ref rdta);
            rcvtot -= 500;
            return lasterr;
        }

        /// <summary>
        /// Sets the override status of the 16 coins.
        /// </summary>
        /// <remarks>
        /// On power on override status for all coins is set to "false". 
        /// Use <see cref="GetCoinValues"/> to identify available coin values and currencies
        /// and <see cref="GetCoinStates"/> to determine current override status.
        /// </remarks>
        /// <param name="currvals">
        /// Array of <see cref="SelCoinStatus"/> - must have at least 16 elements.
        /// Only then <see cref="SelCoinStatus.Override"/> field will be used.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetCoinOverride(SelCoinStatus[] currvals)
        {
            int i;
            uint ovr, msk;
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            if (currvals.Length < 16) return CcTalkErrors.WrongParameter;
            msk = 0x0001;
            ovr = 0xffff;
            for (i = 0; i < 16; i++)
            {
                if (currvals[i].Override) ovr &= ~msk;
                msk <<= 1;
            }
            sdta.DataLength = 2;
            sdta.Header = 222;
            sdta.Data[0] = (byte)(ovr & 0x00ff);
            sdta.Data[1] = (byte)((ovr >> 8) & 0x00ff);
            lasterr = TalkCc(sdta, ref rdta);
            return lasterr;
        }
        /// <summary>
        /// Sets the sorter path of the 16 coins.
        /// </summary>
        /// <remarks>
        /// On power on the sorter path for all coins is set to a default value.
        /// Use <see cref="GetCoinValues"/> to identify available coin values and currencies
        /// and <see cref="GetCoinStates"/> to determine current sorter paths.
        /// </remarks>
        /// <param name="currvals">
        /// Array of <see cref="SelCoinStatus"/>, must have at least 16 elements.
        /// Only then <see cref="SelCoinStatus.SorterPath"/> field will be used.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetCoinSorterPaths(SelCoinStatus[] currvals)
        {
            int i;
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);
            for (i = 0; i < 16; i++)
            {
                sdta.DataLength = 2;
                sdta.Header = 210;
                sdta.Data[0] = (byte)(i + 1);
                sdta.Data[1] = currvals[i].SorterPath;
                lasterr = TalkCc(sdta, ref rdta);
                if (lasterr != CcTalkErrors.Ok) break;
            };
            return lasterr;
        }

        // A coin was inserted
        internal SelPollEvent[] coinevents = new SelPollEvent[] {
            SelPollEvent.Coin,
            SelPollEvent.CoinInhibit,
            SelPollEvent.CoinInhibit00,
            SelPollEvent.CoinInhibit01,
            SelPollEvent.CoinInhibit02,
            SelPollEvent.CoinInhibit03,
            SelPollEvent.CoinInhibit04,
            SelPollEvent.CoinInhibit05,
            SelPollEvent.CoinInhibit06,
            SelPollEvent.CoinInhibit07,
            SelPollEvent.CoinInhibit08,
            SelPollEvent.CoinInhibit09,
            SelPollEvent.CoinInhibit10,
            SelPollEvent.CoinInhibit11,
            SelPollEvent.CoinInhibit12,
            SelPollEvent.CoinInhibit13,
            SelPollEvent.CoinInhibit14,
            SelPollEvent.CoinInhibit15,
            SelPollEvent.CoinReject,
        };

        /// <summary>
        /// Polls the device to retrieve current events.
        /// </summary>
        /// <remarks>
        /// Up to <see cref="SelectorComm.MaxPollEvents"/> events can be retrieved. Poll should be performed app. every
        /// 200msecs otherwise events especially credit may be lost.
        /// If <see cref="SelPollResponse.Status"/> == <see cref="SelPollEvent.Coin"/> 
        /// use <see cref="SelPollResponse.CoinIndex"/>
        /// to retrieve further information from the arrays returned by <see cref="GetCoinValues"/> and
        /// <see cref="GetCoinStates"/>.
        /// </remarks>remarks>
        /// <param name="pollresps">
        /// Array of <see cref="SelPollResponse"/>, must have at least 5 elements.
        /// </param>
        /// <param name="evts">
        /// Number of events since last poll, the first "evts" elements of pollresps are valid.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors PollSelector(ref SelPollResponse[] pollresps, out int evts)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            evts = 0;
            if (pollresps.Length < 5) return CcTalkErrors.WrongParameter;
            if (EncryptionSupport.CommandLevel == CcTalkCryptography.DES)
            {
#if DES_SUPPORT
                byte challenge = (byte)new Random().Next(256);
                sdta.DataLength = 1;
                sdta.Data[0] = challenge;
                sdta.Header = 112;
                lasterr = TalkCc(sdta, ref rdta);
                if (lasterr != whCcTalkErrors.Ok) return lasterr;
                if (rdta.DataLength != 16)  // Wrong data format
                {
                    lasterr = whCcTalkErrors.DataFormat;
                    return lasterr;
                }
                lasterr = decryptdesblock(ref rdta, challenge);
                if (lasterr != whCcTalkErrors.Ok) return lasterr;
#else
                lasterr = CcTalkErrors.UnsupportedEncryption;
#endif
            }
            else
            {
                sdta.DataLength = 0;
                sdta.Header = 229;
                lasterr = TalkCc(sdta, ref rdta);
                if (lasterr != CcTalkErrors.Ok) return lasterr;
                if (rdta.DataLength < 11)  // Wrong data format
                {
                    lasterr = CcTalkErrors.DataFormat;
                    return lasterr;
                }
            }

            if ((rdta.Data[0] == 0) && (evtctr < 0))      // Just reset
            {
                evtctr = rdta.Data[0];
                pollresps[0].Status = SelPollEvent.Reset;
                pollresps[0].CoinIndex = -1;
                pollresps[0].CoinPath = -1;
                evts = 1;
                return lasterr;
            }
            if (rdta.Data[0] >= evtctr) // Get number of events since last poll
                evts = rdta.Data[0] - evtctr;
            else evts = (255 - evtctr) + rdta.Data[0];
            evtctr = rdta.Data[0];
            if (evts > 5)               // More than 5 events since last poll
            {
                lasterr = CcTalkErrors.EventsLost;
                evts = 5;
            }
            for (int i = 0; (i < evts) && (i < pollresps.GetUpperBound(0)); i++)
            {
                if (rdta.Data[i * 2 + 1] == 0)
                {
                    try
                    {
                        pollresps[i].Status = (SelPollEvent)rdta.Data[i * 2 + 2];
                    }
                    catch (System.Exception) { pollresps[i].Status = SelPollEvent.Unknown; }
                    pollresps[i].CoinIndex = -1;
                    pollresps[i].CoinPath = -1;
                }
                else
                {
                    pollresps[i].Status = SelPollEvent.Coin;
                    pollresps[i].CoinIndex = (rdta.Data[i * 2 + 1] - 1) & 0x000f;
                    pollresps[i].CoinPath = rdta.Data[i * 2 + 2];
                }
                pollresps[i].CoinInserted = false;
                for (int j = 0; j < coinevents.Length; j++)
                {
                    if (pollresps[i].Status == coinevents[j])
                    {
                        pollresps[i].CoinInserted = true;
                        break;
                    }
                }
            }
            return lasterr;
        }

        /// <summary>
        /// Polls the device to retrieve pre coin info.
        /// </summary>
        /// <remarks>
        /// The intention is to give e.g. a sorting equipment more time to get ready for the coin.
        /// </remarks>
        /// <param name="pollresp">
        /// Holding the information about an identified but not yet accepted coins.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors PollPreCoinInfo(ref SelPollResponse pollresp)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            pollresp.CoinInserted = false;
            pollresp.CoinIndex = -1;
            pollresp.CoinPath = -1;
            pollresp.Status = SelPollEvent.Unknown;

            sdta.DataLength = 1;
            sdta.Header = 100;
            sdta.Data[0] = 8;       // ccTalk Vor"impuls"
            lasterr = TalkCc(sdta, ref rdta);

            switch (lasterr)
            {
                case CcTalkErrors.Ok:
                    if (rdta.DataLength < 2)
                    {
                        pollresp.Status = SelPollEvent.Null;
                        pollresp.CoinInserted = false;
                    }
                    else
                    {
                        pollresp.CoinInserted = true;
                        if (rdta.Data[0] == 0)
                        {
                            try
                            {
                                pollresp.Status = (SelPollEvent)rdta.Data[1];
                            }
                            catch (System.Exception) { pollresp.Status = SelPollEvent.Unknown; }
                        }
                        else
                        {
                            pollresp.Status = SelPollEvent.Coin;
                            pollresp.CoinIndex = (rdta.Data[0] - 1) & 0x000f;
                            pollresp.CoinPath = rdta.Data[1];
                        }
                    }
                    break;
                case CcTalkErrors.RcvTimout:
                    lasterr = CcTalkErrors.UnSupported;
                    break;
            }

            return lasterr;
        }

        /// <summary>
        /// Sets the state of the escrow (if connected).
        /// If state == <see cref="EscrowState.Collect"/> or state == <see cref="EscrowState.Return"/>
        /// the flaps will open for a fixed time span allowing the coins to drop.
        /// </summary>
        /// <param name="state">State<see cref="EscrowState"/> of the flaps.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetupEscrow(EscrowState state)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 1;
            sdta.Header = 240;
            switch (state)
            {
                case EscrowState.Collect:
                    sdta.Data[0] = 4;
                    lasterr = TalkCc(sdta, ref rdta);
                    break;
                case EscrowState.Return:
                    sdta.Data[0] = 2;
                    lasterr = TalkCc(sdta, ref rdta);
                    break;
                default:
                    lasterr = CcTalkErrors.WrongParameter;
                    break;
            }
            return lasterr;
        }

        /// <summary>
        /// Sets the price to be withdrawn from the medium for an EMP with NFC module.
        /// If state == <see cref="EscrowState.Collect"/> or state == <see cref="EscrowState.Return"/>
        /// the flaps will open for a fixed time span allowing the coins to drop.
        /// </summary>
        /// <param name="pricesetting">Price Setting<see cref="SelCoinPriceSetting"/>.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetupPrice(SelCoinPriceSetting pricesetting)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            int iprice = (int)(pricesetting.Price * 100);

            sdta.DataLength = 1;
            sdta.Header = 100;
            sdta.DataLength = 4;
            sdta.Data[0] = 9;           // Subheader für Preiseinstellung
            sdta.Data[1] = (byte)iprice;
            sdta.Data[2] = (byte)(iprice >> 8);
            sdta.Data[3] = 0x00;
            if (pricesetting.CashlessPaymentBlocking)
            {
                sdta.Data[3] |= 0x01;
            }
            if (pricesetting.MachineOccupied)
            {
                sdta.Data[3] |= 0x02;
            }
            if (pricesetting.ServiceModeActive)
            {
                sdta.Data[3] |= 0x04;
            }
            lasterr = TalkCc(sdta, ref rdta);

            return lasterr;
        }

        #region Private methodes
        // Others
        private void SetMasterInhibit(bool mival)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            //if (!mival)                     // für SR5 & Co.: Alle Bänke aktivieren.
            //{
            //    sdta.DataLength = 0;
            //    sdta.Header = 179;
            //    sdta.Data[0] = 1;
            //    TalkCc(sdta, ref rdta);     // Fehler wird nicht beachtet.
            //}
            sdta.DataLength = 1;
            sdta.Header = 228;
            if (mival)
                sdta.Data[0] = 0;
            else sdta.Data[0] = 1;
            lasterr = TalkCc(sdta, ref rdta);
        }
        private bool GetMasterInhibit()
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 0;
            sdta.Header = 227;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr != CcTalkErrors.Ok) return false;
            if (rdta.DataLength < 1) return false;
            return ((rdta.Data[0] & 0x01) == 0);
        }

        private void SetDefaultPath(int defpath)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 1;
            sdta.Header = 189;
            sdta.Data[0] = (byte)defpath;
            lasterr = TalkCc(sdta, ref rdta);
        }
        private int GetDefaultPath()
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 0;
            sdta.Header = 188;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr != CcTalkErrors.Ok) return -1;
            if (rdta.DataLength < 1) return -1;
            return rdta.Data[0];
        }
        #endregion
    }
}