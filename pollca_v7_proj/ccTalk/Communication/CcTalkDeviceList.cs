using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using Microsoft.Win32;

namespace ccTalk
{
    #region List of ccTalk devices

    /// <summary>
    /// Maintains a list of all available ccTalk devices.
    /// </summary>

    [Serializable]
    public class CcTalkDeviceList
    {

        #region Private variables, methods etc.
        // Constants
        private static string REG_SERIALCOMM = "HARDWARE\\DEVICEMAP\\SERIALCOMM";
        private const string REG_USBENUM = "SYSTEM\\ControlSet001\\Enum\\USB";
        // Addresses and categories to check
        [Serializable]
        private struct ScanDevice
        {
            public byte Address;
            public CcTalkChecksumTypes ChecksumType;
            public CcTalkEncryption Encryption;
            public ScanDevice(byte adr, CcTalkChecksumTypes ctp, CcTalkEncryption enc)
            {
                Address = adr;
                ChecksumType = ctp;
                Encryption = enc;
            }
        }
        // MDB Devices via CCT 9x0
        private ScanDevice[] MdbAdresses = new ScanDevice[]
        {
            new ScanDevice(MdbAddresses.CcChangeGiver,    CcTalkChecksumTypes.Simple8, CcTalkEncryption.None),    // MDB Change Giver
            new ScanDevice(MdbAddresses.CcBillValidator,  CcTalkChecksumTypes.Simple8, CcTalkEncryption.None),    // MDB Bill Validator
            new ScanDevice(MdbAddresses.CcCashless,       CcTalkChecksumTypes.Simple8, CcTalkEncryption.None),    // MDB Cashless Payment
        };
        // True ccTalk Devices
        private ScanDevice[] CcTalkAdresses = new ScanDevice[]
        {
            new ScanDevice(  2, CcTalkChecksumTypes.Simple8, CcTalkEncryption.None),    // Coin Selector
            new ScanDevice(  2, CcTalkChecksumTypes.CRC16,   CcTalkEncryption.None),    // Coin Selector
            new ScanDevice(  3, CcTalkChecksumTypes.Simple8, CcTalkEncryption.None),    // Payout
            new ScanDevice(  4, CcTalkChecksumTypes.Simple8, CcTalkEncryption.None),    // Payout
            new ScanDevice(  5, CcTalkChecksumTypes.Simple8, CcTalkEncryption.None),    // Payout
            new ScanDevice(  6, CcTalkChecksumTypes.Simple8, CcTalkEncryption.None),    // Payout
            new ScanDevice(  7, CcTalkChecksumTypes.Simple8, CcTalkEncryption.None),    // Payout
            new ScanDevice(  8, CcTalkChecksumTypes.Simple8, CcTalkEncryption.None),    // Payout
            new ScanDevice(  9, CcTalkChecksumTypes.Simple8, CcTalkEncryption.None),    // Payout
            new ScanDevice( 10, CcTalkChecksumTypes.Simple8, CcTalkEncryption.None),    // Payout
            new ScanDevice( 40, CcTalkChecksumTypes.CRC16,   CcTalkEncryption.None),    // Bill Validator, unencrypted
            new ScanDevice( 40, CcTalkChecksumTypes.Simple8, CcTalkEncryption.None),    // Bill Validator (PTI)
            new ScanDevice( 80, CcTalkChecksumTypes.Simple8, CcTalkEncryption.None),    // Dongle
            new ScanDevice(130, CcTalkChecksumTypes.Simple8, CcTalkEncryption.None),    // Coin Scale 
            new ScanDevice(131, CcTalkChecksumTypes.Simple8, CcTalkEncryption.None),    // Coin Scale 
            new ScanDevice(132, CcTalkChecksumTypes.Simple8, CcTalkEncryption.None),    // Coin Scale 
            new ScanDevice(133, CcTalkChecksumTypes.Simple8, CcTalkEncryption.None),    // Coin Scale 
            new ScanDevice(134, CcTalkChecksumTypes.Simple8, CcTalkEncryption.None),    // Coin Scale 
            new ScanDevice(140, CcTalkChecksumTypes.Simple8, CcTalkEncryption.None),    // Coin Feeder 
            new ScanDevice(160, CcTalkChecksumTypes.Simple8, CcTalkEncryption.None)     // Escrow Sorter 
        };
        private static readonly CcTalkCategory[] CategoryRange = new CcTalkCategory[]
        {
            CcTalkCategory.Bootloader,
            CcTalkCategory.CoinSelector,
            CcTalkCategory.BillValidator,
            CcTalkCategory.PayOut,
            CcTalkCategory.CoinScale,
            CcTalkCategory.Peripheral,
            CcTalkCategory.ChangeGiver,
            CcTalkCategory.CoinFeeder,
            CcTalkCategory.Cashless,
            CcTalkCategory.EscrowSorter,
            CcTalkCategory.CardReader,
        };

        // List of com ports
        [Serializable]
        private struct PortInfo
        {
            public int Number;
            public string Name;
            public PortTypes Type;
        }

        private int GetPortNo(string comnm)
        {
            string pnstr = "";
            int pos = 0; ;

            while (!Char.IsDigit(comnm[pos])) if (++pos >= comnm.Length) return -1;
            while (Char.IsDigit(comnm[pos]))
            {
                pnstr += comnm[pos++];
                if (pos >= comnm.Length) break;
            };
            if (pnstr == "") return -1;
            try
            {
                return Convert.ToInt32(pnstr);
            }
            catch (System.Exception) { return -1; };
        }
        internal string RemoveNonASCII(string nas)
        {
            string oas;
            int i;

            oas = "";
            for (i = 0; i < nas.Length; i++)
                if ((nas[i] > 0x1f) && (nas[i] < 0x80)) oas += nas[i];
            return oas;
        }

#if OS_WINDOWS
        private RegistryKey lreg;
#endif
        private int portcnt, acccnt;
        private bool indptsearch = false;
        private PortTypes porttp = PortTypes.USB;
        private byte cashlessaddr = 0x00;

        [Serializable]
        private struct PortIndentifier
        {
            public string PortKey;
            public PortTypes PortType;
            public PortIndentifier(string pk, PortTypes pt)
            {
                this.PortKey = pk;
                this.PortType = pt;
            }
        }
        private PortIndentifier[] PortIDs = new PortIndentifier[]
        {
             new PortIndentifier("serial", PortTypes.Serial),
            new PortIndentifier("vcp", PortTypes.USB),
            new PortIndentifier("slabser", PortTypes.USB),
            new PortIndentifier("usbser", PortTypes.USB),
            new PortIndentifier("bthmodem", PortTypes.Bluetooth),
            new PortIndentifier("btport", PortTypes.Bluetooth),
            new PortIndentifier("ircomm", PortTypes.IrDA)
        };

        private void GetCommList()
        {
            int i, j;
            string[] sdevs;
            PortInfo tmpifo = new PortInfo();
            bool swpd;

#if OS_WINDOWS
            System.OperatingSystem osinfo = System.Environment.OSVersion;
            if (osinfo.Platform == System.PlatformID.Win32NT)
            {
                try
                {

                    lreg = Registry.LocalMachine.OpenSubKey(REG_SERIALCOMM);
                    sdevs = lreg.GetValueNames();
                    portcnt = sdevs.GetUpperBound(0) + 1;
                    if (portcnt > 0)
                    {
                        // Get list of serial ports from registry
                        PortList = new PortInfo[portcnt];
                        for (i = 0; i < portcnt; i++)
                        {
                            PortList[i].Name = RemoveNonASCII(lreg.GetValue(sdevs[i]).ToString());
                            PortList[i].Number = GetPortNo(PortList[i].Name);
                            PortList[i].Type = PortTypes.Other;
                            for (j = PortIDs.GetLowerBound(0); j <= PortIDs.GetUpperBound(0); j++)
                            {
                                if (sdevs[i].ToLower().IndexOf(PortIDs[j].PortKey) > 0)
                                {
                                    PortList[i].Type = PortIDs[j].PortType;
                                    break;
                                }
                            }
                        }
                        // Sort list by port number (bubble sort)
                        do
                        {
                            swpd = false;
                            for (i = 1; i < portcnt; i++)
                            {
                                if (PortList[i].Number < PortList[i - 1].Number)
                                {
                                    tmpifo = PortList[i];
                                    PortList[i] = PortList[i - 1];
                                    PortList[i - 1] = tmpifo;
                                    swpd = true;
                                }
                            }
                        } while (swpd);
                    }
                }
                catch (Exception)
                {
                    portcnt = 0;
                    PortList = new PortInfo[portcnt];
                };
            }
            else
            {
                portcnt = 4;
                PortList = new PortInfo[portcnt];
                for (i = 0; i < portcnt; i++)
                {
                    PortList[i].Name = "COM" + (i + 1).ToString(); ;
                    PortList[i].Number = i + 1;
                    PortList[i].Type = PortTypes.USB;
                }
            }
#else
            throw new NotSupportedException();
#endif
        }

        private int PortCount(PortTypes PortType)
        {
            int i, ptctr = 0; ;

            if (PortType == PortTypes.Any)
            {
                return portcnt;
            }
            else
            {
                for (i = 0; i < portcnt; i++)
                    if (PortList[i].Type == PortType) ptctr++;
                return ptctr;
            }
        }
        private PortInfo[] PortList;
#endregion

        #region Constructor/Destructor
        /// <summary>Creates a new empty list of ccTalk devices.</summary>
        public CcTalkDeviceList()
        {
            CcTalkDevices = new CcTalkComm[MaxDeviceCount];
            acccnt = 0;
        }
        #endregion

        /// <summary>Maximum number of CcTalkDevices.</summary>
        public const int MaxDeviceCount = 32;

        /// <summary>Number of CcTalkDevices found by <see cref="SearchDevices(byte[])"/>.</summary>
        public int Count
        {
            get { return acccnt; }
            set { acccnt = value; }
        }
        /// <summary>
        /// Defines which non-ccTalk devices should be searched.
        /// </summary>
        public SearchOptions Options = SearchOptions.SearchCcTalk;
        /// <summary>
        /// If InDepthSearch set to true devices will be found at any valid address.
        /// provided the devices support MDCES Address Poll.
        /// </summary>
        public bool InDepthSearch
        {
            get { return indptsearch; }
            set { indptsearch = value; }
        }
        /// <summary>
        /// Defines the type of port to be searched for devices. Default is whPortTypes.USB.
        /// A value of whPortTypes.Any lets you search at any existing port.
        /// </summary>
        public PortTypes SearchPortType
        {
            get { return porttp; }
            set { porttp = value; }
        }
        public byte MdbCashlessDeviceAddress
        {
            get { return cashlessaddr; }
            set { cashlessaddr = value; }
        }
        /// <summary>List of CcTalkDevices found by by <see cref="SearchDevices(byte[])"/>.</summary>
        public CcTalkComm[] CcTalkDevices;

        // Valid addresses
        internal int adrcnt;
        internal int rcvtot = 250;
        internal int mdbtot = 20;
        internal byte[] addrs = new byte[260];
        internal CcTalkComm srchcomm = new CcTalkComm();

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

        // Process a list of devices
        private void ProcessSearchList(int port, byte[] pin, ScanDevice[] addrlist)
        {
            srchcomm.CcTalkReceiveTimeout = rcvtot;
            srchcomm.MdbReceiveTimeout = mdbtot;
            for (int a = 0; a < addrlist.Length; a++)
            {
                srchcomm.Address = addrlist[a].Address;
                srchcomm.ChecksumType = addrlist[a].ChecksumType;
                srchcomm.EncryptionMode = addrlist[a].Encryption;
                // Check via Simple Poll
                if (srchcomm.SimplePoll() == CcTalkErrors.Ok)
                {
                    // Check all categories
                    CcTalkCategory cat = srchcomm.Category;
                    for (int c = 0; c < CategoryRange.Length; c++)
                    {
                        if ((cat == CategoryRange[c]) && (acccnt < MaxDeviceCount))
                        {
                            acccnt++;
                            CcTalkDevices[acccnt - 1] = new CcTalkComm();
                            CcTalkDevices[acccnt - 1].Port = this.PortList[port].Name;
                            CcTalkDevices[acccnt - 1].Address = srchcomm.Address;
                            CcTalkDevices[acccnt - 1].ChecksumType = srchcomm.ChecksumType;
                            CcTalkDevices[acccnt - 1].EncryptionMode = srchcomm.EncryptionMode;
                            break;
                        }
                    }
                }
                if (srchcomm.EncryptionMode != CcTalkEncryption.None) srchcomm.SendBreak();
            }
        }

        /// <summary>
        /// Search for available ccTalk devices, for encrypted devices the default PIN "123456" will be used.
        /// </summary>
        /// <remarks>
        /// After calling this function the first <see cref="Count"/> items of <see cref="CcTalkDevices"/>
        /// contain valid instances of <see cref="CcTalkComm"/>.
        /// </remarks>
        public void SearchDevices()
        {
            byte[] pin = new byte[] { 1, 2, 3, 4, 5, 6 };
            SearchDevices(pin);
        }

        /// <summary>
        /// Search for available ccTalk devices for encrypted devices the provided PIN will be used.
        /// </summary>
        /// <remarks>
        /// After calling this function the first <see cref="Count"/> items of <see cref="CcTalkDevices"/>
        /// contain valid instances of <see cref="CcTalkComm"/>.
        /// </remarks>
        public void SearchDevices(byte[] PIN)
        {
            srchcomm.CcTalkReceiveTimeout = rcvtot;
            srchcomm.MdbReceiveTimeout = mdbtot;

            GetCommList();
            acccnt = 0;

            for (int i = 0; i < this.PortCount(PortTypes.Any); i++)
            {
                if ((this.PortList[i].Type == porttp) || (porttp == PortTypes.Any))
                {
                    srchcomm.Port = this.PortList[i].Name;
                    srchcomm.Address = 0;
                    srchcomm.MdbCashlessDeviceAddress = cashlessaddr;
                    if (srchcomm.OpenComm() == CcTalkErrors.Ok)
                    {
                        // Search for ccTalk Devices
                        if ((Options & SearchOptions.SearchCcTalk) == SearchOptions.SearchCcTalk)
                        {
                            if (!indptsearch)
                            {
                                #region Process list of devices
                                ProcessSearchList(i, PIN, CcTalkAdresses);
                                #endregion
                            }
                            else
                            {
                                #region Search via Address Poll
                                for (int cst = 0; cst < 2; cst++)
                                {
                                    switch (cst)
                                    {
                                        case 0:
                                            srchcomm.ChecksumType = CcTalkChecksumTypes.Simple8;
                                            break;
                                        case 1:
                                            srchcomm.ChecksumType = CcTalkChecksumTypes.CRC16;
                                            break;
                                    }
                                    Thread.Sleep(250);
                                    srchcomm.AddressPoll(out adrcnt, ref addrs);
                                    Thread.Sleep(250);
                                    // Check all adresses
                                    for (int a = 0; a < adrcnt; a++)
                                    {
                                        srchcomm.Address = addrs[a];
                                        // Check all categories
                                        CcTalkCategory cat = srchcomm.Category;
                                        for (int c = 0; c <= CategoryRange.GetUpperBound(0); c++)
                                        {
                                            if ((cat == CategoryRange[c]) && (acccnt < MaxDeviceCount))
                                            {
                                                acccnt++;
                                                CcTalkDevices[acccnt - 1] = new CcTalkComm
                                                {
                                                    Port = PortList[i].Name,
                                                    Address = srchcomm.Address,
                                                    ChecksumType = srchcomm.ChecksumType,
                                                    EncryptionMode = srchcomm.EncryptionMode
                                                };
                                                break;
                                            }
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                        // Search for MDB Devices
                        if ((Options & SearchOptions.SearchMDB) == SearchOptions.SearchMDB)
                        {
                            ProcessSearchList(i, PIN, MdbAdresses);
                        }
                        srchcomm.CloseComm();
                    }
                }
            }
        }

    }
#endregion

    #region Coin Scale

    #endregion

    #region Dongle

    #endregion

    #region Escrow Sorter

    #endregion

    #region Payout device

    #endregion

    #region Bill Validator

    #endregion

    #region Coin Selector

    #endregion

    #region Change Giver (MDB)

    #endregion

    #region Cashless Payment (MDB)

    #endregion

    #region Coin Feeder

    #endregion

    #region Basic Communication

    #region Encryption and security

    #endregion

#if !WindowsCE

#endif

    #endregion

    #region Constants for MDB devices via CCT 900

    #endregion
}
