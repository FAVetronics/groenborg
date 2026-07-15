////////////////////////////////////////////////////////////////////////////////
// File:   ccTalkHzwDevice.cpp
// Author: Manfred Wollny
// Copyright 2010:
//              wh Münzprüfer Berlin GmbH
//              Teltowerd Damm 276
//              D-10707 Berlin
//              Germany
//
// Created on 23. Juni 2010, 10:46
//
/// \file Source file class ccTalkHzwDevice for HZW 100
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
// Includes

#include "ccTalkHzwDevice.h"

////////////////////////////////////////////////////////////////////////////////
// Macros

#define CHECK_IS_OPEN()    {if(!IsOpen()) return SetError(SIO_OPEN_ERR);}
#define CHECK_COIN_INDEX(a,b)  {if((a)<0 || (a)>=EMP_CCT_MAX_COINS) return (b);}
#define CHECK_COIN_INDEX_NR(a) {if((a)<0 || (a)>=EMP_CCT_MAX_COINS) return ;}

////////////////////////////////////////////////////////////////////////////////
// Constructor / Destructor

ccTalkHzwDevice::ccTalkHzwDevice()
{
}

ccTalkHzwDevice::~ccTalkHzwDevice()
{
}

////////////////////////////////////////////////////////////////////////////////
/// Tara command.
/// \param none
/// \return error code, 0= no error

int ccTalkHzwDevice::Tara(void)
{
    CHECK_IS_OPEN();

    return m_ccTalk->SendCommand(m_DeviceAdr, HZW_CCT_TARA);
}

#define  HZW_LSB_FIRST FALSE

#define HZW_REUEST_COIN_COUNT_LEN 3
#define HZW_REUEST_COIN_COUNT_ID  0
#define HZW_REUEST_COIN_COUNT_POS 1

////////////////////////////////////////////////////////////////////////////////
/// Request counter value.
/// \param Count reference to count value
/// \param Coin selected coin, 0= use default of DIP switches
/// \param Mode 0= coin count, 1= ADC Value, 2= coin count, 3= weight in g
/// \return error code, 0= no error

int ccTalkHzwDevice::RequestCointCount(unsigned int &Count, unsigned int &Coin, enum HZW_REQUEST_COUNT Mode)
{
    int err= 0;

    int buflen = HZW_REUEST_COIN_COUNT_LEN;         
    unsigned char buf[HZW_REUEST_COIN_COUNT_LEN];   
    unsigned char cmd[2];                           

    CHECK_IS_OPEN();

    if(Coin > CCT_MAX_VALUES)
        Coin = 0;

    cmd[0] = Coin;
    cmd[1] = Mode;

    err = m_ccTalk->RequestBinDataEx(m_DeviceAdr, HZW_CCT_REQUEST_COIN_COUNT, cmd, 2, buf, buflen);

    if (!err && buflen >= HZW_REUEST_COIN_COUNT_LEN)
    {
        Coin= buf[HZW_REUEST_COIN_COUNT_ID];
#if HZW_LSB_FIRST
        Count= buf[HZW_REUEST_COIN_COUNT_POS];
        Count+= buf[HZW_REUEST_COIN_COUNT_POS+1] << 8;
#else
        Count= buf[HZW_REUEST_COIN_COUNT_POS+1];
        Count+= buf[HZW_REUEST_COIN_COUNT_POS] << 8;
#endif
    }    
    return err;
}


