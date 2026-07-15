////////////////////////////////////////////////////////////////////////////////
// File:   MdbDevice.h
// Author: manfred
// Copyright 2010:
//              wh Münzprüfer Berlin GmbH
//              Teltower Damm 276
//              D-10707 Berlin
//              Germany
//              info@whberlin.de
//
// Created: 21. März 2011 22:52
//
////////////////////////////////////////////////////////////////////////////////

/// Base class for MDB devices at a CCT900 device

#ifndef _MDBDEVICE_H
#define	_MDBDEVICE_H

#include "ccTalkCCT900Device.h"

////////////////////////////////////////////////////////////////////////////////
//

#define MDB_TX_BUFFER_LEN	40
#define MDB_RX_BUFFER_LEN	40

#define MDB_DEVICE_DEF_TIMEOUT  200

////////////////////////////////////////////////////////////////////////////////
// Globale MDB Definitions
//
// Transportlayer
#define MDB_ACK			0x00
#define MDB_NAK			0xff
#define MDB_RET			0xaa

#define MDB_ADR_MASK		0xf8
#define MDB_ADR_POS		3
#define MDB_ADR(a)		(((a)<<MDB_ADR_POS)&MDB_ADR_MASK)

// devices adresses
#define MDB_ADR_VMC		0
#define MDB_ADR_CHANGER		1
#define MDB_ADR_CHASHLESS_1	2
#define MDB_ADR_GATEWAY		3
#define MDB_ADR_DISPLAY		4
#define MDB_ADR_EMS		5
#define MDB_ADR_BILL		6
#define MDB_ADR_RES_7		7
#define MDB_ADR_USD_1		8
#define MDB_ADR_USD_2		9
#define MDB_ADR_USD_3		10
#define MDB_ADR_HOPPER		11
#define MDB_ADR_CHASHLESS_2	12
#define MDB_ADR_RES_13		13
// reserved 13...27
#define MDB_ADR_RES_27		27
#define MDB_ADR_EXP_1		28
#define MDB_ADR_EXP_2		29
#define MDB_ADR_VM_1		30
#define MDB_ADR_VM_2		31

#define MDB_BUFFER_LEN		1+36+1
// #define MDB_RX_TIMEOUT		5


////////////////////////////////////////////////////////////////////////////////
// Standard Command Types
//
#define MDB_CMD_MASK			0x07
#define MDB_CMD(a)			((a) & MDB_CMD_MASK)
// Globals
#define MDB_CMD_RESET			0
#define MDB_CMD_SETUP			1
#define MDB_CMD_STATUS			2
#define MDB_CMD_POLL			3
#define MDB_CMD_TYPE			4
#define MDB_CMD_5			5
#define MDB_CMD_6			6
#define MDB_CMD_EXPANSION		7

//////////////////////////////////////////////////////////////////////////
// Coin Acceptor
#define MDB_CMD_CA_RESET		0
#define MDB_CMD_CA_SETUP		1
#define MDB_CMD_CA_STATUS		2
#define MDB_CMD_CA_POLL			3
#define MDB_CMD_CA_COIN_TYPE            4
#define MDB_CMD_CA_DISPENSE		5
#define MDB_CMD_CA_6			6
#define MDB_CMD_CA_EXPANSION            7

//////////////////////////////////////////////////////////////////////////
// Bill Validator
#define MDB_CMD_BV_RESET		0
#define MDB_CMD_BV_SETUP		1
#define MDB_CMD_BV_SECURITY		2
#define MDB_CMD_BV_POLL			3
#define MDB_CMD_BV_BILL_TYPE            4
#define MDB_CMD_BV_ESCROW           	5
#define MDB_CMD_BV_STACKER		6
#define MDB_CMD_BV_EXPANSION            7
#define MDB_CMD_BV_BILL_TYPES           16

typedef struct MDB_CMD_BV_SETUP_DATA {
	char Level;
	unsigned char CountryCode[2];				// 2 Byte BCD
	unsigned short ScalingFactor;
	char DecimalPlace;
	unsigned short StackerCapacity;
	unsigned short BillSecurityLevels;
	char Escrow;
	char BillTypeCredit[MDB_CMD_BV_BILL_TYPES];
} MdbBillValidatorSetupData;

#define MDB_CMD_BV_POLL_DATA_LEN		16
#define MDB_CMD_BV_POLL_BILL_ID			0x80
#define MDB_CMD_BV_POLL_BILL_EVENT_MASK		0xf0
#define MDB_CMD_BV_POLL_BILL_VALUE_MASK		0x0f
#define MDB_CMD_BV_POLL_ERROR_MASK		0x0f

#define MDB_CMD_BV_POLL_BILL_STACKED		0x80
#define MDB_CMD_BV_POLL_ESCROW_POSITION		0x90
#define MDB_CMD_BV_POLL_BILL_RETURNED		0xa0
#define MDB_CMD_BV_POLL_NOT_USED		0xb0
#define MDB_CMD_BV_POLL_DISABLED_BILL		0xc0

#define MDB_CMD_BV_POLL_ERR_MOTOR		0x01
#define MDB_CMD_BV_POLL_ERR_SENSOR		0x02
#define MDB_CMD_BV_POLL_ERR_BUSY		0x03
#define MDB_CMD_BV_POLL_ERR_ROM			0x04
#define MDB_CMD_BV_POLL_ERR_JAM			0x05
#define MDB_CMD_BV_POLL_ERR_RESET		0x06
#define MDB_CMD_BV_POLL_ERR_REMOVED		0x07
#define MDB_CMD_BV_POLL_ERR_CASH_BOX		0x08
#define MDB_CMD_BV_POLL_ERR_DISABLED		0x09
#define MDB_CMD_BV_POLL_ERR_REJECTED		0x0b
#define MDB_CMD_BV_POLL_ERR_CREDIT_REMOVAL	0x0c
#define MDB_CMD_BV_POLL_ERR_ATTEMPTS		0x40

typedef struct MDB_CMD_BV_EXPANSION_SUB0_DATA {
	char Manufacturer[3];						// ASCII
	char SerialNumber[12];						// ASCII
	char ModelNumber[12];						// ASCII
	unsigned char SoftwareVersion[2];			// 2 Byte BCD
} MdbBillValidatorExpensionSub0Data;

////////////////////////////////////////////////////////////////////////////////
// MDB error codes
//

enum MDB_ERROR {
// 	MDB_OK= 0,
	MDB_OK= -1,

	MDB_RX_ERROR= 50,
	MDB_RX_TIMEOUT_ERR,
	MDB_RX_NAK_ERR,
	MDB_RX_CSUM_ERR,
	MDB_RX_FRAME_ERR,
	MDB_RX_OV_ERR,
	MDB_RX_FRAME_TOUT_ERR,

	MDB_TX_ERROR= 100,
	MDB_TX_OPEN_ERR,			// Port/Device not opened
	MDB_TX_FRAME_ERR,			// Frame or length
	MDB_TX_CMD_ERR				// unknown commmand
};

/// Base class for MDB bus communication via a CCT 900 or CCT 910.
////////////////////////////////////////////////////////////////////////////////
/// This class includes base functions for data transfer and data encapsulating.
///
////////////////////////////////////////////////////////////////////////////////

class MdbDevice {
public:
    MdbDevice();
    virtual ~MdbDevice();
    MdbDevice(int Adr, ccTalkCCT900Device *Cct900, int Timeout);

    /// @name Base Functions
    //@{
    int Open(int Adr, ccTalkCCT900Device *ccTalk, int Timeout= MDB_DEVICE_DEF_TIMEOUT);
    bool IsOpen();
    void Close();
    int GetDeviceAddress(void);
    //@}

    /// @name base communication CCT900 functions
    //@{
    int Communication(void * SendData, int SendLength,
                      void * ReceiveData, int &ReceiveLength,
                      int &TransferStatus);
    int SendBreak(void);
    int SendCommand(char Cmd, 
                     unsigned char* TxData, 
                     int TxLen,
                     unsigned char *RxData, 
                     int &RxLen);
    unsigned char * MdbRxData(void);
    //@}

    /// @name virtual functions
    //@{
        virtual int Reset(void);
    //@}

private:
    int m_DeviceAdr;
    ccTalkCCT900Device *m_CCT900device;
    int m_Timeout;
    unsigned char m_MdbRxBuffer[MDB_RX_BUFFER_LEN];
    unsigned char m_MdbTxBuffer[MDB_TX_BUFFER_LEN];
};

#endif	/* _MDBDEVICE_H */

// End MdbDevice.h
////////////////////////////////////////////////////////////////////////////////

