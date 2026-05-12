namespace ccTalk
{
    ///<summary>Pay out reject code.</summary>
    public enum BillPayoutRejectCode
    {
        /// <summary>Normal status, no errors.</summary>
        Normal = 0x00,
        /// <summary>Invalid bill length detected.</summary>
        LengthError = 0x01,
        /// <summary>Recycler level error.</summary>
        LevelError = 0x02,
        /// <summary>Other pay out error.</summary>
        OtherError = 0x03,
    }
}