////////////////////////////////////////////////////////////////////////////////
// File:   ccTalkBillValidatorDevice.h
// Author: manfred
// Copyright 2010:
//              wh Münzprüfer Berlin GmbH
//              Teltower Damm 276
//              D-10707 Berlin
//              Germany
//              info@whberlin.de
//
// Created: 13. August 2010 16:07
//
////////////////////////////////////////////////////////////////////////////////

#ifndef _CCTALKBILLVALIDATORDEVICE_H
#define	_CCTALKBILLVALIDATORDEVICE_H

#include "ccTalkDevice.h"

#ifndef USE_STRING
    #define USE_STRING  0
#endif

#ifndef USE_QT
    #define USE_QT      0
#endif

#if USE_STRING
#include <string>
#endif

#if USE_QT
#include <QtGui/QApplication>
#endif
#define CCT_BV_BUFFERED_EVENT_QUEUE 5

// Event Codes
#define CCT_BILL_EVENT_MASTER_INHIBIT			0
#define CCT_BILL_EVENT_RETURNED_FROM_ESCROW		1
#define CCT_BILL_EVENT_INVALID_BILL_VALIDATION	2
#define CCT_BILL_EVENT_INVALID_BILL_TRANSPORT	3
#define CCT_BILL_EVENT_INHIBIT_BILL_SERIAL		4
#define CCT_BILL_EVENT_INHIBIT_BILL_DIP			5
#define CCT_BILL_EVENT_JAM_TRANSPORT			6
#define CCT_BILL_EVENT_JAM_STACKER				7
#define CCT_BILL_EVENT_PULLED_BACK				8
#define CCT_BILL_EVENT_BILL_TAMPER				9
#define CCT_BILL_EVENT_STACKER_OK				10
#define CCT_BILL_EVENT_STACKER_REMOVED			11
#define CCT_BILL_EVENT_STACKER_INSERTED			12
#define CCT_BILL_EVENT_STACKER_FAULTY			13
#define CCT_BILL_EVENT_STACKER_FULL				14
#define CCT_BILL_EVENT_STACKER_JAMMED			15
#define CCT_BILL_EVENT_JAM_TRANSPORT_SAVE		16
#define CCT_BILL_EVENT_OPTO_FAULT				17
#define CCT_BILL_EVENT_STRING_FAULT				18
#define CCT_BILL_EVENT_ANTI_STRING_FAULTY		19
#define CCT_BILL_EVENT_BARCODE_DETECTED			20

#define CCT_BILL_EVENT_CASH					0
#define CCT_BILL_EVENT_ESCROW				1
#define CCT_BILL_EVENT_RETURNED_ESCROW		1
#define CCT_BILL_EVENT_INVALID				2
#define CCT_BILL_EVENT_INVALID_TRANSPORT	3
#define CCT_BILL_EVENT_JAM_TRANSPORT		6
#define CCT_BILL_EVENT_JAM_STACKER			7

#define CCT_BILL_ROUTE_RETURN				0
#define CCT_BILL_ROUTE_CASH					1

/// ssTalk library class for bill validators

class ccTalkBillValidatorDevice  : public CccTalkDevice
{
public:
    ccTalkBillValidatorDevice();
    virtual ~ccTalkBillValidatorDevice();

    typedef struct BUFFERED_EVENTS {
        unsigned char Count;
        struct {
            unsigned char Bill;
            unsigned char Error;
        } Event[CCT_BV_BUFFERED_EVENT_QUEUE];
    } EventBuffer;

    typedef enum {
        ROUTE_RETURN,
        ROUTE_CASH_BOX,
        ROUTE_EXTEND_TIMEOUT=   255
    } BillRoute;

public:
    int ReadBufferedBillEvents(int &Count, unsigned char *Events);
    int ReadBufferedBillEvents(EventBuffer *Events= NULL);

    int RequestBillId(int Bill, char *BillId);                    
    int RequestCountryScalingFactorId(int Country, int &Scaling, int &Place);
    int RouteBill(BillRoute Route, int &ErrorCode);
    int ModifyBillOperationMode(bool Stacker, bool Escrow);                   
    int RequestBillOperationMode(bool &Stacker, bool &Escrow);

    #if USE_STRING
    int RequestBillId(int Bill, string &BillId);                      
    #endif
    #if USE_QT
    int RequestBillId(int Bill, QString &BillId);                
    #endif

    /// @name extended and management functions
    //@{    int ModifyCoinPrecision(const unsigned char *Buffer);
    int PollNext(int &EventCount, int &Bill, int &ErrorCode);
    int Poll(int &EventCount, EventBuffer *Events);
    ///@}
    
private:

    int m_LastPollCounter;
    int m_LastPollEvent;

private:
    EventBuffer m_BufferedEvents;
};

#endif	/* _CCTALKBILLVALIDATORDEVICE_H */

// End ccTalkBillValidatorDevice.h
////////////////////////////////////////////////////////////////////////////////

