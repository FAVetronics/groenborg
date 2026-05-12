namespace ccTalk
{
    /// <summary>
    /// ccTalk cash devices categories.
    /// </summary>
    /// <remarks>
    /// Currently only CoinSelector and BillValidator are implemented.
    /// </remarks>
    public enum CcTalkCategory
    {
        ///<summary>Unknown device.</summary>
        Unknown,
        ///<summary>Coin Selector.</summary>
        CoinSelector,
        ///<summary>Bill Validator.</summary>
        BillValidator,
        ///<summary>Card Reader.</summary>
        CardReader,
        ///<summary>Hopper (payout device).</summary>
        PayOut,
        ///<summary>Coin Scale.</summary>
        CoinScale,
        ///<summary>Dongle (Peripheral Device).</summary>
        Peripheral,
        ///<summary>Change Giver (MDB device via dongle).</summary>
        ChangeGiver,
        ///<summary>Changer (Group of hoppers).</summary>
        Changer,
        ///<summary>Coin Feeder.</summary>
        CoinFeeder,
        ///<summary>Cashless Payment System (MDB device via dongle).</summary>
        Cashless,
        ///<summary>Multi path sorter with escrow function.</summary>
        EscrowSorter,
        ///<summary>Bootloader device.</summary>
        Bootloader,
    }
}