////////////////////////////////////////////////////////////////////////////////
// File:   MdbDispenserDevice.h
// Author: manfred
// Copyright 2010:
//              wh Münzprüfer Berlin GmbH
//              Teltower Damm 276
//              D-10707 Berlin
//              Germany
//              info@whberlin.de
//
// Created: 5. November 2014 10:41
//
////////////////////////////////////////////////////////////////////////////////

#ifndef _MDBDISPENSERDEVICE_H
#define	_MDBDISPENSERDEVICE_H

////////////////////////////////////////////////////////////////////////////////
// Includes
//
#include "MdbDevice.h"

////////////////////////////////////////////////////////////////////////////////
// Coin Hopper or Tube - Dispenser
//
#define MDB_CMD_DP_RESET		0
#define MDB_CMD_DP_SETUP		1
#define MDB_CMD_DP_DISPENSER_STATUS     2
#define MDB_CMD_DP_POLL			3
#define MDB_CMD_DP_DISPENSE_ENABLE	4
#define MDB_CMD_DP_DISPENSE		5
#define MDB_CMD_DP_PAYOUT		6
#define MDB_CMD_DP_EXPANSION    	7


#define MDB_CMD_DP_COINS                16

////////////////////////////////////////
// Typdefs
//

// Setup
typedef struct MDB_CMD_DP_SETUP_RESPONSE_DATA {
    unsigned char FeatureLevel;
    unsigned char CountryCode[2];			// 2 Byte BCD
    unsigned char ScalingFactor;
    unsigned char DecimalPlace;
    unsigned char MaxResponseTime;
    unsigned short Disabled;
    unsigned short SelfFilling;
    unsigned char CoinTypeCredit[MDB_CMD_DP_COINS];
} MdbDispenserSetupResponseData;

// Dispenser Status
typedef struct MDB_CMD_DP_DISPENSER_STATUS_RESPONSE_DATA {
    unsigned char FullStatus[MDB_CMD_DP_COINS];
    unsigned short CoinCount[MDB_CMD_DP_COINS];
} MdbDispenserStatusResponseData;


// Poll
typedef struct MDB_CMD_DP_POLL_RESPONSE_DATA {
    unsigned char Status;				//
    unsigned short DispensedCoins;
    unsigned short CoinsInside;     			// 2 Byte BCD
} MdbDispenserPollResponseData;

// ManualDispenseEnable
typedef struct MDB_CMD_DP_DISPENSE_ENABLE_RESPONSE_DATA {
    unsigned short Enable;				//
} MdbDispenserEnableSendData;

// Dispense Coins
typedef struct MDB_CMD_DP_DISPENSE_ENABLE_RESPONSE_DATA {
    unsigned short Enable;				//
} MdbDispenserEnableSendData;



////////////////////////////////////////////////////////////////////////////////
// classPrototypes

class MdbDispenserDevice  : public MdbDevice {
public:
    MdbDispenserDevice();
    virtual ~MdbDispenserDevice();

    int Reset(void);
    int Setup(void);


private:
    MdbDispenserSetupResponseData m_SetupResponse;

};

#endif	/* _MDBDispenserDEVICE_H */

// End MdbDispenserDevice.h
////////////////////////////////////////////////////////////////////////////////

