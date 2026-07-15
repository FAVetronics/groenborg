////////////////////////////////////////////////////////////////////////////////
// File:   ccTalkCCT910Device.h
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
/// \file Source file class ccTalkCCT910Device
////////////////////////////////////////////////////////////////////////////////

#ifndef _CCTALKCCT910DEVICE_H
#define	_CCTALKCCT910DEVICE_H

#include "ccTalkCCT900Device.h"
#include "cct910globals.h"

#define CCT910_CONF_IO_PORTS 8

/// Device class for the CCT910 hub / dongle

class ccTalkCCT910Device : public ccTalkCCT900Device
{
public:
    ccTalkCCT910Device();
    virtual ~ccTalkCCT910Device();

/// Modes index in SetSingleLED()
    enum {
	CCT910_LED_off,
	CCT910_LED_on,
	CCT910_LED_toggle
    };


/// I/O Port configuration index in ConfigureIOPort()
    enum {
        CCT910_IO_CONF_INP_NORMAL,
        CCT910_IO_CONF_INP_PULL_UP,
        CCT910_IO_CONF_RES_2,
        CCT910_IO_CONF_RES_3,
        CCT910_IO_CONF_RES_4,
        CCT910_IO_CONF_RES_5,
        CCT910_IO_CONF_RES_6,
        CCT910_IO_CONF_RES_7,
        CCT910_IO_CONF_OUT_NORMAL,
        CCT910_IO_CONF_OUT_OPEN_COLLECTOR,
        CCT910_IO_CONF_RES_10        
    };

/// Baudrate index of serial port in SetupSerialPort()
    enum {
        CCT910_BAUD_1200,
        CCT910_BAUD_2400,
        CCT910_BAUD_4800,
        CCT910_BAUD_9600,
        CCT910_BAUD_19200,
        CCT910_BAUD_38400,
        CCT910_BAUD_57600,
        CCT910_BAUD_115200
    };

/// Data bits index of serial port in SetupSerialPort()
    enum {
        CCT910_DBIT_5,
        CCT910_DBIT_6,
        CCT910_DBIT_7,
        CCT910_DBIT_8
    };

/// Parity index of serial port in SetupSerialPort()
    enum {
        CCT910_PARITY_NONE,
        CCT910_PARITY_EVEN,
        CCT910_PARITY_ODD
    };    

/// @name Serial Ports
//@{    
    int SetupSerialPort(int Port, int Baud= CCT910_BAUD_9600, int DBits= CCT910_DBIT_8, int Parity= CCT910_PARITY_NONE);
    int SerialCommunication(unsigned char *TxBuffer, int TxLen, unsigned char *RxBuffer, int RxLen);
    int SerialCommunication(int Port, int Timeout, int LenIndex, unsigned char *TxBuffer, int TxLen, unsigned char *RxBuffer, unsigned char &RxLen, int  &Status);
    int SerialSendBreak(void);
    int GetSerialLines(int Port, int &Status);
    int SetSerialLines(int Port, int Status);

/// @name LED Commands
//@{
    int SetLEDFlashing(int Led, int Periode);
    int SetSingleLED(int Led, int Mode);
    int SetAllLEDs(int Mask);

/// @name I/O Ports Commands
//@{
    int RequestSwitchState(int &Switch1, int &Switch2);
    int ConfigureIOPort(char *Port, int Len);
    int ReadIOPort(int &Port);
    int WriteIOPort(int Port);

/// @name Configuration
//@{
    int RequestFeatures(int &Serial, int &Parallel, int &Extended);
    int RequestIOPortUsage(int &Config);

private:

};

#endif	/* _CCTALKCCT910DEVICE_H */

// End ccTalkCCT910Device.h
////////////////////////////////////////////////////////////////////////////////

