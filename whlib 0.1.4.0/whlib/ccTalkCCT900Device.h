////////////////////////////////////////////////////////////////////////////////
// File:   ccTalkCCT900Device.h
// Author: manfred
// Copyright 2010:
//              wh Münzprüfer Berlin GmbH
//              Teltower Damm 276
//              D-10707 Berlin
//              Germany
//              info@whberlin.de
//
// Created: 17. März 2011 16:22
//
////////////////////////////////////////////////////////////////////////////////

#ifndef _CCTALKCCT900DEVICE_H
#define	_CCTALKCCT900DEVICE_H

//////////////////////////////////////////////////////////////////////

#include "ccTalkDevice.h"
#include "cct900globals.h"



//////////////////////////////////////////////////////////////////////

/// class for communication with a CCT 900 device .
////////////////////////////////////////////////////////////////////////////////
/// This class includes base functions for data transfer and data encapsulating
/// for transfer MDB data via the CCT 900.
////////////////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////

class ccTalkCCT900Device  : public CccTalkDevice
{
public:
    ccTalkCCT900Device();
    virtual ~ccTalkCCT900Device();

/// @name Peripherals command
//@{
    int ModifyEscrowState(int State, int Time= 10);
    int StartMotorReject(void);
    int ModifyAntiPinStatus(int Status);
    int RequestPeripheralStatus(int &Escrow, bool &EscrowSwitch, int &AntiPinCtrl, int &AntiPinStatus);
//@}

/// @name Internals
//@{
    int ClearUptimeCounter(void);
    int RequestUptimeCounter(long &Uptime);
//@}

/// @name Communication commands
//@{
    int MDBCommunication(int Timeout, void * SendData,
                        int SendLength, void * ReceiveData, int &ReceiveLength,
                        int &TransferStatus);
    int MDBSendBreak(void);
//@}

private:

};

#endif	/* _CCTALKCCT900DEVICE_H */

// End ccTalkCCT900Device.h
////////////////////////////////////////////////////////////////////////////////

