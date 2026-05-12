namespace ccTalk
{
    /// <summary>
    /// Possible states of escrow device.
    /// </summary>
    public enum EscrowState
    {
        /// <summary>Status unknown.</summary>
        Unknown,
        /// <summary>Escrow closed.</summary>
        Closed = 0,
        /// <summary>Collect coins.</summary>
        Collect = 1,
        /// <summary>Return coins.</summary>
        Return = 2,
    }
}