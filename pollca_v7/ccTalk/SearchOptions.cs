using System;

namespace ccTalk
{
    /// <summary>
    /// Options for device search: which devices should be searched.
    /// </summary>
    [Flags]
    public enum SearchOptions
    {
        /// <summary>Search for nothing - a rather useless definition :-).</summary>
        SearchNothing = 0x0000,
        /// <summary>Search for MDB devices connected via CCT 900/910.</summary>
        SearchMDB = 0x0001,
        /// <summary>Search for ccTlak devices.</summary>
        SearchCcTalk = 0x0004,
    }
}