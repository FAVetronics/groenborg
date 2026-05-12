using System;

namespace ccTalk
{
    /// <summary>
    /// wh Hopper Coin Scale Communication class.
    /// </summary>
    [Serializable]
    public class CoinScaleComm : CcTalkComm
    {
        #region Constructor/Destructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// Sets some default values.
        /// </remarks>
        public CoinScaleComm()
        {
            Address = SCALE_ADR;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// <param name="basedevice">Instance of the base class<see cref="CcTalkComm"/> were some settings are taken from:</param>
        /// Address, Port and ChecksumType.
        /// </remarks>
        public CoinScaleComm(CcTalkComm basedevice)
        {
            Address = basedevice.Address;
            Port = basedevice.Port;
            ChecksumType = basedevice.ChecksumType;
        }
        #endregion

        /// <summary>
        /// Opens the com port and sets some default values.
        /// </summary>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public override CcTalkErrors OpenComm()
        {
            lasterr = base.OpenComm();

            if ((lasterr == CcTalkErrors.Ok) && !this.IsSupported(this.Manufacturer))
            {
                CloseComm();
                lasterr = CcTalkErrors.UnSupported;
            }
            lastcount = 0;
            lastweight = 0;
            return lasterr;
        }

        #region Private variables
        // Constants
        private const byte SCALE_ADR = 130;
        private int lastcount = 0, lastweight = 0;
        #endregion

        /// <summary>
        /// Retrieves values and currency IDs of the 16 coins.
        /// </summary>
        /// <param name="currvals">Array of <see cref="CoinValue"/>, must have at least 16 elements.</param>
        /// <param name="defcoin">Index of the active coin (set via DIP switches).</param> 
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetCoinValues(ref CoinValue[] currvals, ref int defcoin)
        {
            int i;
            if (currvals.Length < 16) return CcTalkErrors.WrongParameter;
            for (i = 0; i < 16; i++)
            {
                currvals[i] = GetCoinValue(i);
                if (lasterr != CcTalkErrors.Ok) return lasterr;
            }
            CoinValue actval = GetCoinValue(-1);
            defcoin = -1;
            for (i = 0; i < 16; i++)
            {
                if (actval.Compare(currvals[i]))
                {
                    defcoin = i;
                    break;
                }
            }

            return lasterr;
        }

        /// <summary>
        /// Retrieves the number of coins in the hopper.
        /// </summary>
        /// <param name="coinidx">Index of the coin to be weighted. A value of -1 addresses the active coin(set via DIP switches).</param>
        /// <param name="count">Number of coins weighed by the scale.</param>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors GetCoinCount(int coinidx, ref int count)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 2;
            sdta.Header = 90;
            sdta.Data[0] = (byte)(coinidx + 1);
            sdta.Data[1] = (byte)WeighingMode.Coins;
            lasterr = TalkCc(sdta, ref rdta);
            if ((lasterr == CcTalkErrors.Ok) && (rdta.DataLength >= 3))
            {
                if (rdta.Data[0] == 1)
                {
                    count = (short)(rdta.Data[2] | (rdta.Data[1] << 8));
                    //if (count < 0) count = 0;
                    lastcount = count;
                }
                else
                {
                    count = lastcount;
                }
            }
            return lasterr;
        }

        /// <summary>
        /// Retrieves the weight measured in grams.
        /// </summary>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// <param name="weight">The weight measured in grams.</param>
        /// </returns>
        public CcTalkErrors GetWeight(ref int weight)
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 2;
            sdta.Header = 90;
            sdta.Data[0] = 0;
            sdta.Data[1] = (byte)WeighingMode.Grams;
            lasterr = TalkCc(sdta, ref rdta);
            if ((lasterr == CcTalkErrors.Ok) && (rdta.DataLength >= 3))
            {
                if (rdta.Data[0] == 1)
                {
                    weight = (short)(rdta.Data[2] | (rdta.Data[1] << 8));
                    //if (weight < 0) weight = 0;
                    lastweight = weight;
                }
                else
                {
                    weight = lastweight;
                }
            }
            return lasterr;
        }

        /// <summary>
        /// Sets tara value of the scale. Always make sure the attached hopper is empty.
        /// </summary>
        /// <returns>
        /// <see cref="CcTalkErrors"/>.Ok if successful otherwise an error code.
        /// </returns>
        public CcTalkErrors SetTara()
        {
            CcTalkDataBlock sdta = new CcTalkDataBlock(cstype);
            CcTalkDataBlock rdta = new CcTalkDataBlock(cstype);

            sdta.DataLength = 2;
            sdta.Header = 93;
            sdta.Data[0] = 0;
            sdta.Data[1] = (byte)WeighingMode.Grams;
            lasterr = TalkCc(sdta, ref rdta);
            return lasterr;
        }
    }
}