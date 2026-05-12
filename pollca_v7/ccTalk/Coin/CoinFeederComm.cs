using System;

namespace ccTalk
{
    /// <summary>
    /// wh Coin Feeder Communication class.
    /// </summary>
    [Serializable]
    public class CoinFeederComm : CcTalkComm
    {
        #region Constructor/Destructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// Sets some default values.
        /// </remarks>
        public CoinFeederComm()
        {
            Address = COINFEEDER_ADR;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// <param name="basedevice">Instance of the base class<see cref="CcTalkComm"/> were some settings are taken from:</param>
        /// Address, Port and ChecksumType.
        /// </remarks>
        public CoinFeederComm(CcTalkComm basedevice)
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

            if ((lasterr == CcTalkErrors.Ok) && !this.IsSupported(this.Manufacturer))
            {
                CloseComm();
                lasterr = CcTalkErrors.UnSupported;
            }
            return lasterr;

        }

        #region Private variables
        // Constants
        private const byte COINFEEDER_ADR = 140;
        #endregion

        /// <summary>
        /// Master Inhibit status
        /// </summary>
        /// <remarks>
        /// Setting it to "true" disables the eject function.
        /// Setting it to "false" enables the eject function. 
        /// </remarks>
        public bool MasterInhibit
        {
            get
            {
                CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

                sdta.DataLength = 0;
                sdta.Header = 21;
                lasterr = TalkCc(sdta, ref rdta);
                if (rdta.DataLength < 4)
                {
                    return false;
                }
                else
                {
                    return (CoinFeederStatus)rdta.Data[1] == CoinFeederStatus.Locked;
                }
            }
            set
            {
                CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

                sdta.DataLength = 0;
                if (value)
                    sdta.Header = 25;
                else
                    sdta.Header = 26;
                lasterr = TalkCc(sdta, ref rdta);
            }
        }

        /// <summary>
        /// Restarts the sequence control of the feeder.
        /// </summary>
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
        /// Retrieves the current status of the feeder.
        /// </summary>
        /// <param name="pollstatus">Returns the complete status information of the coin feeder.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors PollFeeder(ref CoinFeederPollStatus pollstatus)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);
            pollstatus.CapStatus = CapStatus.Unknown;

            sdta.DataLength = 0;
            sdta.Header = 21;
            lasterr = TalkCc(sdta, ref rdta);
            if (rdta.DataLength < 4)
            {
                lasterr = CcTalkErrors.DataFormat;
                pollstatus.Status = CoinFeederStatus.Unknown;
                pollstatus.StatusFlags = CoinFeederStatusFlags.Nothing;
                pollstatus.ErrorFlags = CoinFeederErrorFlags.Nothing;
            }
            else
            {
                pollstatus.Status = (CoinFeederStatus)rdta.Data[0];
                pollstatus.StatusFlags = (CoinFeederStatusFlags)rdta.Data[1];
                pollstatus.ErrorFlags = (CoinFeederErrorFlags)(rdta.Data[2] + rdta.Data[3] * 256);
                lasterr = CapControl(CapCommand.GetState, out pollstatus.CapStatus);
            }
            return lasterr;
        }

        /// <summary>
        /// Ejects all coins in the feeder.
        /// </summary>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors EjectCoins()
        {
            /*
                Der Header 24 wurde gestrichen. Der folgende Programmcode ist daher hinfällig.
            */
            /*
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 0;
            sdta.Header = 24;
            lasterr = TalkCc(sdta, ref rdta);
            return lasterr;
             */
            return EjectCoins(127);
        }
        /// <summary>
        /// Ejects a certain number of coins.
        /// </summary>
        /// <param name="coincount">Number of coins to be ejected. Maximum is 255.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors EjectCoins(int coincount)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            if ((coincount > 127) || (coincount < 0))
            {
                lasterr = CcTalkErrors.WrongParameter;
            }
            else
            {
                sdta.DataLength = 1;
                sdta.Data[0] = (byte)coincount;
                sdta.Header = 23;
                lasterr = TalkCc(sdta, ref rdta);
            }
            return lasterr;
        }

        /// <summary>
        /// Starts one revolution of the coin feeder.
        /// </summary>
        /// <param name="direction">Direction of the forced move.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors ForcedMove(whCoinFeederMove direction)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 0;
            lasterr = CcTalkErrors.WrongParameter;
            switch (direction)
            {
                case whCoinFeederMove.Forward:
                    lasterr = CcTalkErrors.Ok;
                    sdta.Header = 27;
                    break;
                case whCoinFeederMove.Reverse:
                    lasterr = CcTalkErrors.Ok;
                    sdta.Header = 28;
                    break;
            }
            if (lasterr != CcTalkErrors.WrongParameter)
                lasterr = TalkCc(sdta, ref rdta);
            return lasterr;
        }

        /// <summary>
        /// Sets the status of the external solenoid (if present). 
        /// </summary>
        /// <param name="setting">Setting of the solenoid.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetSolenoid(SolenoidSetting setting)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 0;
            lasterr = CcTalkErrors.WrongParameter;
            switch (setting)
            {
                case SolenoidSetting.Active:
                    lasterr = CcTalkErrors.Ok;
                    sdta.Header = 29;
                    break;
                case SolenoidSetting.Inactive:
                    lasterr = CcTalkErrors.Ok;
                    sdta.Header = 30;
                    break;
            }
            if (lasterr != CcTalkErrors.WrongParameter)
                lasterr = TalkCc(sdta, ref rdta);
            return lasterr;
        }

        /// <summary>
        /// Starts one revolution of the motor reject (if present).
        /// </summary>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors StartMotorReject()
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 0;
            sdta.Header = 31;
            lasterr = TalkCc(sdta, ref rdta);
            return lasterr;
        }

        /// <summary>
        /// Retrieves the current status of the cap and may perform cap operations.
        /// </summary>
        /// <param name="command">The cap command.</param>
        /// <param name="status">Returns the current cap status.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors CapControl(CapCommand command, out CapStatus status)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            status = CapStatus.Unknown;
            sdta.DataLength = 1;
            sdta.Data[0] = (byte)command;
            sdta.Header = 33;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                if (rdta.DataLength < 1)
                    lasterr = CcTalkErrors.DataFormat;
                else
                    status = (CapStatus)rdta.Data[0];
            }
            return lasterr;
        }
    }
}