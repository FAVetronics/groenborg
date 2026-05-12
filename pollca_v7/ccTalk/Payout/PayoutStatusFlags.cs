using System;

namespace ccTalk
{
    /// <summary>
    /// Payout status flags.
    /// </summary>
    [Flags]
    public enum PayoutStatusFlags : long
    {
        /// <summary>Nothing to report.</summary>
        Nothing = 0x000000,
        // Register 1
        /// <summary>Absolute maximum current exceeded.</summary>
        CurrentExceeded = 0x000001,
        /// <summary>Payout timeout  occured</summary>
        PayoutTimeout = 0x000002,
        /// <summary>Motor reversed to clear a jam</summary>
        MotorReversed = 0x000004,
        /// <summary>Opto fraud attempt, path blocked during idle.</summary>
        OptoIdleBlocked = 0x000008,
        /// <summary>Opto fraud attempt, short-circuit during idle.</summary>
        OptoIdleShort = 0x000010,
        /// <summary>Opto fraud attempt, blocked during payout.</summary>
        OptoPayoutBlocked = 0x000020,
        /// <summary>Power-up detected.</summary>
        PowerUp = 0x000040,
        /// <summary>Payout disabled.</summary>
        PayoutDisabled = 0x000080,
        // Register 2
        /// <summary>Opto fraud attempt, short-circuit during payout.</summary>
        OptoPayoutShort = 0x000100,
        /// <summary>Single coin mode.</summary>
        SingleCoin = 0x000200,
        /// <summary>Use other payout for remaining change.</summary>
        UseOtherPayout = 0x000400,
        /// <summary>Opto fraud attempt.</summary>
        OptFraud = 0x000800,
        /// <summary>Motor reverse limit reached.</summary>
        ReverseLimit = 0x001000,
        /// <summary>Inductive coil fault</summary>
        InductiveCoilFault = 0x002000,
        /// <summary>Power fail during non-volatile memory write.</summary>
        PowerFail = 0x004000,
        /// <summary>PIN number mechanism.</summary>
        PinNumber = 0x008000,
        // Register 3
        /// <summary>Power down during payout</summary>
        PowerDownPayout = 0x010000,
        /// <summary>Unknown coin type paid out.</summary>
        UnknownCoin = 0x020000,
        /// <summary>PIN number incorrect.</summary>
        WrongPIN = 0x040000,
        /// <summary>Cipher key incorrect</summary>
        WrongKey = 0x080000,
        /// <summary>Encryption enabled.</summary>
        Encryption = 0x100000,
        /// <summary>Proprietary: card pending.</summary>
        CardPending = 0x800000,
        // Register 4
        /// <summary>X5: Hall sensor faulty.</summary>
        HallSensorError = 0x01000000,
        /// <summary>X5: Right pocket is blocked.</summary>
        RightPocketBlocked = 0x02000000,
        /// <summary>X5: Left pocket is blocked.</summary>
        LeftPocketBlocked = 0x04000000,
        /// <summary>X5: Coin didn't pass or stopped ont the recovery light barrier.</summary>
        CoinRecoveryError = 0x08000000,
        /// <summary>X5: Coin didn't pass or stopped ont the payment light barrier.</summary>
        CoinDeliveryError = 0x10000000,
        /// <summary>X5: Lack of polling during purge.</summary>
        PurgeTimeoutError = 0x20000000,
        /// <summary>X5: Sensor calibration is running.</summary>
        SensorCalibration = 0x40000000,
        // Kommen nicht aus den Bytes
        /// <summary>Payout was reset.</summary>
        Reset = 0x0100000000,
        /// <summary>Hopper is busy purging.</summary>
        Purging = 0x8000000000,
    }
}