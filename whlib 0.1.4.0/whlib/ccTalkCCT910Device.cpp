////////////////////////////////////////////////////////////////////////////////
// File:   ccTalkCCT910Device.cpp
// Author: Manfred Wollny
// Copyright 2010:
//              wh Münzprüfer Berlin GmbH
//              Teltower Damm 276
//              D-10707 Berlin
//              Germany
//              info@whberlin.de
//
// Created: 5. September 2011 18:02
//
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
// Includes
#define	_CCTALKCCT910DEVICE_CPP_SRC

#include "ccTalkCCT910Device.h"

////////////////////////////////////////////////////////////////////////////////
// Constructor / Destructor

ccTalkCCT910Device::ccTalkCCT910Device()
{
}

ccTalkCCT910Device::~ccTalkCCT910Device()
{
}

//////////////////////////////////////////////////////////////////////
/// Sets serial mode
/// \param Port Port Number 0...2
/// \param Baud Buad rate indexed [1200 ... 115200]
/// \param DBits Date Bits indexed [5...8]
/// \param Parity Parity indexed [none, even, odd]
/// \return error code, 0= no error

int ccTalkCCT910Device::SetupSerialPort(int Port, int Baud, int DBits, int Parity)
{
    unsigned char buf[4];

    buf[0]= Port;
    buf[1]= Baud;
    buf[2]= DBits;
    buf[3]= Parity;

    return m_ccTalk->SendData(m_DeviceAdr, CCT_CCT910_SETUP_SERIAL_PORT, 4, buf);
}

#define CCT910_SERIAL_BUF_LEN 255
#define CCT910_SERIAL_RX_DATA 2

//////////////////////////////////////////////////////////////////////
/// Serial communication at serial port
/// \param TxBuffer Raw buffer of send data with 3 byte configuration header
/// \param TxLen Length of raw transmit buffer
/// \param RxBuffer Raw buffer of received data with on byte status header
/// \param RxLen set to maximum receive buufer length return received length
/// \return error code, 0= no error

int ccTalkCCT910Device::SerialCommunication(unsigned char *TxBuffer, int TxLen, unsigned char *RxBuffer, int RxLen)
{
    return m_ccTalk->RequestBinDataEx(m_DeviceAdr,CCT_CCT910_SERIAL_COMMUNICATION, TxBuffer, TxLen, RxBuffer, RxLen);
}


//////////////////////////////////////////////////////////////////////
/// Serial communication at serial port
/// \param Port Port Number 0...2
/// \param Timeout maximal timeout in ms
/// \param LenIndex index of length byt in received frame
/// \param TxBuffer pointer to transmit data
/// \param TxLen length of transmit data
/// \param RxBuffer pointer to receive buffer
/// \param RxLen Reference to receive buffer len, return received data length
/// \param Status Reference returns receive status
/// \return error code, 0= no error

int ccTalkCCT910Device::SerialCommunication(int Port, int Timeout, int LenIndex, unsigned char *TxBuffer, int TxLen, unsigned char *RxBuffer, unsigned char &RxLen, int  &Status)
{
    int err= 0;
    int rxlen= 0;
    unsigned char buf[CCT910_SERIAL_BUF_LEN];

    memset(buf, 0, CCT910_SERIAL_BUF_LEN);

    buf[0]= Port;
    buf[1]= Timeout;
    buf[2]= LenIndex;
    TxLen= min(CCT910_SERIAL_BUF_LEN-3, TxLen);
    memcpy(&buf[3], TxBuffer, TxLen);
    TxLen+= 3;

    err= m_ccTalk->RequestBinDataEx(m_DeviceAdr,CCT_CCT910_SERIAL_COMMUNICATION, buf, TxLen, buf, rxlen);
    if(!err)
    {
        RxLen= (unsigned char) min(rxlen - CCT910_SERIAL_RX_DATA, (int)RxLen);
        memcpy(RxBuffer, &buf[CCT910_SERIAL_RX_DATA], RxLen);
    }
    return err;
}

//////////////////////////////////////////////////////////////////////
/// Set Break Signal at serial port
/// \param Port Port Number 0...2

int ccTalkCCT910Device::SerialSendBreak(void)
{
    return m_ccTalk->SendCommand(m_DeviceAdr, CCT_CCT910_SERIAL_SEND_BREAK);
}

//////////////////////////////////////////////////////////////////////
/// Status of serial handshake lines
/// \param Port Port Number 0...2
/// \param Status Reference returns status of handshake lines
/// \return error code, 0= no error

int ccTalkCCT910Device::GetSerialLines(int Port, int &Status)
{
    int buflen= 1;

    Status= 0;

    return m_ccTalk->RequestBinDataEx(m_DeviceAdr, CCT_CCT910_GET_SERIAL_LINES, &Port, 1, &Status, buflen);
}

//////////////////////////////////////////////////////////////////////
/// Stets serial handshake lines
/// \param Port Port Number 0...2
/// \param Status status to set
/// \return error code, 0= no error

int ccTalkCCT910Device::SetSerialLines(int Port, int Status)
{
    unsigned char buf[2];

    buf[0]= Port;
    buf[1]= Status;

    return m_ccTalk->SendData(m_DeviceAdr, CCT_CCT910_SET_SERIAL_LINES, 2, buf);
}

//////////////////////////////////////////////////////////////////////
/// Returns features of CCT910 device depending on firmware version
/// \param Serial Reference return features of serial ports
/// \param Parallel Reference return features of parallel ports
/// \param Extended Reference return features of extended functions
/// \param Staus Reference return status of handshake lines
/// \return error code, 0= no error

int ccTalkCCT910Device::RequestFeatures(int &Serial, int &Parallel, int &Extended)
{
    int err= 0;
    int buflen= 3;
    unsigned char buf[3];

    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_CCT910_REQUEST_FEATURES, buflen, buf);
    Serial= buf[0];
    Parallel= buf[1];
    Extended= buf[2];

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Returns usages of CCT910 device IO-Ports depending on firmware version
/// \param Config Reference returns configuration mode
/// \return error code, 0= no error

int ccTalkCCT910Device::RequestIOPortUsage(int &Config)
{
    int err= 0;
    int buflen= 1;
    unsigned char buf;

    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_CCT910_REQUEST_FEATURES, buflen, &buf);
    Config= buf;

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Sets LED flashing mode
/// \param Led Led 0...7
/// \param Period flashing period in 50 ms steps
/// \return error code, 0= no error

int ccTalkCCT910Device::SetLEDFlashing(int Led, int Period)
{
   unsigned char buf[2];

    buf[0]= Led;
    buf[1]= Period;

    return m_ccTalk->SendData(m_DeviceAdr, CCT_CCT910_SET_LED_FLASHING, 2, buf);
}

//////////////////////////////////////////////////////////////////////
/// Sets alls LEDs via mask byte
/// \param Mask Mask byte to set LED 0...255
/// \return error code, 0= no error

int ccTalkCCT910Device::SetAllLEDs(int Mask)
{
   unsigned char buf;

    buf= Mask & 0xff;

    return m_ccTalk->SendData(m_DeviceAdr, CCT_CCT910_SET_ALL_LED, 1, &buf);
}

//////////////////////////////////////////////////////////////////////
/// Sets single  LED via index and mode
/// \param LED Index of LED 0...7
/// \param Mode Mode indexed [off, on, toggle]
/// \return error code, 0= no error

int ccTalkCCT910Device::SetSingleLED(int Led, int Mode)
{
   unsigned char buf[2];

    buf[0]= Led;
    buf[1]= Mode;

    return m_ccTalk->SendData(m_DeviceAdr, CCT_CCT910_SET_SINGLE_LED, 2, buf);
}

//////////////////////////////////////////////////////////////////////
/// Return state of DIP switches
/// \param Switch1 reference returns state of switch bank 1
/// \param Switch2 reserved returns 0x00
/// \return error code, 0= no error

int ccTalkCCT910Device::RequestSwitchState(int &Switch1, int &Switch2)
{
    int err= 0;
    int buflen= 2;
    unsigned char buf[2];

    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_CCT910_REQUEST_SWITCH_STATE, buflen, buf);
    Switch1= buf[0];
    Switch2= buf[1];

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Configure IO Ports
/// \param *Port Pointer to eight byte port configuration list
/// \param Len Length of configuration list in byte. Higher port list values are set to zero.
/// \return error code, 0= no error

int ccTalkCCT910Device::ConfigureIOPort(char *Port, int Len)
{
    unsigned char buf[CCT910_CONF_IO_PORTS];

    Len= min(Len, CCT910_CONF_IO_PORTS);
    memset(buf, 0, CCT910_CONF_IO_PORTS);
    memcpy(buf, Port, Len);

    return m_ccTalk->SendData(m_DeviceAdr, CCT_CCT910_CONFIGURE_IO_PORT, CCT910_CONF_IO_PORTS, buf);
}

//////////////////////////////////////////////////////////////////////
/// Return Input Port
/// \param Port Reference to returned value
/// \return error code, 0= no error

int ccTalkCCT910Device::ReadIOPort(int &Port)
{
    int err= 0;
    int buflen= 1;
    unsigned char buf= 0;

    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_CCT910_READ_IO_PORT, buflen, &buf);
    Port= buf;

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Write output Port
/// \param Port Port Value to set
/// \return error code, 0= no error

int ccTalkCCT910Device:: WriteIOPort(int Port)
{
    unsigned char buf= Port;

    return m_ccTalk->SendData(m_DeviceAdr, CCT_CCT910_WRITE_IO_PORT, 1, &buf);
}



// End ccTalkCCT910Device.cpp
////////////////////////////////////////////////////////////////////////////////
