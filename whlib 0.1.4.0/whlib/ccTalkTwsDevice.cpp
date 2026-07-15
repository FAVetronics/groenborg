/// \file Source file class ccTalkTwsDevice - still under developing

//////////////////////////////////////////////////////////////////////
// File:   MdbCashlessDevice.cpp
// Author: manfred
// Copyright 2013:
//              wh Münzprüfer Berlin GmbH
//              Teltower Damm 276
//              D-10707 Berlin
//              Germany
//              info@whberlin.de
////
//////////////////////////////////////////////////////////////////////

#include "ccTalkTwsDevice.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

#define CHECK_IS_OPEN()    {if(!IsOpen()) return SetError(SIO_OPEN_ERR);}
#define CHECK_COIN_INDEX(a,b)  {if((a)<0 || (a)>=EMP_CCT_MAX_COINS) return (b);}
#define CHECK_COIN_INDEX_NR(a) {if((a)<0 || (a)>=EMP_CCT_MAX_COINS) return ;}

//////////////////////////////////////////////////////////////////////
// construktor/destruktor
//////////////////////////////////////////////////////////////////////

CccTalkTwsDevice::CccTalkTwsDevice()
{

}

CccTalkTwsDevice::~CccTalkTwsDevice()
{

}

int CccTalkTwsDevice::Restart()
{
    return m_ccTalk->SendCommand(m_DeviceAdr, TWS_CCT_RESTART);
}


int CccTalkTwsDevice::InjectCoin(int Coin, int Mode)
{
    int err = 0;
    int buflen = 1;
    unsigned char buf;
    unsigned char cmd[2];
    Coin = min(15, Coin);

    CHECK_IS_OPEN();

    cmd[0] = (unsigned char) max(0, Coin);
    cmd[1] = Mode;
    buf = 255;

    err = m_ccTalk->RequestBinDataEx(m_DeviceAdr, TWS_CCT_INJECT_COIN, cmd, 2, &buf, buflen);
    if (err || buflen < 1)
        err = -1;
    else
        err = buf;

    return err;
}


int CccTalkTwsDevice::FlushCoins()
{
    CHECK_IS_OPEN();

    return m_ccTalk->SendCommand(m_DeviceAdr, TWS_CCT_FLUSH_COINS);
}

int CccTalkTwsDevice::EjectCoins(int Slot, bool EjectEmpty)
{
    int err = 0;
    unsigned char buf[2];

    CHECK_IS_OPEN();

    buf[0] = Slot;
    buf[1] = EjectEmpty;

    err = m_ccTalk->SendData(m_DeviceAdr, TWS_CCT_EJECT_COINS, 2, buf);

    return err;

}

int CccTalkTwsDevice::StopEject()
{
    CHECK_IS_OPEN();

    return m_ccTalk->SendCommand(m_DeviceAdr, TWS_CCT_STOP);
}

int CccTalkTwsDevice::ContinueEject()
{
    CHECK_IS_OPEN();

    return m_ccTalk->SendCommand(m_DeviceAdr, TWS_CCT_CONTINUE);
}

int CccTalkTwsDevice::AbortEject()
{
    CHECK_IS_OPEN();

    return m_ccTalk->SendCommand(m_DeviceAdr, TWS_CCT_ABORT);
}

int CccTalkTwsDevice::RequestCoinCount(int &Count)
{
    int err = 0;
    int buflen = 1;
    unsigned char buf= 0;

    CHECK_IS_OPEN();

    err = m_ccTalk->RequestBinData(m_DeviceAdr, TWS_CCT_REQUEST_COIN_COUNT, buflen, &buf);

    if (err || buflen < 1)
        err = -1;
    else
        Count = buf;

    return err;
}

#define CCT_TWS_MAG_COINS_FIRST		0
#define CCT_TWS_MAG_COINS_COUNT		1
#define CCT_TWS_MAG_COINS_DATA		2

#define CCT_TWS_MAG_COINS_BUFLEN        256


int CccTalkTwsDevice::RequestMagazineCoins(int &First, int &Count, unsigned char *Data, int DataBytes)
{
    int err = 0;
    int buflen = CCT_TWS_MAG_COINS_BUFLEN;
    unsigned char buf[CCT_TWS_MAG_COINS_BUFLEN];

    CHECK_IS_OPEN();

    err = m_ccTalk->RequestBinData(m_DeviceAdr, TWS_CCT_REQUEST_MAGAZIN_COINS, buflen, buf);

    if (err || buflen < 2)
        err = -1;
    else
    {
        memset(Data, 0, DataBytes); // Inhalt l�schen
        memcpy(&Data[buf[CCT_TWS_MAG_COINS_FIRST]*2], &buf[CCT_TWS_MAG_COINS_DATA], buf[CCT_TWS_MAG_COINS_COUNT]*2);
        First= buf[CCT_TWS_MAG_COINS_FIRST];
        Count= buf[CCT_TWS_MAG_COINS_COUNT];

        err = 0;
    }

    return err;
}

#define REQUEST_SORTER_BUFLEN 256

int CccTalkTwsDevice::RequestSorterEjects(int *Counter, bool ResetCounter)
{
    int err = 0;
    int buflen = REQUEST_SORTER_BUFLEN;
    unsigned char buf[REQUEST_SORTER_BUFLEN];
    unsigned char cmd;

    CHECK_IS_OPEN();

    cmd = ResetCounter;

    err = m_ccTalk->RequestBinDataEx(m_DeviceAdr, TWS_CCT_REQUEST_SORTER_EJECTS, &cmd, 1, buf, buflen);

    if (err || buflen < 1)
        err = -1;
    else
    {
        for (int i = 0; i < SLOT_ITEMS; i++)
            Counter[i] = buf[i];
    }

    return err;
}

#define MOD_MAG_COIN_LEN 3

int CccTalkTwsDevice::ModifyMagazineCoin(int Pos, int CoinId, int Mode)
{
    int err = 0;
    unsigned char cmd[MOD_MAG_COIN_LEN];

    cmd[0] = Pos;
    cmd[1] = CoinId;
    cmd[2] = Mode;

    CHECK_IS_OPEN();

    err = m_ccTalk->SendData(m_DeviceAdr, TWS_CCT_EJECT_COINS, MOD_MAG_COIN_LEN, cmd);

    return err;
}


#define MOD_GET_STAT_LEN 4

int CccTalkTwsDevice::GetStatus(int &Status, unsigned char &Flags, unsigned short &ErrFlags)
{
    int err = 0;
    int buflen = MOD_GET_STAT_LEN;
    unsigned char buf[MOD_GET_STAT_LEN];

    CHECK_IS_OPEN();

    memset(buf, 0, MOD_GET_STAT_LEN);
    err = m_ccTalk->RequestBinData(m_DeviceAdr, TWS_CCT_GET_STATUS, buflen, buf);
    Status = buf[0];

    Flags = buf[1];
    ErrFlags = buf[2] + (buf[3] << 8);

    return err;
}
