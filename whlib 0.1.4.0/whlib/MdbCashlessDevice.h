////////////////////////////////////////////////////////////////////////////////
// File:   MdbCashlessDevice.h
// Author: manfred
// Copyright 2010:
//              wh Münzprüfer Berlin GmbH
//              Teltower Damm 276
//              D-10707 Berlin
//              Germany
//              info@whberlin.de
//
// Created: 11. November 2013 10:03
//
////////////////////////////////////////////////////////////////////////////////


#ifndef _MDBCASHLESSDEVICE_H
#define	_MDBCASHLESSDEVICE_H

#include "MdbDevice.h"
#include "tools.h"
#include "nomination.h"

// recomded using 1 Byte packing
#pragma pack(1)

////////////////////////////////////////////////////////////////////////////////
// Card Reader States
//
enum MDB_CL_STATE {
	MDB_CL_STATE_INACTIVE,              // No Card Reader
	MDB_CL_STATE_DISABLED,              // Card Reader disabled
	MDB_CL_STATE_ENABLED,               // Card Reader enabled
	MDB_CL_STATE_SESSION_IDLE,          // Karte/Kredit available
	MDB_CL_STATE_VENDING,               // vending activ
	MDB_CL_STATE_VENDING_END,           // vend end
	MDB_CL_STATE_VENDING_CANCELED,      // canceled
	MDB_CL_STATE_REVALUE,               // revalue activ
	MDB_CL_STATE_REVALUE_OK,            // revalue ok
	MDB_CL_STATE_REVALUE_CANCELED,      // revalue canceled

	MDB_CL_STATE_ERROR= 100,
	MDB_CL_STATE_ERR_VENDING,           // error on vending
	MDB_CL_STATE_ERR_NO_CREDIT,         // nor credit
	MDB_CL_STATE_ERR_WRONG_CARD,        // bad card
	MDB_CL_STATE_ERR_REVALUE,           // error on revalue
};


////////////////////////////////////////////////////////////////////////////////
// Cashless - Card Reader
//
#define MDB_CMD_CL_RESET		0
#define MDB_CMD_CL_SETUP		1
#define MDB_CMD_CL_POLL			2
#define MDB_CMD_CL_VEND			3
#define MDB_CMD_CL_READER		4
#define MDB_CMD_CL_REVALUE		5
#define MDB_CMD_CL_6			6
#define MDB_CMD_CL_EXPANSION    	7

////////////////////////////////////////
// MDB_CMD_CL_SETUP
//
#define MDB_CMD_CL_SETUP_CONFIG_DATA			0x00
#define MDB_CMD_CL_SETUP_MIN_MAX_PRICES			0x01
#define MDB_CMD_CL_SETUP_DEFAULT_MIN_PRICE		0x0000
#define MDB_CMD_CL_SETUP_DEFAULT_MAX_PRICE		0xffff
#define MDB_CMD_CL_SETUP_CRRENCY_LEN                    2


typedef struct MDB_CMD_CL_SETUP_CONFIG_SEND_DATA {
    unsigned char Header;						// muss 0x00 sein
    unsigned char FeatureLevel;					// 0x01
    unsigned char DisplayCols;					// 0x00
    unsigned char DisplayRows;					// 0x00
    unsigned char DisplayInfo;					// 0x00
} MdbCashLessSetupConfigSendData;

typedef struct MDB_CMD_CL_SETUP_CONFIG_RESPONSE_DATA {
    unsigned char Header;						// muss 0x01 sein
    unsigned char FeatureLevel;
    unsigned char CountryCode[2];				// 2 Byte BCD
    unsigned char ScalingFactor;
    unsigned char DecimalPlace;
    unsigned char MaxResponseTime;
    unsigned char MiscOptions;
} MdbCashLessSetupConfigResponseData;

typedef struct MDB_CMD_CL_SETUP_MMPRICE_SEND_DATA {
    unsigned char Header;						// muss 0x01 sein
    unsigned short MaxPrice;						// default 0xffff
    unsigned short MinPrice;						// default 0x0000
} MdbCashLessSetupMMPriceSendData;

typedef struct MDB_CMD_CL_SETUP_MMPRICE_SEND_DATA_LEVEL3 {
    unsigned char Header;						// muss 0x01 sein
    unsigned short MaxPrice;						// default 0xffff
    unsigned short MinPrice;						// default 0x0000
    unsigned Curreny[MDB_CMD_CL_SETUP_CRRENCY_LEN];
} MdbCashLessSetupMMPriceSendDataLevel3;


////////////////////////////////////////
// MDB_CMD_CL_POLL
//

#define MDB_CMD_CL_POLL_DATA_LEN			100

#define MDB_CMD_CL_POLL_RESET				0x00
#define MDB_CMD_CL_POLL_READER_CONFIG_DATA		0x01
#define MDB_CMD_CL_POLL_DISPLAY_REQUEST			0x02
#define MDB_CMD_CL_POLL_BEGIN_SESSION			0x03
#define MDB_CMD_CL_POLL_SESSION_CANCEL_REQUEST          0x04
#define MDB_CMD_CL_POLL_VEND_APPROVED			0x05
#define MDB_CMD_CL_POLL_VEND_DENIED			0x06
#define MDB_CMD_CL_POLL_END_SESSION			0x07
#define MDB_CMD_CL_POLL_CANCELLED			0x08
#define MDB_CMD_CL_POLL_PERIPHERAL_ID			0x09
#define MDB_CMD_CL_POLL_MALFUNCTION_ERROR		0x0A
#define MDB_CMD_CL_POLL_CMD_OUT_OF_SEQUENCE		0x0B
//									resverved	0x0C
#define MDB_CMD_CL_POLL_REVALUE_APPROVED		0x0D
#define MDB_CMD_CL_POLL_REVALUE_DENIED			0x0E
#define MDB_CMD_CL_POLL_REVALUE_LIMIT_AMOUNT            0x0F
#define MDB_CMD_CL_POLL_USER_FILE_DATA			0x10
#define MDB_CMD_CL_POLL_TIME_DATE_REQUEST		0x11
#define MDB_CMD_CL_POLL_DATA_ENTRY_REQUEST		0x12
#define MDB_CMD_CL_POLL_DATA_ENTRY_CANCEL		0x13
//									resverved	0x14 - 0x1A
#define MDB_CMD_CL_POLL_FTL_REQ_TO_RCV			0x1B
#define MDB_CMD_CL_POLL_FTL_RETRY_DENY			0x1C
#define MDB_CMD_CL_POLL_FTL_SNED_BLOCK			0x1D
#define MDB_CMD_CL_POLL_FTL_OK_TO_SEND                  0x1E
#define MDB_CMD_CL_POLL_FTL_REQ_TO_SEND			0x1F
//									resverved	0x20 - 0xFE
#define MDB_CMD_CL_POLL_DIAGNOSTIC_RESPONSE		0xFF

////////////////////////////////////////
// MDB_CMD_CL_VEND
//
// Vend Request
#define MDB_CMD_CL_VEND_REQUEST				0x00
#define MDB_CMD_CL_VEND_REQUEST_APPROVED		0x05
#define MDB_CMD_CL_VEND_REQUEST_DENIED			0x06
// Vend Cancel
#define MDB_CMD_CL_VEND_CANCEL				0x01
#define MDB_CMD_CL_VEND_CANCEL_DENIED			0x06

#define MDB_CMD_CL_VEND_SUCCESS				0x02
#define MDB_CMD_CL_VEND_FAILURE				0x03
// Vend Session Complete
#define MDB_CMD_CL_VEND_SESSION_COMPLETE		0x04
#define MDB_CMD_CL_VEND_SESSION_COMPLETE_END            0x07


#define MDB_CMD_CL_VEND_REQUEST_DEFAULT_PRICE	0xffff


typedef struct MDB_CMD_CL_VEND_REQUEST_SEND_DATA {
    unsigned char Header;
    unsigned short Price;
    unsigned short Product;
} MdbCashLessVendRequestSendData;

typedef struct MDB_CMD_CL_VEND_REQUEST_RESPONSE_DATA {
    unsigned char Header;
    unsigned short Price;
} MdbCashLessVendRequestResponseData;

typedef struct MDB_CMD_CL_VEND_SUCCESS_DATA {
    unsigned char Header;
    unsigned short Product;
} MdbCashLessVendSessionSuccessData;


////////////////////////////////////////
// MDB_CMD_CL_READER
//
#define MDB_CMD_CL_READER_DISABLE			0x00
#define MDB_CMD_CL_READER_ENABLE			0x01
#define MDB_CMD_CL_READER_CANCEL			0x02
#define MDB_CMD_CL_READER_LEN				1


////////////////////////////////////////
// MDB_CMD_CL_REVALUE
//

#define MDB_CMD_CL_REVALUE_REQUEST			0x00
#define MDB_CMD_CL_REVALUE_LIMIT_REQUEST		0x01

#define MDB_CMD_CL_REVALUE_REQUEST_APPROVED             0x0d
#define MDB_CMD_CL_REVALUE_REQUEST_DENIED               0x0e


typedef struct MDB_CMD_CL_REVALUE_REQUEST_SEND_DATA {
    unsigned char Header;
    unsigned short Value;
} MdbCashLessRevalueRequestSendData;


typedef struct MDB_CMD_CL_REVALUE_LIMIT_REQUEST_RESPONSE_DATA {
    unsigned char Header;
    unsigned short Amount;
} MdbCashLessRevalueLimitRequestResponseData;



////////////////////////////////////////
// MDB_CMD_CL_EXPANSION

#define MDB_CMD_CL_EXPANSION_REQUEST_ID                         00

#define MDB_CMD_CL_EXPANSION_REQUEST_ID_MANUFACTURER_ID_LEN	3
#define MDB_CMD_CL_EXPANSION_REQUEST_ID_SERIAL_NUMBER_LEN	12
#define MDB_CMD_CL_EXPANSION_REQUEST_ID_MODEL_NUMBER_LEN	12
#define MDB_CMD_CL_EXPANSION_REQUEST_ID_SOFTWARE_VERSION_LEN	2
#define MDB_CMD_CL_EXPANSION_REQUEST_ID_FEATURE_BITS_LEN	4


typedef struct MDB_CMD_CL_EXPANSION_REQUEST_ID_DATA {
    unsigned char Header;                                                                   
    unsigned char ManufacturerId[MDB_CMD_CL_EXPANSION_REQUEST_ID_MANUFACTURER_ID_LEN];      
    unsigned char SerialNumber[MDB_CMD_CL_EXPANSION_REQUEST_ID_SERIAL_NUMBER_LEN];
    unsigned char ModelNumber[MDB_CMD_CL_EXPANSION_REQUEST_ID_MODEL_NUMBER_LEN];
    unsigned char SoftwareVersion[MDB_CMD_CL_EXPANSION_REQUEST_ID_SOFTWARE_VERSION_LEN];
} MdbCashLessExpansionRequestIdData;

#define MDB_CMD_CL_EXPANSION_REQUEST_ID_RESPONSE	09

typedef struct MDB_CMD_CL_EXPANSION_REQUEST_ID_RESPONSE_DATA {
    unsigned char Header;						// muss 0x09 sein
    unsigned char ManufacturerId[MDB_CMD_CL_EXPANSION_REQUEST_ID_MANUFACTURER_ID_LEN];
    unsigned char SerialNumber[MDB_CMD_CL_EXPANSION_REQUEST_ID_SERIAL_NUMBER_LEN];
    unsigned char ModelNumber[MDB_CMD_CL_EXPANSION_REQUEST_ID_MODEL_NUMBER_LEN];
    unsigned char SoftwareVersion[MDB_CMD_CL_EXPANSION_REQUEST_ID_SOFTWARE_VERSION_LEN];
} MdbCashLessExpansionRequestIdResponseData;

typedef struct MDB_CMD_CL_EXPANSION_REQUEST_ID_RESPONSE_DATA_LEVEL3 {
    unsigned char Header;						// muss 0x09 sein
    unsigned char ManufacturerId[MDB_CMD_CL_EXPANSION_REQUEST_ID_MANUFACTURER_ID_LEN];
    unsigned char SerialNumber[MDB_CMD_CL_EXPANSION_REQUEST_ID_SERIAL_NUMBER_LEN];
    unsigned char ModelNumber[MDB_CMD_CL_EXPANSION_REQUEST_ID_MODEL_NUMBER_LEN];
    unsigned char SoftwareVersion[MDB_CMD_CL_EXPANSION_REQUEST_ID_SOFTWARE_VERSION_LEN];
    unsigned char FeaturBits[MDB_CMD_CL_EXPANSION_REQUEST_ID_FEATURE_BITS_LEN];
} MdbCashLessExpansionRequestIdResponseDataLevel3;


/// class for MDB Cashless Devices.
////////////////////////////////////////////////////////////////////////////////
/// This class includes functions for managing cashless payment via a card
/// reader device.
///

////////////////////////////////////////////////////////////////////////////////

class MdbCashlessDevice : public MdbDevice {

public:
    MdbCashlessDevice();
    virtual ~MdbCashlessDevice();

public:	

/// @name Overwritten
    int Reset(void);
//@}


/// @name Setup - Commands
//@{
    int SetupConfigData(int VmcLevel= 1, int Cols= 0, int Rows= 0, int Disp= 0);
    int SetupPrice(int Min= MDB_CMD_CL_SETUP_DEFAULT_MIN_PRICE, int Max= MDB_CMD_CL_SETUP_DEFAULT_MAX_PRICE);
//@}
    
/// @name Poll - Command
//@{
    int Poll(bool AutoResponse= false);
//@}

/// @name Vend - Commands
//@{
    int VendRequest(unsigned short Price, unsigned short Product, int &Approved, int &Ammount);
    int VendCancel(void);
    int VendSuccess(unsigned short Number);
    int VendFailure(void);
    int VendSessionComplete(void);
//@}

/// @name Reader - Commands
//@{
    int ReaderDisable(void);
    int ReaderEnable(void);
    int ReaderCancel(void);
//@}

/// @name Revalue - Commands
//@{
    int RevalueRequest(unsigned short Value, int &Approved);
    int RevalueLimitRequest(int &Amount);
//@}

/// @name Expansion - Commands
//@{
    int RequestID(char * Manufacturer= NULL, char * Serial= NULL, char * Model= NULL, char * Version= NULL);
    void GetPeripheralID(char * Manufacturer, char * Serial, char * Model, char * Version);
//@}

/// @name Example code for managing a chashless device
//@{
    void VendingControl(void);
    void SetState(char State);
    int GetState(void);
    int SetEnable(char Enable);
    int CheckDevice(void);
    int InitDevice(void);

    void GetVendingData(unsigned char *Status, unsigned short *Credit, unsigned char *Currency);
    void SetVendingValue(unsigned short Value);
    void SetRevalue(unsigned short Value);
    unsigned short GetFunds(void);
//@}

private:
    unsigned short m_Funds;
    unsigned short m_VendingValue;
    unsigned short m_RevalueValue;
    unsigned short m_RevalueValuePendig;
    char m_ErrorCounter;
    char m_State;

    Nomination m_Nomination[1];
    MdbCashLessExpansionRequestIdResponseDataLevel3 m_PeripheralID;
    MdbCashLessSetupConfigResponseData m_ConfigResponse;

    // for diagnostic only
    int m_PollRxLen;
};

#endif	/* _MDBCASHLESSDEVICE_H */

// End MdbCashlessDevice.h
////////////////////////////////////////////////////////////////////////////////

