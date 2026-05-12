namespace ccTalk
{
    public enum EscrowSortingMode
    {
        /// <summary>Special setting - don't try this at home.</summary>
        Locked = 0,
        /// <summary>Escrow mode, can handle up to 50 coins.</summary>
        Escrow = 1,
        /// <summary>Direct sorter - coins will be sorted immediately. Any number of coins can be handled.</summary>
        DirectSorter = 2,
        /// <summary>Multi-escrow - to be implemented</summary>
        MultiEscrow = 3,
    }
}