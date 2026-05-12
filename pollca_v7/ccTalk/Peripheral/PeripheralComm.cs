using System;

namespace ccTalk
{
    /// <summary>
    /// wh Dongle Communication class.
    /// </summary>
    [Serializable]
    public class PeripheralComm : CcTalkComm
    {
        #region Constructor/Destructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// Sets some default values.
        /// </remarks>
        public PeripheralComm()
        {
            Address = PERIPHERAL_ADR;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// <param name="basedevice">Instance of the base class<see cref="CcTalkComm"/> were some settings are taken from:</param>
        /// Address, Port and ChecksumType.
        /// </remarks>
        public PeripheralComm(CcTalkComm basedevice)
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
            if (lasterr == CcTalkErrors.Ok)
            {
                string verstr = this.SoftwareRevision;
                cd100 = verstr.ToLower().IndexOf("cd") >= 0;
            }
            return lasterr;

        }

        #region Private variables
        // Constants
        private const byte PERIPHERAL_ADR = 80;
        private bool cd100 = false;
        #endregion

        /// <summary>
        /// Retrieves the implemented features of the dongle.
        /// </summary>
        public DongleFeatures Features
        {
            get
            {
                DongleFeatures features = DongleFeatures.Nothing;
                CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

                int ifeatures = 0x00000000;

                sdta.DataLength = 0;
                sdta.Header = 110;
                if ((lasterr = TalkCc(sdta, ref rdta)) == CcTalkErrors.Ok)
                {
                    for (int i = 0; i < rdta.DataLength; i++)
                    {
                        ifeatures += rdta.Data[i] << (8 * i);
                    }
                    features = (DongleFeatures)ifeatures;
                }
                return features;
            }
        }

        /// <summary>
        /// Retrieves the usage of the universal IO port.
        /// </summary>
        public whDongleIOPortUsage IOPortUsage
        {
            get
            {
                CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

                sdta.DataLength = 0;
                sdta.Header = 109;
                if ((lasterr = TalkCc(sdta, ref rdta)) == CcTalkErrors.Ok)
                    return (whDongleIOPortUsage)rdta.Data[0];
                else
                    return whDongleIOPortUsage.Standard;
            }
        }

        /// <summary>
        /// Retrieves the status of up to 8 switches.
        /// </summary>
        /// <param name="switches">Array of bool holding the state of the switches. True if switch is closed.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetSwitches(ref bool[] switches)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            switches = new bool[8];
            sdta.DataLength = 0;
            sdta.Header = 105;
            if ((lasterr = TalkCc(sdta, ref rdta)) == CcTalkErrors.Ok)
            {
                for (int i = 0; i < 8; i++)
                    switches[i] = (rdta.Data[0] & (0x01 << i)) != 0x00;
            }
            return lasterr;
        }

        /// <summary>
        /// Sets the status of one LED.
        /// </summary>
        /// <param name="no">The number of the LED to change status: 0...7.</param>
        /// <param name="status">The desired status for this LED.</param>
        /// <param name="ontime">If the desired status is "Flashing" this is the on period in ms. Value must be between 25 and 6250</param>
        /// <param name="offtime">If the desired status is "Flashing" this is the off period in ms. Value must be between 25 and 6250</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetLEDState(int no, LEDStatus status, int ontime, int offtime)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            if ((no < 0) || (no > 7))
            {
                lasterr = CcTalkErrors.WrongParameter;
                return lasterr;
            }
            if ((status == LEDStatus.Flashing) && ((ontime < 25) || (ontime > 6250) || (offtime < 25) || (offtime > 6250)))
            {
                lasterr = CcTalkErrors.WrongParameter;
                return lasterr;
            }

            sdta.DataLength = 3;
            sdta.Data[0] = (byte)no;
            switch (status)
            {
                case LEDStatus.Off:
                    sdta.Header = 107;
                    sdta.Data[1] = 0x00;
                    sdta.Data[2] = 0x00;
                    break;
                case LEDStatus.On:
                    sdta.Header = 107;
                    sdta.Data[1] = 0x01;
                    sdta.Data[2] = 0x00;
                    break;
                case LEDStatus.Flashing:
                    sdta.Header = 108;
                    sdta.Data[1] = (byte)((ontime + offtime) / 50);
                    sdta.Data[2] = 0x00;
                    break;
            }
            lasterr = TalkCc(sdta, ref rdta);

            return lasterr;
        }

        /// <summary>
        /// Sets the status of one LED.
        /// </summary>
        /// <param name="no">The number of the LED to change status: 0...7.</param>
        /// <param name="status">The desired status for this LED.</param>
        /// <param name="period">If the desired status is "Flashing" this is the flash period in ms. Value must be between 50 and 12500</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetLEDState(int no, LEDStatus status, int period)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            if ((no < 0) || (no > 7))
            {
                lasterr = CcTalkErrors.WrongParameter;
                return lasterr;
            }
            if ((status == LEDStatus.Flashing) && (period < 50) || (period > 12500))
            {
                lasterr = CcTalkErrors.WrongParameter;
                return lasterr;
            }

            sdta.DataLength = 2;
            sdta.Data[0] = (byte)no;
            switch (status)
            {
                case LEDStatus.Off:
                    sdta.Header = 107;
                    sdta.Data[1] = 0x00;
                    break;
                case LEDStatus.On:
                    sdta.Header = 107;
                    sdta.Data[1] = 0x01;
                    break;
                case LEDStatus.Flashing:
                    sdta.Header = 108;
                    sdta.Data[1] = (byte)(period / 50);
                    break;
            }
            lasterr = TalkCc(sdta, ref rdta);

            return lasterr;
        }
        /// <summary>
        /// Sets synchronous flashing of up to 8 LEDs. Requires firmware 2.07 or higher.
        /// </summary>
        /// <param name="ledmsk">Masking the LEDs for flashing.</param>
        /// <param name="period">Flash period in ms. Value must be between 50 and 12500</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetLEDState(bool[] ledmsk, int period)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            if ((period < 50) || (period > 12500))
            {
                lasterr = CcTalkErrors.WrongParameter;
                return lasterr;
            }

            sdta.DataLength = 3;
            sdta.Header = 108;
            sdta.Data[0] = 8;
            sdta.Data[1] = (byte)(period / 50);
            sdta.Data[2] = 0x00;
            for (int i = 0; i < Math.Min(8, ledmsk.Length); i++)
            {
                if (ledmsk[i]) sdta.Data[2] |= (byte)(0x01 << i);
            }
            lasterr = TalkCc(sdta, ref rdta);

            return lasterr;
        }
        /// <summary>
        /// Sets the status of the relay.
        /// </summary>
        /// <param name="no">The number of the relay to change status: 0...7. Currently only relay no. 0 is supported.</param>
        /// <param name="status">The desired status for the relay.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetRelayState(int no, RelayStatus status)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);


            if ((no < 0) || (no > 7))
            {
                lasterr = CcTalkErrors.WrongParameter;
                return lasterr;
            }
            sdta.DataLength = 2;
            sdta.Data[0] = (byte)(no + 8);
            switch (status)
            {
                case RelayStatus.Off:
                    sdta.Header = 107;
                    sdta.Data[1] = 0x00;
                    break;
                case RelayStatus.On:
                    sdta.Header = 107;
                    sdta.Data[1] = 0x01;
                    break;
            }
            lasterr = TalkCc(sdta, ref rdta);

            return lasterr;
        }

        /// <summary>
        /// Sets the state of the escrow.
        /// </summary>
        /// <param name="state">State<see cref="EscrowState"/> of the flaps.</param>
        /// <param name="duration">Duration<see cref="EscrowState"/> of the state in 0.1 sec. If duration == 0 the state will until it is overwritten with another one.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetupEscrow(EscrowState state, int duration)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 2;
            sdta.Header = 135;
            sdta.Data[0] = (byte)state;
            sdta.Data[1] = (byte)duration;
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
            sdta.Header = 133;
            lasterr = TalkCc(sdta, ref rdta);
            return lasterr;
        }

        /// <summary>
        /// Sets the behaviour of the anti pin system (shutter).
        /// </summary>
        /// <param name="setting">Set up <see cref="AntiPinSetting"/> for the behaviour of the anti pin system.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetupAntiPin(AntiPinSetting setting)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 1;
            sdta.Header = 132;
            sdta.Data[0] = (byte)setting;
            lasterr = TalkCc(sdta, ref rdta);
            return lasterr;
        }

        /// <summary>
        /// Retrieves the status of the peripheral devices.
        /// </summary>
        /// <param name="status">Structure (see: <see cref="PeripheralStatus"/>) holding the information.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetPeripheralState(ref PeripheralStatus status)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 0;
            sdta.Header = 131;
            lasterr = TalkCc(sdta, ref rdta);
            if ((lasterr == CcTalkErrors.Ok) && (rdta.DataLength >= 3))
            {
                status.EscrowFlaps = (EscrowState)rdta.Data[0];
                status.EscrowSwitch = rdta.Data[1] != 0;
                status.AntiPinSetup = (AntiPinSetting)rdta.Data[2];
                status.AntiPinStatus = (AntiPinStatus)rdta.Data[3];
                status.MotorRejectSwitch = rdta.Data[4] != 0;
            }
            return lasterr;
        }

        /// <summary>
        /// Sets the configuration of 8 bits of the programmable IO port, CCT 910 only.
        /// </summary>
        /// <param name="bitsetting">Array of <see cref="DongleIOBitSetting"/> with up to 8 elements.
        /// If the array is shorter the remaining bits will be set up as normal input.
        /// </param>
        /// 
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetIOPortConfiguration(DongleIOBitSetting[] bitsetting)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 8;
            sdta.Header = 104;
            for (int i = 0; i < 8; i++)
            {
                if (i < bitsetting.Length)
                {
                    sdta.Data[i] = (byte)bitsetting[i];
                }
                else
                {
                    sdta.Data[i] = (byte)DongleIOBitSetting.Input;
                }
            }
            lasterr = TalkCc(sdta, ref rdta);
            return lasterr;

        }
        /// <summary>
        /// Sets the configuration of 8 bits of the programmable IO port and the debounce periods for light barier operation, CCT 910 only.
        /// </summary>
        /// <param name="bitsetting">Array of <see cref="DongleIOBitSetting"/> with up to 8 elements.
        /// If the array is shorter the remaining bits will be set up as normal input.
        /// </param>
        /// <param name="lbdebounce">Array of integer with the debounce values for up to 8 light barriers.
        /// If the array is shorter the value will be set to 0 thus disbaling light barrier operation.
        /// The maximum value is 255ms.
        /// </param>
        /// 
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetIOPortConfiguration(DongleIOBitSetting[] bitsetting, int[] lbdebounce)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 16;
            sdta.Header = 104;
            for (int i = 0; i < 8; i++)
            {
                if (i < bitsetting.Length)
                {
                    sdta.Data[i] = (byte)bitsetting[i];
                }
                else
                {
                    sdta.Data[i] = (byte)DongleIOBitSetting.Input;
                }
            }
            for (int i = 0; i < 8; i++)
            {
                if (i < lbdebounce.Length)
                {
                    sdta.Data[i + 8] = (byte)lbdebounce[i];
                }
                else
                {
                    sdta.Data[i + 8] = 0;
                }
            }
            lasterr = TalkCc(sdta, ref rdta);
            return lasterr;

        }
        /// <summary>
        /// Gets the status of 8 bits of the programmable IO port, CCT 910 only.
        /// </summary>
        /// <param name="inpattern"> pattern of the bits configured as input.
        /// </param>
        /// 
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetIOPortStatus(ref byte inpattern)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 0;
            sdta.Header = 103;
            lasterr = TalkCc(sdta, ref rdta);
            if (rdta.DataLength > 0)
            {
                inpattern = rdta.Data[0];
            }
            else
            {
                lasterr = CcTalkErrors.DataFormat;
            }
            return lasterr;
        }
        /// <summary>
        /// Sets the status of 8 bits of the programmable IO port, CCT 910 only.
        /// </summary>
        /// <param name="outpattern"> pattern for the bits configured as output.
        /// </param>
        /// 
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetIOPortStatus(byte outpattern)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 1;
            sdta.Header = 102;
            sdta.Data[0] = outpattern;
            lasterr = TalkCc(sdta, ref rdta);
            return lasterr;
        }

        /// <summary>
        /// Retrieves the counters for light barrier transits, CCT 910 only.
        /// </summary>
        /// <param name="transitcounters"> 
        /// Array of inter values holding the number of light barrier transits.
        /// </param>
        /// 
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetLightBarriersStatus(ref int[] transitcounters)
        {
            bool[] clearmsk = new bool[8];
            return GetLightBarriersStatus(clearmsk, ref transitcounters);
        }
        /// <summary>
        /// Retrieves the counters for light barrier transits, CCT 910 only.
        /// </summary>
        /// <param name="clearcounter"> 
        /// Array of up to 8 boolean values. If set to true the respective counter will be cleared after reading it out.
        /// </param>
        /// <param name="transitcounters"> 
        /// Array of inter values holding the number of light barrier transits.
        /// </param>
        /// 
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetLightBarriersStatus(bool[] clearcounter, ref int[] transitcounters)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            byte clearmsk = 0x00;
            transitcounters = new int[8];

            sdta.DataLength = 1;
            sdta.Header = 101;
            for (int i = 0; i < clearcounter.Length; i++)
            {
                if (clearcounter[i])
                {
                    clearmsk = (byte)(clearmsk | (0x01 << i));
                }
            }
            sdta.Data[0] = clearmsk;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                for (int i = 0; i < 8; i++)
                {
                    transitcounters[i] = rdta.Data[i * 2] + 256 * rdta.Data[i * 2 + 1];
                }
            }
            return lasterr;
        }
    }
}