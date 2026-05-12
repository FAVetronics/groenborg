namespace ccTalk
{
    /// <summary>
    /// Coin routing modes and coin slot states of the escrow sorter.
    /// </summary>
    public enum EscrowSorterCoinControl
    {
        /// <summary>Slot is empty</summary>
        Empty = 0,
        /// <summary>Coin will not be ejected automatically.</summary>
        NoEject,
        /// <summary>Coin will be routed to the default slot.</summary>
        Eject,
        /// <summary>Coin will be routed to the cash box.</summary>
        CashBox,
        /// <summary>Coin will be routed to the reject path.</summary>
        Reject,
        /// <summary>An error occured while trying to route the coin.</summary>
        Error,
    }
}