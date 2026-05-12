namespace ccTalk
{
    /// <summary>Escrow Sorter insertion status as detected by the sensor.</summary>
    public enum EscrowSorterInsertionStatus
    {
        /// <summary>Status unknown.</summary>
        Unknown,
        /// <summary>Not supported by the current hardware or no ACK.</summary>
        Failure,
        /// <summary>No coin or other object detected.</summary>
        Empty,
        /// <summary>Coin or other object detected.</summary>
        Coin,
        /// <summary>Insertion sensor not active.</summary>
        NotActive,
        /// <summary>Insertion sensor not ready yet.</summary>
        NotReady,
    }
}