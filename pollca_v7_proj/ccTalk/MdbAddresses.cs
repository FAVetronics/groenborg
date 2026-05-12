using System;

namespace ccTalk
{
    [Serializable]
    internal class MdbAddresses
    {
        // Addresses in ccTalk address space
        public const byte CcChangeGiver = 240;
        public const byte CcBillValidator = 242;
        public const byte CcCashless = 244;
        // MDB Addresses
        public const byte MdbChangeGiver = 0x08;
        public const byte MdbBillValidator = 0x30;
        public static byte[] MdbCashless = new byte[] { 0x60, 0x10 }; // 0x18 };
    }
}