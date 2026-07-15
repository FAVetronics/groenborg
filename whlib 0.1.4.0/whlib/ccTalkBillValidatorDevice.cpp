////////////////////////////////////////////////////////////////////////////////
// File:   ccTalkBillValidatorDevice.cpp
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

////////////////////////////////////////////////////////////////////////////////
// Includes
#define	_CCTALKBILLVALIDATORDEVICE_CPP_SRC

#include "ccTalkBillValidatorDevice.h"

#define BV_BILL_INDEX_ERR -1
#define BV_MAX_BILLS    16
#define BV_ERR_LOST_EVENTS -2
#define BV_BILL_INDEX_OFFSET 1

#define CHECK_IS_OPEN()    {if(!IsOpen()) return SetError(SIO_OPEN_ERR);}
#define CHECK_BILL_INDEX(a,b)  {if((a)<0 || (a)>=BV_MAX_BILLS) return (b);}
#define CHECK_BILL_INDEX_NR(a) {if((a)<0 || (a)>=BV_MAX_BILLS) return ;}



////////////////////////////////////////////////////////////////////////////////
// Constructor / Destructor

ccTalkBillValidatorDevice::ccTalkBillValidatorDevice()
{
    m_LastPollEvent= 0;
    m_LastPollCounter= 0;
    memset(&m_BufferedEvents, 0, sizeof(m_BufferedEvents));
}

ccTalkBillValidatorDevice::~ccTalkBillValidatorDevice()
{
}

////////////////////////////////////////////////////////////////////////////////
/// Return the last 5 events of bill or error codes. This information queue
/// allows a host polling the device with low cycle rate (159)
/// \param Event pointer to the receive buffer
///  - 1. Byte counter 1...255. \n
///    This counter is reset to zero only at power on or at ResetDevice() command
///  - 5x 2 Bytes Queue
///  -- 1. Byte Bill 0= no coin accepded error accured,  1-16 Bill
///  -- 2. Byte error code 0= none (coin accepted) or error code
/// \return error code, 0= no error

int ccTalkBillValidatorDevice::ReadBufferedBillEvents(int &Count, unsigned char *Events)
{
    int err;

    CHECK_IS_OPEN();

    Count= 0;
    memset(Events, 0, sizeof(m_BufferedEvents.Event));

    err= ReadBufferedBillEvents();
    if(!err)
    {
        Count= m_BufferedEvents.Count;
        memcpy(Events, m_BufferedEvents.Event, sizeof(m_BufferedEvents.Event));
    }
    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Return the last 5 events of credits or error codes. This information queue
/// allows a host polling the device with low cycle rate
/// \param Event pointer structure BUFFERED_EVENT, see ReadBufferedBillEventd(int, unsigened char)
/// \return error code, 0= no error

int ccTalkBillValidatorDevice::ReadBufferedBillEvents(EventBuffer *Events)  // 229
{
    int err;

    CHECK_IS_OPEN();

    int buflen= sizeof(m_BufferedEvents);

    if(Events == NULL)
        Events= &m_BufferedEvents;
    m_ccTalk->SetCRCType(m_CsumType);
    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_READ_BUFFERED_BILL_EVENT, buflen, Events);

    return err;
}


//////////////////////////////////////////////////////////////////////
/// Returns ASCII with bill ID
/// \param Bill Index (zero based) of requested coins 0...15
/// \param BillId String with bill Id
/// \return error code, 0= no error

int ccTalkBillValidatorDevice::RequestBillId(int Bill, char *BillId)                    // 184
{
    char par;
    int buflen= CCT_VALUE_ID_LEN;
    int err= 0;

    CHECK_IS_OPEN();
    CHECK_BILL_INDEX(Bill,BV_BILL_INDEX_ERR);

    memset(BillId, 0, CCT_VALUE_ID_LEN+1);
    par = Bill + BV_BILL_INDEX_OFFSET;

    m_ccTalk->SetCRCType(m_CsumType);
    err = m_ccTalk->RequestBinDataEx(m_DeviceAdr, CCT_REQUEST_BILL_ID, &par, 1, BillId, buflen);

    return err;
}


#ifdef USE_STRING

//////////////////////////////////////////////////////////////////////
/// Returns ASCII string with bill ID
/// \param Bill Index (zero based) of requested bills 0...15
/// \param BillId reference to string class with bill Id
/// \return error code, 0= no error

int ccTalkBillValidatorDevice::RequestBillId(int Bill, string &BillId)                       // 184
{
    int err= 0;
    char buffer[CCT_VALUE_ID_LEN+1];

    CHECK_IS_OPEN();

    memset(buffer, 0, sizeof(buffer));
    err= RequestBillId(Bill, buffer);   // No Offset
    if(!err)
        BillId= buffer;

    return err;
}

#endif

#if USE_QT
//////////////////////////////////////////////////////////////////////
/// Returns ASCII string with bill ID
/// \param Bill Index (zero based) of requested coins 0...15
/// \param BillId reference to QtString class with Coin Id
/// \return error code, 0= no error

int ccTalkBillValidatorDevice::RequestBillId(int Bill, QString &BillId)                 // 184
{
    int err= 0;
    char buffer[CCT_VALUE_ID_LEN+1];

    CHECK_IS_OPEN();
    memset(buffer, 0, sizeof(buffer));

    err= RequestBillId(Bill, buffer);       // No Offset
    if(!err)
        BillId= QString::fromAscii(buffer);
    return err;
}
#endif



//////////////////////////////////////////////////////////////////////
/// Returns scaling factor and decimal places
/// \param Country two byte string with country code
/// \param Scaling reference to scaling factor, if zero country code in not supported
/// \param Place refence to decimal place
/// \return error code, 0= no error

int ccTalkBillValidatorDevice::RequestCountryScalingFactorId(int Country, int &Scaling, int &Place)                    // 184
{
    unsigned char par[2];
    unsigned char buf[3];
    int buflen= 3;
    int err= 0;

    CHECK_IS_OPEN();

    Scaling= 0;
    Place= 0;
    par[0] = Country & 0xff;
    par[1] = (Country >> 8) & 0xff;

    m_ccTalk->SetCRCType(m_CsumType);
    err = m_ccTalk->RequestBinDataEx(m_DeviceAdr, CCT_REQUEST_COUNTRY_SCALING_FACTOR, &par, 1, buf, buflen);

    if(!err)
    {
        Scaling= buf[0] + buf[1]*256;
        Place= buf[2];        
    }

    return err;
}


//////////////////////////////////////////////////////////////////////
/// Controls routing of a bill in an escrow
/// \param Route route code, route to return , cashbox or extend escrow timeout,
/// \param ErrorCode refence to errorcode
/// - 254= escrow is empty
/// - 255= failed to route bill
/// \return error code, 0= no error

int ccTalkBillValidatorDevice::RouteBill(BillRoute Route, int & ErrorCode)                    // 184
{
    int err= 0;
    unsigned char buf;
    int len = 1;

    m_ccTalk->SetCRCType(m_CsumType);
    err= m_ccTalk->RequestBinDataEx(m_DeviceAdr, CCT_ROUTE_BILl, &Route, 1, &buf, len);

    if(!err || len > 1)
        ErrorCode = buf;

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Controls whether various feartures are used
/// \param Stacker use Stacker
/// \param Escrow use escrow
/// \return error code, 0= no error

int ccTalkBillValidatorDevice::ModifyBillOperationMode(bool Stacker, bool Escrow)                    // 184
{
    unsigned char mode= 0;

    mode|= Stacker ? 1 : 0;
    mode|= Escrow ? 2 : 0;

    m_ccTalk->SetCRCType(m_CsumType);
    return m_ccTalk->SendData(m_DeviceAdr, CCT_MODIFY_BILL_OPERATING_MODE, 1, &mode);
}

//////////////////////////////////////////////////////////////////////
/// Return the used bill opertion modes
/// \param Stacker reference to return value use Stacker
/// \param Escrow reference to return value use escrow
/// \return error code, 0= no error

int ccTalkBillValidatorDevice::RequestBillOperationMode(bool &Stacker, bool &Escrow)                    // 184
{
    unsigned char mode= 0;
    int len= 1;
    int err= 0;

    Stacker = (mode & 1) ? true : false;
    Escrow = (mode & 1) ? true : false;

    m_ccTalk->SetCRCType(m_CsumType);
    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_REQUEST_BILL_OPERATING_MODE, len, &mode);

    Stacker = (mode & 1) ? true : false;
    Escrow = (mode & 1) ? true : false;

    return err;
}




////////////////////////////////////////////////////////////////////////////////
// Extended Functions

////////////////////////////////////////////////////////////////////////////////
/// Poll bill validator for accepted bills or error events. This funciotn return a counter of new
/// events in the EventBuffer queue. For that it implzit calls the
/// ReadBufferedBillEvent() function and calulate the count of new events in the queue
/// \param EventCount reference to returned new events counter if zero non new event
/// occured
/// \param CreditBuffer reference to struct with event queue, see also function
/// ReadBufferedCredit()
/// \return error code, 0= no error

int ccTalkBillValidatorDevice::Poll(int &EventCount, EventBuffer *Events)
{
    int err;
    EventCount= 0;
    int billcnt;
    CHECK_IS_OPEN();

    memset(Events, 0, sizeof(EventBuffer));

    err= ReadBufferedBillEvents(Events);

    if(!err)
    {
        billcnt= Events->Count;
        if(billcnt == 0)            // Reset
	{
            m_LastPollCounter= 0;
	}
	else
	{
            if(m_LastPollCounter > billcnt)
		EventCount= billcnt + 255 - m_LastPollCounter;
            else
		EventCount= billcnt - m_LastPollCounter;
            m_LastPollCounter= billcnt;
        }

        if(EventCount > CCT_BV_BUFFERED_EVENT_QUEUE)
        {
            err= BV_ERR_LOST_EVENTS;
            EventCount %= CCT_BV_BUFFERED_EVENT_QUEUE;
        }
    }

    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Poll bill validator for accepted bills or error events. Use this function to get separted
/// Event received from the bill validator. It returns the next event (index of
/// accepted bill or deteded error code) places in the buffered queue. If the
/// queue is empty the function Poll() ist impizit called. \n
/// Call these function to get the next event as long as no new event is signed.
/// \param EventCount Reference to returned new events counter. If zero non new
/// event occured.
/// \param Bill Reference to returned coin index. If zero no coin is accepted.
/// \param ErrorCode Reference to returned error code. If zero nor error occured.
/// \return error code, 0= no error

int ccTalkBillValidatorDevice::PollNext(int &EventCount, int &Bill, int &ErrorCode)
{
    int err= 0;

    EventCount= 0;
    Bill= 0;
    ErrorCode= 0;
    CHECK_IS_OPEN();

    if(m_LastPollEvent == 0)    // no events - request new data
    {
        err= Poll(m_LastPollEvent, &m_BufferedEvents);
        if(err)
        {
            m_LastPollEvent= 0;
            return err;
        }
    }

    if(m_LastPollEvent > 0)     // events in buffer ?
    {
        Bill= m_BufferedEvents.Event[m_LastPollEvent-1].Bill;
        ErrorCode= m_BufferedEvents.Event[m_LastPollEvent-1].Error;
        EventCount= m_LastPollEvent;
        m_LastPollEvent--;
    }

    return err;
}


// End ccTalkBillValidatorDevice.cpp
////////////////////////////////////////////////////////////////////////////////
