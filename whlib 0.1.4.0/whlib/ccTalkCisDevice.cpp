/* 
 * File:   ccTalkCisDevice.cpp
 * Author: manfred
 * 
 * Created on 13. April 2010, 11:54
 */
//////////////////////////////////////////////////////////////////////
#include "ccTalkCisDevice.h"

//////////////////////////////////////////////////////////////////////
// Konstruktion/Destruktion
//////////////////////////////////////////////////////////////////////


ccTalkCisDevice::ccTalkCisDevice()
{
}

/*ccTalkCisDevice::ccTalkCisDevice(const ccTalkCisDevice& orig)
{

}
*/
ccTalkCisDevice::~ccTalkCisDevice()
{
}


//////////////////////////////////////////////////////////////////////
/// Reset coin eject and motor control, error state is reseted
/// \param none
/// \return error code, 0= no error

int ccTalkCisDevice::Restart()
{
	return m_ccTalk->SendCommand(m_DeviceAdr, CIS_CCT_RESTART);
}


#define CCT_GET_STATUS_LEN 4

//////////////////////////////////////////////////////////////////////
/// Returns status of the device for controlling of coin flow
/// \param Status reference to returned device control state
/// \param Flags reference to status flags
/// \param ErrFlags reference to error flags
/// \return error code, 0= no error

int ccTalkCisDevice::GetStatus(int &Status, unsigned char &Flags, unsigned short &ErrFlags)
{
	int err= 0;
	int buflen= CCT_GET_STATUS_LEN;
	unsigned char buf[CCT_GET_STATUS_LEN];

	err= m_ccTalk->RequestBinData(m_DeviceAdr, CIS_CCT_GET_STATUS, buflen, buf);
	Status= buf[0];
	Flags= buf[1];
	ErrFlags= buf[2] + (buf[3] << 8);

	return err;
}

//////////////////////////////////////////////////////////////////////
/// Eject one coin.
/// The requested coins are added to the internal eject counter
/// \param none
/// \return error code, 0= no error

int ccTalkCisDevice::EjectOneCoin(void)
{
	return m_ccTalk->SendCommand(m_DeviceAdr, CIS_CCT_EJECT_ONE_COIN);
}

//////////////////////////////////////////////////////////////////////
/// Eject coins
/// \param Coins coins to eject. The internal eject count ist set to Parameter coins, no
/// incrementing of eject coins counter
/// \return error code, 0= no error

int ccTalkCisDevice::EjectCoins(int Coins)
{
	unsigned char buf; 
	
	buf= Coins;
	return m_ccTalk->SendData(m_DeviceAdr, CIS_CCT_EJECT_COINS, 1, &buf);
}

//////////////////////////////////////////////////////////////////////
/// Lock / disable the device, no coin ist ejected until UNlock() command is send
/// \param none
/// \return error code, 0= no error

int ccTalkCisDevice::GeneralLock()
{
	return m_ccTalk->SendCommand(m_DeviceAdr, CIS_CCT_LOCK);
}

//////////////////////////////////////////////////////////////////////
/// Unlock / enable the device
/// \param none
/// \return error code, 0= no error

int ccTalkCisDevice::Unlock()
{
	return m_ccTalk->SendCommand(m_DeviceAdr, CIS_CCT_UNLOCK);
}

//////////////////////////////////////////////////////////////////////
/// Initial one eject cycle, also if no coin is detected inside
/// \param none
/// \return error code, 0= no error

int ccTalkCisDevice::ForceEject()
{
	return m_ccTalk->SendCommand(m_DeviceAdr, CIS_CCT_FORCE_EJECT);
}

//////////////////////////////////////////////////////////////////////
/// Initial a reverse cylce. Useful to bring undetected coins into the sensor area
/// \param none
/// \return error code, 0= no error

int ccTalkCisDevice::Reverse()
{
	return m_ccTalk->SendCommand(m_DeviceAdr, CIS_CCT_REVERSE);
}

//////////////////////////////////////////////////////////////////////
/// Open external soleonoid (CIF 100)
/// \param none
/// \return error code, 0= no error

int ccTalkCisDevice::CifMagnetOpen()
{
	return m_ccTalk->SendCommand(m_DeviceAdr, CIS_CCT_EXT_MAGNET_OPEN);
}

//////////////////////////////////////////////////////////////////////
/// Close external soleonoid (CIF 100)
/// \param none
/// \return error code, 0= no error

int ccTalkCisDevice::CifMagnetClose()
{
	return m_ccTalk->SendCommand(m_DeviceAdr, CIS_CCT_EXT_MAGNET_CLOSE);
}

//////////////////////////////////////////////////////////////////////
/// Activale motor reject EMR 100 for one cycle
/// \param none
/// \return error code, 0= no error

int ccTalkCisDevice::EmrActivate()
{
	return m_ccTalk->SendCommand(m_DeviceAdr, CIS_CCT_EMR100_REJECT);
}

//////////////////////////////////////////////////////////////////////
/// Control function of the motor cap
/// \param Command
/// \param Status reference to returned status
/// \return error code, 0= no error

int ccTalkCisDevice::CapCommand(int Command, int *Status)
{
	int err= 0;
	int buflen= 1;
	unsigned char buf;
	
	buf= Command;
	err= m_ccTalk->RequestBinDataEx(m_DeviceAdr, CIS_CCT_CAP_CONTROL, &buf, 1, &buf, buflen);
	if(buflen)
            *Status= buf;

	return err;
}

//////////////////////////////////////////////////////////////////////
/// Calibrate motor control and coin sensor of the device.
/// Use this command only if ni coin is detected. Otherwise the command is
/// canceled.
/// \param Cmd Calibration comand
/// \param Data Reference to returned data
/// \param BufLen max buffer length for returned data
/// \return error code, 0= no error

int ccTalkCisDevice::Calibrate(int Cmd, void *Data, int BufLen)
{
	int err= 0;
	int buflen= 0;
	unsigned char buf[256];
	
	if(!Data || BufLen <= 0)
	{
		Data= &m_CalibData;	
		BufLen= sizeof(m_CalibData);
	}

	buf[0]= Cmd;
	err= m_ccTalk->RequestBinDataEx(m_DeviceAdr, CIS_CCT_CALIBRATE, buf, 1, &buf, buflen);
	
	memcpy(Data, buf, min(buflen, BufLen));

	return err ;
}

/// Status Flags Ascii Strings
char g_StatusFlagsText[][10] = {
	{"Ready"},		// #define CIS_CCT_FLG_READY		(1<<0)
	{"Coin1"},		// #define CIS_CCT_FLG_COIN_POS1	(1<<1)
	{"Coin2"},		// #define CIS_CCT_FLG_COIN_POS2	(1<<2)
	{"CoinExt"},            // #define CIS_CCT_FLG_COIN_EXT		(1<<3)
	{"Magnet"},		// #define CIS_CCT_FLG_EXT_MAGNET	(1<<4)
	{"EMR"},		// #define CIS_CCT_FLG_EMR_ACTIVE	(1<<5)
	{"Eject"},		// #define CIS_CCT_FLG_COIN_EJECT	(1<<6)
	{"Closed"}		// #define CIS_CCT_FLG_CAP_CLOSED	(1<<7)
};


#if USE_STRING
////////////////////////////////////////////////////////////////////////////////
/// Return an Ascii string class with the names of all masked status flags
/// \param Flags status flags mask
/// \return string class with status flag names

string ccTalkCisDevice::GetStatusFlagsString(int Flags)
{
    int i;
    string str;

    for(i= 0; i < 8; i++)
    {
	if(Flags & (1<<i))
	{
            if(str.length())			// delimiter if more than one
                str+=  ",";
		str+= g_StatusFlagsText[i];	
	}
    }
    return str;
}
#endif

char g_ErrorMaskText[][12] = {
	{"WDog"},	// CIS_ERR_WDOG,		// 0: Watchdog reset
	{"Sytem"},	// CIS_ERR_SYSTEM,		// Stack Overflow
	{"Emergency"},	// CIS_ERR_EMRGENCY_STOP,	// Emergency stop / cap open
	{"Motor"},	// CIS_ERR_MOTOR_SENSOR,	// Motorsensor fault
	
	{"Sensor"},	// CIS_ERR_COIN_SENSOR,		// Coin Sensor fault
	{"Jam"},	// CIS_ERR_COIN_JAM,		// Coin jam
	{"ExtJam"},	// CIS_ERR_EXT_COIN_JAM,        // Coin jam inexternal coin store CIF 100
	{"ccTalk"},	// CIS_ERR_CCTALK,		// ccTalk communication error
	//
	{"Fail"},	// CIS_ERR_COIN_FAIL,		// coin eject
	{"Catch"},	// CIS_ERR_COIN_CATCH,		// coin catch cycles
	{"Calibration"},// CIS_ERR_CALIBIBRATION        // calibration error
	{"Tout"},	// CIS_ERR_MOTOR_TOUT,		// MotorTimeout
	
	{"Eject"},	// CIS_ERR_COIN_EJECT,		// coin eject error
	{"Clean"},	// CIS_ERR_EXT_COIN_CLEAN,	// cleaning external store CIF 100
	{"ExtEject"},	// CIS_ERR_EXT_COIN_TOUT,	// TimeOut into nal store CIF 100
	{"Cap"}		// CIS_ERR_CAP,			// cap control error
};

#if USE_STRING
////////////////////////////////////////////////////////////////////////////////
/// Return an Ascii string class with the the name of all masked errors
/// \param Errors error mask
/// \return string class with error names

string ccTalkCisDevice::GetErrorMaskString(int Errors)
{
	string str;
	int i;

	for(i= 0; i < 16; i++)
	{
		if(Errors & (1<<i))
		{
			if(str.length())	// delimiter if more than one
				str+= ",";
			str+= g_ErrorMaskText[i];	
		}
	}

	return str;
}
#endif
//
////////////////////////////////////////////////////////////////////////////////