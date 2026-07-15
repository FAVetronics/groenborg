////////////////////////////////////////////////////////////////////////////////
// base class for ccTalk devices.
// ccTalkDevice.h: base class for ccTalk devices
//
/// \file Source file class ccTalkDevice base class for ccTalk devices
////////////////////////////////////////////////////////////////////////////////


#ifndef _CCTALKDEVICE_H
#define _CCTALKDEVICE_H

////////////////////////////////////////////////////////////////////////////////

#include "CccTalk.h"

#ifndef USE_QT
#define USE_QT      0
#endif

#define USE_STRING  1

#if USE_STRING
#include <string>
#endif

#if USE_QT
#include <QtGui/QApplication>
#endif

////////////////////////////////////////////////////////////////////////////////
// ccTalk header definitions
// Coin Id

#define CCT_EMP_COIN_OFFSET               1
#define CCT_VALUE_ID_LEN                  6
#define CCT_COUNTRY_CODE_LEN              2
#define CCT_VALUE_VAL_STR_LEN            15
#define CCT_VALUE_NAME_LEN               15
#define CCT_MAX_VALUES                   16

/// Base class for ccTalk devices.
////////////////////////////////////////////////////////////////////////////////
/// This class is used as base class for all ccTalk devices. With the Open()
/// function the encapsulation the device address and link the device to the
/// communication class CccTalk. More than one device may linked to the same
/// communication class (same bus). On Close() only the device the device is
/// closend, not the communication class.
/// You have to initialize a communication class CccTalk for every physical
/// ccTalk bus before Open() a device. After closing ale devices you must close
/// the commmunication port.
////////////////////////////////////////////////////////////////////////////////

class CccTalkDevice
{
public:
// initializer
    CccTalkDevice();
    virtual ~CccTalkDevice();

// Basics
/// @name Base Functions
//@{
    virtual int Open(int Adr, CccTalk * ccTalk, CccTalk::CCT_CSUM_TYPE CsumType= CccTalk::CCT_CSUM_SIMPLE);
    virtual void Close(void);
    virtual bool IsOpen(void);
    CccTalkDevice(int Adr, CccTalk * ccTalk, CccTalk::CCT_CSUM_TYPE CsumType);
    int GetDeviceAddress(void);

//@}

// Core Commands
/// @name Core Commands
//@{
    int ResetDevice(void);
    int RequestAddressMode(unsigned char *Mode);
    int ClearCommsStatus(void);
    int RequestCommsRevision(string &Str);
    int RequestCommsStatus(unsigned char *Status);
    int RequestCommsStatus(int Addr, int &RxTimeOuts, int &BytesIgnored, int &BadChecksums);
    int RequestCommsRevision(unsigned char *Code);
    int RequestSoftwareRevision(string &Vers);
    int RequestSerialNumber(unsigned long &Serial);
    int RequestBuildCode(string &Build);
    int RequestProductCode(string &Code);
    int RequestCategoryID(string &CatId);
    int RequestManufacturerID(string &ManId);
    int AddressPoll(int *PollTbl);    
    int RequestCoinId(int Coin, char *CoinId);                    // 184
//@}


/// @name extended Core Commands used for coin acceptors, payouts and bill vaildators
//@{
    int RequestVariableSet(unsigned char *Var, int &Len);         // 247
    int ReadInputLines(unsigned char &Lines);               // 237
    int ReadOptoStates(unsigned char &Opto);               // 236
    int ModifyInhibitStatus(unsigned short Mask);       // 231
    int RequestInhibitStatus(unsigned short &Mask);     // 230
    int ModifyMasterInhibitStatus(bool Inhibt= true);   // 228
    int RequestMasterInhibitStatus(bool &Inhibt);       // 227
    int RequestInsertionCounter(long &Count);           // 226
    int RequestAcceptCounter(long &Count);              // 225
    int RequestCreationDate(int &Year, int &Month, int &Day);  // 196
    int RequestCreationDate(time_t &Time);                      // 196
    int RequestLastModificationDate(int &Year, int &Month, int &Day);  // 195
    int RequestLastModificationDate(time_t &Time);                      // 195
    int RequestBaseYear(int &Year);                     // 170
///}

/// @name extended functions using string class
//@{
#if USE_STRING
    int RequestCoinId(int Coin, string &CoinId);                  // 184
#endif
//@}

/// @name extended functions
//@{
    void CalculateCoinValue(int Coin);
    int RequestCoinCountryCode(int Coin, char * CountryCode, bool Update= true);
    int RequestCoinValueStr(int Coin, char * ValStr, char Delimiter, bool Update= true);
    int RequestCoinValue(int Coin, double & Value, bool Update= true);
    int RequestCoinValue(int Coin, long &Value, int &Devisor,  bool Update);
    int RequestCoinName(int Coin, char * Name, char Delimiter= ',', bool Update= true);
//@}

/// @name extended functions using Qt library
//@{
#if USE_QT
    int RequestCoinId(int Coin, QString &CoinId);                 // 184
    int RequestCoinCountryCode(int Coin, QString &CountryCode, bool Update);
    int RequestCoinValueStr(int Coin, QString &ValStr, char Delimiter, bool Update= true);
    int RequestCoinName(int Coin, QString &ValStr, char Delimiter= ',', bool Update= true);
    QString GetCountryName(int Coin);
    QString GetCurrencyName(int Coin);
    QString GetCoinId(int Coin);
    QString CoinTotalStr(int Coin);
    QString CoinTotalSumStr(int First= 0, int Last= CCT_MAX_VALUES);
#endif
//@}

/// @name security functions
//@{
    virtual void Encrypt(unsigned char *Data, int Len);
    virtual void Decrypt(unsigned char *Data, int Len);
//@}

// locals
protected:
    int m_DeviceAdr;
    CccTalk * m_ccTalk;     // pointer to the basic driver funktions
    enum CccTalk::CCT_CSUM_TYPE m_CsumType;

    typedef struct {
        char FactorChar;
        double Factor;
    } VALUE_FACTOR;

    typedef struct {
        const char *Id;
        const char *Currency;
        const char *Country;
        const char *Format;
        int Devisor;
    } COUNTRY_CODE;

    typedef struct VAL_DATA {
        char ValId[CCT_VALUE_ID_LEN + 1];
        double Value;
        int Factor;
        COUNTRY_CODE CountryCode;
        char ValueStr[CCT_VALUE_VAL_STR_LEN + 1];
        char Count;             // coins accepted
        double Total;           // Tatal value of accepted coins
//        char Format[EMP_CCT_COIN_VAL_STR_LEN+1];
    } ValData;

protected:
    static VALUE_FACTOR m_ValueFactors[];
    static COUNTRY_CODE m_CountryCode[];
    ValData m_ValDataList[CCT_MAX_VALUES];
    /// Base year for calculation in RequestCreationDate()
    /// and RequestLastModificationDate()
    int m_BaseYear;
};

#endif // _CCTALKDEVICE_H

////////////////////////////////////////////////////////////////////////////////
