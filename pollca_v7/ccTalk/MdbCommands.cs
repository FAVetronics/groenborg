using System;

namespace ccTalk
{
    [Serializable]
    internal class MdbCommands
    {
        // MDB basic Commands
        public const byte Reset = 0x00;
        public const byte Setup = 0x01;
        public const byte TubeStatus = 0x02;
        public const byte Security = 0x02;
        public const byte Poll = 0x03;
        public const byte CoinType = 0x04;
        public const byte BillType = 0x04;
        public const byte Escrow = 0x05;
        public const byte Dispense = 0x05;
        public const byte Expansion = 0x07;
        // MDB Expansion Commands
        public const byte Identify = 0x00;
        public const byte TubeStatusEx = 0x15;
        // Cashless Payment Commands
        public const byte CPReset = 0x00;
        public const byte CPRevalueRequest = 0x00;
        public const byte CPSetup = 0x01;
        public const byte CPRevalueLimit = 0x01;
        public const byte CPPoll = 0x02;
        public const byte CPVend = 0x03;
        public const byte CPSetDateTime = 0x03;
        public const byte CPEnable = 0x04;
        public const byte CPEnableFeatures = 0x04;
        public const byte CPRevalue = 0x05;
        public const byte CPCashSale = 0x05;
        public const byte CPRequToReceive = 0xfa;
        public const byte CPRetryDeny = 0xfb;
        public const byte CPSendBlock = 0xfc;
        public const byte CPOkToSend = 0xfd;
        public const byte CPRequToSend = 0xfe;
    }
}