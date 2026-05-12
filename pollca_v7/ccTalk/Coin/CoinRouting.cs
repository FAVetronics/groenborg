namespace ccTalk
{
    /// <summary>
    /// Change giver coin routing.
    /// </summary>
    public enum CoinRouting
    {
        /// <summary>Coin routing is unknown.</summary>
        Unknown,
        /// <summary>Coin was routed into the cash box.</summary>
        CashBox,
        /// <summary>Coin was routed into a tube.</summary>
        Tube,
        /// <summary>Coin was rejected for some reason.</summary>
        Reject,
        /// <summary>Coin was manually dispensed.</summary>
        Dispense,
    }
}