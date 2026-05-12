namespace ccTalk
{
    /// <summary>
    /// Possible settings for anti-pin-system controlled via CCT 9x0.
    /// </summary>
    public enum AntiPinSetting
    {
        /// <summary>Setting unknown.</summary>
        Unknown,
        /// <summary>Disabled - always closed.</summary>
        Disabled = 0,
        /// <summary>Opens automatically when a coin approaches.</summary>
        Auto = 1,
        /// <summary>Always open.</summary>
        Open = 2,
        /// <summary>Enable an ES 00x.A (Escrow Sorter TWS 100 only).</summary>
        Enable,
    }
}