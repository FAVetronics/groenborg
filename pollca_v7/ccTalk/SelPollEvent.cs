namespace ccTalk
{
    /// <summary>
    /// Coin selector and change giver poll events.
    /// </summary>
    public enum SelPollEvent
    {
        /// <summary>A coin was accepted.</summary>
        Coin = 512,
        /// <summary>Device was reset.</summary>
        Reset = 513,
        /// <summary>Unknown event.</summary>
        Unknown = 514,
        /// <summary>Nothing to report.</summary>
        Null = 0,
        /// <summary>Reject: coin not recognized.</summary>
        CoinReject = 1,
        /// <summary>Reject: coin inhibited (see: <see cref="SelectorComm.MasterInhibit"/>.</summary>
        CoinInhibit = 2,
        /// <summary>Multiple Window.</summary>
        MultipleWindow = 3,
        /// <summary>Wake-up timeout.</summary>
        WakeUpTimeout = 4,
        /// <summary>Validation timeout.</summary>
        ValidationTimeout = 5,
        /// <summary>Credit sensor timeout.</summary>
        CreditSensorTimeout = 6,
        /// <summary>Sorter opto timeout.</summary>
        SorterOptoTimeout = 7,
        /// <summary>Reject: follow up coin.</summary>
        FollowUp = 8,
        /// <summary>Accept gate not ready.</summary>
        AcceptGateNotReady = 9,
        /// <summary>Credit sensor not ready.</summary>
        CreditSensorNotReady = 10,
        /// <summary>Sorter not ready</summary>
        SorterNotReady = 11,
        /// <summary>Validation sensor not ready.</summary>
        ValidationSensorNotReady = 12,
        /// <summary>Device busy.</summary>
        Busy = 13,
        /// <summary>Credit sensor blocked.</summary>
        CreditSensorBlocked = 14,
        /// <summary>Sorter opto blocked.</summary>
        SorterOptoBlocked = 15,
        /// <summary>Credit sequence blocked.</summary>
        CreditSequenceError = 16,
        /// <summary>Coin going backwards.</summary>
        CoinGoingBackwards = 17,
        /// <summary>Coin too fast.</summary>
        CoinTooFast = 18,
        /// <summary>Coin too slow.</summary>
        CoinJam = 19,
        /// <summary>Coin on a string.</summary>
        CoinOnString = 20,
        /// <summary>DCE(?) opto timeout.</summary>
        DceOptoTimeout = 21,
        /// <summary>DCE(?) opto not seen.</summary>
        DceOptoNotSeen = 22,
        /// <summary>Credit sensor reached too early.</summary>
        CreditSensorReachedTooEarly = 23,
        /// <summary>Reject coin.</summary>
        RejectCoin = 24,
        /// <summary>Reject slug.</summary>
        RejectSlug = 25,
        /// <summary>Reject sensor blocked.</summary>
        RejectSensorBlocked = 26,
        /// <summary>Games overload.</summary>
        GamesOverload = 27,
        /// <summary>Maximum coin meter pulses exceeded.</summary>
        MaxPulsesExceeded = 28,
        /// <summary>Accept gate open not closed.</summary>
        AcceptGateNotClosed = 29,
        /// <summary>Accept gate closed not open.</summary>
        AcceptGateNotOpen = 30,
        /// <summary>Manifold opto timeout.</summary>
        ManifoldOptoTimeout = 31,
        /// <summary></summary>
        ManifeldOptoBlocked = 32,
        /// <summary>Manifold not ready.</summary>
        ManifoldNotReady = 33,
        /// <summary>Security status changed.</summary>
        SecirityStatusChanged = 34,
        /// <summary>Motor exception.</summary>
        MotorException = 35,
        /// <summary>Swallowed coin.</summary>
        SwallowedCoin = 36,
        /// <summary>Coin too fast (over validation sensor).</summary>
        CoinTooFast2 = 37,
        /// <summary>Coin too slow (over validation sensor).</summary>
        CoinTooSlow = 38,
        /// <summary></summary>
        CoinIncorrectlySorted = 39,
        /// <summary>External light attack.</summary>
        ExternalLightAttack = 40,
        /// <summary>Coin number 1 inhibited.</summary>
        CoinInhibit00 = 128,
        /// <summary>Coin number 2 inhibited.</summary>
        CoinInhibit01 = 129,
        /// <summary>Coin number 3 inhibited.</summary>
        CoinInhibit02 = 130,
        /// <summary>Coin number 4 inhibited.</summary>
        CoinInhibit03 = 131,
        /// <summary>Coin number 5 inhibited.</summary>
        CoinInhibit04 = 132,
        /// <summary>Coin number 6 inhibited.</summary>
        CoinInhibit05 = 133,
        /// <summary>Coin number 7 inhibited.</summary>
        CoinInhibit06 = 134,
        /// <summary>Coin number 8 inhibited.</summary>
        CoinInhibit07 = 135,
        /// <summary>Coin number 9 inhibited.</summary>
        CoinInhibit08 = 136,
        /// <summary>Coin number 10 inhibited.</summary>
        CoinInhibit09 = 137,
        /// <summary>Coin number 11 inhibited.</summary>
        CoinInhibit10 = 138,
        /// <summary>Coin number 12 inhibited.</summary>
        CoinInhibit11 = 139,
        /// <summary>Coin number 13 inhibited.</summary>
        CoinInhibit12 = 140,
        /// <summary>Coin number 14 inhibited.</summary>
        CoinInhibit13 = 141,
        /// <summary>Coin number 15 inhibited.</summary>
        CoinInhibit14 = 142,
        /// <summary>Coin number 16 inhibited.</summary>
        CoinInhibit15 = 143,
        /// <summary>Dat block request.</summary>
        DataBlockRequest = 253,
        /// <summary>Return lever pushed.</summary>
        Return = 254,
        /// <summary>Unspecified alarm code.</summary>
        UnspecifiedCode = 255,
        /// <summary>Change giver  is paying out.</summary>
        PayoutBusy = 640,
        /// <summary>Coin routing error.</summary>
        RoutingError = 641,
        /// <summary>Coin acceptor was removed from the change giver.</summary>
        Unplugged = 642,
        /// <summary>Change giver tube has a defective sensor.</summary>
        TubeSensor = 643,
        /// <summary>Change giver tube is jammed.</summary>
        TubeJam = 644,
        /// <summary>Change giver ROM checksum error.</summary>
        RomCheckSum = 645,
        /// <summary>A credited coin was possibly removed from the change giver.</summary>
        CoinRemoved = 646,
    }
}