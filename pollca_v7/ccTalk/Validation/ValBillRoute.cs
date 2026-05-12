namespace ccTalk.Bill
{
    /// <summary>Destination of a bill used by RouteBill().</summary>
    public enum ValBillRoute
    {
        /// <summary>Return the bill from escrow position.</summary>
        Return = 0,
        /// <summary>Stack the bill from escrow position.</summary>
        Stack = 1,
        /// <summary>Extend escrow timeout.</summary>
        Hold = 255,
    }
}