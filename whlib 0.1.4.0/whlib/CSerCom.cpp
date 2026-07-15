/* 
 * File:   CSerCom.cpp
 * Author: Manfred Wollny
 * 
 * Created on 3. November 2009, 12:11
 */

#include "CSerCom.h"
#include "CTimeOut.h"

////////////////////////////////////////////////////////////////////////////////
// class CSerCom

CSerCom::CSerCom()
{
    m_PortFd = -1;
    m_RxTout= RX_TOUT;
}

CSerCom::CSerCom(CSerCom& orig)
{
    m_PortFd = orig.m_PortFd;
    m_RxTout= RX_TOUT;
}

CSerCom::~CSerCom()
{
    Close();
}
////////////////////////////////////////////////////////////////////////
/// Open termio library functions with private settings.
/// \param none
/// \return 0= ok or error code

int CSerCom::OpenTermio()
{
    int error = 0;

    m_PortFd = open(m_PortName, O_RDWR | O_NOCTTY | O_NONBLOCK); //set the user console port up

    if (m_PortFd >= 0)
    {
        // save old paramter
        tcgetattr(m_PortFd, &m_OldTio); // save current port settings   //so commands are interpreted right for this program
        // set new port parameter
        memset(&m_ActTio, 0, sizeof (struct termios));
        m_ActTio.c_cflag = m_Baud | m_DBits | m_SBits | m_ParityOn | m_ParityMode | CLOCAL | CREAD;
        m_ActTio.c_iflag = IGNPAR;
        m_ActTio.c_oflag = 0;
        m_ActTio.c_lflag = 0; //ICANON;
        m_ActTio.c_cc[VMIN] = 1;
        m_ActTio.c_cc[VTIME] = 0;
        tcflush(m_PortFd, TCIOFLUSH);
        tcsetattr(m_PortFd, TCSANOW, &m_ActTio);

        error = 0;
    } else
        error = SetError(SIO_OPEN_ERR);

    return error;
}

////////////////////////////////////////////////////////////////////////
/// Open serial port with given parameter
/// \param PortName device name as ascii char string
/// \param Baud baudrate as long integer, if the value don't fit default is 9600 Baud
/// \param DBits data bits 5...9, if the value don't fit default is 8 data bits
/// \param SBits stop bist 1,2, if the value don't fit default is 2 stop bits
/// \param Parity default is none
///  - 0= none
///  - 1= odd
///  - 2= even
/// \param Mode not used, free for futher options
/// \return 0= ok or error code

int CSerCom::Open(const char * PortName, long Baud, int DBits, int SBits, int Parity, int Mode)
{
    int error = 0;

    if (IsOpen())
        Close();

    Mode= 0;                                // not used
    memset(m_PortName, 0, sizeof (m_PortName));

    if (isdigit(PortName[0]))
    {
        int portnr;

        sscanf(PortName, "%d", &portnr);
        sprintf(m_PortName, "//dev//ttyS%d", portnr);
    } else
        strncpy(m_PortName, PortName, PORT_NAME_LEN);

    // Convert Parameter to termio
    switch (Baud)
    {
        case 230400:
            m_Baud = B230400;
        case 115200:
            m_Baud = B115200;
        case 57600:
            m_Baud = B57600;
        case 38400:
            m_Baud = B38400;
            break;
        case 19200:
            m_Baud = B19200;
            break;
        case 9600:
            m_Baud = B9600;
            break;
        case 4800:
            m_Baud = B4800;
            break;
        case 2400:
            m_Baud = B2400;
            break;
        case 1800:
            m_Baud = B1800;
            break;
        case 1200:
            m_Baud = B1200;
            break;
        case 600:
            m_Baud = B600;
            break;
        case 300:
            m_Baud = B300;
            break;
        case 200:
            m_Baud = B200;
            break;
        case 150:
            m_Baud = B150;
            break;
        case 134:
            m_Baud = B134;
            break;
        case 110:
            m_Baud = B110;
            break;
        case 75:
            m_Baud = B75;
            break;
        case 50:
            m_Baud = B50;
            break;
        default:
            m_Baud = B9600;
            break;
    }

    switch (DBits)
    {
        case 8:
        default:
            m_DBits = CS8;
            break;
        case 7:
            m_DBits = CS7;
            break;
        case 6:
            m_DBits = CS6;
            break;
        case 5:
            m_DBits = CS5;
            break;
    }

    switch (SBits)
    {
        case 1:
            m_SBits = 0;
            break;
        default:
        case 2:
            m_SBits = CSTOPB;
            break;
    }

    switch (Parity)
    {
        case 0:
        default: //none
            m_ParityOn = 0;
            m_ParityMode = 0;
            break;
        case 1: //odd
            m_ParityOn = PARENB;
            m_ParityMode = PARODD;
            break;
        case 2: //even
            m_ParityOn = PARENB;
            m_ParityMode = 0;
            break;
    } //end of switch parity


    error = OpenTermio();

    return error;
}

////////////////////////////////////////////////////////////////////////
/// Close the serial port
/// \param none
/// \return none

void CSerCom::Close()
{
    if (m_PortFd != -1)
    {
        tcsetattr(m_PortFd, TCSANOW, &m_OldTio);
        close(m_PortFd);
        m_PortFd = -1;
    }
}

////////////////////////////////////////////////////////////////////////
/// Send one byte
/// \param Byte byte to send
/// \return 0= ok or error code

int CSerCom::SendByte(unsigned char Byte)
{
    int error = 0;
    if (IsOpen())
    {
        if (write(m_PortFd, &Byte, 1) != 1)
            error = SetError(SIO_SEND_BYTE_ERR);
    } else
    {
        error = SetError(SIO_NOT_OPEN_ERR);
    }


    return error;
}

////////////////////////////////////////////////////////////////////////
/// Receive one Byte
/// \param none
/// \return received byte in LSB or <0 as error code

int CSerCom::ReceiveByte(void)
{
    int rec = 0;
    int ret = 0;
    CTimeOut tout;

    if (IsOpen())
    {
    	tout.SetMilliSec(m_RxTout);

        do
        {
            ret = read(m_PortFd, &rec, 1);
        }
        while(ret != 1 && !tout.Elapse());

        if (ret != 1)
            rec = SetError(SIO_RECEIVE_BYTE_ERR);
        else
            rec&= 0xff;
    }
    else
    {
        rec = SetError(SIO_NOT_OPEN_ERR);
    }

    return rec;
}

////////////////////////////////////////////////////////////////////////
/// Receive one Byte to buffer pointer
/// \param pRec pointer to receive buffer.
/// \return 0= ok or error code

int CSerCom::ReceiveByte(void *pRec)
{
    int ret = 0;
    CTimeOut tout;

    if (IsOpen())
    {
    	tout.SetMilliSec(m_RxTout);

        do
        {
           ret = read(m_PortFd, pRec, 1);
        }
        while(ret != 1 && !tout.Elapse());

        if(ret != 1)
           ret = SetError(SIO_RECEIVE_BYTE_ERR);
    }
    else
    {
        ret = SetError(SIO_NOT_OPEN_ERR);
    }

    return ret;
}


////////////////////////////////////////////////////////////////////////
/// Sends data from buffer
/// \param Buffer pointer to receive buffer
/// \param Len size of buffer in bytes
/// \return 0= OK, <0 error code

#define NO_GLOBAL_ERROR 0

int CSerCom::SendBuffer(unsigned char * Buffer, int Len)
{
    int err = NO_GLOBAL_ERROR;
    int written = 0;
    int rep = 10000;
    int rest = Len;

    if (!IsOpen())
        return SetError(SIO_NOT_OPEN_ERR);

    if (Len <= 0 || !Buffer)
        return SetError(SIO_SENDBUFFER_PAR_ERR);

    do
    {
        written = write(m_PortFd, Buffer, rest);
        rest = Len - written;
        if(rest > 0)
            Buffer+= written;
        //		if(rest > 0)
        //			timer.DelayMilliSec(100);
    } while (rest > 0 && rep--);


    if (rest > 0)
        err = SetError(SIO_SENDBUFFER_ERR);

    return err;
}


////////////////////////////////////////////////////////////////////////
/// Receives data into byte buffer
/// \param Buffer pointer to receive buffer
/// \param Len size of buffer in bytes
/// \return Count of received bytes or 0: no data, <0 error

int CSerCom::ReceiveBuffer(unsigned char * Buffer, int MaxLen)
{
    int ch;
    int cnt;

    for (cnt = 0; cnt < MaxLen; cnt++)
    {
        ch = ReceiveByte();
        if (ch < 0)
            break;
        else
            Buffer[cnt] = ch & 0xff;
    }
    return cnt;
}

////////////////////////////////////////////////////////////////////////
/// Flush all data from ports receive buffer
/// \param none
/// \return 0= OK, <0 error code

int CSerCom::FlushReceiveBuffer(void)
{
    if (!IsOpen())
        return SetError(SIO_NOT_OPEN_ERR);

    tcflush(m_PortFd, TCIFLUSH);

    return 0;
}

