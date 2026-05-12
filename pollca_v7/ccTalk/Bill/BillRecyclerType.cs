namespace ccTalk
{
    /// <summary>Types of currently supported bill recyclers.</summary>
    public enum BillRecyclerType
    {
        /// <summary>No recycler present.</summary>
        None,
        /// <summary>JCM VEGA bill validator with recycler, connected via ccTalk.</summary>
        JCMVegaCcTalk,
    }
}