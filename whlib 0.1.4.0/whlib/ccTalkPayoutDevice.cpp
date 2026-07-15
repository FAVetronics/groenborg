////////////////////////////////////////////////////////////////////////////////
// File:   ccTalkPayoutDevice.cpp
// Author: manfred
// Copyright 2010:
//              wh Münzprüfer Berlin GmbH
//              Teltower Damm 276
//              D-10707 Berlin
//              Germany
//              info@whberlin.de
//
// Created: 2. August 2010 13:41
//
// 23.02.2012   Pointer mismatch in funktion
//              int ccTalkPayoutDevice::DispenseHopperCoins(...)
//              int ccTalkPayoutDevice::RequestIndexedHopperDispenseCount(...)
//
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
// Includes
#define	_CCTALKPAYOUTDEVICE_CPP_SRC

#include "CccTalk.h"
#include "ccTalkPayoutDevice.h"

////////////////////////////////////////////////////////////////////////////////
//
#define PAD_COIN_INDEX_ERR -1

#define CHECK_IS_OPEN()    {if(!IsOpen()) return SetError(SIO_OPEN_ERR);}
#define CHECK_COIN_INDEX(a,b)  {if((a)<0 || (a)>=EMP_CCT_MAX_COINS) return (b);}
#define CHECK_COIN_INDEX_NR(a) {if((a)<0 || (a)>=EMP_CCT_MAX_COINS) return ;}

////////////////////////////////////////////////////////////////////////////////
// Constructor / Destructor

ccTalkPayoutDevice::ccTalkPayoutDevice()
{
    m_Encrytion= ENCRYPT_NONE;
}

ccTalkPayoutDevice::~ccTalkPayoutDevice()
{
}


////////////////////////////////////////////////////////////////////////////////
/// Intialize the hopper coin counter to known value (208)
/// \param Count new counter value
/// \return error code, 0= no error

int ccTalkPayoutDevice::ModifyPayoutAbsoluteCount(unsigned int Count)
{

    CHECK_IS_OPEN();

    return m_ccTalk->SendData(m_DeviceAdr, CCT_MODIFY_PAYOUT_ABSULUTE_COUNT, 2, &Count);
}

////////////////////////////////////////////////////////////////////////////////
/// Intialize the hopper coin counter to known value, variant for multible hoppers (208)
/// \param Hopper hopper number
/// \param Count new counter value
/// \return error code, 0= no error

int ccTalkPayoutDevice::ModifyPayoutAbsoluteCount(int Hopper, unsigned int Count)
{
    unsigned char buf[3];

    CHECK_IS_OPEN();

    buf[0]= Hopper & 0xff;
    buf[1]= Count & 0xff;
    buf[2]= (Count >> 8) & 0xff;

    return m_ccTalk->SendData(m_DeviceAdr, CCT_MODIFY_PAYOUT_ABSULUTE_COUNT, 3, buf);
}

////////////////////////////////////////////////////////////////////////////////
/// Returns actual hopper coin counter (209)
/// \param Count refenece to returned counter value
/// \return error code, 0= no error

int ccTalkPayoutDevice::RequestPayoutAbsoluteCount(unsigned int &Count)
{
    int len= 2;

    CHECK_IS_OPEN();

    Count= 0;

    return m_ccTalk->RequestBinData(m_DeviceAdr, CCT_REQUEST_PAYOUT_ABSULUTE_COUNT, len, &Count);
}

////////////////////////////////////////////////////////////////////////////////
/// Returns actual hopper coin counter, variant for multiple hoppers (209)
/// \param Hopper hopper number
/// \param Count refenece to returned counter value
/// \return error code, 0= no error

int ccTalkPayoutDevice::RequestPayoutAbsoluteCount(int Hopper, unsigned int &Count)
{
    int err= 0;
    int len= 2;
    unsigned char cmd;

    CHECK_IS_OPEN();

    Count= 0;
    cmd= Hopper;

    return err = m_ccTalk->RequestBinDataEx(m_DeviceAdr, CCT_REQUEST_PAYOUT_ABSULUTE_COUNT, &cmd, 1, &Count, len);
}


////////////////////////////////////////////////////////////////////////////////
/// Set the maximum numbers of coin the payout device can hold (187)
/// \param Count new max coin numbers
/// \return error code, 0= no error

int ccTalkPayoutDevice::ModifyPayoutCapacity(unsigned int Count)
{
    int err= 0;

    CHECK_IS_OPEN();

    return err = m_ccTalk->SendData (m_DeviceAdr, CCT_MODIFY_PAYOUT_CAPACITY, 2, &Count);
}

////////////////////////////////////////////////////////////////////////////////
/// Set the maximum numbers of coin the payout device can hold variant for multible hoppers (187)
/// \param Hopper hopper number
/// \param Count new max coin numbers
/// \return error code, 0= no error

int ccTalkPayoutDevice::ModifyPayoutCapacity(int Hopper, unsigned int Count)
{
    int err= 0;
    unsigned char buf[3];

    CHECK_IS_OPEN();

    buf[0]= Hopper;
    buf[1]= Count & 0xff;
    buf[2]= (Count >> 8) & 0xff;

    return err = m_ccTalk->SendData(m_DeviceAdr, CCT_MODIFY_PAYOUT_CAPACITY, 3, buf);
}

////////////////////////////////////////////////////////////////////////////////
/// Returns maximum numbers of coin the payout device can hold (186)
/// \param Count refenece to returned maximal coin numbers
/// \return error code, 0= no error

int ccTalkPayoutDevice::RequestPayoutCapacity(unsigned int &Count)
{
    int err=0;
    int len= 2;

    CHECK_IS_OPEN();

    Count= 0;

    return err = m_ccTalk->RequestBinData(m_DeviceAdr, CCT_REQUEST_PAYOUT_CAPACITY, len, &Count);
}

////////////////////////////////////////////////////////////////////////////////
/// Returns maximum numbers of coin the payout device can hold, variant for multible hoppers (186)
/// \param Hopper hopper number
/// \param Count refenece to returned  maximal coin numbers
/// \return error code, 0= no error

int ccTalkPayoutDevice::RequestPayoutCapacity(int Hopper, unsigned int &Count)
{
    int err= 0;
    int len= 2;
    unsigned char cmd;

    CHECK_IS_OPEN();

    Count= 0;
    cmd= Hopper;

    return err = m_ccTalk->RequestBinDataEx(m_DeviceAdr, CCT_REQUEST_PAYOUT_CAPACITY, &cmd, 1, &Count, len);
}


////////////////////////////////////////////////////////////////////////////////
/// Set the working "float" level (175)
/// \param Level new float level
/// \return error code, 0= no error

int ccTalkPayoutDevice::ModifyPayoutFloat(unsigned int Level)
{
    int err= 0;

    CHECK_IS_OPEN();

    return err = m_ccTalk->SendData(m_DeviceAdr, CCT_MODIFY_PAYOUT_FLOAT, 2, &Level);
}

////////////////////////////////////////////////////////////////////////////////
/// Set the working "float" level, variant for multible hoppers (175)
/// \param Hopper hopper number
/// \param Count new counter value
/// \return error code, 0= no error

int ccTalkPayoutDevice::ModifyPayoutFloat(int Hopper, unsigned int Level)
{
    int err= 0;
    unsigned char buf[3];

    CHECK_IS_OPEN();

    buf[0]= Hopper & 0xff;
    buf[1]= Level & 0xff;
    buf[2]= (Level >> 8) & 0xff;

    return err = m_ccTalk->SendData(m_DeviceAdr, CCT_MODIFY_PAYOUT_FLOAT, 3, buf);
}

////////////////////////////////////////////////////////////////////////////////
/// Returns actual working "float" level (174)
/// \param Level refenece to returned level value
/// \return error code, 0= no error

int ccTalkPayoutDevice::RequestPayoutFloat(unsigned int &Level)
{
    int err=0;
    int len= 2;

    CHECK_IS_OPEN();

    Level= 0;

    return err = m_ccTalk->RequestBinData(m_DeviceAdr, CCT_REQUEST_PAYOUT_FLOAT, len, &Level);
}

////////////////////////////////////////////////////////////////////////////////
/// Returns actual working "float" level, variant for multible hoppers (174)
/// \param Hopper hopper number
/// \param Count refenece to returned level value
/// \return error code, 0= no error

int ccTalkPayoutDevice::RequestPayoutFloat(int Hopper, unsigned int &Level)
{
    int err= 0;
    int len= 2;
    unsigned char cmd;

    CHECK_IS_OPEN();

    Level= 0;
    cmd= Hopper;

    return err = m_ccTalk->RequestBinDataEx(m_DeviceAdr, CCT_REQUEST_PAYOUT_FLOAT, &cmd, 1, &Level, len);
}


////////////////////////////////////////////////////////////////////////////////
/// Halts the device immeadiatly and returns remaining coins (172)
/// \param Coins refenece to returned level value
/// \return error code, 0= no error

int ccTalkPayoutDevice::EmergencyStop(unsigned int &Coins)
{
    int err=0;
    int len= 2;

    CHECK_IS_OPEN();

    Coins= 0;

    return err = m_ccTalk->RequestBinData(m_DeviceAdr, CCT_EMERGENCY_STOP, len, &Coins);
}


#if USE_STRING
////////////////////////////////////////////////////////////////////////////////
/// Halts the device immeadiatly and returns remaining coins (171)
/// \param Coins refenece to returned level value
/// \return error code, 0= no error

int ccTalkPayoutDevice::RequestHopperCoin(string &CoinStr)
{
    CHECK_IS_OPEN();

    return m_ccTalk->RequestASCIIData(m_DeviceAdr, CCT_REQUEST_HOPPER_COIN, CoinStr);
}
#endif


////////////////////////////////////////////////////////////////////////////////
/// Return number of coins dispensed by the hopper (168)
/// \param CoinCount refenece to returned dispense counter
/// \return error code, 0= no error

int ccTalkPayoutDevice::RequestHopperDispenseCount(unsigned int &CoinCount)
{
    int err=0;
    int len= 2;

    CHECK_IS_OPEN();

    CoinCount= 0;

    return err = m_ccTalk->RequestBinData(m_DeviceAdr, CCT_REQUEST_HOPPER_DISPENSE_COUNT, len, &CoinCount);
}

////////////////////////////////////////////////////////////////////////////////
/// Command to dispense between 1 and 255 coins. (168)
/// Depend on hopper type different security algorithms ar used. \n
/// These library only manage "non encryption" and "serial number encryption". Use
/// extended function Dispense() for this two modes. \n
/// Encrypting algorithems stands under NDA of coin master and can't offer here.
/// This function can be use for tansparent data transfer.
/// \param DispenseData
/// \param Len length of data depend on encrypting mode
/// \param Count refenece to returned value, depend on hopper type
/// \return error code, 0= no error

int ccTalkPayoutDevice::DispenseHopperCoins(unsigned char *DispenseData, int Len, unsigned int &Event)
{
    int err= 0;
    int retlen= 4;

    CHECK_IS_OPEN();

    Event= 0;

    return err = m_ccTalk->RequestBinDataEx(m_DeviceAdr, CCT_DISPENSE_HOPPER_COINS, DispenseData, Len, &Event, retlen);
}



////////////////////////////////////////////////////////////////////////////////
/// Returns hopper status, the interprtin of the returned values depends on the
/// specification of the connected hopper type.
/// \param EventCounter reference to returned event counter, incremented one every
/// received valid dispense command. range 1...255, 0= power on
/// \param Remaining reference to returned value of remaining coins
/// \param Remaining reference to returned value of paid coins
/// \param Remaining reference to returned value of unpaid coins
/// \return error code, 0= no error

int ccTalkPayoutDevice::RequestHopperStatus(int &EventCounter, int &Remaining, int &Paid, int &Unpaid)
{
    int err=0;
    unsigned char buf[4];
    int len= sizeof(buf);

    CHECK_IS_OPEN();

    memset(buf, 0, sizeof(buf));

    err = m_ccTalk->RequestBinData(m_DeviceAdr, CCT_REQUEST_HOPPER_STATUS, len, buf);

    if(!err)
    {
        EventCounter= buf[0];
        Remaining=    buf[1];
        Paid=         buf[2];
        Unpaid=       buf[3];
    }

    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Modify variable data in device, functionality depends on device specification
/// \param VarSEt pointer to data
/// \param Len size in byte
/// \return error code, 0= no error

int ccTalkPayoutDevice::ModifyVariableSet(unsigned char * VarSet, int Len)
{
    int err= 0;

    CHECK_IS_OPEN();

    return err = m_ccTalk->SendData(m_DeviceAdr, CCT_MODIFY_VARIABLE_SET, Len, VarSet);
}


////////////////////////////////////////////////////////////////////////////////
/// Enable / disable the hopper device (164)
/// \param Enable true or false, default is true
/// \return error code, 0= no error

int ccTalkPayoutDevice::EnableHopper(bool Enable)
{
    int err= 0;
    unsigned char cmd;

    CHECK_IS_OPEN();

    cmd= Enable ? 165 : ~165;
    return err = m_ccTalk->SendData(m_DeviceAdr, CCT_ENABLE_HOPPER, 1, &cmd);
}

////////////////////////////////////////////////////////////////////////////////
/// Pumps the random number generator of the device with a set of random numbers (161)
/// \param Vars Variabel data
/// \param Len size of data in bytes
/// \return error code, 0= no error

int ccTalkPayoutDevice::PumpRng(unsigned char * Vars, int Len)
{
    CHECK_IS_OPEN();

    return m_ccTalk->SendData(m_DeviceAdr, CCT_PUMP_RNG, Len, &Vars);
}


////////////////////////////////////////////////////////////////////////////////
/// Returns cipher key from device. Returned value depends on used device
/// specification and type (160)
/// \param Remaining reference to returned value of paid coins
/// \param Remaining reference to returned value of unpaid coins
/// \return error code, 0= no error

int ccTalkPayoutDevice::RequestCipherKey(unsigned char *Key, int &Len)
{
    CHECK_IS_OPEN();

    return m_ccTalk->RequestBinData(m_DeviceAdr, CCT_REQUEST_CIPHER_KEY, Len, Key);
}


////////////////////////////////////////////////////////////////////////////////
/// Returns hopper pollin value, the interpretation of the returned values
/// depends on the specification of the conneced hopper type. (133)
/// \param EventCounter reference to returned event counter, incremented one every
/// received valid dispense command. range 1...255, 0= power on
/// \param Remaining reference to returned value of remaining coins
/// \param Remaining reference to returned value of paid coins
/// \param Remaining reference to returned value of unpaid coins
/// \return error code, 0= no error

int ccTalkPayoutDevice::RequestHopperPollingValue(int &EventCounter, int &Remaining, int &Paid, int &Unpaid)
{
    int err=0;
    unsigned char buf[7];
    int len= sizeof(buf);

    CHECK_IS_OPEN();

    memset(buf, 0, sizeof(buf));

    err = m_ccTalk->RequestBinData(m_DeviceAdr, CCT_REQUEST_HOPPER_POLLING_VALUE, len, buf);

    if(!err)
    {
        EventCounter= buf[0];
        Remaining=    ((int)buf[2] << 8) + buf[1];
        Paid=         ((int)buf[4] << 8) + buf[3];
        Unpaid=       ((int)buf[6] << 8) + buf[5];
    }

    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Imediatly halts the payout and returns remaining coins (132)
/// \param Remaining reference to returned value of remaining coins
/// \return error code, 0= no error

int ccTalkPayoutDevice::EmergencyStopValue(int &Remaining)
{
    int len= 2;
    CHECK_IS_OPEN();

    Remaining= 0;

    return m_ccTalk->RequestBinData(m_DeviceAdr, CCT_EMERGENCY_STOP_VALUE, len, &Remaining);
}


////////////////////////////////////////////////////////////////////////////////
/// Returns hopper coin Id as ASCII value and coin value as int (131)
/// \param Id point to an char array[7]
/// \param Value reference to returned coin value
/// \return error code, 0= no error

int ccTalkPayoutDevice::RequestHopperCoinValue(char *Id, int &Value)
{
    int err=0;
    unsigned char buf[7];
    int len= sizeof(buf);

    CHECK_IS_OPEN();

    memset(buf, 0, sizeof(buf));

    err = m_ccTalk->RequestBinData(m_DeviceAdr, CCT_REQUEST_HOPPER_COIN_VALUE, len, buf);

    if(!err)
    {
        memcpy(Id, &buf[0], 6);
        Id[6]= 0;
        Value=   ((int)buf[7] << 8) + buf[6];
    }

  return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Returns the dispensed indexed coin counter
/// \param Coin zero based index of the coin
/// \param Count referenced to returned value, depend on hopper type
/// \return error code, 0= no error

int ccTalkPayoutDevice::RequestIndexedHopperDispenseCount(int Coin,  unsigned int *Count)
{
    int err= 0;
    int retlen= 3;

    CHECK_IS_OPEN();

    Count= 0;

    return err = m_ccTalk->RequestBinDataEx(m_DeviceAdr, CCT_REQUEST_INDEXED_HOPPER_DISPENSE, &Coin, 1, Count, retlen);
}


////////////////////////////////////////////////////////////////////////////////
// enhanced functions

////////////////////////////////////////////////////////////////////////////////
/// Sets encyption mode
/// \param Encryption new encryption mode, use ENCRYPT_UNDEF to leave unchanged
/// \return actual encryption mode

int ccTalkPayoutDevice::SetEncryptionMode(enum ENCRYPTION_MODE Encryption)
{
    if(m_Encrytion != ENCRYPT_UNDEF)
        m_Encrytion= Encryption;

    return m_Encrytion;
}

////////////////////////////////////////////////////////////////////////////////
/// Override CccTalkDevice::Open() function with exended pamameters
/// \param Adr device ccTalk address
/// \param ccTalk pointer to the communication class CccTalk
/// \param Encryption encryption mode, default is unencrypted
/// \return error code, 0= no error

int ccTalkPayoutDevice::Open(int Adr, CccTalk *ccTalk, enum ENCRYPTION_MODE Encryption)
{
    if(m_Encrytion != ENCRYPT_UNDEF)
        m_Encrytion= Encryption;

    return CccTalkDevice::Open(Adr, ccTalk);
}

////////////////////////////////////////////////////////////////////////////////
/// Send dispense counter in the given encption mode to the device
/// \param Count number oft coin to dispense
/// \param Encryption new encryption mode, use ENCRYPT_UNDEF to leave unchanged
/// \return error code, 0= no error

int ccTalkPayoutDevice::Dispense(int Count, enum ENCRYPTION_MODE Encryption)
{
    int err= 0;
    int len;
    union {
        unsigned char byte[10];
        unsigned long sn;
    } buf;

    unsigned int event;

    if(m_Encrytion != ENCRYPT_UNDEF)
        m_Encrytion= Encryption;

    switch(m_Encrytion)
    {
        default:
        case ENCRYPT_UNDEF:
        case ENCRYPT_NONE:
            memset(&buf, 0, sizeof(buf));
            err= PumpRng(buf.byte, 8);

            if(!err)
            {
                err= RequestCipherKey(buf.byte, len);    // dummy request of cypher key
                if(!err)
                {
                    memset(&buf, 0, 8);              // clear key
                    buf.byte[8]= Count & 0xff;           // set count byte
                    err= DispenseHopperCoins(buf.byte, 8, event);
                }
            }
            break;

        case ENCRYPT_SERIAL:
            err= RequestSerialNumber(buf.sn);         // use 3 byte serial number

            if(! err)
            {
                buf.byte[3]= Count & 0xff;               // set count byte
                err= DispenseHopperCoins(buf.byte, 4, event);
            }
            break;
    }
    return err;
}

// End ccTalkPayoutDevice.cpp
////////////////////////////////////////////////////////////////////////////////
