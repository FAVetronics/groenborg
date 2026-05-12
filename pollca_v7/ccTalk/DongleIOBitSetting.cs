namespace ccTalk
{
    /// <summary>
    /// Bit configuration for the programmable I/O port, CCT 910 only.
    /// </summary>
    public enum DongleIOBitSetting
    {
        /// <summary>Normal input</summary>
        Input = 0,
        /// <summary>Input with pull up resistor.</summary>
        InputPullUp = 1,
        /// <summary>Normal output.</summary>
        Output = 8,
        /// <summary>Output with open drain.</summary>
        OutputOpen = 9,
    }
}