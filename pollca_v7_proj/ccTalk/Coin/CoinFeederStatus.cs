namespace ccTalk
{
    /// <summary>
    /// Possible states of the Coin Feeder.
    /// </summary>
    public enum CoinFeederStatus
    {
        /// <summary>Device was powered on, not ready yet.</summary>
        PowerOn = 0,
        /// <summary>Device was restarted.</summary>
        Restart = 1,
        /// <summary>Ready for action.</summary>
        Ready = 2,
        /// <summary>Ready, at least one coin inside.</summary>
        Coins = 3,
        /// <summary>Coin ready for feed.</summary>
        CoinReady = 4,
        /// <summary>Device locked by main blocking.</summary>
        Locked = 5,
        /// <summary>Ejecting single coin</summary>
        Eject = 6,
        /// <summary>Ejecting all coins</summary>
        EjectAll = 7,
        /// <summary>Reverse movement.</summary>
        Reverse = 8,
        /// <summary>Cap is open.</summary>
        CapOpen = 9,
        /// <summary>Cap is open, purge in progress.</summary>
        CapPurge = 10,
        /// <summary>Cap is closing.</summary>
        CapClosing = 11,
        /// <summary>Common error.</summary>
        Error = 100,
        /// <summary>Emergency, motor stopped by external event.</summary>
        Emergency = 101,
        /// <summary>Motor stopped due to internal error.</summary>
        Stop = 102,
        /// <summary>Nobody knows...</summary>
        Unknown,
    }
}