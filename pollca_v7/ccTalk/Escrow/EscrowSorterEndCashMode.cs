namespace ccTalk
{
    /// <summary>The mode EndCash will be handled in the MultiEscrow sorting mode.</summary>
    public enum EscrowSorterEndCashMode
    {
        /// <summary>The inserted coins will be hold in the escrow until meeting their sorting position.</summary>
        Hold = 0,
        /// <summary>All inserted coin will be sorted immediately.</summary>
        Sort = 1,
    }
}