/// \file Source file class ccTalkCCT900Device
/// Device class for the CCT900 hub / dongle
////////////////////////////////////////////////////////////////////////////////
// File:        ccTalkCCT900Device.cpp
// Author:      Manfred wollny
// Copyright 2011:
//              wh Münzprüfer Berlin GmbH
//              Teltower Damm 276
//              D-14167 Berlin
//              Germany
//              info@whberlin.de
//
// Created: 17. März 2011 16:22
//
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
// Includes
#define	_CCTALKCCT900DEVICE_CPP_SRC

#include "ccTalkCCT900Device.h"

#define CHECK_IS_OPEN()    {if(!IsOpen()) return SetError(SIO_OPEN_ERR);}


////////////////////////////////////////////////////////////////////////////////
// Constructor / Destructor

ccTalkCCT900Device::ccTalkCCT900Device()
{
}

ccTalkCCT900Device::~ccTalkCCT900Device()
{
}

/* used cctalk dongle device header

#define CCT_CCT900_MODIFY_ESCROW_STATE          135
#define CCT_CCT900_START_MOTOR_REJECT           133
#define CCT_CCT900_MODIFY_ANTI_PIN_STATUS       132
#define CCT_CCT900_REQUEST_PERIPHERAL_STATUS    131
#define CCT_CCT900_CLEAR_UPTIME_COUNTER         124
#define CCT_CCT900_REQUEST_UPTIME_COUNTER       123
#define CCT_CCT900_MDB_COMMUNICATION            122
#define CCT_CCT900_MDB_SEND_BREAK               121

*/


//////////////////////////////////////////////////////////////////////
// Global Functions

//////////////////////////////////////////////////////////////////////
/// Modify state of escrow device
/// \param Byte 0: Command 0:close 1:into cash box 2:return coins
///  Byte 1: open time in 0,1 sec steps, default is 1,0 sec
/// \return error code, 0= no error

int ccTalkCCT900Device::ModifyEscrowState(int State, int Time)
{
    unsigned char buf[2];

    buf[0]= State;
    buf[1]= Time;
	
    return m_ccTalk->SendData(m_DeviceAdr, CCT_CCT900_MODIFY_ESCROW_STATE, 2, buf);
}

//////////////////////////////////////////////////////////////////////
/// Starts the EMR 100 motor reject
/// The motor reject starts for one turn and stops in idle position
/// \param none
/// \return error code, 0= no error

int ccTalkCCT900Device::StartMotorReject(void)
{
    return m_ccTalk->SendCommand(m_DeviceAdr, CCT_CCT900_START_MOTOR_REJECT);
}

//////////////////////////////////////////////////////////////////////
/// Modify state of anti pin device
/// \param Status 0:disabled, 1:automatic open, 2:open permanent
/// \return error code, 0= no error

int ccTalkCCT900Device::ModifyAntiPinStatus(int Status)
{
    unsigned char buf;

    buf= Status;

    return m_ccTalk->SendData(m_DeviceAdr, CCT_CCT900_MODIFY_ANTI_PIN_STATUS, 1, &buf);
}

#define CCT_CCT900_GET_STATUS_LEN 4

//////////////////////////////////////////////////////////////////////
/// Returns status of peripheral devices
/// \param Status reference to escrow status
/// \param EscrowSwitch reference to status of escrow switch
/// \param AntiPinCtrl reference to anti pin control value, refer to ModifyEscrowState()
/// \param AntiPinStatus reference to anti pin status
/// \return error code, 0= no error

int ccTalkCCT900Device::RequestPeripheralStatus(int &Escrow, bool &EscrowSwitch, int &AntiPinCtrl, int &AntiPinStatus)
{
    int err= 0;
    int buflen= CCT_CCT900_GET_STATUS_LEN;
    unsigned char buf[CCT_CCT900_GET_STATUS_LEN];

    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_CCT900_REQUEST_PERIPHERAL_STATUS, buflen, buf);
    Escrow= buf[0];
    EscrowSwitch= (buf[1] != 0) ? true : false;
    AntiPinCtrl= buf[2];
    AntiPinStatus= buf[3];

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Resets uptime counter
/// \param none
/// \return error code, 0= no error

int ccTalkCCT900Device::ClearUptimeCounter(void)
{
	return m_ccTalk->SendCommand(m_DeviceAdr, CCT_CCT900_CLEAR_UPTIME_COUNTER);
}


#define CCT_CCT900_UPTIME_LEN 4

//////////////////////////////////////////////////////////////////////
/// Resets uptime counter
/// \param Uptime reference to uptime variable
/// \return error code, 0= no error

int ccTalkCCT900Device::RequestUptimeCounter(long &Uptime)
{
    int err= 0;
    int buflen= CCT_CCT900_UPTIME_LEN;

    Uptime= 0;      // !! clear higher byte at long variable, depending on machine

    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_CCT900_REQUEST_UPTIME_COUNTER, buflen, &Uptime);

    return err;
}


#define CCT_CCT900_MDB_BUF_LEN 256

//////////////////////////////////////////////////////////////////////
/// Tunnels MDB commands throught the CCT900 to a peripheral MDB device
/// The MDB daten transfer is fully transparent encapsulated in a cctTalk data frame.
/// \param Timeout maximum response timeout in msec
/// \param SendData pointer to MDB send telegramm data
/// \param SendLength length of MDB send data
/// \param ReceiveData pointer to MDB receive telegramm buffer
/// \param ReceiveLength reference of maximum size of receive buffer. It returns
/// received data length. If the received data length is larger than the given
/// receive buffer size the received data length is returned. But the receive
/// buffer is filled up to the given receive buffer size only.
/// \param TransferStatus reference to transfer status
/// \return error code, 0= no error

int ccTalkCCT900Device::MDBCommunication(int Timeout, void * SendData,
                        int SendLength, void * ReceiveData, int &ReceiveLength,
                        int &TransferStatus)
{
    int err= 0;
    unsigned char txbuf[CCT_CCT900_MDB_BUF_LEN];
    unsigned char rxbuf[CCT_CCT900_MDB_BUF_LEN];
    int len;

///    len=  min(ReceiveLength, CCT_CCT900_MDB_BUF_LEN);
    len= CCT_CCT900_MDB_BUF_LEN;

    txbuf[0]= Timeout & 0xff;
    memcpy(&txbuf[1], SendData, SendLength);



    err= m_ccTalk->RequestBinDataEx(m_DeviceAdr, CCT_CCT900_MDB_COMMUNICATION, txbuf, SendLength+1, rxbuf, len);

    if(!err)
    {
        TransferStatus= rxbuf[0];
        ReceiveLength= len-1;
        memcpy(ReceiveData, &rxbuf[1], ReceiveLength);
    }
    else
    {
        ReceiveLength= 0;
    }

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Sends a break frame at the peripheral MDB bus
/// \param none
/// \return error code, 0= no error

int ccTalkCCT900Device::MDBSendBreak(void)
{
    return m_ccTalk->SendCommand(m_DeviceAdr, CCT_CCT900_MDB_SEND_BREAK);
}

// End ccTalkCCT900Device.cpp
////////////////////////////////////////////////////////////////////////////////
