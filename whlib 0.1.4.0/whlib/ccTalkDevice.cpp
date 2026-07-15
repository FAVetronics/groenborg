//////////////////////////////////////////////////////////////////////
// ccTalkDevice.cpp: Implementierung der Klasse CccTalkDevice.
//
// Base Class for ccTalk Devices
// open Device with pointer on Transport Layer Class CcTalk
//
//////////////////////////////////////////////////////////////////////

#include "ccTalkDevice.h"

#define EMP_COIN_INDEX_ERR -1

#define CHECK_IS_OPEN()    {if(!IsOpen()) return SetError(SIO_OPEN_ERR);}
#define CHECK_COIN_INDEX(a,b)  {if((a)<0 || (a)>=CCT_MAX_VALUES) return (b);}
#define CHECK_COIN_INDEX_NR(a) {if((a)<0 || (a)>=CCT_MAX_VALUES) return ;}

//////////////////////////////////////////////////////////////////////
// construktor/destruktor
//////////////////////////////////////////////////////////////////////

CccTalkDevice::CccTalkDevice()
{
    m_DeviceAdr= 0;         // invalid address
    m_ccTalk= NULL;         // no service
    memset(m_ValDataList, 0, sizeof(m_ValDataList));
    m_BaseYear= 0;
}

//////////////////////////////////////////////////////////////////////
/// initialize cunstructor
/// \param see Open() function

CccTalkDevice::CccTalkDevice(int Adr, CccTalk *ccTalk, CccTalk::CCT_CSUM_TYPE CsumType)
{
    m_DeviceAdr= Adr;
    m_ccTalk= ccTalk;
    m_CsumType= CsumType;
}

CccTalkDevice::~CccTalkDevice()
{

}


//////////////////////////////////////////////////////////////////////
// Basics

//////////////////////////////////////////////////////////////////////
/// Open ccTalk device.
/// \param Adr Device ccTalk address
/// \param ccTalk Pointer to communication class CccTalk
/// \return none

int CccTalkDevice::Open(int Adr, CccTalk *ccTalk, CccTalk::CCT_CSUM_TYPE CsumType)
{
    m_DeviceAdr= Adr;
    m_ccTalk= ccTalk;
    m_CsumType= CsumType;

    return 0;
}

//////////////////////////////////////////////////////////////////////
/// Test if communication port is initialized and is opened
/// \param none
/// \return true or false

bool CccTalkDevice::IsOpen()
{
    if(m_DeviceAdr > 0 && m_ccTalk != NULL && m_ccTalk->IsOpen())
    	return true;
    else
    	return false;
}

//////////////////////////////////////////////////////////////////////
/// Close device. The referenced communication port may still be opened
/// \param none
/// \return none

void CccTalkDevice::Close()
{
    m_DeviceAdr= 0;
    m_ccTalk= NULL;
}

//////////////////////////////////////////////////////////////////////
/// Returns proteced device address
/// \param none
/// \return device address

int CccTalkDevice::GetDeviceAddress(void)
{
    return m_DeviceAdr;
}

//////////////////////////////////////////////////////////////////////
/// Reset cctalk device.
/// After ResetDevice() mormaly the device gives no response to the master
/// \return none

//////////////////////////////////////////////////////////////////////
/// Resets cctalk device
/// After ResetDevice() mormaly the device gives no response to the master
/// \return none

int CccTalkDevice::ResetDevice(void)
{
    if(IsOpen())
    {
        m_ccTalk->SetCRCType(m_CsumType);
        return  m_ccTalk->ResetDevice(m_DeviceAdr);
    }
    else
        return SetError(SIO_OPEN_ERR);
}

//////////////////////////////////////////////////////////////////////
/// Returns adress mode
/// \param Vers reference to string class
/// \return error code, 0= no error

int CccTalkDevice::RequestAddressMode(unsigned char *Mode)
{
    if(IsOpen())
    {
        m_ccTalk->SetCRCType(m_CsumType);
        return m_ccTalk->RequestAddressMode(m_DeviceAdr, Mode);
    }
    else
        return SetError(SIO_OPEN_ERR);
}

//////////////////////////////////////////////////////////////////////
/// Clears communication status
/// \return error code, 0= no error

int CccTalkDevice::ClearCommsStatus(void)
{
    if(IsOpen())
    {
        m_ccTalk->SetCRCType(m_CsumType);
        return m_ccTalk->ClearCommsStatus(m_DeviceAdr);
    }
    else
        return SetError(SIO_OPEN_ERR);
}

//////////////////////////////////////////////////////////////////////
/// Returns string of communitcation revision
/// \param RevStr reference to string class
/// \return error code, 0= no error

int CccTalkDevice::RequestCommsRevision(string &Str)
{
    if(IsOpen())
    {
        m_ccTalk->SetCRCType(m_CsumType);
        return m_ccTalk->RequestCommsRevision(m_DeviceAdr, Str);
    }
    else
        return SetError(SIO_OPEN_ERR);
}

//////////////////////////////////////////////////////////////////////
/// Returns communication 3 byte error counters
/// \param Status pointer to unsigned byte buffer,
///  Status[0]= rx timeouts
///  Status[1]= rx bytes ingnored
///  Status[2]= rx bad checksums
/// \return error code, 0= no error

int CccTalkDevice::RequestCommsStatus(unsigned char *Status)
{
    if(IsOpen())
    {
        m_ccTalk->SetCRCType(m_CsumType);
        return m_ccTalk->RequestCommsStatus(m_DeviceAdr, Status);
    }
    else
        return SetError(SIO_OPEN_ERR);
}

//////////////////////////////////////////////////////////////////////
/// Returns communication error counters
/// \param RxTimeOuts reference to receive timeout counter
/// \param BytesIgnored reference to receive bytes ignored counter
/// \param BadChecksums reference to receive bad checksums counter
/// \return error code, 0= no error

int CccTalkDevice::RequestCommsStatus(int Addr, int &RxTimeOuts, int &BytesIgnored, int &BadChecksums)
{
    if(IsOpen())
    {
        m_ccTalk->SetCRCType(m_CsumType);
        return m_ccTalk->RequestCommsStatus(Addr, RxTimeOuts, BytesIgnored, BadChecksums);
    }
    else
        return SetError(SIO_OPEN_ERR);
}

//////////////////////////////////////////////////////////////////////
/// Returns string of communication revision
/// \param RevStr reference to string class
/// \return error code, 0= no error

int CccTalkDevice::RequestCommsRevision(unsigned char *Code)
{
    if(IsOpen())
    {
        m_ccTalk->SetCRCType(m_CsumType);
        return m_ccTalk->RequestCommsRevision(m_DeviceAdr, Code);
    }
    else
        return SetError(SIO_OPEN_ERR);
}
//////////////////////////////////////////////////////////////////////
/// Returns string of software revision
/// \param Vers reference to string class
/// \return error code, 0= no error

int CccTalkDevice::RequestSoftwareRevision(string &Vers)
{
    if(IsOpen())
    {
        m_ccTalk->SetCRCType(m_CsumType);
        return m_ccTalk->RequestSoftwareRevision(m_DeviceAdr, Vers);
    }
    else
        return SetError(SIO_OPEN_ERR);
}

//////////////////////////////////////////////////////////////////////
/// Returns serialnumber
/// \param ManId reference to unsigned long, size is limited to 3 bytes / 24 bit
/// \return error code, 0= no error

int CccTalkDevice::RequestSerialNumber(unsigned long &Serial)
{
    if(IsOpen())
    {
        m_ccTalk->SetCRCType(m_CsumType);
        return m_ccTalk->RequestSerialNumber(m_DeviceAdr, Serial);
    }
    else
        return SetError(SIO_OPEN_ERR);
}

//////////////////////////////////////////////////////////////////////
/// Return string of build code
/// \param Build  reference to string class
/// \return error code, 0= no error

int CccTalkDevice::RequestBuildCode(string &Build)
{
    if(IsOpen())
    {
        m_ccTalk->SetCRCType(m_CsumType);
        return m_ccTalk->RequestBuildCode(m_DeviceAdr, Build);
    }
    else
        return SetError(SIO_OPEN_ERR);
}

//////////////////////////////////////////////////////////////////////
/// Return string of product code
/// \param Code  reference to string class
/// \return error code, 0= no error

int CccTalkDevice::RequestProductCode(string &Code)
{
    if(IsOpen())
    {
        m_ccTalk->SetCRCType(m_CsumType);
        return m_ccTalk->RequestProductCode(m_DeviceAdr, Code);
    }
    else
        return SetError(SIO_OPEN_ERR);
}
//////////////////////////////////////////////////////////////////////
/// Return string of category ID
/// \param CatId reference to string class
/// \return error code, 0= no error

int CccTalkDevice::RequestCategoryID(string &CatId)
{
    if(IsOpen())
    {
        m_ccTalk->SetCRCType(m_CsumType);
        return m_ccTalk->RequestCategoryID(m_DeviceAdr, CatId);
    }
    else
        return SetError(SIO_OPEN_ERR);
}

//////////////////////////////////////////////////////////////////////
/// Core Command return string of manufactory ID
/// \param ManId  reference to string class
/// \return error code, 0= no error

int CccTalkDevice::RequestManufacturerID(string &ManId)
{
    if(IsOpen())
    {
        m_ccTalk->SetCRCType(m_CsumType);
        return m_ccTalk->RequestManufacturerID(m_DeviceAdr, ManId);
    }
    else
        return SetError(SIO_OPEN_ERR);
}

//////////////////////////////////////////////////////////////////////
/// Poll devices adresses and stores answer in a adress list
/// \param pointer to address list
/// \return number of found devices

int CccTalkDevice::AddressPoll(int *PollTbl)
{
    if(IsOpen())
    {
        m_ccTalk->SetCRCType(m_CsumType);
        return m_ccTalk->AddressPoll(PollTbl);
    }
    else
        return SetError(SIO_OPEN_ERR);
}

////////////////////////////////////////////////////////////////////////////////
// Spezial Standards

//////////////////////////////////////////////////////////////////////
/// Returns ASCII with coin ID
/// \param Coin Index (zero based) of requested coins 0...15
/// \param CoinId string with Coin Id
/// \return error code, 0= no error

int CccTalkDevice::RequestCoinId(int Coin, char *CoinId)                    // 184
{
    char par;
    int buflen= CCT_VALUE_ID_LEN;
    int err= 0;

    CHECK_IS_OPEN();
    CHECK_COIN_INDEX(Coin,EMP_COIN_INDEX_ERR);

    memset(CoinId, 0, CCT_VALUE_ID_LEN+1);
    par = Coin+CCT_EMP_COIN_OFFSET;

    m_ccTalk->SetCRCType(m_CsumType);

    err = m_ccTalk->RequestBinDataEx(m_DeviceAdr, CCT_REQUEST_COIN_ID, &par, 1, CoinId, buflen);

    return err;
}


#ifdef USE_STRING

//////////////////////////////////////////////////////////////////////
/// Returns ASCII string with coin ID
/// \param Coin Index (zero based) of requested coins 0...15
/// \param CoinId reference to string class with Coin Id
/// \return error code, 0= no error

int CccTalkDevice::RequestCoinId(int Coin, string &CoinId)                       // 184
{
    int err= 0;
    char buffer[CCT_VALUE_ID_LEN+1];

    CHECK_IS_OPEN();

    memset(buffer, 0, sizeof(buffer));
    err= RequestCoinId(Coin, buffer);   // No Offset
    if(!err)
        CoinId= buffer;

    return err;
}

#endif

#if USE_QT
//////////////////////////////////////////////////////////////////////
/// Returns ASCII string with coin ID
/// \param Coin Index (zero based) of requested coins 0...15
/// \param CoinId reference to QtString class with Coin Id
/// \return error code, 0= no error

int CccTalkDevice::RequestCoinId(int Coin, QString &CoinId)                 // 184
{
    int err= 0;
    char buffer[CCT_VALUE_ID_LEN+1];

    CHECK_IS_OPEN();
    memset(buffer, 0, sizeof(buffer));

    err= RequestCoinId(Coin, buffer);       // No Offset
    if(!err)
        CoinId= QString::fromAscii(buffer);
    return err;
}
#endif


CccTalkDevice::VALUE_FACTOR CccTalkDevice::m_ValueFactors[] = {
    {'m',	   0.001 },
    {' ',	   1.000 },
    {'.',	   1.000 },
    {'K',	1000.000 },
    {'M',    1000000.000 },
    {'G', 1000000000.000 },
    {' ', 0}
};


CccTalkDevice::COUNTRY_CODE CccTalkDevice::m_CountryCode[]= {
    {"EU", "EUR", "Euro Nation",        "%4.2lf", 100},
    {"CH", "CHF", "Swiss",              "%4.2lf", 100},
    {"DK", "DKK", "Danmark",            "%4.2lf", 100},
    {"CA", "CAD", "Canada",             "%4.2lf", 100},
    {"US", "USD", "USA",                "%4.2lf", 100},
    {"GB", "GBP", "United Kingdom",     "%4.2lf", 100},
    {"NO", "NOK", "Norway",             "%4.2lf", 100},
    {"SE", "SEK", "Sweden",             "%4.2lf", 100},
    {"PL", "PLN", "Poland",             "%4.2lf", 100},
    {"JM", "JMD", "Jamaika",            "%4.2lf", 100},

    {NULL, "???", "unknown",            "%4.2lf", 100}        // not in list, default Format
};



//////////////////////////////////////////////////////////////////////
/// Returns string with coin country code
/// \param Coin Index (zero based) of requested coins 0...15
/// \param CoinId reference to string class with country code
/// \param Update true= request new data from device, false= use data in database
/// \return error code, 0= no error

int CccTalkDevice::RequestCoinCountryCode(int Coin, char * CountryCode, bool Update)
{
    int err= 0;

    CHECK_IS_OPEN();

    if(Update)
    {
        err = RequestCoinId(Coin, m_ValDataList[Coin].ValId);     // No Offset
    }
    if(!err)
    {
        strncpy(CountryCode, m_ValDataList[Coin].ValId, CCT_COUNTRY_CODE_LEN);
        CountryCode[CCT_COUNTRY_CODE_LEN]= 0;
    }
    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Calculates coin value in database. Before calling data have to request from the device
/// using RequestCoinId(), RequestCoinCountryCode(...,true)
/// use it only implizit with functions RequestCoinValue(), RequestCoinValueStr(),
/// RequestCoinName(), RequestCoinValueStr(), RequestCoinCountryCode()
/// \param Coin Index (zero based) of requested coins 0...15
/// \return none

void CccTalkDevice::CalculateCoinValue(int Coin)
{
    char valstr[CCT_VALUE_ID_LEN+1];
    double value= 0.0;
    int i;
    char *pos, *CoinId;
    char countrycode[CCT_COUNTRY_CODE_LEN+1];

    CoinId= m_ValDataList[Coin].ValId;
    memset(countrycode, 0, sizeof(countrycode));

    RequestCoinCountryCode(Coin, countrycode, false);

    for (i = 0; m_CountryCode[i].Id; i++)
    {
         if (!strcmp(m_CountryCode[i].Id, countrycode))
                break;
    }
    memcpy(&m_ValDataList[Coin].CountryCode, &m_CountryCode[i], sizeof(COUNTRY_CODE));

    if(strlen(CoinId) == CCT_VALUE_ID_LEN && isalpha(CoinId[0]))      // not empty
    {
        strcpy(valstr, &CoinId[CCT_COUNTRY_CODE_LEN]);
        sscanf(valstr, "%lf", &value);
        m_ValDataList[Coin].Value = value;

        for(i = 0; m_ValueFactors[i].Factor != 0; i++)
        {
//            pos = strchr((const char*)valstr, m_ValueFactors[i].FactorChar);
            pos = strchr(valstr, m_ValueFactors[i].FactorChar);
            if(pos != NULL)
            {
                *pos= '.';           // set float
                sscanf(valstr, "%lf", &value);
                m_ValDataList[Coin].Value*= m_ValueFactors[i].Factor;
                break;
            }
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
/// Returns coin value as fixed integer
/// \param Coin Index (zero based) of requested coins 0...15
/// \param Value reference to coin value
/// \param Devisor reference to value devisor
/// \param Update true= request new data from device, false= use data in database
/// \return error code, 0= no error

int CccTalkDevice::RequestCoinValue(int Coin, long &Value, int &Devisor,  bool Update)
{
    int err= 0;

    CHECK_IS_OPEN();

    if(Update)
    {
        err = RequestCoinId(Coin, m_ValDataList[Coin].ValId);     // No Offset
        if(!err)
            CalculateCoinValue(Coin);
    }
    Value= m_ValDataList[Coin].Value;
    Devisor=  m_ValDataList[Coin].CountryCode.Devisor;

    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Returns coin value as float
/// \param Coin Index (zero based) of requested coins 0...15
/// \param Value reference to coin value
/// \param Update true= request new data from device, false= use data in database
/// \return error code, 0= no error

int CccTalkDevice::RequestCoinValue(int Coin, double & Value, bool Update)
{
    int err= 0;

    CHECK_IS_OPEN();

    if(Update)
    {
        err = RequestCoinId(Coin, m_ValDataList[Coin].ValId);     // No Offset
        if(!err)
            CalculateCoinValue(Coin);
    }
    Value= (double)m_ValDataList[Coin].Value / (double)m_ValDataList[Coin].CountryCode.Devisor;

    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Returns coin value as string
/// \param Coin Index (zero based) of requested coins 0...15
/// \param ValStrue reference to char
/// \param Delimiter placeholder as delimiter character
/// \param Update true= request new data from device, false= use data in database
/// \return error code, 0= no error

int CccTalkDevice::RequestCoinValueStr(int Coin, char * ValStr, char Delimiter, bool Update)
{
    int err= 0;
    char *pos;
    double coinvalue;

    CHECK_IS_OPEN();

    err= RequestCoinValue(Coin, coinvalue, Update);     // No Offset
    if (!err)
    {
        sprintf(ValStr, m_ValDataList[Coin].CountryCode.Format, coinvalue);
        pos = strchr(ValStr, '.');
        if (pos != NULL)
           *pos = Delimiter;
    }
    return err;
}


////////////////////////////////////////////////////////////////////////////////
/// Returns full coin name
/// \param Coin Index (zero based) of requested coins 0...15
/// \param Name reference to char
/// \param Delimiter placeholder as delimiter character
/// \param Update true= request new data from device, false= use data in database
/// \return error code, 0= no error

int CccTalkDevice::RequestCoinName(int Coin, char * Name, char Delimiter, bool Update)
{
    char countrycode[CCT_COUNTRY_CODE_LEN+1];
    double coinvalue;
    int err= 0;
    char *pos;

    CHECK_IS_OPEN();

    memset(countrycode, 0, sizeof (countrycode));
    err = RequestCoinValue(Coin, coinvalue, Update); // No Offset

    if(!err)
    {
        sprintf(Name, m_ValDataList[Coin].CountryCode.Format, coinvalue); //  / m_CountryCode[i].Devisor);
        pos = strchr(Name, '.');
        if (pos != NULL)
            *pos = Delimiter;
        strcat(Name, m_ValDataList[Coin].CountryCode.Currency);
    }

    return err;
}


#if USE_QT

//////////////////////////////////////////////////////////////////////
/// Returns string with coin country code
/// \param Coin Index (zero based) of requested coins 0...15
/// \param CountryCode reference to QString class
/// \param Update true= request new data from device, false= use data in database
/// \return error code, 0= no error

int CccTalkDevice::RequestCoinCountryCode(int Coin, QString &CountryCode, bool Update)
{
    int err;
    char countrycode[CCT_COUNTRY_CODE_LEN+1];

    err= RequestCoinCountryCode(Coin, countrycode, Update);     // No Offset

    if(!err)
        CountryCode= QString::fromAscii(countrycode);

    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Returns coin value as string
/// \param Coin Index (zero based) of requested coins 0...15
/// \param ValStr reference to QString class
/// \param Delimiter placeholder as delimiter character
/// \param Update true= request new data from device, false= use data in database
/// \return error code, 0= no error

int CccTalkDevice::RequestCoinValueStr(int Coin, QString &ValStr, char Delimiter, bool Update)
{
    int err;
    char str[CCT_VALUE_VAL_STR_LEN+1];

    err= RequestCoinValueStr(Coin, str, Delimiter, Update);     // No Offset

    if(!err)
        ValStr= QString::fromAscii(str);

    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Returns full coin name
/// \param Coin Index (zero based) of requested coins 0...15
/// \param CoinName reference to QString class
/// \param Delimiter placeholder as delimiter character
/// \param Update true= request new data from device, false= use data in database
/// \return error code, 0= no error

int CccTalkDevice::RequestCoinName(int Coin, QString &CoinName, char Delimiter, bool Update)
{
    int err;
    char str[CCT_VALUE_NAME_LEN+1];

    err= RequestCoinName(Coin, str, Delimiter, Update);         // No Offset

    if(!err)
        CoinName= QString::fromAscii(str);

    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Returns country name from coin data base. Call one Reqest... function first
/// \param Coin Index (zero based) of requested coins 0...15
/// \return country name as QString

QString CccTalkDevice::GetCountryName(int Coin)
{
    QString Country;
    int i, err;
    char countrycode[CCT_COUNTRY_CODE_LEN+1];

    memset(countrycode, 0, sizeof(countrycode));

    err= RequestCoinCountryCode(Coin , countrycode, false);           // No Offset
    if (!err)
    {
        for (i = 0; m_CountryCode[i].Id; i++)
        {
            if (!strcmp(m_CountryCode[i].Id, countrycode))
                break;
        }

        if (m_CountryCode[i].Id != NULL)
        {
            Country = QString::fromAscii(m_CountryCode[i].Country);
        } else
        {
            Country = "unknown";
        }
    }
    return Country;
}

////////////////////////////////////////////////////////////////////////////////
/// Returns currency name from coin data base. Call one Reqest... function first
/// \param Coin Index (zero based) of requested coins 0...15
/// \return currency name as QString

QString CccTalkDevice::GetCurrencyName(int Coin)
{
    QString currency= "";
    int i, err;
    char countrycode[CCT_COUNTRY_CODE_LEN+1];

    err = RequestCoinCountryCode(Coin, countrycode, false); // No Offset
    if (!err)
    {
        for (i = 0; m_CountryCode[i].Id; i++)
        {
            if (!strcmp(m_CountryCode[i].Id, countrycode))
                break;
        }

        if (m_CountryCode[i].Id != NULL)
        {
            currency = QString::fromAscii(m_CountryCode[i].Currency);
        } else
        {
            currency = "unknown";
        }
    }
    return currency;
}

////////////////////////////////////////////////////////////////////////////////
/// Returns coin id string from coin data base. Call one Reqest... function first
/// \param Coin Index (zero based) of requested coins 0...15
/// \return coin id as QString

QString CccTalkDevice::GetCoinId(int Coin)
{
    CHECK_COIN_INDEX(Coin,"");

    return QString::fromAscii(m_ValDataList[Coin].ValId);     // No Offset
}

#endif



//////////////////////////////////////////////////////////////////////
// extended core commands

//////////////////////////////////////////////////////////////////////
/// Request coin acceptor status
/// \param Status refenece to returned status
/// \return error code, 0= no error

int CccTalkDevice::RequestVariableSet(unsigned char *Var, int &Len)         // 247
{
    int err;

    CHECK_IS_OPEN();

    m_ccTalk->SetCRCType(m_CsumType);
    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_REQUEST_VARIABLE_SET, Len, &Var);

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Returns the input lines status
/// \param Lines refrence tor return variable status input lines, depending on product specifications
/// \return error code, 0= no error

int CccTalkDevice::ReadInputLines(unsigned char &Lines)           // 237
{
    int err;
    int len= 1;

    CHECK_IS_OPEN();

    m_ccTalk->SetCRCType(m_CsumType);
    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_READ_INPUT_LINES, len, &Lines);

    return err;
}
//////////////////////////////////////////////////////////////////////
/// Returns state of the device optos
/// \param Opto reference to return variable, Bit mask
/// \return error code, 0= no error

int CccTalkDevice::ReadOptoStates(unsigned char &Opto)                       // 236
{
    int err;
    int len= 1;

    CHECK_IS_OPEN();

    m_ccTalk->SetCRCType(m_CsumType);
    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_READ_OPTO_STATES, len, &Opto);

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Set the 16 bit inhibit pattern of the device
/// \param Mask 16 Bit mask, Bit 0= first coin bit 15 last coin, set mask bit to 1
/// to enable, clear to disable
/// \return error code, 0= no error

int CccTalkDevice::ModifyInhibitStatus(unsigned short Mask)       // 231
{
    int err;

    CHECK_IS_OPEN();

    m_ccTalk->SetCRCType(m_CsumType);
    err= m_ccTalk->SendData(m_DeviceAdr, CCT_MODIFY_INHIBIT_STATUS, 2, &Mask);

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Return 16 bit inhibit pattern of the device
/// \param Mask reference to 16 Bit mask, Bit 0= first coin bit 15 last coin,
/// enabled= mask bit is set to 1, disabled= mask bit ist cleared
/// \return error code, 0= no error

int CccTalkDevice::RequestInhibitStatus(unsigned short &Mask)     // 230
{
    int err;
    int buflen= 2;

    CHECK_IS_OPEN();

    Mask= 0;
    m_ccTalk->SetCRCType(m_CsumType);
    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_REQUEST_INHIBIT_STATUS, buflen, &Mask);

    return err;
}


////////////////////////////////////////////////////////////////////////////////
/// Change the master inhibit bit in the device
/// \param Inhibit true= device is disabled, false= device works normal
/// \return error code, 0= no error

int CccTalkDevice::ModifyMasterInhibitStatus(bool Inhibit)   // 228
{
    int err;
    unsigned char cmd;

    CHECK_IS_OPEN();

    cmd= Inhibit ? 0 : 1;
    m_ccTalk->SetCRCType(m_CsumType);
    err= m_ccTalk->SendData(m_DeviceAdr, CCT_MODIFY_MASTER_INHIBIT_STATUS, 1, &cmd);
    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Returns the master inhibit status
/// \param Inhibit reference to the result buffer
///  - true= device is disabled
///  - false= device works normal
/// \return error code, 0= no error

int CccTalkDevice::RequestMasterInhibitStatus(bool &Inhibit)       // 227
{
    int err;
    char buf;
    int buflen= 1;

    CHECK_IS_OPEN();
    
    m_ccTalk->SetCRCType(m_CsumType);
    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_REQUEST_MASTER_INHIBIT_STATUS, buflen, &buf);

    if(!err)
        Inhibit = buf == 0;
    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Returns the insertion counter
/// \param Count reference to the result buffer, the result is unsigned int
/// maximal 24 bit
/// \return error code, 0= no error

int CccTalkDevice::RequestInsertionCounter(long &Count)           // 226
{
    int err;
    int buflen= sizeof(long);

    CHECK_IS_OPEN();

    Count= 0;

    m_ccTalk->SetCRCType(m_CsumType);
    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_REQUEST_INSERTION_COUNTER, buflen, &Count);

    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Returns the counter of accepted coins
/// \param Count reference to the result buffer, the result is unsigned int
/// maximal 24 bit
/// \return error code, 0= no error

int CccTalkDevice::RequestAcceptCounter(long &Count)              // 225
{
    int err;
    int buflen= sizeof(long);

    CHECK_IS_OPEN();

    Count= 0;

    m_ccTalk->SetCRCType(m_CsumType);
    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_REQUEST_ACCEPT_COUNTER, buflen, &Count);

    return err;
}


////////////////////////////////////////////////////////////////////////////////
/// Return the manufactoring date / factory setup date
/// \param Year reference to returned year value
/// \param Month reference to returned month value
/// \param date reference to returned date value
/// \return error code, 0= no error

int CccTalkDevice::RequestCreationDate(int &Year, int &Month, int &Day)  // 196
{
    int err = 0;
    unsigned short buf;
    int buflen= sizeof(buf);

    CHECK_IS_OPEN();

    m_ccTalk->SetCRCType(m_CsumType);
    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_REQUEST_CREATION_DATE, buflen, &buf);

    if(m_BaseYear <= 0)
        RequestBaseYear(m_BaseYear);

    if(!err && buflen == 2)
    {
        Day= buf & 0x1f;
        Month= (buf >> 5) & 0x0f;
        Year=  ((buf >> 9) & 0x1f) + m_BaseYear;
    }

    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Return the manufactoring date / factory setup date
/// \param Time reference to returned time_t value
/// \return error code, 0= no error

int CccTalkDevice::RequestCreationDate(time_t &Time)                      // 196
{
    int err = 0;
    int y, m, d;
    struct tm t;

    CHECK_IS_OPEN();

    err= RequestCreationDate(y, m, d);

    if(!err)
    {
        t.tm_sec= 0;            /* seconds after the minute - [0,59] */
        t.tm_min= 0;            /* minutes after the hour - [0,59] */
        t.tm_hour= 0;           /* hours since midnight - [0,23] */
        t.tm_mday= d;           /* day of the month - [1,31] */
        t.tm_mon= m - 1;        /* months since January - [0,11] */
        t.tm_year= y - 1900;    /* years since 1900 */
        t.tm_wday= 0;           /* days since Sunday - [0,6] */
        t.tm_yday= 0;           /* days since January 1 - [0,365] */
        t.tm_isdst= 0;          /* daylight savings time flag */

        Time= mktime(&t);
    }
    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Return the last modification date
/// \param Year reference to returned year value
/// \param Month reference to returned month value
/// \param date reference to returned date value
/// \return error code, 0= no error

int CccTalkDevice::RequestLastModificationDate(int &Year, int &Month, int &Day) // 195
{
    int err = 0;
    unsigned short buf;
    int buflen= sizeof(buf);

    CHECK_IS_OPEN();

    m_ccTalk->SetCRCType(m_CsumType);
    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_REQUEST_LAST_MODIFICATION_DATE, buflen, &buf);

    if(m_BaseYear <= 0)
        RequestBaseYear(m_BaseYear);

    if(!err && buflen == 2)
    {
        Day= buf & 0x1f;
        Month= (buf >> 5) & 0x0f;
        Year=  ((buf >> 9) & 0x1f) + m_BaseYear;
    }

    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Return the last modification date
/// \param Time reference to returned time_t value
/// \return error code, 0= no error

int CccTalkDevice::RequestLastModificationDate(time_t &Time)                      // 195
{
    int err = 0;
    int y, m, d;
    struct tm t;

    CHECK_IS_OPEN();

    err= RequestLastModificationDate(y, m, d);

    if(!err)
    {
        t.tm_sec= 0;            /* seconds after the minute - [0,59] */
        t.tm_min= 0;            /* minutes after the hour - [0,59] */
        t.tm_hour= 0;           /* hours since midnight - [0,23] */
        t.tm_mday= d;           /* day of the month - [1,31] */
        t.tm_mon= m - 1;        /* months since January - [0,11] */
        t.tm_year= y - 1900;    /* years since 1900 */
        t.tm_wday= 0;           /* days since Sunday - [0,6] */
        t.tm_yday= 0;           /* days since January 1 - [0,365] */
        t.tm_isdst= 0;          /* daylight savings time flag */

        Time= mktime(&t);
    }
    return err;
}
////////////////////////////////////////////////////////////////////////////////
/// Return products base year. Inpizit called in RequestCreationDate() and
/// RequestLastModificationDate()
/// \param Year reference to returned base year
/// \return error code, 0= no error

int CccTalkDevice::RequestBaseYear(int &Year)                   // 170
{
    int err;
    char buf[4];
    int buflen= 4;

    CHECK_IS_OPEN();

    m_ccTalk->SetCRCType(m_CsumType);
    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_REQUEST_BASE_YEAR, buflen, buf);

    if(!err && buflen == 4)
    {
        Year=  buf[3];
        Year+= buf[2]*10;
        Year+= buf[1]*100;
        Year+= buf[1]*1000;
    }

    return err;
}


////////////////////////////////////////////////////////////////////////////////
/// Function for ccTalk data encryption. These virtual function my overload by
/// the developer. No source code is offered here because a NDA license ist needed
/// from coin master ltd
/// RequestLastModificationDate()
/// \param Data pointer of data,
/// \param Len size of data in byte count
/// \return none

void CccTalkDevice::Encrypt(unsigned char *Data, int Len)
{
}

////////////////////////////////////////////////////////////////////////////////
/// Function for ccTalk data decryption. These virtual function my overload by
/// the developer. No source code is offered here because a NDA license ist needed
/// from coin master ltd
/// RequestLastModificationDate()
/// \param Data pointer of data,
/// \param Len size of data in byte count
/// \return none

void CccTalkDevice::Decrypt(unsigned char *Data, int Len)
{
}


// 
////////////////////////////////////////////////////////////////////////////////
