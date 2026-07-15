/// \file Source file class ccTalk communication class
/* 
 * File:   CccTalk.cpp
 * Author: manfred
 * 
 * Created on 4. November 2009, 10:51
 *
 * History:
 * 02.10.2014   extendet CheckSum calculation (mw)
 */


#include <stdlib.h>
#include <string.h>
#include <memory.h>
#include <stdio.h>
#include <ctype.h>
#include <math.h>
#include "CTimeOut.h"
#include "CccTalk.h"

//////////////////////////////////////////////////////////////////////

#define DEF_PORTNAME "/dev/ttyS%d"

//////////////////////////////////////////////////////////////////////
// Konstruktion/Destruktion
//////////////////////////////////////////////////////////////////////

CccTalk::CccTalk()
{
    m_TxBuffer = NULL;
    m_RxBuffer = NULL;
    m_RxFrameReady = false;
    m_Mode = MODE_DEFAULT;
    m_Abbort= false;
    m_CsumType= CCT_CSUM_SIMPLE;
}

CccTalk::~CccTalk()
{
    Close();
}

//////////////////////////////////////////////////////////////////////
/// Open standard serial device /dev/ttySx Port for ccTalk bus communication.
/// \param Port Portnummer will be formated to" /dev/ttyS%d" internaly
/// \param MasterAdr Adress of ccTalk master, default is 0
/// \param BaudRate Baudrate on the Bus, default is 9600
/// \return error code, 0= no error
int CccTalk::Open(int Port, int MasterAdr, int BaudRate)
{
    char portname[30];

    if (IsOpen())
        Close();
    sprintf(portname, DEF_PORTNAME, Port);

    return Open(portname, MasterAdr, BaudRate);
}

//////////////////////////////////////////////////////////////////////
/// Open serial device for ccTalk bus communication.
/// \param PortName Port name, for example "/dev/ttyUSB0"
/// \param MasterAdr Adress of ccTalk master, default is 0
/// \param BaudRate Baudrate on the bus, default is 9600
/// \return error code, 0= no error

int CccTalk::Open(const char* PortName, int MasterAdr, int BaudRate)
{
    int err;

    if (IsOpen())
        Close();


    //	err= CSerCom::Open(Port, CBR_9600, 8, NOPARITY, ONESTOPBIT);
    err = CSerCom::Open(PortName, BaudRate, 8, 1, 0);
    if (!err)
    {
        m_TxBuffer = new unsigned char[CCT_BUF_LEN];
        m_RxBuffer = new unsigned char[CCT_BUF_LEN];
        m_MasterAddress = MasterAdr;
        //		SetRxTimeout(20);
        ClearRxBuffer();
        if(!m_TxBuffer || !m_RxBuffer)
        {
            Close();
            err= SetError(SIO_OPEN_ERR);
        }
    }
    return err;
}

//////////////////////////////////////////////////////////////////////
/// Sends binary data with out waiting on response
/// \param Addr Device address
/// \param Len Length of data bytes
/// \param Header ccTalk header / command number
/// \param Data Pointer to transfer data
/// \return error code, 0= no error

int CccTalk::Send(int Addr, int Len, int Header, void *Data)
{
    int p, i;
    int err = CCT_OK;
    int echo;

    if(!IsOpen())
        return SetError(SIO_NOT_OPEN_ERR);

    p = 0;
    m_TxBuffer[p++] = Addr;
    m_TxBuffer[p++] = Len;
    m_TxBuffer[p++] = m_MasterAddress;
    m_TxBuffer[p++] = Header;
    memcpy(&m_TxBuffer[p], (unsigned char*)Data, Len);
    SetCsum(m_TxBuffer);
    m_TxLen = Len + 5; // 4 + Len + 1

    if (!(m_Mode & MODE_TWO_WIRE))
        FlushReceiveBuffer();
    SendBuffer(m_TxBuffer, m_TxLen);

    if (!(m_Mode & MODE_TWO_WIRE))
    {
        for (i = 0; i < m_TxLen; i++) // Echo abholenb
        {
            echo = ReceiveByte();
            if (echo < 0) // kein Zeichen empfangen
                err |= CCT_ERR_TX_TOUT;
            else
                if (echo != m_TxBuffer[i])
                    err |= CCT_ERR_TX_ECHO;
        }
    }

    return err;
}


//////////////////////////////////////////////////////////////////////
/// Close serial port
/// \param none
/// \return none
void CccTalk::Close()
{
    CSerCom::Close();

    if (m_TxBuffer)
    {
        delete m_TxBuffer;
        m_TxBuffer = NULL;
    }
    if (m_RxBuffer)
    {
        delete m_RxBuffer;
        m_RxBuffer = NULL;
    }
}

//////////////////////////////////////////////////////////////////////
/// Received data ready
/// \param none
/// \return true or false
bool CccTalk::IsRxFrameReady()
{
    RxPolling();

    return m_RxFrameReady;
}

//////////////////////////////////////////////////////////////////////
/// Receive / request data from
/// \param Addr Device address
/// \param Len Receive buffer length
/// \param Header ccTalk header / command number from data request
/// \param Data Pointer to transfer data
/// \return data ready true or false, Len return received data length
int CccTalk::Receive(int &Adr, int &Len, int &Header, void *Data, bool Wait)
{
    int p, i, rdy = 0;
    CTimeOut tout;

    if(Wait)
    {
        tout.SetMilliSec(100);

        while (!IsRxFrameReady())
        {
            if (tout.Elapse())
            {
                m_Status |= CCT_ERR_RX_TOUT;

                ClearRxBuffer();
                break;
            }
        }
    }

    if (IsRxFrameReady())
    {
        p = 0;
        p++;
        Len = (Len < m_RxBuffer[p]) ? Len : m_RxBuffer[p]; // Truncate Daten to size Len
        p++;
        Adr = m_RxBuffer[p++];
        Header = m_RxBuffer[p++];

        for (i = 0; i < Len; i++)                          // copy data
        {
            ((unsigned char*) Data)[i] = m_RxBuffer[p++];
        }
        rdy = true;

        ClearRxBuffer();
    }

    return rdy;
}

//////////////////////////////////////////////////////////////////////
/// Poll serial port for received date
/// \param none
/// \return receive status
int CccTalk::RxPolling()
{
    int rxch;

    if (IsOpen() && !m_RxFrameReady && !m_Status)
    {
        rxch = ReceiveByte();

        if (rxch >= 0) // Zeichen empfangen
        {
            if (m_RxFrameReady)
            {
                m_Status |= CCT_ERR_RX_OF;
            }
            else
            {
                if (m_RxPos < m_RxLen)
                {
                    m_RxBuffer[m_RxPos++] = rxch;
                    if (m_RxPos == 2) // Daten L�nge
                        m_RxLen += rxch;

                    if (m_RxPos == m_RxLen) // Frame Ende erreicht
                    {
                        if(ChkCsum(m_RxBuffer))
                        {
                            m_Status |= CCT_ERR_RX_CSUM;
                        }
                        else
                        {
                            if (m_RxBuffer[0] == m_MasterAddress)
                                 m_RxFrameReady = true;
                            else
                                ClearRxBuffer();
                        }
                    }
                }
                else
                {
                    m_Status |= CCT_ERR_RX_OF;
                }
            }
        }
    }
    return m_Status;
}

//////////////////////////////////////////////////////////////////////
/// Clear data frame buffer, reset all pointer and counter
/// \param none
/// \return none
void CccTalk::ClearRxBuffer()
{
    FlushReceiveBuffer();
    m_RxFrameReady = false;
    m_RxLen = 5;
    m_RxPos = 0;
    m_RxCsum = 0;
    m_RxCnt = 0;
    m_Status = 0;
}

//////////////////////////////////////////////////////////////////////
/// Get transmit status
/// \param Clear, true= reset actual transmit status
/// \return transmit status
int CccTalk::GetStatus(bool Clear)
{
    int status;

    status = m_Status;

    if (Clear)
        m_Status = 0;

    return status;
}

//////////////////////////////////////////////////////////////////////
/// Request data from device in ASCII format to string class
/// \param Addr Device address
/// \param Header ccTalk Header / Command number
/// \param Reference to string class
/// \return error code, 0= no error
int CccTalk::RequestASCIIData(int Addr, int Command, string &Str)
{
    char *buf;
    int err, len, hdr;

    Str.assign((size_t) CCT_BUF_LEN, 0);    // empty buffer
    buf = (char*) Str.data();
    // Commando senden
    err = Send(Addr, 0, Command, buf);

    len = CCT_BUF_LEN;
    if (!Receive(Addr, len, hdr, buf))
        err |= CCT_ERR_RX_TOUT;
    
    if(!err && len > 0)
    {
        len= min(len, CCT_BUF_LEN-1);
        Str= Str.substr(0, len);
    }
    else
        Str.clear();

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Request data from device in ASCII format to char buffer
/// \param Addr Device address
/// \param Header ccTalk Header / Command number
/// \param Pointer to char buffer
/// \param Len max length in byte of the char buffer
/// \return error code, 0= no error
int CccTalk::RequestASCIIData(int Addr, int Command, char *Buf, int Len)
{
    int err, hdr;

    err = Send(Addr, 0, Command, Buf);

    if (!Receive(Addr, Len, hdr, Buf))      // receive direct into Buffer
        err |= CCT_ERR_RX_TOUT;
    Buf[Len] = 0;

    return err;
}


//////////////////////////////////////////////////////////////////////
/// Poll devices adresses and store answer in a address list
/// \param Pointer to address list
/// \return Number of found devices
int CccTalk::AddressPoll(int *PollTbl)
{
    int cnt;
    CTimeOut tout;
    int rxch;

    Send(0, 0, CCT_ADDRESS_POLL, NULL);
    tout.SetMilliSec(1500);

    cnt = 0;
    while (!tout.Elapse())
    {
        if (m_Abbort)
        {
            m_Abbort = false;
            return 0;
        }
        rxch = ReceiveByte();
        if (rxch >= 0)
            PollTbl[cnt++] = rxch & 0xff;
        if (cnt >= CCT_ADR_POLL_COUNT)      // max 256 addresses
            break;
    }
    return cnt;
}

//////////////////////////////////////////////////////////////////////
/// Request binary data from device
/// \param Addr Device address
/// \param Header ccTalk Header / Command number
/// \param Cmds pointer to subcommands or data to send
/// \param CmdLen length of subcommands or data in bytes
/// \param Data pointer to received data 
/// \param DataLen reference to data buffer length - returns count of received data bytes
/// \return error code, 0= no error

int CccTalk::RequestBinDataEx(int Addr, int Header, void *Cmds, int CmdLen, void *Data, int &DataLen)
{
    int err, hdr;

    err = Send(Addr, CmdLen, Header, Cmds); // Erweitertes Commando senden

    // Len = CCT_BUF_LEN; // Daten empfangen und zu CString
    if (!Receive(Addr, DataLen, hdr, Data))
        err |= CCT_ERR_RX_TOUT;

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Sends binary data waiting on reponse
/// \param Addr Device address
/// \param Header ccTalk header / command number
/// \param Len Length of data bytes
/// \param Data pointer to transfer data
/// \return error code, 0= no error

int CccTalk::SendData(int Addr, int Header, int Len, void *Data)
{
    int hdr, err;
    unsigned char buf[CCT_BUF_LEN];

    err = Send(Addr, Len, Header, Data);

    memset(buf, 0, CCT_BUF_LEN); // zum debugen
    Len = CCT_BUF_LEN;
    if (!Receive(Addr, Len, hdr, buf))
        err |= CCT_ERR_RX_TOUT;

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Sends single commmand without data, waiting onm reponse
/// \param Addr Device address
/// \param Header ccTalk Header / Command number
/// \return error code, 0= no error

int CccTalk::SendCommand(int Addr, int Header)
{
    int hdr, err, len;
    unsigned char buf[CCT_BUF_LEN];

    err = Send(Addr, 0, Header, NULL);

    len = CCT_BUF_LEN;
    if (!Receive(Addr, len, hdr, buf))
        err |= CCT_ERR_RX_TOUT;

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Requests binary data on single command, no data send
/// Extended version with send data is RequestBinDataEx()
/// \param Addr Device address
/// \param Header ccTalk header / command number
/// \param Len Reference to Bufferlen, return received data counter
/// \param Data pointer to receive buffer
/// \return error code, 0= no error

int CccTalk::RequestBinData(int Addr, int Header, int &Len, void *Data)
{
    return RequestBinDataEx(Addr, Header, NULL, 0, Data, Len);
}

//////////////////////////////////////////////////////////////////////
/// Core Command return string of manufactory ID
/// \param Addr Device address
/// \param ManId  Reference to string class
/// \return error code, 0= no error

int CccTalk::RequestManufacturerID(int Addr, string &ManId)
{
    return RequestASCIIData(Addr, CCT_REQUEST_MANUFACTURER_ID, ManId);
}

//////////////////////////////////////////////////////////////////////
/// Sends binary data without waiting on reponse.
/// For debug only, can simulate address and check sum errors
/// \param Addr Device address
/// \param Len Length of data bytes
/// \param Header ccTalk Header / Command number
/// \param Data Pointer to transfer data
/// \param Master Master address
/// \param Csum Check Sum
/// \return error code, 0= no error

int CccTalk::SendEx(int Addr, int Len, int Header, void *Data, int Master, int Csum)
{
    int p, i;
    int err = CCT_OK;
    int echo;

    p = 0;
    m_TxBuffer[p++] = Addr;
    m_TxBuffer[p++] = Len;
    m_TxBuffer[p++] = Master;
    m_TxBuffer[p++] = Header;

    for (i = 0; i < Len; i++) // Daten
    {
        m_TxBuffer[p++] = ((unsigned char*) Data)[i];
    }

    m_TxBuffer[p++] = Csum; // Csum

    m_TxLen = Len + 5; // 4 + Len + 1


    if (!(m_Mode & MODE_TWO_WIRE))
        FlushReceiveBuffer();

    SendBuffer(m_TxBuffer, m_TxLen);
    if (!(m_Mode & MODE_TWO_WIRE))
    {

        for (i = 0; i < m_TxLen; i++) // Echo abholenb
        {
            echo = ReceiveByte();
            if (echo < 0) // kein Zeichen empfangen
                err |= CCT_ERR_TX_TOUT;
            else
                if (echo != m_TxBuffer[i])
                err |= CCT_ERR_TX_ECHO;
        }
    }

    return err;
}


void CccTalk::AddToCsum(int &Csum, int Data)
{
    switch (m_CsumType)
    {
	case CCT_CSUM_SIMPLE:
	{
            Csum+= Data;
            Csum&= 0xff;
            break;
	}
	break;

	case CCT_CSUM_CRC16:
	{
            Csum ^= (Data << 8);
            for (int j = 0; j < 8; j++)
            {
                if ((Csum & 0x8000) != 0)
                    Csum = (Csum << 1) ^ 0x1021;
		else
                    Csum <<= 1;
		}
	}
	break;
    }
}

int CccTalk::CalcCsum(int Addr, int Len, int Header, void *Data, int Master)
{
	int i;
	int csum;

	switch (m_CsumType)
	{
	case CCT_CSUM_SIMPLE:
		{
			csum= 0;								// Header
/*			csum+= Addr;
			csum&= 0xff;
			csum+= Len;
			csum&= 0xff;
			csum+= Master;
			csum&= 0xff;
			csum+= Header;
			csum&= 0xff;
*/
			AddToCsum(csum, Addr);
			AddToCsum(csum, Len);
			AddToCsum(csum, Master);
			AddToCsum(csum, Header);

			for(i= 0; i < Len; i++)				// Daten
			{
				AddToCsum(csum, ((unsigned char*)Data)[i]);

///				csum+= ((unsigned char*)Data)[i];
///				csum&= 0xff;
			}
			csum= -csum & 0xff;					// Csum
			break;
		}

	case CCT_CSUM_CRC16:
		{
			AddToCsum(csum, Addr);
			AddToCsum(csum, Len);
			AddToCsum(csum, Header);

            for (int i = 0; i < Len; i++)
			{
				AddToCsum(csum, ((unsigned char*)Data)[i]);
			}
			csum &= 0x0000ffff;
		}
	}
	return csum;
}

//////////////////////////////////////////////////////////////////////
/// Set checksum in ccTalk (Send) Buffer
/// \param Buffer Pointer to ccTalk Buffer
/// \return none
void CccTalk::SetCsum(unsigned char *Buffer)
{
    int csum = 0;
    int i, j, len;

    len= Buffer[FRM_LEN] + FRM_DAT;

    switch(m_CsumType)
    {
	case CCT_CSUM_SIMPLE:
            for(i = 0; i < len; i++)
            {
		csum+= Buffer[i];
		csum&= 0xff;
            }
            Buffer[i]= 256 - (csum & 0xff);
            break;

	case CCT_CSUM_CRC16:
            for (i = 0; i < len; i++)
            {
		if (i != FRM_SRC)					// source adr wird ausgebelendet und wird mit LSB der csum �berladen
		{
                    csum ^= Buffer[i] << 8;
                    for (j = 0; j < 8; j++)
                    {
			if ((csum & 0x8000) != 0)
                            csum = (csum << 1) ^ 0x1021;
			else
                            csum <<= 1;
                    }
		}
            }

//		csum&= 0xffff;
            Buffer[FRM_SRC]= csum & 0xff;			// LSB der CheckSum
            Buffer[i]= (csum >> 8) & 0xff;					// MSB der Checksum

            break;
    }
}

//////////////////////////////////////////////////////////////////////
/// Check checksum from ccTalk buffer
/// \param Buffer Pointer to the (Receive) Buffer
/// \return 0= OK, > 0 error
int CccTalk::ChkCsum(unsigned char *Buffer)
{
    int csum = 0;
    int bsum;
    int i, j, len;
    int err= 0;

    len= Buffer[FRM_LEN] + FRM_DAT;

    switch(m_CsumType)
    {
	case CCT_CSUM_SIMPLE:
            for(i = 0; i < len; i++)
            {
		csum+= Buffer[i];
		csum&= 0xff;
            }
            if(Buffer[i] != (256 - (csum & 0xff)))
            	err= 1;
            break;

	case CCT_CSUM_CRC16:

            for (i = 0; i < len; i++)
            {
		if (i != FRM_SRC)					// source adr wird ausgebelendet und wird mit leb der csum �berladen
		{
                    csum ^= ((int)Buffer[i] << 8);
                    for (j = 0; j < 8; j++)
                    {
			if ((csum & 0x8000) != 0)
                            csum = (csum << 1) ^ 0x1021;
			else
                            csum <<= 1;
                    }
		}
            }
            csum&= 0xffff;
            bsum= ((int)Buffer[i] << 8) + (int)Buffer[FRM_SRC];
            if(bsum != csum)
            	err= 2;

            break;
    }
    return err;
}


//////////////////////////////////////////////////////////////////////
/// Select the active checksum Type
/// \param CRCType checksum
/// \return none
void CccTalk::SetCRCType(enum CCT_CSUM_TYPE CRCType)
{
    if(CRCType < CCT_CSUM_UNDEF && CRCType >= 0)
	m_CsumType= CRCType;
}


//////////////////////////////////////////////////////////////////////
/// Get received and calculated checksum from buffer
/// For debug use only, for test and detect address and check sum errors
/// \param RxCsum Reference to received checksum
/// \param CalcCsum Reference to calculated checksum
/// \return none

void CccTalk::GetReceiveCsum(int &RxCsum, int &CalcCsum)
{
    RxCsum = m_LastRxCsum;
    CalcCsum = m_LastRxCalcCsum;
}


//////////////////////////////////////////////////////////////////////
// Core Commands

//////////////////////////////////////////////////////////////////////
/// Return string of category ID
/// \param Addr Device address
/// \param CatId reference to string class
/// \return error code, 0= no error

int CccTalk::RequestCategoryID(int Addr, string &CatId)
{
    return RequestASCIIData(Addr, CCT_REQUEST_CATEGORY_ID, CatId);
}

//////////////////////////////////////////////////////////////////////
/// Return string of product code
/// \param Addr Device address
/// \param Code  reference to string class
/// \return error code, 0= no error

int CccTalk::RequestProductCode(int Addr, string &Code)
{
    return RequestASCIIData(Addr, CCT_REQUEST_PRODUCT_CODE, Code);
}

//////////////////////////////////////////////////////////////////////
/// Return string of build code
/// \param Addr Device address
/// \param Build  reference to string class
/// \return error code, 0= no error

int CccTalk::RequestBuildCode(int Addr, string &Build)
{
    return RequestASCIIData(Addr, CCT_REQUEST_BUILD_CODE, Build);
}

//////////////////////////////////////////////////////////////////////
/// Returns serialnumber
/// \param Addr Device address
/// \param ManId reference to unsigned long, size is limited to 3 bytes / 24 bit
/// \return error code, 0= no error

int CccTalk::RequestSerialNumber(int Addr, unsigned long &Serial)
{
    int len = 4;

    return RequestBinData(Addr, CCT_REQUEST_SERIAL_NUMBER, len, &Serial);
}

//////////////////////////////////////////////////////////////////////
/// Returns string of software revision
/// \param Addr Device address
/// \param Vers reference to string class
/// \return error code, 0= no error

int CccTalk::RequestSoftwareRevision(int Addr, string &Vers)
{
    return RequestASCIIData(Addr, CCT_REQUEST_SOFTWARE_REVISION, Vers);
}

//////////////////////////////////////////////////////////////////////
/// Returns 3 digits communication revision
/// \param Addr Device address
/// \param Code pointer to unsigned byte bit mask
/// \return error code, 0= no error

int CccTalk::RequestCommsRevision(int Addr, unsigned char *Code)
{
    int len = 3;

    return RequestBinData(Addr, CCT_REQUEST_COMMS_REVISION, len, Code);
}

//////////////////////////////////////////////////////////////////////
/// Returns communication 3 byte error counters
/// \param Addr Device address
/// \param *Status pointer to unsigned byte buffer,
///  Status[0]= rx timeouts
///  Status[1]= rx bytes ingnored
///  Status[2]= rx bad checksums
/// \return error code, 0= no error

int CccTalk::RequestCommsStatus(int Addr, unsigned char *Status)
{
    int len = 3;

    return RequestBinData(Addr, CCT_REQUEST_COMM_STATUS, len, Status);
}

//////////////////////////////////////////////////////////////////////
/// Returns communication error counters
/// \param Addr Device address
/// \param RxTimeOuts reference to receive timeout counter
/// \param BytesIgnored reference to receive bytes ignored counter
/// \param BadChecksums reference to receive bad checksums counter
/// \return error code, 0= no error

int CccTalk::RequestCommsStatus(int Addr, int &RxTimeOuts, int &BytesIgnored, int &BadChecksums)
{
    int len = 3;
    int err= 0;
    int buf[3];

    err= RequestBinData(Addr, CCT_REQUEST_COMM_STATUS, len, buf);
    if(!err)
    {
        RxTimeOuts=   buf[0];
        BytesIgnored= buf[1];
        BadChecksums= buf[2];
    }

    return err;
}


//////////////////////////////////////////////////////////////////////
/// Returns string of communitcation revision
/// \param Addr Device address
/// \param RevStr reference to string class
/// \return error code, 0= no error

int CccTalk::RequestCommsRevision(int Addr, string &RevStr)
{
    int err = 0;
    unsigned char buf[3];
    char frm[100];

    err = RequestCommsRevision(Addr, buf);

    //	Str.Format("%d.%d.%d", (int)(buf[0]), (int)(buf[1]), (int)(buf[2]));
    sprintf(frm, "%d.%d.%d", (int) (buf[0]), (int) (buf[1]), (int) (buf[2]));
    RevStr.assign(frm);

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Clear communication status
/// \param Addr Device address
/// \return error code, 0= no error

int CccTalk::ClearCommsStatus(int Addr)
{
    return SendCommand(Addr, CCT_CLEAR_COMM_STATUS);
}

//////////////////////////////////////////////////////////////////////
/// Returns adress mode
/// \param Addr Device address
/// \param Vers reference to string class
/// \return error code, 0= no error

int CccTalk::RequestAddressMode(int Addr, unsigned char *Mode)
{
    int len = 1;

    return RequestBinData(Addr, CCT_REQUEST_ADDRESS_MODE, len, Mode);
}

#define EMP_ERR_STR_TBL	16

//////////////////////////////////////////////////////////////////////
/// Errorcode to string list

struct EMP_ERROR_CODE_TBL
{
    int Code;
    char Str[32];
} _EmpErrorCodeTbl[EMP_ERR_STR_TBL + 1] = {
    {254, "Reject"},
    { 8, "Folgem�nze"},
    { 19, "M�nzstau1"},
    { 1, "kein Para"},
    { 2, "Multidrop"},
    { 19, "M�nzstau2"},
    { 2, "M�nzsperre"},
    { 19, "M�nzstau3"},
    { 19, "M�nzstau4"},
    { 19, "M�nzstau5"},
    { 1, "Riffel"},
    { 1, "Blei"},
    {255, "TCAP1"},
    { 20, "Fadentrick"},
    { 2, "Sorter"},
    { 13, "EMP busy"},
    {0, "Unknown"}
};

/*
string CccTalk::GetEmpErrorCodeString(int ErrorCode)
{
        int i;

        for(i= 0; i < EMP_ERR_STR_TBL; i++)
                if(_EmpErrorCodeTbl[i].Code == ErrorCode)
                        break;

        return _EmpErrorCodeTbl[i].Str;
}
 */

//////////////////////////////////////////////////////////////////////
/// Reset cctalk device
/// after ResetDevice() mormaly the device give no response to the master
/// \param Addr Device address
/// \return none

int CccTalk::ResetDevice(int Addr)
{
    return SendCommand(Addr, CCT_RESET_DEVIVCE);
}

//////////////////////////////////////////////////////////////////////
/// Debug function for using the ccTalk communication with a two wired bus.
/// \param TwoWire boolean true or false
/// \return none

void CccTalk::SetTwoWire(bool TwoWire)
{
    if (TwoWire)
        m_Mode |= MODE_TWO_WIRE;
    else
        m_Mode &= ~MODE_TWO_WIRE;
}


//////////////////////////////////////////////////////////////////////
/// Set default client address for a simple point to point solution
/// \param Addr default device address
/// \return actual client address

int CccTalk::SetDefaultClientAdr(int Adr)
{
    if (Adr >= 0)
        m_DefaultClientAdr = Adr;

    return m_DefaultClientAdr;
}


// End cctalk.cpp
//////////////////////////////////////////////////////////////////////

