namespace ccTalk
{
    /// <summary>
    /// Cryptography methodes on command level.
    /// </summary>
    /// <remarks>
    /// Currently only DES encryption is supported.
    /// </remarks>
    public enum CcTalkCryptography
    {
        ///<summary>No cryptography.</summary>
        None = 0,
        ///<summary>DES Encryption.</summary>
        DES = 101,
        ///<summary>AES Encryption (future support only).</summary>
        AES = 102,
        ///<summary>Triple DES Encryption (future support only).</summary>
        TripleDES = 103,
    }
}