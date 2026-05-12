using System;
using System.Threading;

namespace ccTalk
{
    /// <summary>
    /// wh Change Giver Communication class .
    /// This a MDB device connected via CCT 900. 
    /// It will appear at special ccTalk address <see cref="CcTalkComm.MdbChangeGiverAddress"/>.
    /// </summary>
    [Serializable]
    public class ChangeGiverComm : CcTalkComm
    {
        #region Constructor/Destructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// Sets some default values.
        /// </remarks>
        public ChangeGiverComm()
        {
            Address = MdbAddresses.CcChangeGiver; ;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// <param name="basedevice">Instance of the base class<see cref="CcTalkComm"/> were some settings are taken from:</param>
        /// Address, Port and ChecksumType.
        /// </remarks>
        public ChangeGiverComm(CcTalkComm basedevice)
        {
            Address = MdbAddresses.CcChangeGiver;
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
            lasterr = base.OpenComm();
            return lasterr;

        }

        /// <summary>Maximum number of poll events</summary>
        public const int MaxPollEvents = 16;

        /// <summary>
        /// Retrieves values and currency IDs of the 16 coins.
        /// </summary>
        /// <param name="currvals">Array of <see cref="CoinValue"/>, must have at least 16 elements.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetCoinValues(ref CoinValue[] currvals)
        {
            MdbDataBlock smdb = new MdbDataBlock(true);
            MdbDataBlock rmdb = new MdbDataBlock(true);
            string ccid = "XX";

            if (currvals.Length < 16) return CcTalkErrors.WrongParameter;

            ChangerSetupInstance.InitStructure();
            smdb.DataLength = 1;
            smdb.Data[0] = MdbAddresses.MdbChangeGiver | MdbCommands.Setup;
            lasterr = TalkMdb(smdb, ref rmdb);
            if (!ChangerSetupInstance.GetFromBuffer(rmdb)) lasterr = CcTalkErrors.DataFormat;
            if (lasterr != CcTalkErrors.Ok) return lasterr;

            ccid = GetCcTalkID(ChangerSetupInstance.CountryHi * 256 + ChangerSetupInstance.CountryLo);
            if (ccid.Trim() == "XX")
            {
                ccid = GetCcTalkID(ChangerSetupInstance.CountryHi + ChangerSetupInstance.CountryLo * 256);
            }
            for (int i = 0; i < 16; i++)
            {
                switch (ChangerSetupInstance.CoinCredit[i])
                {
                    case 254:
                    case 255:
                        currvals[i].IntValue = ChangerSetupInstance.CoinCredit[i] - 253;
                        currvals[i].Value = currvals[i].IntValue;
                        currvals[i].ID = "TK";
                        break;
                    case 241:
                    case 242:
                    case 243:
                    case 244:
                    case 245:
                    case 246:
                    case 247:
                    case 248:
                    case 249:
                    case 250:
                    case 251:
                    case 252:
                        currvals[i].IntValue = ChangerSetupInstance.CoinCredit[i] - 240;
                        currvals[i].Value = currvals[i].IntValue / Math.Pow(10, ChangerSetupInstance.Decimals);
                        currvals[i].ID = "TK";
                        break;
                    default:
                        currvals[i].IntValue = ChangerSetupInstance.CoinCredit[i] * ChangerSetupInstance.Scaling;
                        currvals[i].Value = currvals[i].IntValue / Math.Pow(10, ChangerSetupInstance.Decimals);
                        currvals[i].ID = ccid;
                        break;
                }
            }

            return lasterr;
        }

        /// <summary>
        /// Retrieves the tube status of the 16 coins.
        /// </summary>
        /// <param name="currtubes">Array of <see cref="CoinTube"/>, must have at least 16 elements.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetCoinTubes(ref CoinTube[] currtubes)
        {
            MdbDataBlock smdb = new MdbDataBlock(true);
            MdbDataBlock rmdb = new MdbDataBlock(true);

            if (currtubes.Length < 16) return CcTalkErrors.WrongParameter;

            ChangerSetupInstance.InitStructure();
            smdb.DataLength = 1;
            smdb.Data[0] = MdbAddresses.MdbChangeGiver | MdbCommands.Setup;
            lasterr = TalkMdb(smdb, ref rmdb);
            if (!ChangerSetupInstance.GetFromBuffer(rmdb)) lasterr = CcTalkErrors.DataFormat;
            if (lasterr != CcTalkErrors.Ok) return lasterr;

            ChangerTubeStatusInstance.InitStructure();
            smdb.DataLength = 1;
            smdb.Data[0] = MdbAddresses.MdbChangeGiver | MdbCommands.TubeStatus;
            lasterr = TalkMdb(smdb, ref rmdb);
            if (!ChangerTubeStatusInstance.GetFromBuffer(rmdb)) lasterr = CcTalkErrors.DataFormat;
            if (lasterr != CcTalkErrors.Ok) return lasterr;

            for (int i = 0; i < 16; i++)
            {
                currtubes[i].Present = ((ChangerSetupInstance.RoutingHi * 256 + ChangerSetupInstance.RoutingLo) & (0x0001 << i)) != 0;
                if (currtubes[i].Present)
                {
                    currtubes[i].Coins = ChangerTubeStatusInstance.TubeStatus[i];
                    currtubes[i].Full = ((ChangerTubeStatusInstance.TubeFullHi * 256 + ChangerTubeStatusInstance.TubeFullLo) & (0x0001 << i)) != 0;
                    currtubes[i].Error = false;
                    if (currtubes[i].Full && (currtubes[i].Coins == 0))
                    {
                        currtubes[i].Full = false;
                        currtubes[i].Error = true;
                    }
                }
                else
                {
                    currtubes[i].Coins = 0;
                    currtubes[i].Full = false;
                    currtubes[i].Error = false;
                }
            }

            return lasterr;
        }

        /// <summary>
        /// Retrieves the extended tube status of the 16 coins.
        /// Not supported by all brand
        /// </summary>
        /// <param name="currtubes">Array of <see cref="CoinTubeEx"/>, must have at least 16 elements.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetCoinTubes(ref CoinTubeEx[] CurrTubesEx)
        {
            MdbDataBlock smdb = new MdbDataBlock(true);
            MdbDataBlock rmdb = new MdbDataBlock(true);

            if (CurrTubesEx.Length < 16)
            {
                lasterr = CcTalkErrors.WrongParameter;
            }

            if (lasterr == CcTalkErrors.Ok)
            {
                CoinTube[] currtubes = new CoinTube[16];
                lasterr = GetCoinTubes(ref currtubes);
                if (lasterr == CcTalkErrors.Ok)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        CurrTubesEx[i].Present = currtubes[i].Present;
                        CurrTubesEx[i].Coins = currtubes[i].Coins;
                        CurrTubesEx[i].Full = currtubes[i].Full;
                        CurrTubesEx[i].Error = currtubes[i].Error;
                    }
                }
            }
            if (lasterr == CcTalkErrors.Ok)
            {
                smdb.DataLength = 2;
                smdb.Data[0] = MdbAddresses.MdbChangeGiver | MdbCommands.Expansion;
                smdb.Data[1] = MdbCommands.TubeStatusEx;
                if (TalkMdb(smdb, ref rmdb) == CcTalkErrors.Ok)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        CurrTubesEx[i].SecureCoins = rmdb.Data[i + 16];
                    }
                }
                else
                {
                    for (int i = 0; i < 16; i++)
                    {
                        CurrTubesEx[i].SecureCoins = -1;
                    }
                }
            }

            return lasterr;
        }

        /// <summary>
        /// Sets the inhibit status of the 16 coins.
        /// </summary>
        /// <remarks>
        /// On power on all coins are inhibited. 
        /// Use <see cref="GetCoinValues"/> to identify available coin values and currencies.
        /// </remarks>
        /// <param name="currvals">
        /// Array of <see cref="SelCoinStatus"/> - must have at least 16 elements.
        /// Only then <see cref="SelCoinStatus.Inhibit"/> field will be used.
        /// </param>
        /// <param name="mandispenabled">
        /// If set to false manual dispense using inventory keys is disabled.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetCoinInhibit(SelCoinStatus[] currvals, bool mandispenabled)
        {
            MdbDataBlock smdb = new MdbDataBlock(true);
            MdbDataBlock rmdb = new MdbDataBlock(true);

            if (currvals.Length < 16) return CcTalkErrors.WrongParameter;

            UInt16 inhwd = 0x0000;
            for (int i = 0; i < 16; i++)
                if (currvals[i].Inhibit)
                    inhwd |= (UInt16)(0x0001 << i);

            smdb.DataLength = 5;
            smdb.Data[0] = MdbAddresses.MdbChangeGiver | MdbCommands.CoinType;
            smdb.Data[1] = (byte)(inhwd >> 8);
            smdb.Data[2] = (byte)(inhwd);
            if (mandispenabled)
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
        /// <summary>
        /// Sets the same inhibit status for all 16 coins.
        /// </summary>
        /// <remarks>
        /// On power on all coins are inhibited. 
        /// </remarks>
        /// <param name="coinenable">
        /// The status that will be set for all coins. If true all coins will be enabled.
        /// </param>
        /// <param name="mandispenabled">
        /// If set to false manual dispense using inventory keys is disabled.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetCoinInhibit(bool coinenable, bool mandispenabled)
        {
            SelCoinStatus[] currvals = new SelCoinStatus[16];
            for (int i = 0; i < 16; i++)
            {
                currvals[i].Inhibit = coinenable;
            }
            return SetCoinInhibit(currvals, mandispenabled);
        }

        public CcTalkErrors PayoutCoins(ref int[] coincounts)
        {
            int pocnt, potmot, evts;
            bool pngot;
            ChgPollResponse[] pollresp = new ChgPollResponse[16];
            CcTalkErrors res = CcTalkErrors.Unknown;


            if (coincounts.Length < 16) return CcTalkErrors.WrongParameter;

            for (int i = 0; i < 16; i++)
            {
                if (coincounts[i] > 0)
                {
                    do
                    {
                        if (coincounts[i] > 15) pocnt = 15; else pocnt = coincounts[i];
                        if ((lasterr = ChangeGiverPayout(i, pocnt)) == CcTalkErrors.Ok)
                        {
                            coincounts[i] -= pocnt;
                            potmot = pocnt * 2000;
                            do
                            {
                                Thread.Sleep(200);
                                potmot -= 200;
                                pngot = false;
                                res = PollChangeGiver(ref pollresp, out evts);
                                if (res == CcTalkErrors.Ok)
                                {
                                    for (int p = 0; p < evts; p++)
                                    {
                                        switch (pollresp[p].Status)
                                        {
                                            case SelPollEvent.PayoutBusy:
                                                pngot = true;
                                                break;
                                            case SelPollEvent.TubeJam:
                                                pngot = false;
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    pngot = true;
                                }
                            }
                            while (pngot && (potmot > 0));
                            if (pngot) lasterr = CcTalkErrors.PayoutExceeded;
                        }
                    }
                    while ((coincounts[i] > 0) && (lasterr == CcTalkErrors.Ok));
                }
                if (lasterr != CcTalkErrors.Ok) break;
            }

            return lasterr;
        }

        /// <summary>
        /// Polls the device to retrieve current events.
        /// </summary>
        /// <remarks>
        /// Up to <see cref="ChangeGiverComm.MaxPollEvents"/> events can be retrieved. Poll should be performed app. every
        /// 200msecs otherwise events especially credit may be lost.
        /// If <see cref="ChgPollResponse.Status"/> == <see cref="ChgPollResponse"/> use <see cref="ChgPollResponse.CoinIndex"/>
        /// to retrieve further information from the arrays returned by <see cref="GetCoinValues"/>.
        /// The <see cref="ChgPollResponse.Routing"/> field indicates the routing of the coin. If the coin is
        /// routed to a tube the <see cref="ChgPollResponse.Count"/> field holds the new contens of the tube.
        /// </remarks>
        /// <param name="pollresps">
        /// Array of <see cref="ChgPollResponse"/>, must have at least <see cref="ChangeGiverComm.MaxPollEvents"/> elements
        /// </param>
        /// <param name="evts">
        /// Number of events since last poll, the first "evts" elements of pollresps are valid.
        /// </param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors PollChangeGiver(ref ChgPollResponse[] pollresps, out int evts)
        {
            MdbDataBlock smdb = new MdbDataBlock(true);
            MdbDataBlock rmdb = new MdbDataBlock(true);

            evts = 0;
            if (pollresps.Length < MaxPollEvents) return CcTalkErrors.WrongParameter;

            smdb.DataLength = 1;
            smdb.Data[0] = MdbAddresses.MdbChangeGiver | MdbCommands.Poll;
            lasterr = TalkMdb(smdb, ref rmdb);
            if (lasterr != CcTalkErrors.Ok) return lasterr;

            int respidx = 0;
            while (respidx < rmdb.DataLength)
            {
                pollresps[evts].Count = 0;
                pollresps[evts].Status = SelPollEvent.Unknown;
                pollresps[evts].Routing = CoinRouting.Unknown;

                if (((rmdb.Data[respidx] & 0x80) == 0x80) || ((rmdb.Data[respidx] & 0xc0) == 0x40)) // Münze
                {
                    pollresps[evts].Status = SelPollEvent.Coin;
                    pollresps[evts].CoinIndex = rmdb.Data[respidx] & 0x0f;
                    if ((rmdb.Data[respidx] & 0xc0) == 0x40)    // Deposited
                    {

                        switch (rmdb.Data[respidx] & 0x30)
                        {
                            case 0x00:
                                pollresps[evts].Routing = CoinRouting.CashBox;
                                break;
                            case 0x10:
                                pollresps[evts].Routing = CoinRouting.Tube;
                                break;
                            case 0x20:
                                pollresps[evts].Routing = CoinRouting.Unknown;
                                break;
                            case 0x30:
#if DEBUG
                                pollresps[evts].Status = (SelPollEvent)((int)SelPollEvent.CoinInhibit00 + (rmdb.Data[respidx] & 0x0f));
#else
                                pollresps[evts].Routing = CoinRouting.Reject;
#endif
                                break;
                        }
                    }
                    if ((rmdb.Data[respidx] & 0x80) == 0x80)    // Dispensed Manually
                    {
                        pollresps[evts].Routing = CoinRouting.Dispense;
                        pollresps[evts].Count = (rmdb.Data[respidx] & 0x70) >> 4;
                    }
                    respidx++;
                    if (pollresps[evts].Routing == CoinRouting.Tube) pollresps[evts].Count = rmdb.Data[respidx];
                }
                else
                {
                    if ((rmdb.Data[respidx] & 0xe0) == 0x20)    // Slug
                    {
                        pollresps[evts].Status = SelPollEvent.CoinReject;
                    }
                    else
                    {
                        pollresps[evts].Status = TranslateChangerPoll(rmdb.Data[respidx]);
                    }
                }
                evts++;
                respidx++;
            }

            return lasterr;
        }

    }
}