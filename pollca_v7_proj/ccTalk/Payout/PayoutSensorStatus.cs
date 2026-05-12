namespace ccTalk
{
    /// <summary>
    /// Possible states of payout level sensors.
    /// </summary>
    public enum PayoutSensorStatus
    {
        /// <summary>Status unknown.</summary>
        Unknown,
        /// <summary>Sensor not installed or not supported.</summary>
        NotSupported,
        /// <summary>Sensor is not triggered.</summary>
        Untriggered,
        /// <summary>Sensor is triggered.</summary>
        Triggered,
    }
}