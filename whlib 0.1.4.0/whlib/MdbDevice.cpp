/// \file Source file class MdbDevice

////////////////////////////////////////////////////////////////////////////////
// File:   MdbDevice.cpp
// Author: Manfred Wollny
// Copyright 2011:
//              wh Münzprüfer Berlin GmbH
//              Teltower Damm 276
//              D-14167 Berlin
//              Germany
//              info@whberlin.de
//
// Created: 21. März 2011 22:52
//
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
// Includes
#define	_MDBDEVICE_CPP_SRC

#include "stdlib.h"
#include "stdio.h"
#include "ccTalkCCT900Device.h"
#include "MdbDevice.h"



////////////////////////////////////////////////////////////////////////////////
// Prototypen
//
/*void MdbOpen(void);
void MdbBreak(void);

char MdbSend(char Adr, char Cmd, unsigned char * Data, char Len);
char MdbReceive(unsigned char *Data);
void MdbTimerIsr(void);
unsigned char * MdbRxData(void);
char MdbIsReady(void);
*/
////////////////////////////////////////////////////////////////////////////////


////////////////////////////////////////////////////////////////////////////////
// Constructor / Destructor

MdbDevice::MdbDevice()
{
    m_DeviceAdr= 0;
    m_CCT900device= NULL;
    m_Timeout= MDB_DEVICE_DEF_TIMEOUT;
}

MdbDevice::~MdbDevice()
{
}

//////////////////////////////////////////////////////////////////////
/// initialize cunstructor
/// \param see Open() function

MdbDevice::MdbDevice(int Adr, ccTalkCCT900Device *Cct900, int Timeout)
{
    m_DeviceAdr= Adr;
    m_CCT900device= Cct900;
    m_Timeout= Timeout;
}

//////////////////////////////////////////////////////////////////////
// Basics

//////////////////////////////////////////////////////////////////////
/// Open MDB Device via CCT900 Dongle.
/// \param Adr MDB device address
/// \param Cct900 Pointer to CCT900 ccTalk device class
/// \param Timeout default is 100ms
/// \return none

int MdbDevice::Open(int Adr, ccTalkCCT900Device *Cct900, int Timeout)
{
    m_DeviceAdr= Adr;
    m_CCT900device= Cct900;
    m_Timeout= Timeout;

    return 0;
}

//////////////////////////////////////////////////////////////////////
/// Test if communication port is initialized and is opened
/// \param none
/// \return true or false

bool MdbDevice::IsOpen()
{
    if(m_DeviceAdr > 0 && m_CCT900device != NULL && m_CCT900device->IsOpen())
    	return true;
    else
    	return false;
}

//////////////////////////////////////////////////////////////////////
/// Close device. The referenced CCT900 ccTalk device port may still be opened
/// \param none
/// \return none

void MdbDevice::Close()
{
    m_DeviceAdr= 0;
    m_CCT900device= NULL;
}

//////////////////////////////////////////////////////////////////////
/// Returns proteced device address
/// \param none
/// \return device address

int MdbDevice::GetDeviceAddress(void)
{
    return m_DeviceAdr;
}

//////////////////////////////////////////////////////////////////////
/// Tunnels MDB commands throught the CCT900 to a peripheral MDB device
/// The MDB daten transfer is fully transparent encapsulated in a cctTalk data frame.
/// \param SendData pointer to MDB send telegramm data
/// \param SendLength length of MDB send data
/// \param ReceiveData pointer to MDB receive telegramm buffer
/// \param ReceiveLength reference of maximum size of receive buffer. It returns
/// received data length. If the received data length is larger than the given
/// receive buffer size the received data length is returned. But the receive
/// buffer is filled up to the given receive buffer size only.
/// \param TransferStatus reference to transfer status
/// \return error code, 0= no error

int MdbDevice::Communication(void * SendData, int SendLength,
                      void * ReceiveData, int &ReceiveLength,
                      int &TransferStatus)
{
    return m_CCT900device->MDBCommunication(m_Timeout, SendData, SendLength, ReceiveData, ReceiveLength, TransferStatus);
}

//////////////////////////////////////////////////////////////////////
/// Sends a break frame at the peripheral MDB bus
/// \param none
/// \return error code, 0= no error

int MdbDevice::SendBreak(void)
{
    return m_CCT900device->MDBSendBreak();
};

//////////////////////////////////////////////////////////////////////
/// Sends a Reset Command to the MDB -Device
/// \param none
/// \return error code, 0= no error

int MdbDevice::Reset(void)
{
    int len= 0;

    return SendCommand(MDB_CMD_RESET, NULL, 0, NULL, len);
}

//////////////////////////////////////////////////////////////////////
/// Sends/receives Commands to/from the MDB -Device
/// \param Cmd  Commadn Code
/// \param TxData Pointer on send data buffer
/// \param TxLen Length of data
/// \param RxData Pointer on receive data buffer
/// \param Rxlen maximal size receive data
/// \return error code, 0= no error

int MdbDevice::SendCommand(char Cmd, unsigned char* TxData, int TxLen, unsigned char *RxData, int &RxLen)
{
    char err= 0;
    int i= 0;
    int len;            // temp receive length
    int status;         // transmit status

    // Clear buffer
    memset((void*)m_MdbRxBuffer, 0, sizeof(m_MdbRxBuffer));
    memset((void*)m_MdbTxBuffer, 0, sizeof(m_MdbTxBuffer));

    // fill send buffer
    m_MdbTxBuffer[0]= MDB_ADR(m_DeviceAdr) + MDB_CMD(Cmd);	// header Byte
    if(TxData != NULL)
        memcpy(&m_MdbTxBuffer[1], TxData, min((int)TxLen, (int)sizeof(m_MdbTxBuffer)));

    len= sizeof(m_MdbRxBuffer);         // maximum size
    // send/receive via CCT 900/910
    err= Communication(m_MdbTxBuffer, TxLen+1, m_MdbRxBuffer, len, status);

    if(!err)            // ccTalk tranmission OK?
    {
        if(status != CCT_CCT900_MDB_DATA_BLOCK_RECEIVED)  // any error?
        {
            if(status & CCT_CCT900_MDB_CHECK_SUM_ERROR)
                err= MDB_RX_CSUM_ERR;

            if(status &  CCT_CCT900_MDB_RECEIVE_TIMEOUT)
                err= MDB_RX_TIMEOUT_ERR;

            if(status &  CCT_CCT900_MDB_BREAK_ACTIVE)
                err= MDB_RX_FRAME_ERR;
        }
        else
        {
            RxLen= min(len, (int)RxLen);
            if(RxData != NULL && RxLen > 0)
                memcpy(RxData, m_MdbRxBuffer, RxLen);
        }
    }

    return err;
}

unsigned char * MdbDevice::MdbRxData(void)
{
    return m_MdbRxBuffer;
}

// End MdbDevice.cpp
////////////////////////////////////////////////////////////////////////////////
