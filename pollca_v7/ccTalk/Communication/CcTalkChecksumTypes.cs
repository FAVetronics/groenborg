namespace ccTalk
{
    /// <summary>
    /// ccTalk cash devices checksum type.
    /// </summary>
    /// <remarks>
    /// Currently simple 8-Bit and 16-bit CRC (x^16+x^12+x^5+1) are implemented.
    /// </remarks>
    public enum CcTalkChecksumTypes
    {
        ///<summary>Simple 8-bit checksum.</summary>
        Simple8,
        ///<summary>16-bit CRC checksum.</summary>
        CRC16,
    };
}