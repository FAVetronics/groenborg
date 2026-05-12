using System;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using ccTalk.Validation;

namespace ccTalk
{
    /// <summary>
    /// Basic Communication class for ccTalk cash devices.
    /// </summary>
    [Serializable]
    public class CcTalkComm
    {
        #region Constructor/Destructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// Sets some default values.
        /// </remarks>
        public CcTalkComm()
        {
            isopen = false;
            lasterr = CcTalkErrors.Ok;
            hostaddr = 1;
            devaddr = 0;

            rcvtot = 250;
            bytetot = 75;
            
            localecho = true;

            cat = CcTalkCategory.Unknown;
            EncryptionSupport.ProtocolLevel = CcTalkEncryption.None;
            ChecksumType = CcTalkChecksumTypes.Simple8;
            catstr = "";
            if (!initialised)
            {
                for (int i = 0; i < cctports.Length; i++) cctports[i] = new CcTalkPort(true);
                initialised = true;
            }
#if DES_SUPPORT
            decryptor = descrypt.CreateDecryptor();
            encryptor = descrypt.CreateEncryptor();
#endif
        }
#if !WindowsCE
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// Sets some default values obtained form CcTalkListDevice.
        /// </remarks>
        public CcTalkComm(CcTalkListDevice device)
        {
            isopen = false;
            lasterr = CcTalkErrors.Ok;
            hostaddr = 1;
            devaddr = 0;

            rcvtot = 250;
            bytetot = 75;
            localecho = true;

            cat = CcTalkCategory.Unknown;
            this.EncryptionSupport = device.EncryptionSupport.Clone();
            this.EncryptionMode = device.EncryptionMode;
            this.ChecksumType = device.ChecksumType;
            catstr = "";
            if (!initialised)
            {
                for (int i = 0; i < cctports.Length; i++) cctports[i] = new CcTalkPort(true);
                initialised = true;
            }
            this.Port = device.Port;
            this.Address = device.Address;
        }
#endif
        /// <summary>
        /// Destructor
        /// </summary>
        /// <remarks>
        /// Closes the device if open.
        /// </remarks>
        ~CcTalkComm()
        {
            isopen = false;
        }
        #endregion

        #region Interna und Privates
        internal CcTalkErrors lasterr;
        internal byte hostaddr = 1, devaddr = 0;
        internal int evtctr;
        internal CcTalkChecksumTypes cstype = CcTalkChecksumTypes.CRC16;
        internal int portidx = -1;
        internal bool nospecialaddresses = false;

        internal int lastcommtick = 0;

        internal void setusartidx(byte address)
        {
            switch (address)
            {
                default:
                    usartidx = 0;
                    break;
            }
        }
        // List of com ports in use
        [Serializable]
        internal struct CcTalkPort
        {
            public SerialPort Port;
            public int Number;
            public int OpenCount;

            public CcTalkPort(bool init)
            {
                Port = new SerialPort();
                Number = -1;
                OpenCount = 0;
            }
        }
        internal static bool initialised = false;
#if PORTABLE
        internal static CcTalkPort[] cctports = new CcTalkPort[1];
#else
        internal static CcTalkPort[] cctports = new CcTalkPort[32];
#endif

#if DES_SUPPORT
        // DES Encryption
        DESCryptoServiceProvider descrypt = new DESCryptoServiceProvider();
        ICryptoTransform decryptor, encryptor;

        internal UInt16 calccrc16(byte[] data, int offs, int len)
        {
            int csum = 0x0000;
            for (int i = 0; i < len; i++)
            {
                csum ^= (data[i + offs] << 8);
                for (int j = 0; j < 8; j++)
                {
                    if ((csum & 0x8000) != 0)
                        csum = (csum << 1) ^ 0x1021;
                    else
                        csum <<= 1;
                }
            }
            return (UInt16)csum;
        }
        internal CcTalkErrors decryptdesblock(ref CcTalkDataBlock desblock, byte challenge)
        {
            byte[] transblock = new byte[16];
            if (desblock.DataLength != 16)
            {
                lasterr = CcTalkErrors.Decryption;
                return lasterr;
            }

            for (int i = 0; i < 16; i++) transblock[i] = desblock.Data[i];

            decryptor.TransformBlock(transblock, 0, 16, transblock, 0);
            if (transblock[7] != challenge)
            {
                lasterr = CcTalkErrors.Decryption;
                return lasterr;
            }
            UInt16 rcsum = (UInt16)(transblock[0] + 256 * transblock[15]);
            UInt16 ccsum = calccrc16(transblock, 1, 14);
            if (rcsum != ccsum)
            {
                lasterr = CcTalkErrors.Decryption;
                return lasterr;
            }
            // Umsortieren um Füllbytes zu entfernen
            for (int i = 0; i < 11; i++)
            {
                if (i < 5)
                {
                    desblock.Data[i] = transblock[i + 2];
                }
                else
                {
                    desblock.Data[i] = transblock[i + 4];
                }
            }
            return lasterr;
        }
        internal CcTalkErrors encryptdesblock(ref CcTalkDataBlock desblock)
        {
            byte[] transblock = new byte[16];
            if (desblock.DataLength != 16)
            {
                lasterr = CcTalkErrors.Encryption;
                return lasterr;
            }

            for (int i = 0; i < 16; i++) transblock[i] = desblock.Data[i];

            encryptor.TransformBlock(transblock, 0, 16, transblock, 0);

            for (int i = 0; i < 16; i++) desblock.Data[i] = transblock[i];
            return lasterr;
        }
#endif
        #endregion

#if TRACE_FUNCTION
        #region Delegate für ccTalk Trace File
        /// <summary>Current activity to be logged.</summary>
        public enum CctalkActivity
        {
            /// <summary>No Activity.</summary>
            None,
            /// <summary>Data block was sent.</summary>
            Sent,
            /// <summary>Data blcok was received.</summary>
            Received,
        }

        /// <summary>Data structure for communication logging.</summary>
        public struct CcTalkSendReceiveData
        {
            /// <summary>The current activity.</summary>
            public CctalkActivity Activity;
            /// <summary>The current data block.</summary>
            public CcTalkDataBlock DataBlock;
            /// <summary>The current status of the communication.</summary>
            public CcTalkErrors Status;

            /// <summary>Initialises a new instance of the structure.</summary>
            public CcTalkSendReceiveData(bool init)
            {
                Activity = CctalkActivity.None;
                DataBlock = new CcTalkDataBlock(CcTalkChecksumTypes.Simple8);
                Status = CcTalkErrors.Unknown;
            }
        }
        /// <summary>Delegate method for logging ccTalk communication.</summary>
        public delegate bool CctalkSendReceiveLogging(CcTalkSendReceiveData data);

        /// <summary>Event raised on every send or receive activity on the bus.</summary>
        public event CctalkSendReceiveLogging LogCctalkCommunication;

        #endregion
#endif

#if !WindowsCE
        /// <summary>A MDB bill validator connected via CCT 900 will appear under this ccTalk address.</summary>
        public static byte MdbBillValidatorAddress
        {
            get { return MdbAddresses.CcBillValidator; }
        }
#endif
        /// <summary>A MDB change giver connected via CCT 900 will appear under this ccTalk address.</summary>
        public static byte MdbChangeGiverAddress
        {
            get { return MdbAddresses.CcChangeGiver; }
        }
        /// <summary>A MDB cashless payment system connected via CCT 900 will appear under this ccTalk address.</summary>
        public static byte MdbCashlessAddress
        {
            get { return MdbAddresses.CcCashless; }
        }
        /// <summary>The real MDB address of cashless payment system.</summary>
        public byte MdbCashlessDeviceAddress
        {
            get { return cashlessaddr; }
            set { cashlessaddr = value; }
        }

        /// <summary>Number of the com port. Applicable only for Windows platform.</summary>
        public string Port
        {
            get { return _portname; }
            set
            {
                _portname = value;
            }
        }

        public byte Address
        {
            get { return devaddr; }
            set { devaddr = value; }
        }
        /// <summary>Checksum methode for this device.</summary>
        public CcTalkChecksumTypes ChecksumType
        {
            get { return cstype; }
            set { cstype = value; }
        }

        ///// <summary>Wait time before each communication.</summary>
        //public int CommDelay = 0;

        /// <summary>Minimal time between two communications.</summary>
        public int MinimalCommInterval = 0;

        /// <summary>Wait time before each communication.</summary>
        public int CommDelay = 0;

        /// <summary>The baudrate of the communication.</summary>
        public int BaudRate = 9600;

        /// <summary>Receive Timeout for ccTalk communication in milliseconds. Will be set to a default value when creating a new instance.</summary>
        public int CcTalkReceiveTimeout
        {
            get { return rcvtot; }
            set { rcvtot = value; }
        }
        /// <summary>Receive Timeout for MDB communication in milliseconds. Will be set to a default value when creating a new instance. Maximaum value is 255.</summary>
        public int MdbReceiveTimeout
        {
            get { return mdbtot; }
            set { mdbtot = value; }
        }

        /// <summary>Number of retries on receive time-out.</summary>
        public int CommRetry = 1;

        /// <summary>Detailed information about encryption support</summary>
        public CcEncryptionSupport EncryptionSupport = new CcEncryptionSupport(true);

        /// <summary>Encryption mode.</summary>
        public CcTalkEncryption EncryptionMode
        {
            get { return EncryptionSupport.ProtocolLevel; }
            set { EncryptionSupport.ProtocolLevel = value; }
        }

        /// <summary>Is the the device opened for communication?</summary>
        public bool IsOpen
        {
            get { return isopen; }
        }
        /// <summary>Result of last function.</summary>
        public CcTalkErrors LastError            // Last error
        {
            get { return lasterr; }
        }
        /// <summary>Category of the device.</summary>
        public CcTalkCategory Category
        {
            get
            {
                if (!nospecialaddresses)
                {
                    switch (this.Address)
                    {
#if !WindowsCE
                        case MdbAddresses.CcChangeGiver:
                            return CcTalkCategory.ChangeGiver;
                        case MdbAddresses.CcBillValidator:
                            return CcTalkCategory.BillValidator;
#endif
                        case MdbAddresses.CcCashless:
                            return CcTalkCategory.Cashless;
                        default:
                            GetCategory();
                            return cat;
                    }
                }
                else
                {
                    GetCategory();
                    return cat;
                }
            }
        }
        /// <summary>Category of the device as string.</summary>
        public string CategoryString
        {
            get
            {
                if (!nospecialaddresses)
                {
                    switch (this.Address)
                    {
#if !WindowsCE

                        case MdbAddresses.CcChangeGiver:
                            for (int i = 0; i <= CategoryIDs.Length; i++)
                            {
                                if (CategoryIDs[i].Category == CcTalkCategory.ChangeGiver)
                                    return CategoryIDs[i].CategoryStr;
                            }
                            return CcTalkCategory.Unknown.ToString();
                        case MdbAddresses.CcBillValidator:
                            for (int i = 0; i <= CategoryIDs.Length; i++)
                            {
                                if (CategoryIDs[i].Category == CcTalkCategory.BillValidator)
                                    return CategoryIDs[i].CategoryStr;
                            }
                            return CcTalkCategory.Unknown.ToString();
                        case MdbAddresses.CcCashless:
                            for (int i = 0; i <= CategoryIDs.Length; i++)
                            {
                                if (CategoryIDs[i].Category == CcTalkCategory.Cashless)
                                    return CategoryIDs[i].CategoryStr;
                            }
                            return CcTalkCategory.Unknown.ToString();
#endif
                        default:
                            GetCategory();
                            return catstr;
                    }
                }
                else
                {
                    GetCategory();
                    return catstr;
                }
            }
        }
        /// <summary>Product code string of the device.</summary>
        public string ProductCode
        {
            get
            {
                string pcd = "", swstr = "";
                switch (this.Address)
                {
#if !WindowsCE

                    case MdbAddresses.CcChangeGiver:

                        for (int i = 0; i < ChangerIdentifyInstance.Model.Length; i++)
                        {
                            if (ChangerIdentifyInstance.Model[i] == 0) break;
                            pcd = pcd + (Char)ChangerIdentifyInstance.Model[i];
                        }
                        return pcd;
                    case MdbAddresses.CcBillValidator:
                        for (int i = 0; i < BillValidatorIdentifyInstance.Model.Length; i++)
                        {
                            if (BillValidatorIdentifyInstance.Model[i] == 0) break;
                            pcd = pcd + (Char)BillValidatorIdentifyInstance.Model[i];
                        }
                        return pcd;
#if SUPPORT_CASHLESS
                    case MdbAddresses.CcCashless:
                        try
                        {
                            for (int i = 0; i < CashlessIdentify.Model.Length; i++)
                            {
                                if (CashlessIdentify.Model[i] == 0) break;
                                pcd = pcd + (Char)CashlessIdentify.Model[i];
                            }
                        }
                        catch { };
                        return pcd;
#endif
#endif
                    default:
                        return GetStringResponse(244);
                }
            }
        }
        /// <summary>Manufacturer string of the device.</summary>
        public string Manufacturer
        {
            get
            {
                string man = "";
                switch (this.Address)
                {
#if !WindowsCE

                    case MdbAddresses.CcChangeGiver:
                        for (int i = 0; i < ChangerIdentifyInstance.Manufacturer.Length; i++)
                        {
                            if (ChangerIdentifyInstance.Manufacturer[i] == 0) break;
                            man = man + (Char)ChangerIdentifyInstance.Manufacturer[i];
                        }
                        return man;
                    case MdbAddresses.CcBillValidator:
                        for (int i = 0; i < BillValidatorIdentifyInstance.Manufacturer.Length; i++)
                        {
                            if (BillValidatorIdentifyInstance.Manufacturer[i] == 0) break;
                            man = man + (Char)BillValidatorIdentifyInstance.Manufacturer[i];
                        }
                        return man;
#if SUPPORT_CASHLESS
                    case MdbAddresses.CcCashless:
                        try
                        {
                            for (int i = 0; i < CashlessIdentify.Manufacturer.Length; i++)
                            {
                                if (CashlessIdentify.Manufacturer[i] == 0) break;
                                man = man + (Char)CashlessIdentify.Manufacturer[i];
                            }
                        }
                        catch { };
                        return man;
#endif
#endif
                    default:
                        return GetStringResponse(246);
                }
            }
        }
        /// <summary>Software revision string.</summary>
        public string SoftwareRevision
        {
            get
            {
                string verstr = "";
                CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
                CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);
                bool cctalkver = false;

                if (!nospecialaddresses)
                {
                    switch (this.Address)
                    {
#if !WindowsCE
                        case MdbAddresses.CcChangeGiver:
                            verstr = string.Format("V {0:X1}.{1:X02}", ChangerIdentifyInstance.Version, ChangerIdentifyInstance.Release);
                            break;
                        case MdbAddresses.CcBillValidator:
                            verstr = string.Format("V {0:X1}.{1:X02}", BillValidatorIdentifyInstance.Version, BillValidatorIdentifyInstance.Release);
                            break;
#if SUPPORT_CASHLESS
                        case MdbAddresses.CcCashless:
                            verstr = string.Format("V {0:X1}.{1:X02}", CashlessIdentify.Version, CashlessIdentify.Release);
                            break;
#endif
#endif
                        default:
                            verstr = GetStringResponse(241);
                            cctalkver = true;
                            break;
                    }
                }
                else
                {
                    verstr = GetStringResponse(241);
                    cctalkver = true;
                }
                if (cctalkver && (verstr.Trim() == ""))       // Workaround für Azkoyen
                {
                    sdta.Destination = devaddr;
                    sdta.DataLength = 0;
                    sdta.Header = 241;
                    lasterr = TalkCc(sdta, ref rdta);
                    if (rdta.DataLength >= 2)
                    {
                        verstr = string.Format("V{0:d}.{1:d02}", rdta.Data[0], rdta.Data[1]);
                    }
                }

                return verstr;
            }
        }
        /// <summary>Unique serial number of the device as long integer.</summary>
        public long SerialNumber
        {
            get
            {
                string ssn = "";
                long isn = 0;
                if (!nospecialaddresses)
                {
                    switch (this.Address)
                    {
#if !WindowsCE

                        case MdbAddresses.CcChangeGiver:
                            for (int i = 0; i < ChangerIdentifyInstance.Serial.Length; i++)
                            {
                                if (ChangerIdentifyInstance.Serial[i] == 0) break;
                                ssn = ssn + (Char)ChangerIdentifyInstance.Serial[i];
                            }
                            try
                            {
                                isn = long.Parse(ssn);
                            }
                            catch { isn = 0; };
                            return isn;
                        case MdbAddresses.CcBillValidator:
                            for (int i = 0; i < BillValidatorIdentifyInstance.Serial.Length; i++)
                            {
                                if (BillValidatorIdentifyInstance.Serial[i] == 0) break;
                                ssn = ssn + (Char)BillValidatorIdentifyInstance.Serial[i];
                            }
                            try
                            {
                                isn = long.Parse(ssn);
                            }
                            catch { isn = 0; };
                            return isn;
#if SUPPORT_CASHLESS
                        case MdbAddresses.CcCashless:
                            try
                            {
                                for (int i = 0; i < CashlessIdentify.Serial.Length; i++)
                                {
                                    if (CashlessIdentify.Serial[i] == 0) break;
                                    ssn = ssn + (Char)CashlessIdentify.Serial[i];
                                }
                                isn = long.Parse(ssn);
                            }
                            catch { isn = 0; };
                            return isn;
#endif
#endif
                        default:
                            return GetLongResponse(242);
                    }
                }
                else
                {
                    return GetLongResponse(242);
                }
            }
        }

        /// <summary>
        /// Opens the device for communication.
        /// </summary>
        /// <remarks>
        /// Before opening the device set <see cref="Port"/> to the proper value. 
        /// <see cref="CcTalkDeviceList.SearchDevices(byte[])"/> sets the property automatically 
        /// for all available devices.
        /// </remarks>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public virtual CcTalkErrors OpenComm()
        {
            if (isopen)
            {
                lasterr = CcTalkErrors.AlreadyOpen;
                return lasterr;
            }
            //EncryptionSupport = new CcEncryptionSupport(true);
            #region Open Com Port
#if PORTABLE
            portidx = 0;
            if (cctports[0].OpenCount == 0)
            {
                try
                {
                    cctports[0].Port.PortName = portname; // string.Format("COM{0:d}", Port);
                    cctports[0].Port.BaudRate = this.BaudRate;
                    cctports[0].Port.Parity = Parity.None;
                    cctports[0].Port.StopBits = StopBits.One;
                    cctports[0].Port.Handshake = Handshake.None;
                    cctports[0].Port.Encoding = Encoding.Unicode;
                    cctports[0].Port.Open();
                    cctports[0].OpenCount++;
                    isopen = true;
                    lasterr = CcTalkErrors.Ok;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"[ccTalk] SerialPort.Open('{portname}') threw {e.GetType().Name}: {e.Message}");
                    return CcTalkErrors.OpenErr;
                }
            }
            else
            {
                cctports[0].OpenCount++;
                isopen = true;
                lasterr = CcTalkErrors.Ok;
            }
#else
            portidx = -1;
            for (int i = 0; i < cctports.Length; i++)
            {
                if (cctports[i].Port.PortName == Port)
                {
                    portidx = i;
                    cctports[portidx].OpenCount++;
                    break;
                }
            }
            if (portidx < 0)
            {
                for (int i = 0; i < cctports.Length; i++)
                {
                    if (cctports[i].Number == -1)
                    {
                        try
                        {
                            cctports[i].Port.PortName = Port; // string.Format("COM{0:d}", Port);
                            cctports[i].Port.BaudRate = this.BaudRate;
                            cctports[i].Port.Parity = Parity.None;
                            cctports[i].Port.StopBits = StopBits.One;
                            cctports[i].Port.Handshake = Handshake.None;
                            cctports[i].Port.Encoding = Encoding.Unicode;
                            cctports[i].Port.Open();
                            cctports[i].OpenCount++;
                            cctports[i].Number = i; //does not provide the real port number, but I do not want to rewrite the whole lib
                            portidx = i;
                        }
                        catch (Exception e)
                        {
                            Console.Error.WriteLine($"[ccTalk] SerialPort.Open('{Port}') threw {e.GetType().Name}: {e.Message}");
                            return CcTalkErrors.OpenErr;
                        }
                        break;
                    }
                }
            }
            if (portidx >= 0)
            {
                lasterr = CcTalkErrors.Ok;
            }
            else
            {
                lasterr = CcTalkErrors.OpenErr;
            }
            isopen = portidx >= 0;
#endif
            #endregion

            #region Setting up serial port of CCT 910
            CcTalkDataBlock sndblk = new CcTalkDataBlock(CcTalkChecksumTypes.Simple8);
            CcTalkDataBlock rcvblk = new CcTalkDataBlock(CcTalkChecksumTypes.Simple8);
            CcTalkErrors res = CcTalkErrors.Unknown;
            if (!nospecialaddresses)
            {
                switch (this.Address)
                {
                    default:
                        break;
                }
            }
            #endregion

            // Test if there's anybody at home
            if (this.Address != 0)
            {
                lasterr = SimplePoll();
                if (lasterr == CcTalkErrors.Ok)
                {
                    #region MDB: Get Identify
                    MdbDataBlock smdb = new MdbDataBlock(true);
                    MdbDataBlock rmdb = new MdbDataBlock(true);

                    if (!nospecialaddresses)
                    {
                        switch (this.Address)
                        {
                            case MdbAddresses.CcChangeGiver:
                            case MdbAddresses.CcBillValidator:
                            case MdbAddresses.CcCashless:
                                this.ChecksumType = CcTalkChecksumTypes.Simple8;
                                break;
                        }

#if !WindowsCE
                        switch (this.Address)
                        {
                            case MdbAddresses.CcChangeGiver:
                                // Identify
                                ChangerIdentifyInstance.InitStructure();
                                smdb.DataLength = 2;
                                smdb.Data[0] = MdbAddresses.MdbChangeGiver | MdbCommands.Expansion;
                                smdb.Data[1] = MdbCommands.Identify;
                                lasterr = TalkMdb(smdb, ref rmdb);
                                if (lasterr == CcTalkErrors.Ok)
                                {
                                    if (!ChangerIdentifyInstance.GetFromBuffer(rmdb))
                                    {
                                    }
                                }
                                break;
                            case MdbAddresses.CcBillValidator:
                                // Identify
                                BillValidatorIdentifyInstance.InitStructure();
                                smdb.DataLength = 2;
                                smdb.Data[0] = MdbAddresses.MdbBillValidator | MdbCommands.Expansion;
                                smdb.Data[1] = MdbCommands.Identify;
                                lasterr = TalkMdb(smdb, ref rmdb);
                                if (lasterr == CcTalkErrors.Ok) BillValidatorIdentifyInstance.GetFromBuffer(rmdb);
                                BillInhibit = 0x0000;
                                BillMasterInhibit = false;
                                BillEscrowEnable = true;
                                break;
                            case MdbAddresses.CcCashless:
                                CashlessConfigInstance.InitStructure();
                                smdb.DataLength = Marshal.SizeOf(CashlessConfigInstance) + 1;
                                smdb.Data[0] = (byte)(cashlessaddr | MdbCommands.CPSetup);
                                CashlessConfigInstance.SetToBuffer(smdb, 1);
                                lasterr = TalkCPMdb(smdb, ref rmdb, CashlessPollEvent.Config);
                                if ((rmdb.DataLength == 0) || (lasterr != CcTalkErrors.Ok))
                                {
                                    lasterr = CcTalkErrors.UnSupported;
                                }
                                else
                                {
                                    if (lasterr == CcTalkErrors.Ok)
                                    {
                                        CashlessSetupInstance.GetFromBuffer(rmdb);
                                    }

                                    LibraryIdentify.InitStructure(true);
                                    CashlessIdentifyInstance.InitStructure(false);
                                    smdb.DataLength = Marshal.SizeOf(LibraryIdentify) + 1;
                                    smdb.Data[0] = (byte)(cashlessaddr | MdbCommands.Expansion);
                                    smdb.Data[1] = MdbCommands.Identify;
                                    LibraryIdentify.SetToBuffer(smdb, 1);
                                    lasterr = TalkCPMdb(smdb, ref rmdb, CashlessPollEvent.Identify);
                                    if (lasterr == CcTalkErrors.Ok)
                                        CashlessIdentifyInstance.GetFromBuffer(rmdb);
                                    else
                                        lasterr = CcTalkErrors.UnSupported;
                                }
                                break;
                        }
#endif
                    }
                    #endregion
                }
                else
                {
                    CloseComm();
                    lasterr = CcTalkErrors.NoDevice;
                }
            }
            return lasterr;
        }

        /// <summary>
        /// Closes a previously opened device.
        /// </summary>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public virtual CcTalkErrors CloseComm()
        {
            if (!isopen)
            {
                lasterr = CcTalkErrors.NotOpen;
                return lasterr;
            }
            lasterr = CcTalkErrors.Ok;
#if PORTABLE
            if (cctports[0].OpenCount > 1)
            {
            }
            else
            {
                try
                {
                    cctports[portidx].Port.Close();
                }
                catch
                {
                    lasterr = CcTalkErrors.CloseErr;
                }
                cctports[0].OpenCount--;
            }
#else
            isopen = false;
            while (commpdg) ;
            lasterr = CcTalkErrors.Ok;
            cat = CcTalkCategory.Unknown;
            catstr = "";
            lasterr = CcTalkErrors.CloseErr;
            if (cctports[portidx].Number > -1)
            {
                cctports[portidx].OpenCount--;
                lasterr = CcTalkErrors.Ok;
                if (cctports[portidx].OpenCount == 0)
                {
                    try
                    {
                        cctports[portidx].Port.Close();
                        cctports[portidx].Number = -1;
                        lasterr = CcTalkErrors.Ok;
                    }
                    catch { lasterr = CcTalkErrors.CloseErr; }
                }
                portidx = -1;
            }
            else
            {
                lasterr = CcTalkErrors.CloseErr;
            }
#endif
            isopen = false;
            return lasterr;
        }

#if PUBLIC_TALKCC
        /// <summary>
        /// Sends a ccTalk block to the bus and tries to receive an answer. 
        /// </summary>
        /// <param name="sndblk">The data to be send. The "Destination" field will be replaced by the "Address" property of the class. The "Source" field will always be set to 1. 
        /// The checksum will be calculated automatically depending on the "CheckSumType" property.</param>
        /// <param name="rcvblk">The received data.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>

        public CcTalkErrors TalkCc(CcTalkDataBlock sndblk, ref CcTalkDataBlock rcvblk)
#else
        internal CcTalkErrors TalkCc(CcTalkDataBlock sndblk, ref CcTalkDataBlock rcvblk)
#endif
        {
            return TalkCc(sndblk, ref rcvblk, rcvtot);
        }

        internal CcTalkErrors TalkCc(CcTalkDataBlock sndblk, ref CcTalkDataBlock rcvblk, int tmot)
        {
            if (!isopen)
            {
                lasterr = CcTalkErrors.NotOpen;
                return lasterr;
            }
            if (!nospecialaddresses)
            {
                switch (devaddr)
                {
                    case MdbAddresses.CcChangeGiver:
                    case MdbAddresses.CcBillValidator:
                    case MdbAddresses.CcCashless:
                        sndblk.Destination = dongleaddr;
                        break;
                    default:
                        sndblk.Destination = devaddr;
                        break;
                }
            }
            else
            {
                sndblk.Destination = devaddr;
            }
            while (commpdg) Thread.Sleep(5);
            commpdg = true;
            //if (CommDelay > 0) Thread.Sleep(CommDelay);
            if (MinimalCommInterval > 0)
            {
                while ((Environment.TickCount - lastcommtick) < MinimalCommInterval)
                {
                    Thread.Sleep(1);
                }
            }
            int trycnt = CommRetry;
            do
            {
                lasterr = SendData(sndblk);
                if (lasterr == CcTalkErrors.Ok) lasterr = ReceiveData(tmot, ref rcvblk);
                if (lasterr != CcTalkErrors.Ok) commpdg = false;
            } while ((lasterr == CcTalkErrors.RcvTimout) && (--trycnt > 0));
            lastcommtick = Environment.TickCount;
            commpdg = false;
            //if (rcvblk.Destination != hostaddr) 
            //    lasterr = CcTalkErrors.WrongAddr;
            if (rcvblk.Header == 0x05) lasterr = CcTalkErrors.NoAck;
            return lasterr;
        }
        #region DES Encryption
        /// <summary>
        /// Retrieve the current support for encryption methodes. 
        /// Results will be stored in the <see cref="EncryptionSupport"/> field.
        /// If the coins selector is in the Trusted Key Exchange Mode the keys for protocol level encryption and command level
        /// encryption will be set as far as applicable.
        /// The DES functionality is only applicable to native ccTalk coin selectors and bill validators.
        /// </summary>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetEncryptionSupport()
        {
            #region DES support only for native ccTalk Devices
            switch (this.Address)
            {
                case MdbAddresses.CcBillValidator:
                    return CcTalkErrors.UnSupported;
            }
            #endregion

            CcTalkChecksumTypes oldcstype = this.cstype;
            CcTalkEncryption oldencryption = this.EncryptionMode;

            this.EncryptionMode = CcTalkEncryption.None;
            this.cstype = CcTalkChecksumTypes.Simple8;

            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            //EncryptionSupport = new CcEncryptionSupport(true);

            sdta.Destination = devaddr;
            sdta.DataLength = 6;
            sdta.Data[0] = 0xaa;
            sdta.Data[1] = 0x55;
            sdta.Data[2] = 0x00;
            sdta.Data[3] = 0x00;
            sdta.Data[4] = 0x55;
            sdta.Data[5] = 0xaa;
            sdta.Header = 111;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr == CcTalkErrors.Ok)
            {
                if (rdta.DataLength > 16)
                {
                    EncryptionSupport.ProtocolLevel = (CcTalkEncryption)rdta.Data[0];
                    EncryptionSupport.CommandLevel = (CcTalkCryptography)rdta.Data[1];
                    EncryptionSupport.ProtocolKeySize = rdta.Data[2];
                    EncryptionSupport.CommandKeySize = rdta.Data[3];
                    EncryptionSupport.BlockSize = rdta.Data[4];
                    EncryptionSupport.OperatingMode = (rdta.Data[5] != 255 ? OperatingMode.Normal : OperatingMode.Trusted);
#if DES_SUPPORT
                    if ((EncryptionSupport.CommandLevel == CcTalkCryptography.DES)) // && (EncryptionSupport.OperatingMode == OperatingMode.Trusted))
                    {
                        int deskeylen = Math.Min(EncryptionSupport.DESKey.Length, descrypt.Key.Length);
                        for (int i = 0; i < deskeylen; i++)
                        {
                            EncryptionSupport.DESKey[i] = rdta.Data[9 + i];
                            //EncryptionSupport.DESKey[8 - i - 1] = rdta.Data[9 + i];;
                        }
                    }
                    // Verschlüsselung initialisieren
                    this.EncryptionMode = EncryptionSupport.ProtocolLevel;
                    if (EncryptionSupport.CommandLevel == CcTalkCryptography.DES) InitDESEncryption();
#else
                    lasterr = CcTalkErrors.UnsupportedEncryption;
#endif
                }
                else
                {
                    EncryptionSupport = new CcEncryptionSupport(true);
                    lasterr = CcTalkErrors.DataFormat;
                }
            }
            this.cstype = oldcstype;
            return lasterr;
        }

        /// <summary>
        /// Initialises DES encryption with the provided key. 
        /// </summary>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors InitDESEncryption(byte[] DESKey)
        {
            if (DESKey.Length < 8) return CcTalkErrors.DESKeyLength;

            EncryptionSupport.DESKey = DESKey;
            return InitDESEncryption();
        }
        /// <summary>
        /// Initialises DES encryption with the key from the <see cref="EncryptionSupport"/> field.
        /// </summary>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors InitDESEncryption()
        {
#if DES_SUPPORT
            try
            {
                descrypt.Mode = CipherMode.ECB;
                descrypt.Padding = PaddingMode.Zeros;
                descrypt.KeySize = EncryptionSupport.CommandKeySize;
                descrypt.BlockSize = EncryptionSupport.BlockSize;

                descrypt.Key = EncryptionSupport.DESKey;
                descrypt.IV = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, };
                decryptor = descrypt.CreateDecryptor();
                encryptor = descrypt.CreateEncryptor();

                lasterr = CcTalkErrors.Ok;
            }
            catch (Exception e) { lasterr = CcTalkErrors.InitEncryption; }
#else
            lasterr = CcTalkErrors.UnsupportedEncryption;
#endif
            return lasterr;
        }

        /// <summary>
        /// Verifies the current DES key.
        /// </summary>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if the current key is the correct one otherwise an error code.
        /// </returns>
        public CcTalkErrors VerifyDESKey()
        {
#if DES_SUPPORT
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.Destination = devaddr;
            sdta.DataLength = 16;
            sdta.Header = 110;
            for (int i = 0; i < 8; i++)
            {
                sdta.Data[2 * i + 0] = EncryptionSupport.DESKey[i];
                sdta.Data[2 * i + 1] = EncryptionSupport.DESKey[i];
            }

            int oldrcvto = this.rcvtot;
            this.rcvtot = 200;
            if (encryptdesblock(ref sdta) == CcTalkErrors.Ok)
            {
                lasterr = TalkCc(sdta, ref rdta);
            }
            this.rcvtot = oldrcvto;
#else
            lasterr = CcTalkErrors.UnsupportedEncryption;
#endif
            return lasterr;

        }
        /// <summary>
        /// Changes the active DES key.
        /// </summary>
        /// <param name="newkey">The new DES key.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SwitchDESKey(byte[] newkey)
        {
#if DES_SUPPORT
            if (newkey.Length < 8) return CcTalkErrors.DESKeyLength;

            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.Destination = devaddr;
            sdta.DataLength = 16;
            sdta.Header = 110;
            for (int i = 0; i < 8; i++)
            {
                sdta.Data[2 * i + 0] = EncryptionSupport.DESKey[i];
                sdta.Data[2 * i + 1] = newkey[i];
            }

            int oldrcvto = this.rcvtot;
            this.rcvtot = 200;
            if (encryptdesblock(ref sdta) == CcTalkErrors.Ok)
            {
                lasterr = TalkCc(sdta, ref rdta);
            }
            this.rcvtot = oldrcvto;
#else
            lasterr = CcTalkErrors.UnsupportedEncryption;
#endif
            return lasterr;
        }
        #endregion

        /// <summary>
        /// Sends a Simple Poll to check if device is alive.
        /// </summary>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public virtual CcTalkErrors SimplePoll()
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            MdbDataBlock smdb = new MdbDataBlock(true);
            MdbDataBlock rmdb = new MdbDataBlock(true);

            if (nospecialaddresses)
            {
                sdta.Destination = devaddr;
                sdta.DataLength = 0;
                sdta.Header = 254;
                lasterr = TalkCc(sdta, ref rdta);
            }
            else
            {
                switch (this.Address)
                {
#if !WindowsCE

                    case MdbAddresses.CcChangeGiver:
                        smdb.DataLength = 1;
                        smdb.Data[0] = MdbAddresses.MdbChangeGiver | MdbCommands.Poll;
                        lasterr = TalkMdb(smdb, ref rmdb);
                        break;
                    case MdbAddresses.CcBillValidator:
                        smdb.DataLength = 1;
                        smdb.Data[0] = MdbAddresses.MdbBillValidator | MdbCommands.Poll;
                        lasterr = TalkMdb(smdb, ref rmdb);
                        break;
                    case MdbAddresses.CcCashless:
                        smdb.DataLength = 1;
                        if (cashlessaddr == 0x00)
                        {
                            for (int i = 0; i < MdbAddresses.MdbCashless.Length; i++)
                            {
                                smdb.Data[0] = (byte)(MdbAddresses.MdbCashless[i] | MdbCommands.CPPoll);
                                lasterr = TalkMdb(smdb, ref rmdb);
                                if (lasterr == CcTalkErrors.Ok)
                                {
                                    cashlessaddr = MdbAddresses.MdbCashless[i];
                                    break;
                                }
                            }
                        }
                        else
                        {
                            smdb.Data[0] = (byte)(cashlessaddr | MdbCommands.CPPoll);
                            lasterr = TalkMdb(smdb, ref rmdb);
                        }
                        break;
#endif
                    default:
                        sdta.Destination = devaddr;
                        sdta.DataLength = 0;
                        sdta.Header = 254;
                        lasterr = TalkCc(sdta, ref rdta);
                        break;
                }
            }
            return lasterr;
        }

        /// <summary>
        /// Sends break to reset the device the hard way.
        /// </summary>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SendBreak()
        {
            if (!isopen) return CcTalkErrors.NotOpen;

            switch (this.Address)
            {
#if !WindowsCE

                case MdbAddresses.CcChangeGiver:
                case MdbAddresses.CcBillValidator:
                    lasterr = SendMdbBreak();
                    break;
#endif
                default:
                    evtctr = 0;
                    lasterr = CcTalkErrors.Ok;
                    Thread.Sleep(200);

                    break;
            }
            return lasterr;
        }

        /// <summary>
        /// Sends the reset command to reset the device the soft way.
        /// </summary>
        /// <param name="wt">If wt != 0 the function will wait this number of milliseconds before returning.</param>        
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public virtual CcTalkErrors ResetDevice(int wt)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            if (!isopen) return CcTalkErrors.NotOpen;

            MdbDataBlock smdb = new MdbDataBlock(true);
            MdbDataBlock rmdb = new MdbDataBlock(true);

            if (nospecialaddresses)
            {
                sdta.Destination = devaddr;
                sdta.DataLength = 0;
                sdta.Header = 1;
                lasterr = SendData(sdta);
                if (lasterr != CcTalkErrors.Ok) return lasterr;
                lasterr = ReceiveData(1000, ref rdta);
                //if (lasterr != CcTalkErrors.Ok) return lasterr;
                lasterr = CcTalkErrors.Ok;
                //if (rdta.Destination != hostaddr) lasterr = CcTalkErrors.WrongAddr;

                evtctr = -1;
            }
            else
            {
                switch (this.Address)
                {
#if !WindowsCE

                    case MdbAddresses.CcChangeGiver:
                        smdb.DataLength = 1;
                        smdb.Data[0] = MdbAddresses.MdbChangeGiver | MdbCommands.Reset;
                        lasterr = TalkMdb(smdb, ref rmdb);
                        break;
                    case MdbAddresses.CcBillValidator:
                        smdb.DataLength = 1;
                        smdb.Data[0] = MdbAddresses.MdbBillValidator | MdbCommands.Reset;
                        lasterr = TalkMdb(smdb, ref rmdb);
                        break;
                    case MdbAddresses.CcCashless:
                        smdb.DataLength = 1;
                        smdb.Data[0] = (byte)(cashlessaddr | MdbCommands.Reset);
                        lasterr = TalkMdb(smdb, ref rmdb);
                        break;
#endif
                    default:
                        sdta.Destination = devaddr;
                        sdta.DataLength = 0;
                        sdta.Header = 1;
                        lasterr = SendData(sdta);
                        if (lasterr != CcTalkErrors.Ok) return lasterr;
                        lasterr = ReceiveData(1000, ref rdta);
                        //if (lasterr != CcTalkErrors.Ok) return lasterr;
                        lasterr = CcTalkErrors.Ok;
                        //if (rdta.Destination != hostaddr) lasterr = CcTalkErrors.WrongAddr;

                        evtctr = -1;
                        break;
                }
            }
            if (wt > 0) Thread.Sleep(wt);
            return lasterr;
        }

        /// <summary>
        /// Counts unsuccessfull communication attempts. Any successfull communication will set it back to 0.
        /// </summary>
        public int ErrorCount = 0;

        #region Private variables, methods etc.
        #region Liste erlaubter Hersteller
        // Check if it is a supported brand
        internal bool IsSupported(string manufacturer)
        {
            return true;
            //for (int m = 0; m < SupportedBrands.Length; m++)
            //{
            //    if (SupportedBrands[m].ToLower() == manufacturer.ToLower())
            //        return true;
            //}
            //return false;
        }
        internal string[] SupportedBrands = new string[]
        {
            "WHM",
            "HOPPER Srl",
            "JCM",
            "PTI"
        };
        #endregion

        #region Gerätekategorien
        [Serializable]
        private struct CategoryIdentifier
        {
            public CcTalkCategory Category;
            public string CategoryStr;
            public string DisplayStr;
            public CategoryIdentifier(CcTalkCategory cat, string catstr)
            {
                Category = cat;
                CategoryStr = catstr;
                DisplayStr = CategoryStr;
            }
            public CategoryIdentifier(CcTalkCategory cat, string catstr, string dispstr)
            {
                Category = cat;
                CategoryStr = catstr;
                DisplayStr = dispstr;
            }
        }
        private CategoryIdentifier[] CategoryIDs = new CategoryIdentifier[]
        {
            new CategoryIdentifier(CcTalkCategory.Bootloader, "Bootloader"),
            new CategoryIdentifier(CcTalkCategory.CoinSelector, "Coin Acceptor"),
            new CategoryIdentifier(CcTalkCategory.BillValidator, "Bill Validator"),
            new CategoryIdentifier(CcTalkCategory.BillValidator, "Bill Dispenser"),
            new CategoryIdentifier(CcTalkCategory.PayOut, "Coin Inject System"),
            new CategoryIdentifier(CcTalkCategory.PayOut, "Payout"),
            new CategoryIdentifier(CcTalkCategory.PayOut, "Dispenser"),
            new CategoryIdentifier(CcTalkCategory.PayOut, "Payout Multi"),
            new CategoryIdentifier(CcTalkCategory.CoinScale, "Coin Scale"),
            new CategoryIdentifier(CcTalkCategory.CoinScale, "Hopper Scale"),
            new CategoryIdentifier(CcTalkCategory.Peripheral, "Peripheral Device", "Dongle"),
            new CategoryIdentifier(CcTalkCategory.Peripheral, "Dongle"),
            new CategoryIdentifier(CcTalkCategory.ChangeGiver, "Change Giver"),
            new CategoryIdentifier(CcTalkCategory.Changer, "Changer"),
            new CategoryIdentifier(CcTalkCategory.CoinFeeder, "Coin Feeder"),
            new CategoryIdentifier(CcTalkCategory.EscrowSorter, "Escrow Sorter"),
            new CategoryIdentifier(CcTalkCategory.Cashless, "Cashless Payment"),
            new CategoryIdentifier(CcTalkCategory.Cashless, "Card Reader"),
        };
        #endregion

        internal const int MAX_BLOCK_LENGTH = 260;
        internal bool localecho;
        internal int badlinectr = 0;
        internal bool isopen, commpdg;
        internal int rcvtot, bytetot;
        private string _portname = "";
        internal CcTalkCategory cat;
        internal string catstr;

        // Für MDB und serielle Geräte
        internal byte dongleaddr = 80;
        internal byte cashlessaddr = 0x00; // MdbAddresses.MdbCashless[0];
        internal byte usartidx = 0;
        internal bool ack = true;

        #region ccTalk Cash Device Data Block
#if PUBLIC_TALKMDB || PUBLIC_TALKCC
        /// <summary>Structure for sending and receiving ccTalk data.</summary>
        public class CcTalkDataBlock
#else
        [Serializable]
        internal class CcTalkDataBlock
#endif
        {
            // "Constants"
            internal int MaxBlockLength
            {
                get { return 260; }
            }
            // Structs and classes
            /// <summary>Structure for accessing the data bytes.</summary>
            public struct DataBuff
            {
                internal DataBuff(int size)
                {
                    dsz = size;
                    bytes = new byte[size];
                }
                /// <summary>Indexed access to the data bytes.</summary>
                public byte this[int idx]
                {
                    get { if ((idx >= 0) && (idx < dsz)) return bytes[idx]; else return 0; }
                    set { if ((idx >= 0) && (idx < dsz)) bytes[idx] = value; }
                }
                internal int DataSize
                {
                    get { return dsz; }
                }

                internal byte[] bytes;
                private int dsz;
            }
            // Methods
#if TRACE_FUNCTION
            /// <summary>Get the raw data of the block.</summary>
            public byte GetRawData(int idx)
#else
            internal byte GetRawData(int idx)
#endif
            {
                if (idx >= MaxBlockLength) return 0xff;
                switch (idx)
                {
                    case 0: return destaddr;
                    case 1: return datalen;
                    case 2: return srcaddr;
                    case 3: return header;
                    default:
                    {
                        if (idx > 3)
                        {
                            return Data[idx - 4];
                        }
                        else
                        {
                            return 0xff;
                        }
                    }
                }
            }
            internal void SetRawData(int idx, byte value)
            {
                if (idx >= MaxBlockLength) return;
                switch (idx)
                {
                    case 0: { destaddr = value; break; }
                    case 1: { datalen = value; break; }
                    case 2: { srcaddr = value; break; }
                    case 3: { header = value; break; }
                    default: { if (idx > 3) Data[idx - 4] = value; break; }
                }
            }
            internal bool IdenticalWith(CcTalkDataBlock cdta)
            {
                int i;

                if (cdta.DataLength != datalen) return false;
                for (i = 0; i < datalen + 4; i++)
                    if (cdta.GetRawData(i) != this.GetRawData(i)) return false;
                return true;
            }
            internal void CopyFrom(CcTalkDataBlock sdta)
            {
                int i;

                for (i = 0; i < sdta.DataLength + 5; i++) this.SetRawData(i, sdta.GetRawData(i));
            }
            internal void Clear()
            {
                int i;

                for (i = 0; i < 5; i++) this.SetRawData(i, 0);
                docalccsum = true;
            }
            // Properties
            /// <summary>Address of the destination device.</summary>
            public byte Destination
            {
                get { return destaddr; }
                set { destaddr = value; }
            }
            /// <summary>Number of data bytes.</summary>
            public byte DataLength
            {
                get { return datalen; }
                set { datalen = value; }
            }
            /// <summary>Address of the Source device, usually 1.</summary>
            public byte Source
            {
                get { return srcaddr; }
                set { srcaddr = value; }
            }
            /// <summary>The ccTalk header i.a. the command.</summary>
            public byte Header
            {
                get { return header; }
                set { header = value; }
            }
            /// <summary>The data bytes.</summary>
            public DataBuff Data;
            /// <summary>The ccTalk checksum will be calculated automatically while sending.</summary>
            public ushort CheckSum
            {
                set
                {
                    switch (cstype)
                    {
                        case CcTalkChecksumTypes.Simple8:
                        {
                            checksum = (byte)value;
                            break;
                        }
                        case CcTalkChecksumTypes.CRC16:
                        {
                            srcaddr = (byte)(value);
                            checksum = (byte)(value >> 8);
                            break;
                        }
                    }

                    docalccsum = false;
                }
                get { return calcchsum(); }
            }
            internal bool DoCalcChecksum
            {
                set { docalccsum = value; }
                get { return docalccsum; }
            }
            internal bool CheckSumOk
            {
                get
                {
                    switch (cstype)
                    {
                        case CcTalkChecksumTypes.Simple8: return (Data[datalen] == (byte)calcchsum());
                        case CcTalkChecksumTypes.CRC16: return ((Data[datalen] * 256 + srcaddr) == calcchsum());
                        default: return false;

                    }
                }
            }
            /// <summary>Determines the way the checksum will be calculated when sending and will be checked when receiving.</summary>
            public CcTalkChecksumTypes CheckSumType
            {
                get { return cstype; }
                set { cstype = value; }
            }

            #region Private variables and methods
            private byte destaddr;
            private byte datalen;
            private byte srcaddr;
            private byte header;
            private byte checksum;
            private CcTalkChecksumTypes cstype = CcTalkChecksumTypes.Simple8;
            private bool docalccsum = true;

            private ushort calcchsum()
            {
                int csum = 0;
                if (docalccsum)
                {
                    switch (cstype)
                    {
                        case CcTalkChecksumTypes.Simple8:
                        {
                            csum = destaddr + datalen + srcaddr + header;
                            for (int i = 0; i < datalen; i++) csum += Data[i];
                            csum = 256 - (csum & 0x00ff);
                            break;
                        }
                        case CcTalkChecksumTypes.CRC16:
                        {
                            csum = 0x0000;
                            for (int i = 0; i < datalen + 4; i++) if (i != 2)
                            {
                                csum ^= (GetRawData(i) << 8);
                                for (int j = 0; j < 8; j++)
                                {
                                    if ((csum & 0x8000) != 0)
                                        csum = (csum << 1) ^ 0x1021;
                                    else
                                        csum <<= 1;
                                }
                            }
                            break;
                        }
                    }
                    return (ushort)csum;
                }
                else
                {
                    switch (cstype)
                    {
                        case CcTalkChecksumTypes.Simple8: return checksum;
                        case CcTalkChecksumTypes.CRC16: return (ushort)(checksum * 256 + srcaddr);
                        default: return 0;
                    }
                }
            }

            #endregion
            /// <summary>Creates an new instance with the given checksum type.</summary>
            public CcTalkDataBlock(CcTalkChecksumTypes checksumtype)
            {
                Data = new DataBuff(MaxBlockLength);
                cstype = checksumtype;
            }
        }
        #endregion

        #region Encryption
        //private CcTalkEncryption EncMode = CcTalkEncryption.None;

        private byte[] tapArray = new byte[] { 7, 4, 5, 3, 1, 2, 3, 2, 6, 1 };
        private byte[] secArray = new byte[6];

        private const int rotatePlaces = 12;
        private const byte feedMasher = 0x63;

        private void ccDecrypt(ref CcTalkDataBlock encblk)
        {
            int b, c, cl, n;
            byte xorKey;
            byte[] buffer = new byte[260];

            n = encblk.DataLength + 3;
            for (int i = 0; i < n; i++) buffer[i] = encblk.GetRawData(i + 2);

            #region Original MC algorythm
            xorKey = (byte)(16 * secArray[2] + secArray[2]);
            for (int i = 0; i < n; i++) buffer[i] ^= xorKey;

            for (int i = rotatePlaces - 1; i >= 0; i--)
            {
                cl = (buffer[0] & 0x80) != 0 ? 1 : 0;
                for (int j = 0; j < n; j++)
                {
                    if ((buffer[j] & (1 << (tapArray[(secArray[1] + j) % 10] - 1))) != 0) cl ^= 1;
                }
                for (int j = n - 1; j >= 0; j--)
                {
                    c = (buffer[j] & 0x80) != 0 ? 1 : 0;
                    if (((secArray[5] ^ feedMasher) & (1 << ((i + j - 1) % 8))) != 0) c ^= 1;
                    buffer[j] = (byte)((buffer[j] << 1) + cl);
                    cl = c;
                }
            }
            for (int i = 0; i < n; i++)
            {
                if ((secArray[3] & (1 << (i % 4))) != 0)
                {
                    b = 0;
                    for (int j = 0; j < 8; j++)
                    {
                        if ((buffer[i] & (1 << j)) != 0) b += (128 >> j);
                    }
                    buffer[i] = (byte)b;
                }
            }
            xorKey = (byte)(~(16 * secArray[0] + secArray[4]));
            for (int i = 0; i < n; i++) buffer[i] ^= xorKey;
            #endregion

            for (int i = 0; i < n; i++) encblk.SetRawData(i + 2, buffer[i]);
        }
        #endregion

        #region Basic ccTalk communication
        internal bool receivingecho = false;
        // Send routine
        private CcTalkErrors SendData(CcTalkDataBlock sdta)
        {
            int i;
            uint cnt;
            cnt = (uint)sdta.DataLength + 5;
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock odta = new CcTalkDataBlock(cstype);
            CcTalkErrors res = CcTalkErrors.Ok;

#if TRACE_FUNCTION
            CcTalkSendReceiveData logdata = new CcTalkSendReceiveData(true);

#endif

            sdta.Source = hostaddr;

            byte[] sndbff = new byte[MAX_BLOCK_LENGTH];

            if (res == CcTalkErrors.Ok)
            {
                switch (sdta.CheckSumType)
                {
                    case CcTalkChecksumTypes.Simple8:
                    {
                        sdta.Data[sdta.DataLength] = (byte)sdta.CheckSum;
                        break;
                    }
                    case CcTalkChecksumTypes.CRC16:
                    {
                        sdta.Data[sdta.DataLength] = (byte)(sdta.CheckSum >> 8);
                        sdta.Source = (byte)sdta.CheckSum;
                        break;
                    }
                }
                odta.CopyFrom(sdta);
                for (i = 0; i < cnt; i++) sndbff[i] = sdta.GetRawData(i);
                try
                {
                    cctports[portidx].Port.DiscardOutBuffer();
                    cctports[portidx].Port.DiscardInBuffer();
                }
                catch { res = CcTalkErrors.Internal; }
            }
            if (res == CcTalkErrors.Ok)
            {
                try
                {
                    cctports[portidx].Port.Write(sndbff, 0, (int)cnt);
                }
                catch { res = CcTalkErrors.SendErr; }
            }
            if (res == CcTalkErrors.Ok)
            {
                if (localecho)
                {
                    receivingecho = true;
                    if ((res = ReceiveData(10 * (int)cnt, ref rdta)) != CcTalkErrors.Ok)
                    {
                        res = CcTalkErrors.BadLine;
                    }
                    if (res == CcTalkErrors.Ok)
                    {
                        for (i = 0; i < cnt; i++)
                        {
                            if (odta.GetRawData(i) != rdta.GetRawData(i))
                            {
                                res = CcTalkErrors.BadLine;
                            }
                        }
                    }
                    receivingecho = false;
                    if (res == CcTalkErrors.BadLine)
                    {
                        badlinectr++;
                        if (badlinectr < 5)
                        {
                            res = CcTalkErrors.Ok;
                        }
                    }
                    else
                    {
                        badlinectr = 0;
                    }
                }
            }
#if TRACE_FUNCTION
            if (LogCctalkCommunication != null)
            {
                logdata.Activity = CctalkActivity.Sent;
                logdata.DataBlock.CopyFrom(sdta);
                logdata.Status = res;
                LogCctalkCommunication(logdata);
            }
#endif
            return res;
        }
        // Receive Routine
        private CcTalkErrors ReceiveData(int tmot, ref CcTalkDataBlock rdta)
        {
            int i;
            uint ridx, rcnt;
            byte[] btrcv = new byte[16];
            rcnt = (uint)rdta.MaxBlockLength;
            byte[] rcvbff = new byte[MAX_BLOCK_LENGTH];
            CcTalkErrors res = CcTalkErrors.Ok;

#if TRACE_FUNCTION
            CcTalkSendReceiveData logdata = new CcTalkSendReceiveData(true);
#endif

            ridx = 0;
            cctports[portidx].Port.ReadTimeout = 1;
            int maxtick = Environment.TickCount + tmot * 2;

            if (res == CcTalkErrors.Ok)
            {
                try
                {
                    while (rcnt > 0)
                    {
                        while (cctports[portidx].Port.BytesToRead < 1)
                        {
                            if (Environment.TickCount > maxtick)
                            {
                                res = CcTalkErrors.RcvTimout;
                                break;
                            }
                            Thread.Sleep(10);
                        }
                        cctports[portidx].Port.Read(rcvbff, (int)ridx, 1);
                        if (ridx == 1) rcnt = (uint)(rcvbff[1] + 4);
                        ridx++;
                        rcnt--;
                        if (ridx > 0) maxtick = Environment.TickCount + bytetot * 2;
                    }
                }
                catch (Exception e) { res = CcTalkErrors.RcvTimout; } // { res = CcTalkErrors.Internal; }
            }

            if (res == CcTalkErrors.Ok)
            {
                for (i = 0; i < rcvbff[1] + 5; i++) rdta.SetRawData(i, rcvbff[i]);
                if (!rdta.CheckSumOk) res = CcTalkErrors.ChSumErr;
            }

#if TRACE_FUNCTION
            if (!receivingecho && (LogCctalkCommunication != null))
            {
                logdata.Activity = CctalkActivity.Received;
                logdata.DataBlock.CopyFrom(rdta);
                logdata.Status = res;
                LogCctalkCommunication(logdata);
            }
#endif
            return res;
        }

        internal CcTalkErrors AddressPoll(out int adrcnt, ref byte[] addrs)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);

            adrcnt = 0;
            if (!isopen)
            {
                lasterr = CcTalkErrors.NotOpen;
                return lasterr;
            }
            sdta.DataLength = 0;
            sdta.Destination = 0;
            sdta.Header = 253;
            lasterr = SendData(sdta);

            int maxtick = Environment.TickCount + 1500;
            adrcnt = 0;
            int rcnt = 254;
            try
            {
                while ((rcnt > 0) && (Environment.TickCount < maxtick))
                {
                    while (cctports[portidx].Port.BytesToRead < 1)
                    {
                        if (Environment.TickCount > maxtick) return CcTalkErrors.RcvTimout;
                        Thread.Sleep(1);
                    }
                    cctports[portidx].Port.Read(addrs, (int)adrcnt, 1);
                    adrcnt++;
                    rcnt--;
                }
            }
            catch { return CcTalkErrors.Internal; }
            return lasterr;
        }

        #region Value factor characters
        [Serializable]
        internal struct ValueFactorEntry
        {
            public ValueFactorEntry(char facc, double face)
            {
                FactorChar = facc;
                Factor = (long)Math.Pow(10, face);
            }
            public char FactorChar;
            public long Factor;
        }
        internal ValueFactorEntry[] ValueFactors = new ValueFactorEntry[]
        {
            new ValueFactorEntry('m', -3),
            new ValueFactorEntry(' ', 0),
            new ValueFactorEntry('.', 0),
            new ValueFactorEntry('K', 3),
            new ValueFactorEntry('M', 6),
            new ValueFactorEntry('G', 9),
        };
        #endregion

        #endregion

        #region MDB Communication
        // MDB Constants
        internal int mdbtot = 20;
        internal const int MdbCcTalkTimeout = 150;
        // MDB Status of CCT 900
        [Flags]
        internal enum CcMdbFlags
        {
            None = 0x00,
            BlockReceived = 0x01,
            ChecksumError = 0x02,
            ReceiveTimeout = 0x04,
            SendingBreak = 0x08,
        }
        // MDB Data Block
        [Serializable]
#if PUBLIC_TALKMDB
        public struct MdbDataBlock
#else
        internal struct MdbDataBlock
#endif
        {
            public int DataLength;
            public byte[] Data;

            public MdbDataBlock(bool init)
            {
                DataLength = 0;
                Data = new byte[64];
            }

#if !WindowsCE
            public MdbDataBlock Clone()
            {
                try
                {
                    MemoryStream ms = new MemoryStream();
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(ms, this);
                    ms.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    return (MdbDataBlock)formatter.Deserialize(ms);
                }
                catch { return this; }
            }
#endif
        }

        #region Mapping of MDB and ID003 currency codes to ccTalk currency IDs
        [Serializable]
        internal struct CurrencyEntry
        {
            public string Description;
            public string IsoCode;
            public string CcTalkID;
            public int MdbCode;
            public int Decimals;
            public int JcmCode;
            public int DialingCode;

            public CurrencyEntry(string desc, string iso, string cc, int mdb, int dcls, int jcm, int dial)
            {
                Description = desc;
                IsoCode = iso;
                CcTalkID = cc;
                MdbCode = mdb;
                Decimals = dcls;
                JcmCode = jcm;
                DialingCode = dial;
            }
        }
        #region Currency Code Mappings
        internal CurrencyEntry[] CurrencyCodeMapping = {
            new CurrencyEntry("Albanian Lek", "ALL", "AL", 0x1008, 2, 0x77, 0x0355),
            new CurrencyEntry("Algerian Dinar", "DZD", "DZ", 0x1012, 2, 0x00, 0x0213),
            new CurrencyEntry("Antillian Guilder", "ANG", "AN", 0x1532, 2, 0x00, 0x0000),
            new CurrencyEntry("Argentine Peso", "ARS", "AR", 0x1032, 2, 0x0e, 0x0054),
            new CurrencyEntry("Australian Dollar", "AUD", "AU", 0x1036, 2, 0x02, 0x0061),
            new CurrencyEntry("Azerbaijanian Manat - New", "AZN", "AZ", 0x1944, 2, 0x00, 0x0994),
            new CurrencyEntry("Bahraini Dinar", "BHD", "BH", 0x1048, 3, 0x00, 0x0973),
            new CurrencyEntry("Bermudian Dollar", "BMD", "BM", 0x1060, 2, 0x00, 0x0000),
            new CurrencyEntry("Bolivian Boliviano", "BOB", "BO", 0x1068, 2, 0x00, 0x0591),
            new CurrencyEntry("Bosnia-Herzegovina Convert. Marks", "BAM", "BA", 0x1977, 2, 0x75, 0x0387),
            new CurrencyEntry("Brazilian Real", "BRL", "BR", 0x1986, 2, 0x00, 0x0055),
            new CurrencyEntry("Bulgarian Lev", "BGL", "BG", 0x1100, 2, 0x5d, 0x0359),
            new CurrencyEntry("Canadian Dollar", "CAD", "CA", 0x1124, 2, 0x08, 0x0000),
            new CurrencyEntry("Chilean Peso", "CLP", "CL", 0x1152, 2, 0x4e, 0x0056),
            new CurrencyEntry("Chinese Yuan Renminbi", "CNY", "CN", 0x1156, 2, 0x2d, 0x0086),
            new CurrencyEntry("Colombian Peso", "COP", "CO", 0x1170, 2, 0x19, 0x0057),
            new CurrencyEntry("Costa Rican Colon", "CRC", "CR", 0x1188, 2, 0x4d, 0x0506),
            new CurrencyEntry("Croatian Kuna", "HRK", "HR", 0x1191, 2, 0x6e, 0x0000),
            new CurrencyEntry("Cyprus Pound", "CYP", "CY", 0x1196, 2, 0x62, 0x0357),
            new CurrencyEntry("Czech Koruna", "CZK", "CZ", 0x1203, 2, 0x2c, 0x0420),
            new CurrencyEntry("Danish Krone", "DKK", "DK", 0x1208, 2, 0x3a, 0x0045),
            new CurrencyEntry("Dominican Peso", "DOP", "DO", 0x1214, 2, 0x00, 0x0000),
            new CurrencyEntry("Egyptian Pound", "EGP", "EG", 0x1818, 2, 0x00, 0x0020),
            new CurrencyEntry("Estonian Kroon", "EEK", "EE", 0x1233, 2, 0x24, 0x0372),
            new CurrencyEntry("Euro", "EUR", "EU", 0x1978, 2, 0xe0, 0x0049),
            new CurrencyEntry("Fiji Dollar", "FJD", "FJ", 0x1242, 2, 0x00, 0x0679),
            new CurrencyEntry("Georgian Lari", "GEL", "GE", 0x1981, 2, 0x76, 0x0995),
            new CurrencyEntry("Ghanaian Cedi - New", "GHS", "GH", 0x1936, 2, 0x97, 0x0233),
            new CurrencyEntry("Ghanaian Cedi - Old", "GHC", "GH", 0x1288, 2, 0x97, 0x0000),
            new CurrencyEntry("Hong Kong Dollar", "HKD", "HK", 0x1344, 2, 0x59, 0x0852),
            new CurrencyEntry("Hungarian Forint", "HUF", "HU", 0x1348, 2, 0x30, 0x0036),
            new CurrencyEntry("Iceland Krona", "ISK", "IS", 0x1352, 2, 0x49, 0x0354),
            new CurrencyEntry("Indian Rupee", "INR", "IN", 0x1356, 2, 0x63, 0x0091),
            new CurrencyEntry("Israeli Sheqel", "ILS", "IL", 0x1376, 2, 0x58, 0x0972),
            new CurrencyEntry("Jamaican Dollar", "JMD", "JM", 0x1388, 2, 0x00, 0x0000),
            new CurrencyEntry("Japanese Yen", "JPY", "JP", 0x1392, 0, 0x0a, 0x0081),
            new CurrencyEntry("Jordanian Dinar", "JOD", "JO", 0x1400, 2, 0x00, 0x0962),
            new CurrencyEntry("Kazakhstan Tenge", "KZT", "KZ", 0x1398, 2, 0x51, 0x0007),
            new CurrencyEntry("Kenyan Shilling", "KES", "KE", 0x1404, 2, 0x00, 0x0254),
            new CurrencyEntry("Latvian Lats", "LVL", "LV", 0x1428, 2, 0x46, 0x0371),
            new CurrencyEntry("Lebanese Pound", "LBP", "LB", 0x1422, 2, 0x00, 0x0961),
            new CurrencyEntry("Lithuanian Litas", "LTL", "LT", 0x1440, 2, 0x4f, 0x0370),
            new CurrencyEntry("Macau Pataca", "MOP", "MO", 0x1446, 2, 0x00, 0x0853),
            new CurrencyEntry("Macedonian Denar", "MKD", "MK", 0x1807, 2, 0x73, 0x0389),
            new CurrencyEntry("Malaysian Ringgit", "MYR", "MY", 0x1458, 2, 0x21, 0x0060),
            new CurrencyEntry("Maltese Lira", "MTL", "MT", 0x1470, 2, 0x61, 0x0356),
            new CurrencyEntry("Mauritius Rupee", "MUR", "MU", 0x1480, 2, 0x47, 0x0230),
            new CurrencyEntry("Mexican Peso", "MXN", "MX", 0x1484, 2, 0x09, 0x0052),
            new CurrencyEntry("Moroccan Dirham", "MAD", "MA", 0x1504, 2, 0x6c, 0x0212),
            new CurrencyEntry("Namibia Dollar", "NAD", "NA", 0x1516, 2, 0x3b, 0x0000),
            new CurrencyEntry("New Caledonian Franc", "XPF", "XP", 0x1953, 0, 0x00, 0x0687),
            new CurrencyEntry("New Zealand Dollar", "NZD", "NZ", 0x1554, 2, 0x0d, 0x0064),
            new CurrencyEntry("Norvegian Crown", "NOK", "NO", 0x1578, 2, 0x07, 0x0000),
            new CurrencyEntry("Pacific Franc", "XPF", "PF", 0x1953, 2, 0x00, 0x0000),
            new CurrencyEntry("Panama Balboa", "PAB", "PA", 0x1590, 2, 0x00, 0x0507),
            new CurrencyEntry("Peruvian Nuevo Sol", "PEN", "PE", 0x1604, 2, 0x2f, 0x0051),
            new CurrencyEntry("Philippine Peso", "PHP", "PH", 0x1608, 2, 0x4a, 0x0063),
            new CurrencyEntry("Polish Zloty", "PLN", "PL", 0x1985, 2, 0x1a, 0x0048),
            new CurrencyEntry("Pound Sterling", "GBP", "GB", 0x1826, 2, 0x17, 0x0044),
            new CurrencyEntry("Qatari Rial", "QAR", "QA", 0x1634, 2, 0x42, 0x0974),
            new CurrencyEntry("Rial Omani", "OMR", "OM", 0x1512, 3, 0x7d, 0x0968),
            new CurrencyEntry("Romanian Leu - New", "RON", "RO", 0x1946, 2, 0x4c, 0x0040),
            new CurrencyEntry("Romanian Leu - Old", "ROL", "RO", 0x1642, 2, 0x4c, 0x0000),
            new CurrencyEntry("Russian Ruble", "RUB", "RU", 0x1810, 2, 0x27, 0x0007),
            new CurrencyEntry("Serbia and Montenegro New Dinar", "YUM", "YU", 0x1891, 2, 0x74, 0x0381),
            new CurrencyEntry("Singapore Dollar", "SGD", "SG", 0x1702, 2, 0x22, 0x0065),
            new CurrencyEntry("Slovakian Crown", "SKK", "SK", 0x1703, 2, 0x41, 0x0421),
            new CurrencyEntry("Slovenian Tolar", "SIT", "SI", 0x1705, 2, 0x53, 0x0000),
            new CurrencyEntry("South African Rand", "ZAR", "ZA", 0x1710, 2, 0x06, 0x0027),
            new CurrencyEntry("Swedish Crown", "SEK", "SE", 0x1752, 2, 0x05, 0x0046),
            new CurrencyEntry("Swiss Franc", "CHF", "CH", 0x1756, 2, 0x16, 0x0041),
            new CurrencyEntry("Taiwan Dollar", "TWD", "TW", 0x1901, 2, 0x1d, 0x0886),
            new CurrencyEntry("Tajikistani Somoni", "TJS", "TJ", 0x1972, 2, 0x00, 0x0992),
            new CurrencyEntry("Thailandian Baht", "THB", "TH", 0x1764, 2, 0x12, 0x0066),
            new CurrencyEntry("Tunisian Dinar", "TND", "TN", 0x1788, 3, 0x00, 0x0216),
            new CurrencyEntry("Turkish Lira - New", "YTL", "TY", 0x1949, 2, 0x7b, 0x0090),
            new CurrencyEntry("Turkish Lira - Old", "TRL", "TR", 0x1792, 2, 0x7b, 0x0000),
            new CurrencyEntry("Ukrainian Hryvnia", "UAH", "UA", 0x1980, 2, 0x5c, 0x0380),
            new CurrencyEntry("United Arab Emirates Dirham", "AED", "AE", 0x1784, 2, 0x1c, 0x0971),
            new CurrencyEntry("US Dollar", "USD", "US", 0x1840, 2, 0x01, 0x0001),
            new CurrencyEntry("Venezuelan Bolivar", "VEB", "VE", 0x1862, 2, 0x1f, 0x0058)
        };
        internal string GetCcTalkID(int mdbcode)
        {
            if (mdbcode != 0x0000)
            {
                for (int i = 0; i < CurrencyCodeMapping.Length; i++)
                {
                    switch (mdbcode & 0xf000)
                    {
                        case 0x1000:
                            if (CurrencyCodeMapping[i].MdbCode == mdbcode)
                                return CurrencyCodeMapping[i].CcTalkID;
                            break;
                        case 0x0000:
                            if (CurrencyCodeMapping[i].DialingCode == mdbcode)
                                return CurrencyCodeMapping[i].CcTalkID;
                            break;
                    }
                }
            }
            /*
#if DEBUG
                        return string.Format("<0x{0:X04}>", mdbcode);
#else
                        return "XX";
#endif
            */
            return "XX";
        }
        internal string GetCcTalkIDFromID003(int jcmcode)
        {
            for (int i = 0; i < CurrencyCodeMapping.Length; i++)
            {
                if (CurrencyCodeMapping[i].JcmCode == jcmcode)
                    return CurrencyCodeMapping[i].CcTalkID;
            }
            return "XX";
        }
        internal string GetCcTalkID(string IsoCode)
        {
            if (IsoCode.Length >= 3)
            {
                IsoCode = IsoCode.Substring(0, 3);
                for (int i = 0; i < CurrencyCodeMapping.Length; i++)
                {
                    if (CurrencyCodeMapping[i].IsoCode == IsoCode)
                        return CurrencyCodeMapping[i].CcTalkID;
                }
            }
            return "XX";
        }
        internal int GetMdbCode(string ccid)
        {
            if (ccid.Length >= 2)
            {
                ccid = ccid.Substring(0, 2);
                for (int i = 0; i < CurrencyCodeMapping.Length; i++)
                {
                    if (CurrencyCodeMapping[i].CcTalkID.Trim().ToUpper() == ccid.Trim().ToUpper())
                        return CurrencyCodeMapping[i].MdbCode;
                }
            }
            return -1;
        }
        internal int GetCcTalkDecimals(string ccid)
        {
            if (ccid.Length >= 2)
            {
                ccid = ccid.Substring(0, 2);
                for (int i = 0; i < CurrencyCodeMapping.Length; i++)
                {
                    if (CurrencyCodeMapping[i].CcTalkID.Trim().ToUpper() == ccid.Trim().ToUpper())
                        return CurrencyCodeMapping[i].Decimals;
                }
            }
            return 2;
        }

        #endregion


#if !WindowsCE
        #region MDB Bill Validator Data Structures
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        internal struct BillValidatorSetup
        {
            public byte FeatureLevel;
            public byte CountryHi, CountryLo;
            public byte ScalingHi, ScalingLo;
            public byte Decimals;
            public byte CapacityHi, CapacityLo;
            public byte SecurityHi, SecurityLo;
            public byte CanEscrow;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] BillCredit;

            public bool InitStructure()
            {
                byte[] dmybff = new byte[Marshal.SizeOf(this)];

                try
                {
                    IntPtr rdip = Marshal.AllocHGlobal(Marshal.SizeOf(this));
                    Marshal.Copy(dmybff, 0, rdip, Marshal.SizeOf(this));
                    this = (BillValidatorSetup)Marshal.PtrToStructure(rdip, typeof(BillValidatorSetup));
                    Marshal.FreeHGlobal(rdip);
                }
                catch { return false; };
                return true;
            }

            public bool GetFromBuffer(MdbDataBlock mdbbuff)
            {
                if (mdbbuff.DataLength < 1) return false;
                try
                {
                    IntPtr rdip = Marshal.AllocHGlobal(Marshal.SizeOf(this));
                    Marshal.Copy(mdbbuff.Data, 0, rdip, Marshal.SizeOf(this));
                    this = (BillValidatorSetup)Marshal.PtrToStructure(rdip, typeof(BillValidatorSetup));
                    Marshal.FreeHGlobal(rdip);
                }
                catch { return false; };
                return true;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        internal struct BillValidatorIdentify
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] Manufacturer;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] Serial;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] Model;
            public byte Version, Release;
            public bool InitStructure()
            {
                byte[] dmybff = new byte[Marshal.SizeOf(this)];

                try
                {
                    IntPtr rdip = Marshal.AllocHGlobal(Marshal.SizeOf(this));
                    Marshal.Copy(dmybff, 0, rdip, Marshal.SizeOf(this));
                    this = (BillValidatorIdentify)Marshal.PtrToStructure(rdip, typeof(BillValidatorIdentify));
                    Marshal.FreeHGlobal(rdip);
                }
                catch { return false; };
                return true;
            }

            public bool GetFromBuffer(MdbDataBlock mdbbuff)
            {
                if (mdbbuff.DataLength < 1) return false;
                try
                {
                    IntPtr rdip = Marshal.AllocHGlobal(Marshal.SizeOf(this));
                    Marshal.Copy(mdbbuff.Data, 0, rdip, Marshal.SizeOf(this));
                    this = (BillValidatorIdentify)Marshal.PtrToStructure(rdip, typeof(BillValidatorIdentify));
                    Marshal.FreeHGlobal(rdip);
                }
                catch { return false; };
                return true;
            }
        }
        internal BillValidatorIdentify BillValidatorIdentifyInstance = new BillValidatorIdentify();
        internal BillValidatorSetup BillValidatorSetupInstance = new BillValidatorSetup();

        #endregion

        #region MDB Bill Validator Poll Message Mapping
        [Serializable]
        internal struct BillValidatorPollEntry
        {
            public byte Message;
            public ValPollEvent Event;

            public BillValidatorPollEntry(byte msg, ValPollEvent evt)
            {
                Message = msg;
                Event = evt;
            }
        }

        internal BillValidatorPollEntry[] BillValidatorPollMapping =
        {
            new BillValidatorPollEntry(0x00, ValPollEvent.Unknown),
            new BillValidatorPollEntry(0x01, ValPollEvent.DefectiveMotor),
            new BillValidatorPollEntry(0x02, ValPollEvent.OptoFraud),
            new BillValidatorPollEntry(0x03, ValPollEvent.Busy),
            new BillValidatorPollEntry(0x04, ValPollEvent.RomChecksum),
            new BillValidatorPollEntry(0x05, ValPollEvent.TransportProblem),
            new BillValidatorPollEntry(0x06, ValPollEvent.Reset),
            new BillValidatorPollEntry(0x07, ValPollEvent.Tamper),
            new BillValidatorPollEntry(0x08, ValPollEvent.StackerRemoved),
            new BillValidatorPollEntry(0x09, ValPollEvent.Disabled),
            new BillValidatorPollEntry(0x0a, ValPollEvent.InvalidEscrow),
            new BillValidatorPollEntry(0x0b, ValPollEvent.ValidationFailed),
            new BillValidatorPollEntry(0x0c, ValPollEvent.Tamper),
        };

        internal ValPollEvent TranslateBillValidatorPoll(byte message)
        {
            for (int i = 0; i < BillValidatorPollMapping.Length; i++)
            {
                if (BillValidatorPollMapping[i].Message == message)
                    return BillValidatorPollMapping[i].Event;
            }
            return ValPollEvent.Unknown;
        }
        #endregion

        #region MDB Change Giver Data Structures
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        internal struct ChangerSetup
        {
            public byte FeatureLevel;
            public byte CountryHi, CountryLo;
            public byte Scaling;
            public byte Decimals;
            public byte RoutingHi, RoutingLo;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] CoinCredit;

            public bool InitStructure()
            {
                byte[] dmybff = new byte[Marshal.SizeOf(this)];

                try
                {
                    IntPtr rdip = Marshal.AllocHGlobal(Marshal.SizeOf(this));
                    Marshal.Copy(dmybff, 0, rdip, Marshal.SizeOf(this));
                    this = (ChangerSetup)Marshal.PtrToStructure(rdip, typeof(ChangerSetup));
                    Marshal.FreeHGlobal(rdip);
                }
                catch { return false; };
                return true;
            }

            public bool GetFromBuffer(MdbDataBlock mdbbuff)
            {
                if (mdbbuff.DataLength < 1) return false;
                try
                {
                    IntPtr rdip = Marshal.AllocHGlobal(Marshal.SizeOf(this));
                    Marshal.Copy(mdbbuff.Data, 0, rdip, Marshal.SizeOf(this));
                    this = (ChangerSetup)Marshal.PtrToStructure(rdip, typeof(ChangerSetup));
                    Marshal.FreeHGlobal(rdip);
                }
                catch { return false; };
                return true;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        internal struct ChangerTubeStatus
        {
            public byte TubeFullHi, TubeFullLo;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] TubeStatus;

            public bool InitStructure()
            {
                byte[] dmybff = new byte[Marshal.SizeOf(this)];

                try
                {
                    IntPtr rdip = Marshal.AllocHGlobal(Marshal.SizeOf(this));
                    Marshal.Copy(dmybff, 0, rdip, Marshal.SizeOf(this));
                    this = (ChangerTubeStatus)Marshal.PtrToStructure(rdip, typeof(ChangerTubeStatus));
                    Marshal.FreeHGlobal(rdip);
                }
                catch { return false; };
                return true;
            }

            public bool GetFromBuffer(MdbDataBlock mdbbuff)
            {
                if (mdbbuff.DataLength < 1) return false;
                try
                {
                    IntPtr rdip = Marshal.AllocHGlobal(Marshal.SizeOf(this));
                    Marshal.Copy(mdbbuff.Data, 0, rdip, Marshal.SizeOf(this));
                    this = (ChangerTubeStatus)Marshal.PtrToStructure(rdip, typeof(ChangerTubeStatus));
                    Marshal.FreeHGlobal(rdip);
                }
                catch { return false; };
                return true;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        internal struct ChangerIdentify
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] Manufacturer;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] Serial;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] Model;
            public byte Version, Release;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] Features;

            public bool InitStructure()
            {
                byte[] dmybff = new byte[Marshal.SizeOf(this)];

                try
                {
                    IntPtr rdip = Marshal.AllocHGlobal(Marshal.SizeOf(this));
                    Marshal.Copy(dmybff, 0, rdip, Marshal.SizeOf(this));
                    this = (ChangerIdentify)Marshal.PtrToStructure(rdip, typeof(ChangerIdentify));
                    Marshal.FreeHGlobal(rdip);
                }
                catch { return false; };
                return true;
            }

            public bool GetFromBuffer(MdbDataBlock mdbbuff)
            {
                if (mdbbuff.DataLength < 1) return false;
                try
                {
                    IntPtr rdip = Marshal.AllocHGlobal(Marshal.SizeOf(this));
                    Marshal.Copy(mdbbuff.Data, 0, rdip, Marshal.SizeOf(this));
                    this = (ChangerIdentify)Marshal.PtrToStructure(rdip, typeof(ChangerIdentify));
                    Marshal.FreeHGlobal(rdip);
                }
                catch { return false; };
                return true;
            }
        }

        internal ChangerSetup ChangerSetupInstance = new ChangerSetup();
        internal ChangerTubeStatus ChangerTubeStatusInstance = new ChangerTubeStatus();
        internal ChangerIdentify ChangerIdentifyInstance = new ChangerIdentify();
        #endregion

        #region MDB Change Giver Poll Message Mapping
        [Serializable]
        internal struct ChangeGiverPollEntry
        {
            public byte Message;
            public SelPollEvent Event;

            public ChangeGiverPollEntry(byte msg, SelPollEvent evt)
            {
                Message = msg;
                Event = evt;
            }
        }

        internal ChangeGiverPollEntry[] ChangerPollMapping = {
            new ChangeGiverPollEntry(0x00, SelPollEvent.Unknown),
            new ChangeGiverPollEntry(0x01, SelPollEvent.Return),
            new ChangeGiverPollEntry(0x02, SelPollEvent.PayoutBusy),
            new ChangeGiverPollEntry(0x03, SelPollEvent.RoutingError),
            new ChangeGiverPollEntry(0x04, SelPollEvent.TubeSensor),
            new ChangeGiverPollEntry(0x05, SelPollEvent.FollowUp),
            new ChangeGiverPollEntry(0x06, SelPollEvent.Unplugged),
            new ChangeGiverPollEntry(0x07, SelPollEvent.TubeJam),
            new ChangeGiverPollEntry(0x08, SelPollEvent.RomCheckSum),
            new ChangeGiverPollEntry(0x09, SelPollEvent.RoutingError),
            new ChangeGiverPollEntry(0x0a, SelPollEvent.Busy),
            new ChangeGiverPollEntry(0x0b, SelPollEvent.Reset),
            new ChangeGiverPollEntry(0x0c, SelPollEvent.CoinJam),
            new ChangeGiverPollEntry(0x0d, SelPollEvent.CoinRemoved),
            new ChangeGiverPollEntry(0x0e, SelPollEvent.Unknown),
            new ChangeGiverPollEntry(0x0f, SelPollEvent.Unknown),
        };

        internal SelPollEvent TranslateChangerPoll(byte message)
        {
            for (int i = 0; i < ChangerPollMapping.Length; i++)
            {
                if (ChangerPollMapping[i].Message == message)
                    return ChangerPollMapping[i].Event;
            }
            return SelPollEvent.Unknown;
        }
        #endregion

        #region Basic change giver pay out routine
        internal CcTalkErrors ChangeGiverPayout(int tubno, int count)
        {
            MdbDataBlock smdb = new MdbDataBlock(true);
            MdbDataBlock rmdb = new MdbDataBlock(true);

            if ((tubno > 15) || (count > 15)) return CcTalkErrors.WrongParameter;

            smdb.DataLength = 2;
            smdb.Data[0] = MdbAddresses.MdbChangeGiver | MdbCommands.Dispense;
            smdb.Data[1] = (byte)(tubno | (count << 4));
            lasterr = TalkMdb(smdb, ref rmdb);

            return lasterr;
        }
        #endregion

        #region MDB Cashless Payment Data Structures
        // Copy String to byte array and vice versa
        internal static void bytes2string(byte[] btarr, ref string dstr)
        {

            Encoding enc = Encoding.ASCII;
            for (int i = 0; i < btarr.Length; i++)
                if ((btarr[i] < 0x20) || (btarr[i] > 0x7f)) btarr[i] = 0x020;
            dstr = enc.GetString(btarr);
            dstr = dstr.TrimEnd();
        }
        internal static void string2bytes(string sstr, ref byte[] btarr)
        {
            for (int i = 0; i < btarr.Length; i++) btarr[i] = 0x00;
            sstr = sstr.TrimEnd();
            for (int i = 0; i < Math.Min(btarr.Length, sstr.Length); i++) btarr[i] = (byte)sstr[i];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        internal struct CashlessConfig
        {
            public byte DataID;
            public byte FeatureLevel;
            public byte Columns;
            public byte Rows;
            public byte DisplayInfo;

            public bool InitStructure()
            {
                byte[] dmybff = new byte[Marshal.SizeOf(this)];

                try
                {
                    IntPtr rdip = Marshal.AllocHGlobal(Marshal.SizeOf(this));
                    Marshal.Copy(dmybff, 0, rdip, Marshal.SizeOf(this));
                    this = (CashlessConfig)Marshal.PtrToStructure(rdip, typeof(CashlessConfig));
                    Marshal.FreeHGlobal(rdip);
                }
                catch { return false; };
                DataID = 0x00;
                FeatureLevel = 0x03;
                Columns = 16;
                Rows = 2;
                DisplayInfo = 0x01;
                return true;
            }

            public bool GetFromBuffer(MdbDataBlock mdbbuff)
            {
                if (mdbbuff.DataLength < 1) return false;
                try
                {
                    IntPtr rdip = Marshal.AllocHGlobal(Marshal.SizeOf(this));
                    Marshal.Copy(mdbbuff.Data, 0, rdip, Marshal.SizeOf(this));
                    this = (CashlessConfig)Marshal.PtrToStructure(rdip, typeof(CashlessConfig));
                    Marshal.FreeHGlobal(rdip);
                }
                catch { return false; };
                return true;
            }

            public bool SetToBuffer(MdbDataBlock mdbbuff, int offset)
            {
                try
                {
                    IntPtr rdip = Marshal.AllocHGlobal(Marshal.SizeOf(this));
                    Marshal.StructureToPtr(this, rdip, false);
                    Marshal.Copy(rdip, mdbbuff.Data, offset, Marshal.SizeOf(this));
                    Marshal.FreeHGlobal(rdip);
                }
                catch { return false; };
                return true;
            }

        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        internal struct CashlessSetup
        {
            public byte DataID;
            public byte FeatureLevel;
            public byte CountryHi, CountryLo;
            public byte Scaling;
            public byte Decimals;
            public byte MaxResponseTime;
            public byte Options;

            public bool InitStructure()
            {
                byte[] dmybff = new byte[Marshal.SizeOf(this)];

                try
                {
                    IntPtr rdip = Marshal.AllocHGlobal(Marshal.SizeOf(this));
                    Marshal.Copy(dmybff, 0, rdip, Marshal.SizeOf(this));
                    this = (CashlessSetup)Marshal.PtrToStructure(rdip, typeof(CashlessSetup));
                    Marshal.FreeHGlobal(rdip);
                }
                catch { return false; };
                return true;
            }

            public bool GetFromBuffer(MdbDataBlock mdbbuff)
            {
                if (mdbbuff.DataLength < 1) return false;
                try
                {
                    IntPtr rdip = Marshal.AllocHGlobal(Marshal.SizeOf(this));
                    Marshal.Copy(mdbbuff.Data, 0, rdip, Marshal.SizeOf(this));
                    this = (CashlessSetup)Marshal.PtrToStructure(rdip, typeof(CashlessSetup));
                    Marshal.FreeHGlobal(rdip);
                }
                catch { return false; };
                return true;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        internal struct CashlessIdentify
        {
            public byte DataID;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] Manufacturer;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] Serial;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] Model;
            public byte Version, Release;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] FeatureBytes;

            public bool InitStructure(bool init)
            {
                byte[] dmybff = new byte[Marshal.SizeOf(this)];

                try
                {
                    IntPtr rdip = Marshal.AllocHGlobal(Marshal.SizeOf(this));
                    Marshal.Copy(dmybff, 0, rdip, Marshal.SizeOf(this));
                    this = (CashlessIdentify)Marshal.PtrToStructure(rdip, typeof(CashlessIdentify));
                    Marshal.FreeHGlobal(rdip);
                }
                catch { return false; };
                if (init) // Initialise with VMC data
                {
                    DataID = MdbCommands.Identify;
                    string2bytes("WHM", ref Manufacturer);
                    string2bytes("00000000000", ref Serial);
                    string2bytes("ccTalk Lib ", ref Model);
                    Version = 1;
                    Release = 0;
                }
                return true;
            }

            public bool GetFromBuffer(MdbDataBlock mdbbuff)
            {
                if (mdbbuff.DataLength < 1) return false;
                try
                {
                    IntPtr rdip = Marshal.AllocHGlobal(Marshal.SizeOf(this));
                    Marshal.Copy(mdbbuff.Data, 0, rdip, Marshal.SizeOf(this));
                    this = (CashlessIdentify)Marshal.PtrToStructure(rdip, typeof(CashlessIdentify));
                    Marshal.FreeHGlobal(rdip);
                }
                catch { return false; };
                return true;
            }

            public bool SetToBuffer(MdbDataBlock mdbbuff, int offset)
            {
                try
                {
                    IntPtr rdip = Marshal.AllocHGlobal(Marshal.SizeOf(this));
                    Marshal.StructureToPtr(this, rdip, false);
                    Marshal.Copy(rdip, mdbbuff.Data, offset, Marshal.SizeOf(this));
                    Marshal.FreeHGlobal(rdip);
                }
                catch { return false; };
                return true;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        internal struct VMCIdentify
        {
            public byte DataID;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] Manufacturer;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] Serial;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] Model;
            public byte Version, Release;

            public bool InitStructure(bool init)
            {
                byte[] dmybff = new byte[Marshal.SizeOf(this)];

                try
                {
                    IntPtr rdip = Marshal.AllocHGlobal(Marshal.SizeOf(this));
                    Marshal.Copy(dmybff, 0, rdip, Marshal.SizeOf(this));
                    this = (VMCIdentify)Marshal.PtrToStructure(rdip, typeof(VMCIdentify));
                    Marshal.FreeHGlobal(rdip);
                }
                catch { return false; };
                if (init) // Initialise with VMC data
                {
                    DataID = MdbCommands.Identify;
                    string2bytes("WHM", ref Manufacturer);
                    string2bytes("00000000000", ref Serial);
                    string2bytes("ccTalk Lib ", ref Model);
                    Version = 1;
                    Release = 0;
                }
                return true;
            }

            public bool GetFromBuffer(MdbDataBlock mdbbuff)
            {
                if (mdbbuff.DataLength < 1) return false;
                try
                {
                    IntPtr rdip = Marshal.AllocHGlobal(Marshal.SizeOf(this));
                    Marshal.Copy(mdbbuff.Data, 0, rdip, Marshal.SizeOf(this));
                    this = (VMCIdentify)Marshal.PtrToStructure(rdip, typeof(VMCIdentify));
                    Marshal.FreeHGlobal(rdip);
                }
                catch { return false; };
                return true;
            }

            public bool SetToBuffer(MdbDataBlock mdbbuff, int offset)
            {
                try
                {
                    IntPtr rdip = Marshal.AllocHGlobal(Marshal.SizeOf(this));
                    Marshal.StructureToPtr(this, rdip, false);
                    Marshal.Copy(rdip, mdbbuff.Data, offset, Marshal.SizeOf(this));
                    Marshal.FreeHGlobal(rdip);
                }
                catch { return false; };
                return true;
            }
        }

        internal CashlessConfig CashlessConfigInstance = new CashlessConfig();
        internal CashlessSetup CashlessSetupInstance = new CashlessSetup();
        internal CashlessIdentify CashlessIdentifyInstance = new CashlessIdentify();
        internal VMCIdentify LibraryIdentify = new VMCIdentify();


        #endregion
#endif
        internal ushort BillInhibit = 0x0000;
        internal bool BillMasterInhibit = false;
        internal bool BillEscrowEnable = true;

        #endregion

#if !WindowsCE
        // Retrieve Data from a cashless payment system
        // Polle Response Types
        internal CcTalkErrors TalkCPMdb(MdbDataBlock sndblk, ref MdbDataBlock rcvblk, CashlessPollEvent resp)
        {
            return TalkCPMdb(sndblk, ref rcvblk, resp, mdbtot);
        }
        internal CcTalkErrors TalkCPMdb(MdbDataBlock sndblk, ref MdbDataBlock rcvblk, CashlessPollEvent resp, int tmot)
        {
            CcTalkErrors res = CcTalkErrors.Unknown;
            MdbDataBlock resblk = new MdbDataBlock(true);
            MdbDataBlock pllblk = new MdbDataBlock(true);

            if ((res = TalkMdb(sndblk, ref resblk, tmot)) == CcTalkErrors.Ok)
            {
                if (resblk.DataLength > 0)
                {
                    rcvblk = resblk.Clone();
                }
                else
                {
                    pllblk.DataLength = 1;
                    pllblk.Data[0] = (byte)(cashlessaddr | MdbCommands.CPPoll);
                    for (int i = 0; i < 4; i++)
                    {
                        Thread.Sleep(20);
                        if ((res = TalkMdb(pllblk, ref rcvblk, tmot)) == CcTalkErrors.Ok)
                        {
                            if ((rcvblk.DataLength > 1) && (rcvblk.Data[0] == (byte)resp))
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return res;
        }
#endif
        // Send and receive a MDB block
        internal CcTalkErrors TalkMdb(MdbDataBlock sndblk, ref MdbDataBlock rcvblk)
        {
            return TalkMdb(sndblk, ref rcvblk, mdbtot);
        }
#if PUBLIC_TALKMDB
        public CcTalkErrors TalkMdb(MdbDataBlock sndblk, ref MdbDataBlock rcvblk, int tmot)
#else
        internal CcTalkErrors TalkMdb(MdbDataBlock sndblk, ref MdbDataBlock rcvblk, int tmot)
#endif
        {
            CcTalkDataBlock ccsnd = new CcTalkDataBlock(CcTalkChecksumTypes.Simple8);
            CcTalkDataBlock ccrcv = new CcTalkDataBlock(CcTalkChecksumTypes.Simple8);
            CcTalkErrors res = CcTalkErrors.Unknown;
            CcMdbFlags flags = CcMdbFlags.None;

            ccsnd.Header = 122;
            ccsnd.Destination = (byte)dongleaddr;
            ccsnd.DataLength = (byte)(sndblk.DataLength + 1);
            ccsnd.Data[0] = (byte)tmot;
            for (int i = 0; i < rcvblk.Data.Length; i++) rcvblk.Data[i] = 0x00;
            for (int i = 0; i < sndblk.DataLength; i++) ccsnd.Data[i + 1] = sndblk.Data[i];

            int trycnt = 0;
            do
            {
                res = TalkCc(ccsnd, ref ccrcv, MdbCcTalkTimeout + tmot + rcvblk.Data.Length + sndblk.Data.Length);

                if (res == CcTalkErrors.Ok)
                {
                    if (ccrcv.DataLength > 0)
                    {
                        flags = (CcMdbFlags)ccrcv.Data[0];
                        if ((flags & CcMdbFlags.ChecksumError) != 0) res = CcTalkErrors.ChSumErr;
                        if ((flags & CcMdbFlags.ReceiveTimeout) != 0) res = CcTalkErrors.RcvTimout;
                        if ((flags & CcMdbFlags.SendingBreak) != 0) res = CcTalkErrors.SendErr;
                    }
                    else
                    {
                        res = CcTalkErrors.DataFormat;
                    }
                    if ((res == CcTalkErrors.Ok))
                    {
                        if ((flags & CcMdbFlags.BlockReceived) != 0)
                        {
                            rcvblk.DataLength = ccrcv.DataLength - 1;
                            for (int i = 1; i < ccrcv.DataLength; i++)
                                rcvblk.Data[i - 1] = ccrcv.Data[i];
                        }
                        else
                        {
                            res = CcTalkErrors.ReceiveError;
                        }
                    }
                }
                if (res != CcTalkErrors.Ok)
                {
                    trycnt++;
                    Thread.Sleep(sndblk.DataLength * 2);
                }
            } while ((res != CcTalkErrors.Ok) && (res != CcTalkErrors.BadLine) && (trycnt < 4));

            return res;
        }
        // Send MDB Break
        internal CcTalkErrors SendMdbBreak()
        {
            CcTalkDataBlock ccsnd = new CcTalkDataBlock(CcTalkChecksumTypes.Simple8);
            CcTalkDataBlock ccrcv = new CcTalkDataBlock(CcTalkChecksumTypes.Simple8);
            CcTalkErrors res = CcTalkErrors.Unknown;

            ccsnd.Header = 121;
            ccsnd.Destination = (byte)dongleaddr;
            ccsnd.DataLength = 1;
            res = TalkCc(ccsnd, ref ccrcv, MdbCcTalkTimeout);

            return res;
        }
        #endregion

        #region Core Commands
        // Get device category
        internal void GetCategory()
        {
            int i;


            catstr = GetStringResponse(245);
            cat = CcTalkCategory.Unknown;
            if (lasterr == CcTalkErrors.Ok)
            {
                for (i = 0; i < CategoryIDs.Length; i++)
                {
                    if (catstr.ToLower() == CategoryIDs[i].CategoryStr.ToLower())
                    {
                        cat = CategoryIDs[i].Category;
                        catstr = CategoryIDs[i].DisplayStr;
                        break;
                    }
                }
            }
            else
            {
                catstr = "Comm Error";
            };
            //}
        }

        // Device's string or long response
        internal string GetStringResponse(byte hdr)
        {
            int i;
            string devresp;
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.Destination = devaddr;
            sdta.DataLength = 0;
            sdta.Header = hdr;
            devresp = "";
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr != CcTalkErrors.Ok)
            {
                return devresp;
            }

            for (i = 0; i < rdta.DataLength; i++)
            {
                if ((rdta.Data[i] > 0x1f) && (rdta.Data[i] < 0x80))
                {
                    devresp += (char)rdta.Data[i];
                }
            }

            return devresp;
        }
        internal long GetLongResponse(byte hdr)
        {
            int i;
            long devresp;
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.Destination = devaddr;
            sdta.DataLength = 0;
            sdta.Header = hdr;
            devresp = 0xffffffff;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr != CcTalkErrors.Ok) return devresp;
            devresp = 0;
            for (i = 0; i < rdta.DataLength; i++)
                devresp += (long)((UInt64)(rdta.Data[i] * Math.Pow(256, i)));

            return devresp;
        }
        internal long GetReverseLongResponse(byte hdr)
        {
            int i, pwr;
            long devresp;
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.Destination = devaddr;
            sdta.DataLength = 0;
            sdta.Header = hdr;
            devresp = 0xffffffff;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr != CcTalkErrors.Ok) return devresp;
            devresp = 0;
            pwr = rdta.DataLength - 1;
            for (i = 0; i < rdta.DataLength; i++)
            {
                devresp += (int)(rdta.Data[i] * Math.Pow(256, pwr));
                pwr--;
            }

            return devresp;
        }

        internal CcTalkCategory GetDeviceCategory()
        {
            return CcTalkCategory.Unknown;
        }

        // Get sorter Path
        internal byte GetSorterPath(int coinno)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 1;
            sdta.Header = 209;
            sdta.Data[0] = (byte)(coinno + 1);
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr != CcTalkErrors.Ok) return 0xff;
            return rdta.Data[0];
        }

        [Serializable]
        internal struct CountryScalingFactor
        {
            public string ID;
            public long factor;
            public int decimals;
        };

        internal CountryScalingFactor[] scalfactors = new CountryScalingFactor[16];

        // Get a coin value
        internal CoinValue GetCoinValue(int coinno)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            char sepchar = (5.5).ToString()[1];

            CoinValue rval = new CoinValue(true);
            double fac;
            string coinid, valstr;
            char facch;
            if (EncryptionSupport.CommandLevel == CcTalkCryptography.DES)
            {
#if DES_SUPPORT
                byte challenge = (byte)new Random().Next(256);
                sdta.DataLength = 2;
                sdta.Data[0] = (byte)(coinno + 1);
                sdta.Data[1] = challenge;
                sdta.Header = 108;
                lasterr = TalkCc(sdta, ref rdta);
                if (lasterr != CcTalkErrors.Ok) return rval;
                if (rdta.DataLength != 16)  // Wrong data format
                {
                    lasterr = CcTalkErrors.DataFormat;
                    return rval;
                }
                lasterr = decryptdesblock(ref rdta, challenge);
                if (lasterr != CcTalkErrors.Ok) return rval;
                string vstr = "";
                rval.ID = "";
                if (rdta.Data[0] == (byte)'#')
                {
                    for (int i = 1; i < 3; i++) rval.ID += (char)rdta.Data[i];
                }
                else
                {
                    for (int i = 0; i < 3; i++) rval.ID += (char)rdta.Data[i];
                }
                for (int i = 0; i < 4; i++) vstr += (char)rdta.Data[i + 5];
                try
                {
                    double sfac;
                    double dfac;
                    if ((rdta.Data[3] > 0) && (rdta.Data[4] > 0))
                    {
                        if (rdta.Data[3] < 0x30)
                        {
                            sfac = Math.Pow(10, rdta.Data[3]);
                        }
                        else
                        {
                            sfac = Math.Pow(10, rdta.Data[3] - '0');
                        }
                        if (rdta.Data[4] < 0x30)
                        {
                            rval.Decimals = rdta.Data[4];
                        }
                        else
                        {
                            rval.Decimals = rdta.Data[4] - '0';
                        }
                    }
                    else
                    {
                        sfac = 1;
                        rval.Decimals = GetCcTalkDecimals(rval.ID);
                    }
                    dfac = Math.Pow(10, rval.Decimals);
                    rval.IntValue = (int)(int.Parse(vstr) * sfac);
                    rval.Value = rval.IntValue / dfac;
                }
                catch (Exception e) { rval = new CoinValue(true); }
#else
                lasterr = CcTalkErrors.UnsupportedEncryption;
#endif
            }
            else
            {
                coinid = GetCoinIDStr(coinno);
                if (lasterr != CcTalkErrors.Ok) return rval;
                if (coinid.Length < 6)
                {
                    lasterr = CcTalkErrors.WrongParameter;
                    return rval;
                }

                // Analyse coin string
                // ID
                rval.ID = coinid.Substring(0, 2);
                // Value
                valstr = "";
                facch = ' ';
                for (int i = 2; i < 5; i++)
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
                rval.Decimals = GetCcTalkDecimals(rval.ID);
                try
                {
                    if (valstr.Split(',').Length > 2)
                    {
                        return new CoinValue(true);
                    }

                    double parsed;
                    double.TryParse(valstr, out parsed);
                    rval.Value = parsed / Math.Pow(10, rval.Decimals);
                    rval.IntValue = (int)(rval.Value * Math.Pow(10, rval.Decimals));
                }
                catch
                {
                    return new CoinValue(true);
                }
                fac = 1;
                for (int i = 0; i < ValueFactors.Length; i++)
                {
                    if (ValueFactors[i].FactorChar == facch)
                    {
                        fac = ValueFactors[i].Factor;
                        break;
                    }
                }
                rval.Value *= fac;
                rval.IntValue = (int)(rval.IntValue * fac);
            }
            return rval;
        }
        internal string GetCoinIDStr(int coinno)
        {
            int i;
            string devresp;
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.Header = 184;
            sdta.DataLength = 0;
            if (coinno >= -1)
            {
                sdta.DataLength = 1;
                sdta.Data[0] = (byte)(coinno + 1);
            }
            devresp = "";
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr != CcTalkErrors.Ok) return devresp;

            for (i = 0; i < rdta.DataLength; i++)
                devresp += (char)rdta.Data[i];

            return devresp;
        }
        // Get Bill value
        internal CcTalkErrors GetCountryScalingFactor(string id, out long fact, out int dec)
        {
            int sfi;
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            // Check if it was already retrieved
            for (sfi = 0; sfi < 16; sfi++)
            {
                if (scalfactors[sfi].ID == "XX") break;   // End of list
                if (scalfactors[sfi].ID == id)
                {
                    fact = scalfactors[sfi].factor;
                    dec = scalfactors[sfi].decimals;
                    return lasterr;
                };
            };
            sdta.Destination = devaddr;
            sdta.DataLength = 2;
            sdta.Header = 156;
            sdta.Data[0] = (byte)id[0];
            sdta.Data[1] = (byte)id[1];
            fact = 1;
            dec = 2;
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr != CcTalkErrors.Ok) return lasterr;
            if (rdta.DataLength > 1) fact = (int)(rdta.Data[0]) + (int)(rdta.Data[1] * 256);
            if (rdta.DataLength > 2) dec = (int)rdta.Data[2];

            // Add to the list
            scalfactors[sfi].ID = id;
            scalfactors[sfi].factor = fact;
            scalfactors[sfi].decimals = dec;

            return lasterr;
        }

        internal BillValue GetBillValue(int billno)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            char sepchar = (5.5).ToString()[1];

            BillValue rval = new BillValue();
            int dec;
            string billid, valstr;
            char facch;

            if (EncryptionSupport.CommandLevel == CcTalkCryptography.DES)
            {
#if DES_SUPPORT
                byte challenge = (byte)new Random().Next(256);
                sdta.DataLength = 2;
                sdta.Data[0] = (byte)(billno + 1);
                sdta.Data[1] = challenge;
                sdta.Header = 108;
                lasterr = TalkCc(sdta, ref rdta);
                if (lasterr != CcTalkErrors.Ok) return rval;
                if (rdta.DataLength != 16)  // Wrong data format
                {
                    lasterr = CcTalkErrors.DataFormat;
                    return rval;
                }
                lasterr = decryptdesblock(ref rdta, challenge);
                if (lasterr != CcTalkErrors.Ok) return rval;
                string vstr = "";
                rval.ID = "";
                if (rdta.Data[0] == (byte)'#')
                {
                    for (int i = 1; i < 3; i++) rval.ID += (char)rdta.Data[i];
                }
                else
                {
                    for (int i = 0; i < 3; i++) rval.ID += (char)rdta.Data[i];
                }
                for (int i = 0; i < 4; i++) vstr += (char)rdta.Data[i + 5];
                try
                {
                    double sfac;
                    double dfac;
                    if ((rdta.Data[3] > 0) && (rdta.Data[4] > 0))
                    {
                        sfac = Math.Pow(10, rdta.Data[3] - '0');
                        rval.Decimals = rdta.Data[4] - '0';
                    }
                    else
                    {
                        sfac = 1;
                        rval.Decimals = GetCcTalkDecimals(rval.ID);
                    }
                    dfac = Math.Pow(10, rval.Decimals);
                    rval.Value = (int)(int.Parse(vstr) * sfac) / dfac;
                }
                catch { rval = new BillValue(); }
#else
                lasterr = CcTalkErrors.UnsupportedEncryption;
#endif
            }
            else
            {
                long vfac, sfac;
                rval.Value = 0;
                rval.ID = "";
                billid = GetBillIDStr(billno);
                if (lasterr != CcTalkErrors.Ok) return rval;
                if (billid.Length < 7)
                {
                    lasterr = CcTalkErrors.WrongParameter;
                    return rval;
                }

                // Analyse Bill string
                // ID
                rval.ID = billid.Substring(0, 2);
                // Value
                valstr = "";
                facch = ' ';
                for (int i = 2; i < 6; i++)
                {
                    if (Char.IsDigit(billid[i]))
                    {
                        valstr += billid[i];
                    }
                    else
                    {
                        valstr += sepchar;
                        facch = billid[i];
                    }
                }
                try
                {
                    rval.Value = (double)(Convert.ToDouble(valstr) / Math.Pow(10, GetCcTalkDecimals(rval.ID)));
                }
                catch (System.Exception)
                {
                    rval.Value = 0;
                    rval.ID = "";
                    return rval;
                }
                vfac = 1;
                for (int i = 0; i < ValueFactors.Length; i++)
                {
                    if (ValueFactors[i].FactorChar == facch)
                    {
                        vfac = ValueFactors[i].Factor;
                        break;
                    }
                }
                GetCountryScalingFactor(rval.ID, out sfac, out dec);
                rval.Decimals = dec;
                rval.Value *= vfac * sfac;
            }
            return rval;
        }
        internal string GetBillIDStr(int billno)
        {
            int i;
            string devresp;
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 1;
            sdta.Header = 157;
            sdta.Data[0] = (byte)(billno + 1);
            devresp = "";
            lasterr = TalkCc(sdta, ref rdta);
            if (lasterr != CcTalkErrors.Ok) return devresp;

            for (i = 0; i < rdta.DataLength; i++)
                devresp += (char)rdta.Data[i];

            return devresp;
        }
        #endregion
        #endregion
    }
}