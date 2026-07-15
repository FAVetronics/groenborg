/// \file Source file class MdbCashlessDevice

// MDB Device Class for Cashless Devices
////////////////////////////////////////////////////////////////////////////////
// File:   MdbCashlessDevice.cpp
// Author: manfred
// Copyright 2013:
//              wh Münzprüfer Berlin GmbH
//              Teltower Damm 276
//              D-10707 Berlin
//              Germany
//              info@whberlin.de
//
// Created: 11. November 2013 10:03
//
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
// Includes
//
#define	_MDBCASHLESSDEVICE_CPP_SRC

#include "stdlib.h"
#include "stdio.h"
#include "string.h"
#include "MdbCashlessDevice.h"

////////////////////////////////////////////////////////////////////////////////
// Macros
//

#define CHECK_IS_OPEN()    {if(!IsOpen()) return SetError(SIO_OPEN_ERR);}

// for debugging
#define MDB_CL_DEBUG_OUTPUT(a,b,c,d) fprintf(stderr,(a),(b),(c),(d))

////////////////////////////////////////////////////////////////////////////////
// Constructor / Destructor
//

MdbCashlessDevice::MdbCashlessDevice()
{
    m_Funds= 0;
    m_VendingValue= 0;
    m_RevalueValue= 0;
    m_RevalueValuePendig= 0;
    m_State= 0;
    m_ErrorCounter= 0;
    memset(&m_PeripheralID, 0, sizeof(m_PeripheralID));
    memset(&m_ConfigResponse, 0,  sizeof(m_ConfigResponse));

    // for diagnostic only
    m_PollRxLen= 0;
}

MdbCashlessDevice::~MdbCashlessDevice()
{
}

//////////////////////////////////////////////////////////////////////
/// Overwrites parent function
/// \return error code, 0= no error

int MdbCashlessDevice::Reset(void)
{
    m_Funds= 0;
    m_VendingValue= 0;
    m_RevalueValue= 0;
    m_RevalueValuePendig= 0;
    m_State= 0;
    m_ErrorCounter= 0;
    memset(&m_PeripheralID, 0, sizeof(m_PeripheralID));
    memset(&m_ConfigResponse, 0,  sizeof(m_ConfigResponse));

    // for diagnostic only
    m_PollRxLen= 0;

    CHECK_IS_OPEN();

    return MdbDevice::Reset();
}

//////////////////////////////////////////////////////////////////////
/// Setup the configuration of the card reader
/// Sends VMC features. Device response card reader features sored class intern
/// in m_ConfigResponse.
/// \param VmcLevel 1..3 indicates feature level of the VMC
/// \param Cols Number of columns on the display
/// \param Rows Number of rows on the display, set 00h if no display is available
/// \param Rows Number of rows on the display
/// \param Disp Display type information. 0= Numbers, 1= full ASCII
/// \return error code, 0= no error

int MdbCashlessDevice::SetupConfigData(int VmcLevel, int Cols, int Rows, int Disp)
{
    int err;
    int len;
    MdbCashLessSetupConfigSendData cfgdata;

    CHECK_IS_OPEN();

    // clear buffers
    memset(&cfgdata, 0, sizeof (cfgdata));
    memset(&m_ConfigResponse, 0,  sizeof(m_ConfigResponse));
            
    cfgdata.Header = MDB_CMD_CL_SETUP_CONFIG_DATA; // must be 0x00 sein
    cfgdata.FeatureLevel = VmcLevel;
    cfgdata.DisplayCols = Cols; 
    cfgdata.DisplayRows = Rows; 
    cfgdata.DisplayInfo = Disp; 

    len= sizeof(MdbCashLessSetupConfigResponseData);
    err= SendCommand(MDB_CMD_CL_SETUP, (unsigned char*)&cfgdata, 
                     sizeof(MdbCashLessSetupConfigSendData),
                     (unsigned char*)&m_ConfigResponse, len);
    
    if(!err)
    {       
        m_Nomination[0].SetNomination(Bcd2Short(m_ConfigResponse.CountryCode),
                                     1, m_ConfigResponse.ScalingFactor);
    }    
    
    return err;
}

//////////////////////////////////////////////////////////////////////
/// Setup price range
/// Sends VMC features. Device response card reader features sored class intern
/// in m_ConfigResponse.
/// \param Min Minimum Price set to 0 if undefined
/// \param Man Maximum Price set to 0xffff if undefined
/// \return error code, 0= no error

int MdbCashlessDevice::SetupPrice(int Min, int Max)
{
    int err;
    int len;
    MdbCashLessSetupMMPriceSendData mmprices;

    CHECK_IS_OPEN();

    mmprices.Header = MDB_CMD_CL_SETUP_MIN_MAX_PRICES;
    mmprices.MaxPrice = Max;
    SwapShort(&mmprices.MaxPrice);
    mmprices.MinPrice = Min;
    SwapShort(&mmprices.MinPrice);

    len = 0;
    err = SendCommand(MDB_CMD_CL_SETUP, (unsigned char*) & mmprices,
                      sizeof(mmprices), NULL, len);

    return err;
}

////////////////////////////////////////////////////////////////////////////////
// POLL - Command
//

//////////////////////////////////////////////////////////////////////
/// Poll device command / data reponse
/// \param AutoResponse Depending on the response data Actions Commands will
/// be send automatical
/// \return error code, 0= no error

int MdbCashlessDevice::Poll(bool AutoResponse)
{
    int err = 0;
    int len;
    int suberr = 0;
    unsigned char *data;
    unsigned char hdr;
    unsigned short fund; // , mul;

    CHECK_IS_OPEN();

    MdbCashLessSetupConfigResponseData *rxdata;

    len = MDB_CMD_CL_POLL_DATA_LEN;
    err = SendCommand(MDB_CMD_CL_POLL, NULL, 0, NULL, len);
    
    m_PollRxLen= len; // for diagnostic only

    if(!err & len > 0)
    {
        data = MdbRxData();
        hdr = data[0];

        switch (hdr)
        {
            case MDB_CMD_CL_POLL_RESET: // 0x00
                m_Funds = m_RevalueValue = m_VendingValue = 0;
                MDB_CL_DEBUG_OUTPUT("\n\r>> Poll - Reset\n\r",0,0,0);
                SetState(MDB_CL_STATE_DISABLED);
                break;

            case MDB_CMD_CL_POLL_READER_CONFIG_DATA: // 0x01
                rxdata = (MdbCashLessSetupConfigResponseData*) MdbRxData();
                m_Nomination[0].SetNomination(Bcd2Short(rxdata->CountryCode),
                            1, rxdata->ScalingFactor);
                MDB_CL_DEBUG_OUTPUT("\n\r>> Poll - Reader Config Data\n\r",0,0,0);
                break;

            case MDB_CMD_CL_POLL_DISPLAY_REQUEST: //0x02
                char str[32];
                memset(str, 0, sizeof(str));
                strncpy(str, (char*)&data[2], 31);
                MDB_CL_DEBUG_OUTPUT("\n\r>> Poll - Display Request - Time:%d Ascii: %s\n\r", (int)data[1], str,0);
                break;

            case MDB_CMD_CL_POLL_BEGIN_SESSION: // 0x03
                m_Funds = fund = ((unsigned int) data[1])*256 + data[2]; // High Byte First !!!
                m_RevalueValue = 0;     // clear values
                m_VendingValue = 0;
                SetState(MDB_CL_STATE_SESSION_IDLE);
                MDB_CL_DEBUG_OUTPUT("\n\r>> Poll - Begin Session - Fund: %d\n\r", fund,0,0);
                break;

            case MDB_CMD_CL_POLL_SESSION_CANCEL_REQUEST: // 0x04
                m_Funds = 0;
                SetState(MDB_CL_STATE_VENDING_CANCELED);
                MDB_CL_DEBUG_OUTPUT("\n\r>> Poll - Session Cancel Request\n\r", 0, 0, 0);
                break;

            case MDB_CMD_CL_POLL_VEND_APPROVED: // 0x05
                m_Nomination[1].SetValue(m_VendingValue);
                m_VendingValue = 0;
                m_Funds = 0;
                //			DelayMs(10);
                if(AutoResponse)
                  suberr = VendSuccess(0);
                //				DelayMs(10);
                //				err= VendSessionComplete();
                SetState(MDB_CL_STATE_VENDING_END);
                MDB_CL_DEBUG_OUTPUT("\n\r>> Poll - Vend Approved\n\r",0,0,0);
                break;

            case MDB_CMD_CL_POLL_VEND_DENIED: // 0x06
                m_Funds = 0;
                SetState(MDB_CL_STATE_VENDING_CANCELED);
                MDB_CL_DEBUG_OUTPUT("\n\r>> Poll - Vend Denied \n\r", 0,0,0);
                break;

            case MDB_CMD_CL_POLL_END_SESSION: // 0x07
                m_Funds = 0;
                MDB_CL_DEBUG_OUTPUT("\n\r>> Poll - End Session\n\r", 0,0,0);
                break;

            case MDB_CMD_CL_POLL_CANCELLED: // 0x08
                m_Funds = 0;
                SetState(MDB_CL_STATE_VENDING_CANCELED);
                MDB_CL_DEBUG_OUTPUT("\n\r>> Poll - Canceled \n\r", 0,0,0);
                break;

            case MDB_CMD_CL_POLL_PERIPHERAL_ID: // 0x09
                memcpy(&m_PeripheralID, data, min((int)sizeof(m_PeripheralID), len));
                MDB_CL_DEBUG_OUTPUT("\n\r>> Poll - Peripheral IS (%d bytes) \n\r", len, 0, 0);
                break;

            case MDB_CMD_CL_POLL_MALFUNCTION_ERROR: // 0x0A
                m_Funds = 0;
                SetState(MDB_CL_STATE_ERROR);
                MDB_CL_DEBUG_OUTPUT("\n\r>> Poll - Malfunction Error (%d)\n\r", data[1], 0, 0);
                break;

            case MDB_CMD_CL_POLL_CMD_OUT_OF_SEQUENCE: // 0x0B
                //		m_Funds= 0;
                //		SetState(MDB_CL_STATE_ERROR);
                MDB_CL_DEBUG_OUTPUT("\n\r>> Poll - Out of Sequence\n\r", 0, 0, 0);
                break;

            case MDB_CMD_CL_POLL_REVALUE_APPROVED: // 0x0d
                m_RevalueValue = 0;
                if(AutoResponse)
                   suberr = VendSessionComplete();
                SetState(MDB_CL_STATE_REVALUE_OK);
                m_Nomination[1].SetValue(m_RevalueValuePendig);
                m_RevalueValuePendig = 0;
                MDB_CL_DEBUG_OUTPUT("\n\r>> Poll - Revalue Approved\n\r", 0, 0, 0);
                break;

            case MDB_CMD_CL_POLL_REVALUE_DENIED: // 0x0E
                m_RevalueValue = 0;
                if(AutoResponse)
                    suberr = VendSessionComplete();
                SetState(MDB_CL_STATE_ERR_REVALUE);
                MDB_CL_DEBUG_OUTPUT("\n\r>> Poll - Revalue Denied\n\r", 0, 0, 0);
                break;

            case MDB_CMD_CL_POLL_REVALUE_LIMIT_AMOUNT: // 0x0F
                MDB_CL_DEBUG_OUTPUT("\n\r>> Poll - Revalue Limit Amount: %d \n\r", (int)data[1]*256+(int)data[2], 0, 0);
                break;

            case MDB_CMD_CL_POLL_USER_FILE_DATA: // 0x10
                MDB_CL_DEBUG_OUTPUT("\n\r>> Poll - User File Data\n\r", 0, 0, 0);
                break;

            case MDB_CMD_CL_POLL_TIME_DATE_REQUEST: // 0x11
                MDB_CL_DEBUG_OUTPUT("\n\r>> Poll - Time-Date Request\n\r", 0, 0, 0);
                break;

                // not implemented
            case MDB_CMD_CL_POLL_DATA_ENTRY_REQUEST:// 0x12
            case MDB_CMD_CL_POLL_DATA_ENTRY_CANCEL: // 0x13
                //resverved	0x14 - 0x1A
            case MDB_CMD_CL_POLL_FTL_REQ_TO_RCV:    // 0x1B
            case MDB_CMD_CL_POLL_FTL_RETRY_DENY:    // 0x1C
            case MDB_CMD_CL_POLL_FTL_SNED_BLOCK:    // 0x1D
            case MDB_CMD_CL_POLL_FTL_OK_TO_SEND:    // 0x1E
            case MDB_CMD_CL_POLL_FTL_REQ_TO_SEND:   // 0x1F
                // resverved	0x20 - 0xFE
            case MDB_CMD_CL_POLL_DIAGNOSTIC_RESPONSE: // 0xFF
                break;

            default:

                break;
        }
        m_ErrorCounter = 0;
    }
    return err;
}

////////////////////////////////////////////////////////////////////////////////
// VEND - Commands
//

#define MDB_CL_APPROVED     1
#define MDB_CL_DENIED   -1
#define MDB_CL_NO_RESPONSE  0

//////////////////////////////////////////////////////////////////////
/// Initialize a Vending sequence after receiving a Begin Session message via
/// Poll() command.
/// \param Price Scaled Price of selected product
/// \param Product Id of selected product. Set to 0xfff if undefined
/// \return error code, 0= no error

int MdbCashlessDevice::VendRequest(unsigned short Price, unsigned short Product, int &Approved, int &Ammount)
{
    char err = 0;
    int len;
    MdbCashLessVendRequestSendData txdata;
    MdbCashLessVendRequestResponseData *rxdata;

    CHECK_IS_OPEN();

    txdata.Header = MDB_CMD_CL_VEND_REQUEST;
    txdata.Price = Price;
    SwapShort(&txdata.Price);
    txdata.Product = Product;
    SwapShort(&txdata.Product);

    len = sizeof (MdbCashLessVendRequestResponseData);
    err = SendCommand(MDB_CMD_CL_VEND, (unsigned char*) & txdata,
                      sizeof (MdbCashLessVendRequestSendData), NULL, len);
    
    Ammount= 0;
    Approved= MDB_CL_NO_RESPONSE;
    
    if(!err)
    {
        if (len > 0)
        {
            rxdata = (MdbCashLessVendRequestResponseData*) MdbRxData();
            switch (rxdata->Header)
            {
                case MDB_CMD_CL_VEND_REQUEST_APPROVED:
                    Approved = MDB_CL_APPROVED;
                    Ammount = rxdata->Price;
                    MDB_CL_DEBUG_OUTPUT("\n\r>> Vend Request - Amount %d\n\r", (int) Ammount, 0, 0);
                    break;

                case MDB_CMD_CL_VEND_REQUEST_DENIED: // Header als Fehlermeldung zurckgeben
                    MDB_CL_DEBUG_OUTPUT("\n\r>> Vend Request - Denied\n\r", 0, 0, 0);
                    Approved = MDB_CL_DENIED;
                    break;

                default:
                    err = 0;
                   break;
            }
        }
        else
        {
            MDB_CL_DEBUG_OUTPUT("\n\r>> Vend Request - No Response\n\r", 0, 0, 0);
        }   
    }

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Cancels a Vend Request before a approved/denied is responsed from the reader
/// \param none
/// \return error code, 0= no error

int MdbCashlessDevice::VendCancel(void)
{
    int len;
    int err;
    unsigned char cmd;
    unsigned char buf;

    CHECK_IS_OPEN();

    cmd= MDB_CMD_CL_VEND_CANCEL;
    buf= 0;

    len= sizeof(buf);
    err= SendCommand(MDB_CMD_CL_VEND, &cmd, sizeof(cmd), &buf, len);

    if(!len)
        MDB_CL_DEBUG_OUTPUT("\n\r>> Vend Cancel - No Response\n\r",0, 0, 0);
    else
        MDB_CL_DEBUG_OUTPUT("\n\r>> Vend Cancel - Response (%02Xh)\n\r", buf, 0, 0);

    return err;
}

//////////////////////////////////////////////////////////////////////
/// The selected product has been successfully dispensed
/// \param Product is the item number of the selected product. Set to 0xffff if
/// undefined
/// \return error code, 0= no error

int MdbCashlessDevice::VendSuccess(unsigned short Product)
{
    int err;
    int len = 0;
    MdbCashLessVendSessionSuccessData success;

    CHECK_IS_OPEN();

    success.Header = MDB_CMD_CL_VEND_SUCCESS;
    success.Product = Product;
    SwapShort(&success.Product);

    err = SendCommand(MDB_CMD_CL_VEND, (unsigned char*) & success, sizeof (MdbCashLessVendSessionSuccessData), NULL, len);

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Inform the reader that a problem has deteced and the product is not
/// dispensed.
/// \param none
/// \return error code, 0= no error

int MdbCashlessDevice::VendFailure(void)
{
    int len = 0;
    unsigned char buf;

    CHECK_IS_OPEN();

    buf = MDB_CMD_CL_VEND_FAILURE;
    return SendCommand(MDB_CMD_CL_VEND, &buf, sizeof(buf), NULL, len);
}

//////////////////////////////////////////////////////////////////////
/// This tells the reader that the sessionis complete and to return to the
/// enable state.
/// \param none
/// \return error code, 0= no error

int MdbCashlessDevice::VendSessionComplete(void)
{
    int len;
    int err;
    unsigned char buf, cmd;
    
    CHECK_IS_OPEN();

    cmd= MDB_CMD_CL_VEND_SESSION_COMPLETE;
    buf= 0;
    len= sizeof(buf);

    err= SendCommand(MDB_CMD_CL_VEND, &cmd, sizeof(cmd), &buf, len);

    if(!len)
        MDB_CL_DEBUG_OUTPUT("\n\r>> Vend Session Complete (%d) - No Response\n\r", err, 0, 0);
    else
        MDB_CL_DEBUG_OUTPUT("\n\r>> Vend Session Complete (%d) - Response: %02Xh\n\r", err, buf, 0);

    return err;
}

////////////////////////////////////////////////////////////////////////////////
// READER - Commands
//

//////////////////////////////////////////////////////////////////////
/// Disables Cashless Device
/// \param none
/// \return error code, 0= no error

int MdbCashlessDevice::ReaderDisable(void)
{
    int len= 0;
    unsigned char buf;

    CHECK_IS_OPEN();

    buf= MDB_CMD_CL_READER_DISABLE;

    return SendCommand(MDB_CMD_CL_READER, &buf, sizeof(buf), NULL, len);
}

//////////////////////////////////////////////////////////////////////
/// Enables Cashless Device
/// \param none
/// \return error code, 0= no error

int MdbCashlessDevice::ReaderEnable(void)
{
    int len= 0;
    unsigned char buf;

    CHECK_IS_OPEN();

    buf= MDB_CMD_CL_READER_ENABLE;

    return SendCommand(MDB_CMD_CL_READER, &buf, sizeof(buf), NULL, len);
}

//////////////////////////////////////////////////////////////////////
/// Cancel payment
/// only in enable state
/// \param none
/// \return error code, 0= no error

int MdbCashlessDevice::ReaderCancel(void)
{
    int len;
    unsigned char buf;

    CHECK_IS_OPEN();

    len= sizeof(buf);

    return SendCommand(MDB_CMD_CL_READER_CANCEL, NULL, 0, &buf,len);
}


////////////////////////////////////////////////////////////////////////////////
// REVALUE

//////////////////////////////////////////////////////////////////////
/// Request a revalue of a prepaid card
/// \param Value Scaled value to load
/// \param Approved Return the acceptance of this request
///    0: no response Poll Command is requiered
///    1: Approved
///    -1: Denied
/// \return error code, 0= no error

int MdbCashlessDevice::RevalueRequest(unsigned short Value, int &Approved)
{
    char err = 0;
    int len;
    MdbCashLessRevalueRequestSendData txdata;
    unsigned char rxdata;

    CHECK_IS_OPEN();

    txdata.Header = MDB_CMD_CL_REVALUE_REQUEST;
    txdata.Value = Value;
    SwapShort(&txdata.Value);

    len = sizeof(rxdata);
    err = SendCommand(MDB_CMD_CL_REVALUE, (unsigned char*) &txdata,
                      sizeof (MdbCashLessRevalueRequestSendData), &rxdata, len);

    Approved= MDB_CL_NO_RESPONSE;
    if(!err)
    {
        if(!len)
            MDB_CL_DEBUG_OUTPUT("\n\r>> Vend Revalue Request (%d) - No Response\n\r", err, 0, 0);
        else
        {
            MDB_CL_DEBUG_OUTPUT("\n\r>> Vend Revalue Request (%d) - Response: %02Xh\n\r", err, rxdata, 0);

            switch (rxdata)
            {
                case MDB_CMD_CL_REVALUE_REQUEST_APPROVED:
                    Approved= MDB_CL_APPROVED;
                    SetState(MDB_CL_STATE_REVALUE_OK);
                    break;
                case MDB_CMD_CL_REVALUE_REQUEST_DENIED: 
                    Approved= MDB_CL_DENIED;
                    SetState(MDB_CL_STATE_ERR_REVALUE);
                    break;
                default:
                    break;
            }
        }
    }
    return err;
}

//////////////////////////////////////////////////////////////////////
/// Request a revalue loasd limit
/// \param Amount Returns maximum Value
/// \return error code, 0= no error

int MdbCashlessDevice::RevalueLimitRequest(int &Amount)
{
    char err;
    int len;
    unsigned char cmd;
    MdbCashLessRevalueLimitRequestResponseData buf;

    CHECK_IS_OPEN();

    memset(&buf, 0, sizeof(buf));
    cmd= MDB_CMD_CL_REVALUE_LIMIT_REQUEST;
    len = sizeof(buf);
    err = SendCommand(MDB_CMD_CL_REVALUE, &cmd, sizeof(cmd), (unsigned char*)&buf, len);

    if(!err)
    {
        if(len > 0)
        {
            SwapShort(&buf.Amount);
            Amount= buf.Amount;
            MDB_CL_DEBUG_OUTPUT("\n\r>> Vend Revalue Limit Request - Amount: %d\n\r", Amount, 0, 0);
        }
        else
            MDB_CL_DEBUG_OUTPUT("\n\r>> Vend Revalue Limit Request (%d) - No Response\n\r", err, 0, 0);
    }

    return err;
}



////////////////////////////////////////
// Expansion

//////////////////////////////////////////////////////////////////////
/// Returns formated string of device ID data received via RequestId
/// or Poll Command
/// \param Manufacturer Pointer to a char string to retrieve manufacturer ID
/// \param Serial Pointer to a char string to retrieve serial number
/// \param Model Pointer to a char string  to retrieve model name
/// \param Version Pointer to a char string  to retrieve firmware version
/// \return error code, 0= no error

void MdbCashlessDevice::GetPeripheralID(char * Manufacturer, char * Serial, char * Model, char * Version)
{
    if(Manufacturer)
    {
        strncpy(Manufacturer, (char*)m_PeripheralID.ManufacturerId, sizeof(m_PeripheralID.ManufacturerId));
        Manufacturer[sizeof(m_PeripheralID.ManufacturerId)]= 0;
    }

    if(Manufacturer)
    {
        strncpy(Serial, (char*)m_PeripheralID.SerialNumber, sizeof(m_PeripheralID.SerialNumber));
        Serial[sizeof(m_PeripheralID.SerialNumber)]= 0;
    }

    if(Serial)
    {
        strncpy(Model, (char*)m_PeripheralID.ModelNumber, sizeof(m_PeripheralID.ModelNumber));
        Model[sizeof(m_PeripheralID.ModelNumber)]= 0;
    }
    if(Version)
    {
        sprintf(Version, "%d.%d.%d.%d", (int)(m_PeripheralID.SoftwareVersion[0] >> 4),
                                            (int)(m_PeripheralID.SoftwareVersion[0] & 0xff),
                                            (int)(m_PeripheralID.SoftwareVersion[1] >> 4),
                                            (int)(m_PeripheralID.SoftwareVersion[1] & 0xff));
    }
}

//////////////////////////////////////////////////////////////////////
/// Returns formated string of device ID data. The original RequestID Command
/// is used first. If the device don't response directly the datat the Poll
/// Command is used for three timer to retrieve the ID data.
/// or Poll Command
/// \param Manufacturer Pointer to a char string to retrieve manufacturer ID
/// \param Serial Pointer to a char string to retrieve serial number
/// \param Model Pointer to a char string  to retrieve model name
/// \param Version Pointer to a char string  to retrieve firmware version
/// \return error code, 0= no error

int MdbCashlessDevice::RequestID(char * Manufacturer, char * Serial, char * Model, char * Version)
{
    int len;
    MdbCashLessExpansionRequestIdData txdata;
    MdbCashLessExpansionRequestIdResponseData rxdata;
    int err;
    int poll;

    CHECK_IS_OPEN();

    // clear buffer
    memset(&txdata, 0, sizeof(txdata));
    memset(&rxdata, 0, sizeof(rxdata));
    memset(&m_PeripheralID, 0, sizeof(MdbCashLessExpansionRequestIdResponseData));

    txdata.Header = MDB_CMD_CL_EXPANSION_REQUEST_ID;       // subcommand

    len= sizeof(rxdata);
    err= SendCommand (MDB_CMD_CL_EXPANSION, (unsigned char*) &txdata, sizeof(txdata), (unsigned char*) &rxdata, len);

    if(!err)
    {
       if(len > 0)
       {
           MDB_CL_DEBUG_OUTPUT("\n\r>> RequestId (direct response %d byte)\n\r", len, 0, 0);
           memcpy(&m_PeripheralID, &rxdata, min((int)sizeof(m_PeripheralID), len));
           GetPeripheralID(Manufacturer, Serial, Model, Version);
       }
       else
       {
           for(poll= 0; poll < 3; poll++)
           {
               err= Poll();
               if(!err &&  m_PeripheralID.Header != 0)
               {
                   MDB_CL_DEBUG_OUTPUT("\n\r>> RequestId (via polling %d times %d byte)\n\r", poll+1, m_PollRxLen, 0);
                   break;
               }
           }

           if(m_PeripheralID.Header)
                GetPeripheralID(Manufacturer, Serial, Model, Version);
       }
    }
    else
    {
        Manufacturer[0]= 0;
        Serial[0]=0;
        Model[0]= 0;
        Version[0]= 0;
    }

    return err;
}

////////////////////////////////////////////////////////////////////////////////
// Example code for managing a cashless device  --- not proved
//
// - First Open() the device with a just opened ccTalk class (serial port)
// - Enable the device using SetEnable(true)
// - frequently Poll the device using VendingControl()
// - Check State using GetVendingData()
// - Manage vending using SetVendingValue()
//

int MdbCashlessDevice::CheckDevice(void)
{
    int err= 0;

    err= Poll();
    return err;
}

//////////////////////////////////////////////////////////////////////
/// Init the cashless device and leave it in the disable state
/// \param none
/// \return error code, 0= no error

int MdbCashlessDevice::InitDevice(void)
{
    int err;

    CHECK_IS_OPEN();

    // starting initialization with a first Poll();
    err = Poll(true);

    if (!err)
    {
        err = SetupConfigData(2);       // Level 2 if Revalue is requiered
        if (!err)
        {
            err = SetupPrice();         // set default price limits
            if (!err)
                err = RequestID();      // get device ID
            if (!err)
                SetState(MDB_CL_STATE_DISABLED);    // ready but still disabled
        }
    }

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Enable / Disable Device
/// \param Enable 0= disable, 1 enable
/// \return error code, 0= no error

int MdbCashlessDevice::SetEnable(char Enable)
{
    int err;
    int len = 0;
    unsigned char cmd;

    CHECK_IS_OPEN();

    cmd = Enable ? MDB_CMD_CL_READER_ENABLE : MDB_CMD_CL_READER_DISABLE;
    if(Enable)
    {
        cmd= MDB_CMD_CL_READER_ENABLE;
        err = SendCommand(MDB_CMD_CL_READER, &cmd, MDB_CMD_CL_READER_LEN, NULL, len);
        if(!err && GetState() == MDB_CL_STATE_DISABLED)
            SetState(MDB_CL_STATE_ENABLED);
    }
    else
    {
        cmd= MDB_CMD_CL_READER_DISABLE;
        err = SendCommand(MDB_CMD_CL_READER, &cmd, MDB_CMD_CL_READER_LEN, NULL, len);
    }

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Retuns the maximal fund of teh presented card
/// \param none
/// \return fund

unsigned short MdbCashlessDevice::GetFunds(void)
{
    return m_Funds;
}

//////////////////////////////////////////////////////////////////////
/// Set the vendig value
/// \param Value of the products price
/// \return error code, 0= no error

void MdbCashlessDevice::SetVendingValue(unsigned short Value)
{
    m_VendingValue= Value;
}

//////////////////////////////////////////////////////////////////////
/// Set Revalue ammount
/// \param Value Ammount to revalue
/// \return error code, 0= no error

void MdbCashlessDevice::SetRevalue(unsigned short Value)
{
    m_RevalueValue= Value;
}

//////////////////////////////////////////////////////////////////////
/// Returns the Status an actuell credit of the presented card
/// \param Status State of the Vending State Machine
/// \param Credit Return the Credit of the presented card
/// \param Currency of the cashless device
/// \return error code, 0= no error

void MdbCashlessDevice::GetVendingData(unsigned char *Status, unsigned short *Credit, unsigned char *Currency)
{
    unsigned short credit, mul;
    *Status= m_State;

    if(m_State == MDB_CL_STATE_SESSION_IDLE)
    {
	*Credit= m_Funds * m_Nomination[0].GetValue();
	credit= m_Funds;
	mul= m_Nomination[0].GetValue();
	m_Nomination[0].GetCurrency(&Currency[0], &Currency[1]);
    }
    else
    {
    	*Credit= 0;
	Currency[0]= '.';
	Currency[1]= '.';
    }
}

//////////////////////////////////////////////////////////////////////
/// Set State auf the Vending State Machine
/// \param Status State of the Vending State Machine
/// \return error code, 0= no error

void MdbCashlessDevice::SetState(char State)
{
    m_State= State;
}

//////////////////////////////////////////////////////////////////////
/// Returns State auf the Vending State Machine
/// \param Status State of the Vending State Machine
/// \return error code, 0= no error

int MdbCashlessDevice::GetState(void)
{
    return m_State;
}

//////////////////////////////////////////////////////////////////////
/// Vending State Machine
/// \param none
/// \return none

void MdbCashlessDevice::VendingControl(void)
{
    int err = 0;
    unsigned short value;
    unsigned short funds;
    int ammount;
    int approved;

    err= Poll();
    if(err)
        SetState(MDB_CL_STATE_INACTIVE);

    switch (m_State)
    {
        case MDB_CL_STATE_INACTIVE: // 0
            if(!CheckDevice())
            {
                if(!InitDevice())
                    SetState(MDB_CL_STATE_DISABLED);
            }
            break;
        case MDB_CL_STATE_DISABLED: // 1
            break;
        case MDB_CL_STATE_ENABLED: // 2
            break;
        case MDB_CL_STATE_SESSION_IDLE:     // 3 waiting for Vend
            value = m_VendingValue;
            funds = m_Funds;
            if (value && funds)
            {
                err = VendRequest (m_VendingValue, 0xffff, approved, ammount);
                if (err)
                {
                    SetState(MDB_CL_STATE_ERR_VENDING);
                    m_VendingValue = 0;
                    m_Funds= 0;
                }
                else
                    SetState(MDB_CL_STATE_VENDING);
            }
            else
            {
                value = m_RevalueValue;
                m_RevalueValue = 0;
                if (value)
                {
                    m_RevalueValuePendig = value;
                    err = RevalueRequest(value, approved);
                    if (err)
                    {
                        m_RevalueValue= 0;                        
                        SetState(MDB_CL_STATE_ERR_REVALUE);
                    }
                    else
                        SetState(MDB_CL_STATE_REVALUE);
                }
            }
            break;
        case MDB_CL_STATE_VENDING: // 4
            break;
        case MDB_CL_STATE_VENDING_END: // 5
            err = VendSessionComplete();
            break;
        case MDB_CL_STATE_VENDING_CANCELED: // 6 
            break;
        case MDB_CL_STATE_REVALUE:
            break;
        case MDB_CL_STATE_REVALUE_OK:
            break;
        case MDB_CL_STATE_REVALUE_CANCELED:
            break;

        case MDB_CL_STATE_ERROR:
        case MDB_CL_STATE_ERR_VENDING:
        case MDB_CL_STATE_ERR_NO_CREDIT: 
        case MDB_CL_STATE_ERR_WRONG_CARD: 
        case MDB_CL_STATE_ERR_REVALUE:
        default:
            break;
    }
}

// End MdbCashlessDevice.cpp
////////////////////////////////////////////////////////////////////////////////
