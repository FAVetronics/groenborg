/* 
 * File:   CSerCom.h
 * Author: manfred
 *
 * Created on 3. November 2009, 12:11
 */

#include <termios.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <ctype.h>
#include <unistd.h>
#include <fcntl.h>
#include <sys/signal.h>
#include <sys/types.h>
#include "whglobals.h"
#ifndef _CSERCOM_H
#define	_CSERCOM_H

#define PORT_NAME_LEN 256
// #define RX_TOUT 100 // changed for MDB via CCT 900
#define RX_TOUT 200

/* transfered to whglobals.h
enum SERCOM_ERROR {
	NO_GLOBAL_ERROR,

        // serial I/O
	SIO_ERROR,
	SIO_OPEN_ERR,
	SIO_READTOUT_ERR,
	SIO_NOT_OPEN_ERR,
        SIO_SEND_BYTE_ERR,
	SIO_SENDBUFFER_ERR,
	SIO_SENDBUFFER_PAR_ERR,
        SIO_RECEIVE_BYTE_ERR,
        SIO_RECEIVE_BUFFER_ERR
};

#define SetError(e) -e
*/

/// Base class for serial communication ports.
////////////////////////////////////////////////////////////////////////////////
/// This class use the termio.h library functions. It is tested on serial standard
/// ports. Also on ftdi (http://www.ftdichip.com/) based USB to serial adaptors
/// as wh products, CCT 110, CCT 900, CCT 910, EMP 800.14 (USB coin acceptor).
/// To use the derivated classes of the whLib you may port this class to your
/// specifiy application.

class CSerCom {
public:
    CSerCom();
    CSerCom(CSerCom& orig);
    virtual ~CSerCom();

    /// @name Intitalize  Functions
    //@{
    int Open(const char * PortName, long Baud= 9600, int DBits= 8, int SBits= 2, int Parity= 0, int Mode= 0);
    void Close();
    bool IsOpen() {return m_PortFd != -1 ? true : false; };
    //@}

    /// @name Byte I/O Functions
    //@{
    int SendByte(unsigned char Byte);
    int ReceiveByte(void);
    int ReceiveByte(void *pRec);
    //@}

    /// @name Buffered I/O Functions
    //@{
    int SendBuffer(unsigned char * Buffer, int Len);
    int ReceiveBuffer(unsigned char * Buffer, int MaxLen);
    int FlushReceiveBuffer(void);
    //@}

    /// @name privates
    //@{
private:
    int OpenTermio();
   // local vars
    char m_PortName[PORT_NAME_LEN+1];
    long m_Baud;
    long m_DBits;
    long m_SBits;
    long m_ParityOn;
    long m_ParityMode;
    long m_Mode;
    long m_PortFd;
    int m_RxTout;
    struct termios m_OldTio, m_ActTio;
    //@}
};

#endif	/* _CSERCOM_H */

