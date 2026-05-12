namespace ccTalk
{
    /// <summary>
    /// Possible modes of weighing
    /// </summary>
    public enum WeighingMode
    {
        /// <summary>Default mode - controlled by internal flag.</summary>
        Default = 0,
        /// <summary>Raw ADC value.</summary>
        ADCValue = 1,
        /// <summary>Number of coins.</summary>
        Coins = 2,
        /// <summary>Grams.</summary>
        Grams = 3,
    }
}