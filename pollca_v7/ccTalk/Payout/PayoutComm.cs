using System;
using System.Text;
using System.Threading;

namespace ccTalk
{
    /// <summary>
    /// wh Payout (Hopper) Communication class.
    /// </summary>
    [Serializable]
    public class PayoutComm : CcTalkComm
    {
        #region Constructor/Destructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// Sets some default values.
        /// </remarks>
        public PayoutComm()
        {
            Address = PAYOUT_ADR;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// <param name="basedevice">Instance of the base class<see cref="CcTalkComm"/> were some settings are taken from:</param>
        /// Address, Port and ChecksumType.
        /// </remarks>
        public PayoutComm(CcTalkComm basedevice)
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
        private const byte PAYOUT_ADR = 3;
        #endregion

        /// <summary>
        /// Retrieves the current status of the payout device.
        /// </summary>
        /// <param name="currstat">Struct <see cref="PayoutStatus"/> for payout status information.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetPayoutStatus(ref PayoutStatus currstat)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            currstat.Status = PayoutStatusFlags.Nothing;
            currstat.HighLevelSensor = PayoutSensorStatus.Unknown;
            currstat.LowLevelSensor = PayoutSensorStatus.Unknown;

            sdta.DataLength = 0;
            sdta.Header = 163;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                int stt = 0;
                if (rdta.DataLength > 0)
                {
                    for (int i = 0; i < rdta.DataLength; i++) stt += rdta.Data[i] << (i * 8);
                    currstat.Status |= (PayoutStatusFlags)stt;
                }
                else
                {
                    lasterr = CcTalkErrors.DataFormat;
                }
                Thread.Sleep(10);
            }
            else
            {
                return lasterr;
            }
            sdta.DataLength = 0;
            sdta.Header = 166;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                if (rdta.DataLength > 3)
                {
                    if (rdta.Data[0] == 0) currstat.Status |= PayoutStatusFlags.Reset;
                    currstat.Remaining = rdta.Data[1];
                    currstat.LastPaidout = rdta.Data[2];
                    currstat.LastUnpaid = rdta.Data[3];
                }
                else
                {
                    lasterr = CcTalkErrors.DataFormat;
                }
                Thread.Sleep(10);
            }
            else
            {
                return lasterr;
            }
            sdta.DataLength = 0;
            sdta.Header = 217;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                if (rdta.DataLength > 0)
                {
                    if ((rdta.Data[0] & 0x10) != 0)
                    {
                        if ((rdta.Data[0] & 0x01) == 0)
                            currstat.LowLevelSensor = PayoutSensorStatus.Triggered;
                        else
                            currstat.LowLevelSensor = PayoutSensorStatus.Untriggered;
                    }
                    else
                    {
                        currstat.LowLevelSensor = PayoutSensorStatus.NotSupported;
                    }
                    if ((rdta.Data[0] & 0x20) != 0)
                    {
                        if ((rdta.Data[0] & 0x02) == 0)
                            currstat.HighLevelSensor = PayoutSensorStatus.Triggered;
                        else
                            currstat.HighLevelSensor = PayoutSensorStatus.Untriggered;
                    }
                    else
                    {
                        currstat.HighLevelSensor = PayoutSensorStatus.NotSupported;
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
        /// Retrieves the extended status of the payout device.
        /// </summary>
        /// <param name="currstat">Struct <see cref="PayoutStatusEx"/> for payout status information.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetPayoutStatus(ref PayoutStatusEx currstat)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            currstat.Status = PayoutStatusFlags.Nothing;
            currstat.HighLevelSensor = PayoutSensorStatus.Unknown;
            currstat.LowLevelSensor = PayoutSensorStatus.Unknown;

            sdta.DataLength = 0;
            sdta.Header = 163;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                int stt = 0;
                if (rdta.DataLength > 0)
                {
                    for (int i = 0; i < rdta.DataLength; i++) stt += rdta.Data[i] << (i * 8);
                    currstat.Status |= (PayoutStatusFlags)stt;
                }
                else
                {
                    lasterr = CcTalkErrors.DataFormat;
                }
                Thread.Sleep(10);
            }
            else
            {
                return lasterr;
            }
            sdta.DataLength = 0;
            sdta.Header = 166;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                if (rdta.DataLength > 3)
                {
                    if (rdta.Data[0] == 0) currstat.Status |= PayoutStatusFlags.Reset;
                    currstat.Events = rdta.Data[0];
                    currstat.Remaining = rdta.Data[1];
                    currstat.LastPaidout = rdta.Data[2];
                    currstat.LastUnpaid = rdta.Data[3];
                }
                else
                {
                    lasterr = CcTalkErrors.DataFormat;
                }
                Thread.Sleep(10);
            }
            else
            {
                return lasterr;
            }
            sdta.DataLength = 0;
            sdta.Header = 217;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                if (rdta.DataLength > 0)
                {
                    if ((rdta.Data[0] & 0x10) != 0)
                    {
                        if ((rdta.Data[0] & 0x01) == 0)
                            currstat.LowLevelSensor = PayoutSensorStatus.Triggered;
                        else
                            currstat.LowLevelSensor = PayoutSensorStatus.Untriggered;
                    }
                    else
                    {
                        currstat.LowLevelSensor = PayoutSensorStatus.NotSupported;
                    }
                    if ((rdta.Data[0] & 0x20) != 0)
                    {
                        if ((rdta.Data[0] & 0x02) == 0)
                            currstat.HighLevelSensor = PayoutSensorStatus.Triggered;
                        else
                            currstat.HighLevelSensor = PayoutSensorStatus.Untriggered;
                    }
                    else
                    {
                        currstat.HighLevelSensor = PayoutSensorStatus.NotSupported;
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
        /// Retrieves the current status of a multi coin payout device.
        /// </summary>
        /// <param name="currstat">Struct <see cref="PayoutStatus"/> for payout status information.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetPayoutStatus(ref whPayoutValueStatus currstat)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            currstat.Status = PayoutStatusFlags.Nothing;
            currstat.HighLevelSensor = PayoutSensorStatus.Unknown;
            currstat.LowLevelSensor = PayoutSensorStatus.Unknown;

            // Request hopper polling value
            sdta.DataLength = 0;
            sdta.Header = 133;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                if (rdta.DataLength > 3)
                {
                    if (rdta.Data[0] == 0) currstat.Status |= PayoutStatusFlags.Reset;
                    ispurging = (rdta.Data[1] == 255) && (rdta.Data[2] == 255);
                    if (ispurging)
                    {
                        currstat.Remaining = 0.0;
                        currstat.Status = PayoutStatusFlags.Purging;
                    }
                    else
                    {
                        currstat.Remaining = (rdta.Data[1] + 256 * rdta.Data[2]) / 100.0;
                    }
                    currstat.LastPaidout = (rdta.Data[3] + 256 * rdta.Data[4]) / 100.0;
                    currstat.LastUnpaid = (rdta.Data[5] + 256 * rdta.Data[6]) / 100.0;

                }
                else
                {
                    lasterr = CcTalkErrors.DataFormat;
                }
                Thread.Sleep(10);
            }
            else
            {
                return lasterr;
            }
            if (!ispurging)
            {
                // Test Hopper
                sdta.DataLength = 0;
                sdta.Header = 163;
                lasterr = TalkCc(sdta, ref rdta);
                if (lasterr == CcTalkErrors.Ok)
                {
                    int stt = 0x000000;
                    if (rdta.DataLength > 0)
                    {
                        for (int i = 0; i < rdta.DataLength; i++) stt |= (int)rdta.Data[i] << (i * 8);
                        currstat.Status |= (PayoutStatusFlags)stt;
                    }
                    else
                    {
                        lasterr = CcTalkErrors.DataFormat;
                    }
                    Thread.Sleep(10);
                }
                else
                {
                    return lasterr;
                }
                // Level Sensors
                sdta.DataLength = 0;
                sdta.Header = 217;
                lasterr = TalkCc(sdta, ref rdta);
                if (lasterr == CcTalkErrors.Ok)
                {
                    if (rdta.DataLength > 0)
                    {
                        if ((rdta.Data[0] & 0x10) != 0)
                        {
                            if ((rdta.Data[0] & 0x01) == 0)
                                currstat.LowLevelSensor = PayoutSensorStatus.Triggered;
                            else
                                currstat.LowLevelSensor = PayoutSensorStatus.Untriggered;
                        }
                        else
                        {
                            currstat.LowLevelSensor = PayoutSensorStatus.NotSupported;
                        }
                        if ((rdta.Data[0] & 0x20) != 0)
                        {
                            if ((rdta.Data[0] & 0x02) == 0)
                                currstat.HighLevelSensor = PayoutSensorStatus.Triggered;
                            else
                                currstat.HighLevelSensor = PayoutSensorStatus.Untriggered;
                        }
                        else
                        {
                            currstat.HighLevelSensor = PayoutSensorStatus.NotSupported;
                        }
                    }
                    else
                    {
                        lasterr = CcTalkErrors.DataFormat;
                    }
                }
            }
            return lasterr;
        }

        /// <summary>
        /// Payout enable status.
        /// </summary>
        /// <remarks>
        /// Must be set to true before paying out any coins using <see cref="PayoutComm.PayoutCoins(int)"/>.
        /// </remarks>
        public bool PayoutEnabled
        {
            get { return GetPayoutEnable(); }
            set { SetPayoutEnable(value); }
        }

        /// <summary>
        /// Payout mode.
        /// </summary>
        /// <remarks>
        /// Defines the way the payout command is verified./>.
        /// </remarks>
        public PayoutMode PayoutMode
        {
            get { return payoutmode; }
            set { payoutmode = value; }
        }

        /// <summary>
        /// Is it a multicoin (accumulator) hopper?
        /// </summary>
        public bool MultiCoin
        {
            get { return multicoin; }
        }

        /// <summary>
        /// The last GetPayoutStatus() for a multicoin hopper indicated that the device is currently purging.
        /// </summary>
        public bool IsPurging
        {
            get
            {
                if (multicoin)
                {
                    return ispurging;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Number of different coins the hopper can payout. Only valid if <see cref="MultiCoin"/> is true;
        /// </summary>
        public int NumberOfCoins
        {
            get { return (coincnt); }
        }

        /// <summary>
        /// Retrieves values and currency IDs up to 16 coins.
        /// </summary>
        /// <param name="currvals">Array of <see cref="CoinValue"/>, must have at least 16 elements</param>
        /// <remarks>
        /// Only the first <see cref="NumberOfCoins"/> entries are valid.
        /// </remarks>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetHopperCoins(ref CoinValue[] currvals)
        {
            ReadHopperCoins();

            //if (multicoin)
            //    payoutmode = whPayoutMode.NoEncryption;
            //else
            //    payoutmode = whPayoutMode.SerialNumber;

            for (int i = 0; i < coincnt; i++)
                currvals[i] = coinvals[i];
            return CcTalkErrors.Ok;
        }

        /// <summary>
        /// Pays out between 1 and 255 coins.
        /// </summary>
        /// <param name="count">Number of coins to be paid out</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code, e.g.:
        /// <see cref="CcTalkErrors.SetupErr"/> if payout is disabled.
        /// <see cref="CcTalkErrors.WrongParameter"/> if count isn't between 1 and 255.
        /// </returns>
        public CcTalkErrors PayoutCoins(int count)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            if ((count < 0) || (count > 255)) lasterr = CcTalkErrors.WrongParameter;
            //if (!GetPayoutEnable()) lasterr = whCcTalkErrors.SetupErr;
            if (multicoin) lasterr = CcTalkErrors.WrongCommand;
            if (lasterr != CcTalkErrors.Ok) return lasterr;
            switch (payoutmode)
            {
                case PayoutMode.SerialNumber:
                    sdta.DataLength = 0;
                    sdta.Header = 242;
                    lasterr = TalkCc(sdta, ref rdta);
                    if ((lasterr == CcTalkErrors.Ok) && (rdta.DataLength >= 3))
                    {
                        for (int i = 0; i < 3; i++) sdta.Data[i] = rdta.Data[i];
                        sdta.Data[3] = (byte)count;
                        sdta.DataLength = 4;
                        sdta.Header = 167;
                        lasterr = TalkCc(sdta, ref rdta);
                    }
                    break;
                case PayoutMode.NoEncryption:
                    for (int i = 0; i < 8; i++) sdta.Data[i] = 0x00;
                    sdta.DataLength = 8;
                    sdta.Header = 161;      // Pump RNG
                    lasterr = TalkCc(sdta, ref rdta);
                    if (lasterr == CcTalkErrors.Ok)
                    {
                        sdta.DataLength = 0;
                        sdta.Header = 160;      // Request cipher key
                        lasterr = TalkCc(sdta, ref rdta);
                    }
                    if (lasterr == CcTalkErrors.Ok)
                    {
                        for (int i = 0; i < 8; i++) sdta.Data[i] = 0x00;
                        sdta.Data[8] = (byte)count;
                        sdta.DataLength = 9;
                        sdta.Header = 167;      // Payout coins
                        lasterr = TalkCc(sdta, ref rdta);
                    }
                    break;
                case PayoutMode.Encrypted:
                    lasterr = CcTalkErrors.UnSupported;
                    break;
            }
            return lasterr;
        }
        /// <summary>
        /// Pays out the value of coins requested.
        /// </summary>
        /// <param name="value">Value to be paid out</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code, e.g.:
        /// <see cref="CcTalkErrors.WrongCommand"/> if it no multicoin hopper.
        /// <see cref="CcTalkErrors.SetupErr"/> if payout is disabled.
        /// <see cref="CcTalkErrors.WrongParameter"/> if count isn't between 0 and 65535.
        /// </returns>
        public CcTalkErrors PayoutCoins(double value)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            int count = (int)(value * 100);

            if ((count < 0) && (count > 65535)) lasterr = CcTalkErrors.WrongParameter;
            //if (!GetPayoutEnable()) lasterr = whCcTalkErrors.SetupErr;
            if (!multicoin) lasterr = CcTalkErrors.WrongCommand;
            if (lasterr != CcTalkErrors.Ok) return lasterr;

            switch (payoutmode)
            {
                case PayoutMode.SerialNumber:
                    lasterr = CcTalkErrors.WrongCommand;
                    break;
                case PayoutMode.NoEncryption:
                    //for (int i = 0; i < 8; i++) sdta.Data[i] = 0x00;
                    //sdta.DataLength = 8;
                    //sdta.Header = 161;      // Pump RNG
                    //lasterr = TalkCc(sdta, ref rdta);
                    //if (lasterr == whCcTalkErrors.Ok)
                    //{
                    //    sdta.DataLength = 0;
                    //    sdta.Header = 160;      // Request cipher key
                    //    lasterr = TalkCc(sdta, ref rdta);
                    //}
                    SetPayoutEnable(true);
                    if (lasterr == CcTalkErrors.Ok)
                    {
                        for (int i = 0; i < 8; i++) sdta.Data[i] = 0x00;
                        sdta.Data[8] = (byte)count;
                        sdta.Data[9] = (byte)(count >> 8);
                        sdta.DataLength = 10;
                        sdta.Header = 134;      // Payout coins
                        lasterr = TalkCc(sdta, ref rdta);
                    }
                    break;
                case PayoutMode.Encrypted:
                    lasterr = CcTalkErrors.UnSupported;
                    break;
            }
            return lasterr;
        }
        /// <summary>
        /// Pays out a certain number of each coin type.
        /// </summary>
        /// <param name="counts">An array holding the number of coins to be paid out</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code, e.g.:
        /// <see cref="CcTalkErrors.WrongCommand"/> if it no multicoin hopper.
        /// <see cref="CcTalkErrors.SetupErr"/> if payout is disabled.
        /// <see cref="CcTalkErrors.WrongParameter"/> if array has no elements.
        /// </returns>
        public CcTalkErrors PayoutCoins(int[] counts)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);


            //if (!GetPayoutEnable()) lasterr = whCcTalkErrors.SetupErr;
            if (!multicoin) lasterr = CcTalkErrors.WrongCommand;
            if (lasterr != CcTalkErrors.Ok) return lasterr;

            switch (payoutmode)
            {
                case PayoutMode.SerialNumber:
                    lasterr = CcTalkErrors.WrongCommand;
                    break;
                case PayoutMode.NoEncryption:
                    for (int i = 0; i < 8; i++) sdta.Data[i] = 0x00;
                    sdta.DataLength = 8;
                    sdta.Header = 161;      // Pump RNG
                    lasterr = TalkCc(sdta, ref rdta);
                    if (lasterr == CcTalkErrors.Ok)
                    {
                        sdta.DataLength = 0;
                        sdta.Header = 160;      // Request cipher key
                        lasterr = TalkCc(sdta, ref rdta);
                    }
                    SetPayoutEnable(true);
                    if (lasterr == CcTalkErrors.Ok)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            sdta.Data[i] = 0x00;
                            sdta.Data[i + 8] = (byte)counts[i];
                        }
                        sdta.DataLength = 16;
                        sdta.Header = 134;      // Payout coins
                        lasterr = TalkCc(sdta, ref rdta);
                    }
                    break;
                case PayoutMode.Encrypted:
                    lasterr = CcTalkErrors.UnSupported;
                    break;
            }
            return lasterr;
        }
        /// <summary>
        /// Recharges the pockets of a multicoin hopper.
        /// </summary>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code, e.g.:
        /// <see cref="CcTalkErrors.WrongCommand"/> if it no multicoin hopper.
        /// <see cref="CcTalkErrors.SetupErr"/> if payout is disabled.
        /// </returns>
        public CcTalkErrors RechargePockets()
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);


            //if (!GetPayoutEnable()) lasterr = whCcTalkErrors.SetupErr;
            if (!multicoin) lasterr = CcTalkErrors.WrongCommand;
            if (lasterr != CcTalkErrors.Ok) return lasterr;

            switch (payoutmode)
            {
                case PayoutMode.SerialNumber:
                    lasterr = CcTalkErrors.WrongCommand;
                    break;
                case PayoutMode.NoEncryption:
                    for (int i = 0; i < 8; i++) sdta.Data[i] = 0x00;
                    sdta.DataLength = 8;
                    sdta.Header = 161;      // Pump RNG
                    lasterr = TalkCc(sdta, ref rdta);
                    if (lasterr == CcTalkErrors.Ok)
                    {
                        sdta.DataLength = 0;
                        sdta.Header = 160;      // Request cipher key
                        lasterr = TalkCc(sdta, ref rdta);
                    }
                    SetPayoutEnable(true);
                    if (lasterr == CcTalkErrors.Ok)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            sdta.Data[i] = 0x00;
                        }
                        sdta.DataLength = 10;
                        sdta.Header = 134;      // Payout coins
                        lasterr = TalkCc(sdta, ref rdta);
                    }
                    break;
                case PayoutMode.Encrypted:
                    lasterr = CcTalkErrors.UnSupported;
                    break;
            }
            return lasterr;
        }

        /// <summary>
        /// Stops the payout procedure immediately.
        /// </summary>
        /// <param name="count">Number of coins which failed to be paid out.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors EmergencyStop(ref int count)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            count = 0;
            sdta.DataLength = 0;
            sdta.Header = 172;
            lasterr = TalkCc(sdta, ref rdta);
            if ((lasterr == CcTalkErrors.Ok) && (rdta.DataLength > 0)) count = rdta.Data[0];
            return lasterr;
        }
        /// <summary>
        /// Stops the payout value procedure immediately.
        /// </summary>
        /// <param name="value">Value which failed to be paid out.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors EmergencyStop(ref double value)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            value = 0.0;
            sdta.DataLength = 0;
            sdta.Header = 132;
            lasterr = TalkCc(sdta, ref rdta);
            if ((lasterr == CcTalkErrors.Ok) && (rdta.DataLength > 0))
            {
                value = (rdta.Data[0] + 256 * rdta.Data[1]) / 100;
            }
            return lasterr;
        }


        /// <summary>
        /// This command can be used to completely empty the hopper.
        /// Please refer to the hopper's manual if header 121 is supported.
        /// </summary>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors PurgeHopper()
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);
            byte[] serial = new byte[3];

            if (multicoin)
            {
                ResetDevice(150);
                sdta.DataLength = 0;
                sdta.Header = 242;
                lasterr = TalkCc(sdta, ref rdta);
                if ((lasterr == CcTalkErrors.Ok) && (rdta.DataLength >= 3))
                {
                    SetPayoutEnable(true);
                    for (int i = 0; i < 3; i++) sdta.Data[i] = rdta.Data[i];
                    sdta.DataLength = 3;
                    sdta.Header = 121;
                    lasterr = TalkCc(sdta, ref rdta);
                }
            }
            else
            {
                sdta.DataLength = 1;
                sdta.Header = 121;
                sdta.Data[0] = 0x00;
                lasterr = TalkCc(sdta, ref rdta);
            }
            return lasterr;
        }

        #region Data Storage
        /// <summary>
        /// Retrieves the details of available data storage in the payout.
        /// </summary>
        /// <param name="storageavailability"> struct <see cref="DataStorageAvailability"/>, holding the details of the data storage.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors RequestDataStorageAvailability(ref DataStorageAvailability storageavailability)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            storageavailability = new DataStorageAvailability();
            storageavailability.StorageType = DataStorageType.None;

            sdta.DataLength = 0;
            sdta.Header = 216;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                if (rdta.DataLength >= 5)
                {
                    storageavailability.StorageType = (DataStorageType)rdta.Data[0];
                    storageavailability.ReadBlockCount = rdta.Data[1];
                    storageavailability.ReadBlockSize = rdta.Data[2];
                    storageavailability.WriteBlockCount = rdta.Data[3];
                    storageavailability.WriteBlockSize = rdta.Data[4];
                }
                else
                {
                    lasterr = CcTalkErrors.DataFormat;
                }
            }

            return lasterr;
        }

        /// <summary>
        /// Reads out the available data storage and stores it in a byte array.
        /// </summary>
        /// <param name="databuffer"> byte array returning the contens of the data storage.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors ReadDataStorage(ref byte[] databuffer)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            DataStorageAvailability storage = new DataStorageAvailability();
            int datacnt = 0;

            databuffer = new byte[0];

            lasterr = RequestDataStorageAvailability(ref storage);
            datacnt = storage.ReadBlockCount * storage.ReadBlockSize;

            if ((lasterr == CcTalkErrors.Ok) && (datacnt > 0))
            {
                databuffer = new byte[datacnt];
                for (int i = 0; i < storage.ReadBlockCount; i++)
                {
                    sdta.DataLength = 1;
                    sdta.Header = 215;
                    sdta.Data[0] = (byte)i;
                    lasterr = TalkCc(sdta, ref rdta);
                    if (lasterr == CcTalkErrors.Ok)
                    {
                        if (rdta.DataLength >= storage.ReadBlockSize)
                        {
                            for (int d = 0; d < storage.ReadBlockSize; d++)
                            {
                                databuffer[storage.ReadBlockSize * i + d] = rdta.Data[d];
                            }
                        }
                        else
                        {
                            lasterr = CcTalkErrors.DataFormat;
                        }
                    }
                    if (lasterr != CcTalkErrors.Ok)
                    {
                        break;
                    }
                }
            }
            return lasterr;
        }

        /// <summary>
        /// Writes the contens of a byte array into the data storage. The methode will return an error if the length of the array is greater than the available storage.
        /// </summary>
        /// <param name="databuffer"> byte array to be written into the storage.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors WriteDataStorage(byte[] databuffer)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            DataStorageAvailability storage = new DataStorageAvailability();
            int datacnt = 0;

            lasterr = RequestDataStorageAvailability(ref storage);
            datacnt = Math.Min(storage.WriteBlockCount * storage.WriteBlockSize, databuffer.Length);

            if (databuffer.Length > (storage.WriteBlockCount * storage.WriteBlockSize))
            {
                lasterr = CcTalkErrors.InvalidParameter;
            }

            if ((lasterr == CcTalkErrors.Ok) && (datacnt > 0))
            {
                sdta.DataLength = (byte)(storage.WriteBlockSize + 1);
                sdta.Header = 214;
                int blkcnt = databuffer.Length / storage.WriteBlockSize;
                if ((databuffer.Length % storage.WriteBlockSize) != 0)
                {
                    blkcnt++;
                }
                for (int i = 0; i < blkcnt; i++)
                {
                    sdta.Data[0] = (byte)i;
                    for (int d = 0; d < storage.WriteBlockSize; d++)
                    {
                        int didx = storage.WriteBlockSize * i + d;
                        if (didx < databuffer.Length)
                        {
                            sdta.Data[d + 1] = databuffer[didx];
                        }
                    }
                    lasterr = TalkCc(sdta, ref rdta);

                    if (lasterr != CcTalkErrors.Ok)
                    {
                        break;
                    }
                }
            }

            return lasterr;
        }


        /// <summary>
        /// Reads a null terminated customer string from the hopper user storage if available.
        /// </summary>
        /// /// <param name="customerstring">The string retrieved from storage.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetCustomerString(ref string customerstring)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            customerstring = "";
            byte[] stringbuff = new byte[0];

            lasterr = ReadDataStorage(ref stringbuff);

            if (lasterr == CcTalkErrors.Ok)
            {
                for (int i = 0; i < stringbuff.Length; i++)
                {
                    if (stringbuff[i] != 0x00)
                    {
                        customerstring += (char)stringbuff[i];
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return lasterr;
        }

        /// <summary>
        /// Writes a null-termineated customer string into the hopper user storage if available.
        /// </summary>
        /// /// <param name="customerstring">The string to be written into the storage.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetCustomerString(string customerstring)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            customerstring += (char)0x00;
            lasterr = WriteDataStorage(Encoding.ASCII.GetBytes(customerstring));

            return lasterr;
        }
        #endregion

        #region Private and internal methodes and variables
        private void SetPayoutEnable(bool mival)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            if (this.IsOpen)
            {
                sdta.DataLength = 1;
                sdta.Header = 164;
                if (mival)
                    sdta.Data[0] = 165;
                else
                    sdta.Data[0] = 0;
                lasterr = TalkCc(sdta, ref rdta);
            }
        }
        private bool GetPayoutEnable()
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            if (!this.IsOpen) return false;
            sdta.DataLength = 0;
            sdta.Header = 163;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr != CcTalkErrors.Ok) return false;
            if (rdta.DataLength < 1) return false;
            return ((rdta.Data[0] & (int)PayoutStatusFlags.PayoutDisabled) == 0);
        }
        // Get hopper coins
        internal void ReadHopperCoins()
        {
            coincnt = 0;
            for (int i = 0; i < 16; i++)
            {
                if (GetHopperCoinValue(i, ref coinvals[i]) == CcTalkErrors.Ok)
                {
                    if (coinvals[i].Value > 0.00001) coincnt++;
                }
                else
                {
                    if ((lasterr == CcTalkErrors.DataFormat) || (lasterr == CcTalkErrors.RcvTimout))
                        lasterr = CcTalkErrors.Ok;
                    break;
                }
            }
            multicoin = coincnt > 1;
        }
        internal CcTalkErrors GetHopperCoinValue(int coinno, ref CoinValue coinval)
        {
            char sepchar = (5.5).ToString()[1];

            int i;
            double fac;
            string coinid = "", valstr;
            char facch;

            coinval.Value = 0;
            coinval.ID = "";
            GetHopperCoinID(coinno, ref coinid, ref coinval.IntValue);
            if (lasterr != CcTalkErrors.Ok) return lasterr;
            if (coinid.Length < 6)
            {
                lasterr = CcTalkErrors.DataFormat;
                return lasterr;
            }

            // Analyse coin string
            // ID
            coinval.ID = coinid.Substring(0, 2);
            // Value
            valstr = "";
            facch = ' ';
            for (i = 2; i < 5; i++)
            {
                if (Char.IsDigit(coinid[i]))
                {
                    valstr += coinid[i];
                }
                else
                {
                    valstr += sepchar;
                    facch = coinid[i];
                }
            }
            coinval.Decimals = GetCcTalkDecimals(coinval.ID);
            try
            {
                coinval.Value = (double)(Convert.ToDouble(valstr) / Math.Pow(10, coinval.Decimals));
                coinval.IntValue = (int)(coinval.Value * Math.Pow(10, coinval.Decimals));
            }
            catch (System.Exception)
            {
                coinval.Value = 0;
                coinval.ID = "";
                return lasterr;
            }
            fac = 1;
            for (i = 0; i < ValueFactors.Length; i++)
            {
                if (ValueFactors[i].FactorChar == facch)
                {
                    fac = ValueFactors[i].Factor;
                    break;
                }
            }
            coinval.Value *= fac;
            return lasterr;
        }
        internal CcTalkErrors GetHopperCoinID(int coinno, ref string coinstr, ref int coinint)
        {
            int i;
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.Header = 131;
            sdta.DataLength = 0;
            if (coinno > -1)
            {
                sdta.DataLength = 1;
                sdta.Data[0] = (byte)(coinno + 1);
            }
            else
            {
                lasterr = CcTalkErrors.WrongParameter;
                return lasterr;
            }

            coinstr = "";
            coinint = 0;
            lasterr = TalkCc(sdta, ref rdta);

            if (rdta.DataLength < 8) lasterr = CcTalkErrors.DataFormat;

            if (lasterr == CcTalkErrors.Ok)
            {
                for (i = 0; i < rdta.DataLength - 2; i++)
                    coinstr += (char)rdta.Data[i];

                coinint = rdta.Data[rdta.DataLength - 2] + 256 * rdta.Data[rdta.DataLength - 1];
            }

            return lasterr;
        }

        internal PayoutMode payoutmode = PayoutMode.SerialNumber;
        internal bool multicoin = false;
        internal bool ispurging = false;
        internal int coincnt = 0;
        internal CoinValue[] coinvals = new CoinValue[16];

        #endregion
    }
}