////////////////////////////////////////////////////////////////////////////////
// cisGlobals.h CIS 100 - coin feeder
//	
//	(c)  wh Münzprüfer Berlin GmbH
//              14167 Berlin
//		Teltower Damm 276
// 
//	Create:	21.06.2005	Manfred Wollny
//
//	Doc:	Global Constants
//	
////////////////////////////////////////////////////////////////////////////////

#ifndef CIS_GLOBALS_H
#define CIS_GLOBALS_H


////////////////////////////////////////////////////////////////////////////////
// default settings
//
#define CIS_MANUFACTURER_ID	"WHM"
#define CIS_PRODUCT_CODE	"CIS100"
#define CIS_CATEGORY_ID		"Coin Feeder"
#define CIS_DEVICE_ADDR		140



////////////////////////////////////////////////////////////////////////////////
// ccTalk Header
//

#define CIS_CCT_BASE 20

enum CIS_CCT { // CIS 100 ccTalk Kommandos
    CIS_CCT_RESTART = CIS_CCT_BASE,     // 20 Reset, alles in Grundzustand zurücksetzen // Status abfragen
    CIS_CCT_GET_STATUS,                 // 21 Status abfragen
    CIS_CCT_EJECT_ONE_COIN,             // 22 Eine Münze auswerfen
    CIS_CCT_EJECT_COINS,                // 23 Anzahl Münzen (1-255) auswerfen
    CIS_CCT_REJECT_ALL,                 // 24 alle Münzen auswerfen
    CIS_CCT_LOCK,                       // 25 Ausgabe verriegeln, überschreibt ext. Signal
    CIS_CCT_UNLOCK,                     // 26 Ausgabe entriegeln, ext. Signal überschreibt diese Kommando
    CIS_CCT_FORCE_EJECT,                // 27 Ausgabazyklus auch ohne Münze - keine Auswurfkontrolle
    CIS_CCT_REVERSE,                    // 28 (1/2) Umdrehung rückwürts bis Grundposition

    // CIF100 und EMR100 Kommandos
    CIS_CCT_EXT_MAGNET_OPEN,            // 29 direkte Ansteuerung des externen Zwischenspeichers
    CIS_CCT_EXT_MAGNET_CLOSE,           // 30

    CIS_CCT_EMR100_REJECT,              // 31 direkte Ansterung EMR100
    CIS_CCT_EXT_MAGNET_CLEAN,           // 32 Magnet Süuberung (klappern)
    CIS_CCT_CAP_CONTROL,                // 33 Haubensteuerung
    CIS_CCT_CALIBRATE                   // 34
};


////////////////////////////////////////////////////////////////////////////////
// Command Flags and Errors


////////////////////////////////////////
// GetStatus()
//

// CIS Status

enum CIS_CCT_STATE {
    CIS_STAT_POWER_ON,          // 0 Power On - keine Bereitschaft
    CIS_STAT_RESTART,           // 1 Power On - keine Bereitschaft
    CIS_STAT_READY,             // 2 ready no coin
    CIS_STAT_COIN,              // 3 ready with coin
    CIS_STAT_COIN_READY,        // 4 ready with coin posion 2
    CIS_STAT_LOCK,              // 5 General Lock
    CIS_STAT_EJECT,             // 6 ejecting coin
    CIS_STAT_REJECT,            // 7 obsolate
    CIS_STAT_REVERSE,           // 8 reverse
    CIS_STAT_CAP_OPEN,          // 9 cap is open
    CIS_STAT_CAP_CLEANING,      // 10 cap cleaning active
    CIS_STAT_CALIBRATE,         // 11 calibration active

    CIS_STAT_ERROR = 100,       // 100 error state undefinded
    CIS_STAT_EMERGENCY,         // 101 cap open or external signal
    CIS_STAT_STOP               // 102 stop on error
};


// status flags
#define CIS_CCT_FLG_READY	(1<<0)
#define CIS_CCT_FLG_COIN_POS1	(1<<1)
#define CIS_CCT_FLG_COIN_POS2	(1<<2)
#define CIS_CCT_FLG_COIN_EXT	(1<<3)
#define CIS_CCT_FLG_EXT_MAGNET	(1<<4)
#define CIS_CCT_FLG_EMR_ACTIVE	(1<<5)
#define CIS_CCT_FLG_COIN_EJECT	(1<<6)
#define CIS_CCT_FLG_CAP_CLOSED	(1<<7)

// error codes
enum CIS_ERROR {
    CIS_ERR_WDOG,               // 0: internal system 1
    CIS_ERR_SYSTEM,             // 1: internal system 2
    CIS_ERR_EMRGENCY_STOP,      // 2: cap open or external signal
    CIS_ERR_MOTOR_SENSOR,       // 3: motor sensor

    CIS_ERR_COIN_SENSOR,        // 4: coin sensor
    CIS_ERR_COIN_JAM,           // 5: coin jam
    CIS_ERR_EXT_COIN_JAM,       // 6: extenal coin jam . CIF 100
    CIS_ERR_CCTALK,             // 7: ccTalk communication error

    CIS_ERR_COIN_FAIL,          // 8: obsolete
    CIS_ERR_COIN_CATCH,         // 9: catching coin
    CIS_ERR_CALIBIBRATION,      // 10: calibaration
    CIS_ERR_MOTOR_TOUT,         // 11: motor timeout

    CIS_ERR_COIN_EJECT,         // 12: x tries coin ejecting
    CIS_ERR_EXT_COIN_CLEAN,     // 13: cleaning extternal CIF 100
    CIS_ERR_EXT_COIN_TOUT,      // 14: extern coin time out
    CIS_ERR_CAP,                // 15: cap controlling

    CIS_ERR_COUNT = 16
};

////////////////////////////////////////
// CapCommand()
//

enum CIS_CAP_COMMANDS {
    CIS_CAP_GETSTATE,           // 0 request cap state
    CIS_CAP_OPEN,               // 1 open cap
    CIS_CAP_CLOSE,              // 2 close cap
    CIS_CAP_CLEAN,              // 3 clean cap
    CIS_CAP_FIND_POSITION,      // 4 find next position (open or closed)
    CIS_CAP_RESET = 9           // 9 obsolete
};

enum CAP_STATUS {
    CAP_DISABLE,                // 0: deactiviert

    CAP_RESET,                  // 1: Reset
    CAP_FIND_POSITION,          // 2: finding position

    CAP_IS_CLOSE,               // 3: cap closed
    CAP_IS_OPEN,                // 4: cap open

    CAP_OPEN,                   // 5: got open command
    CAP_WAIT_OPEN,              // 6: moving to open position

    CAP_CLEANING,               // 7: cleaning after timeout or command

    CAP_CLOSE,                  // 8: got close command
    CAP_WAIT_CLOSE,             // 9: moving to close position

    CAP_ERROR = 100,            // 100: undefinde error
    CAP_OPEN_TIMEOUT,           // 101: timeout on opening
    CAP_OPEN_ERROR,             // 102: closed
    CAP_CLOSE_TIMEOUT,          // 103: timeout on closing
    CAP_CLOSE_ERROR,            // 104: opened
    CAP_SENSOR_ERROR            // 105: sensor error
};


////////////////////////////////////////
// Calibration()
//

// calibration command mask
#define CIS_CALIB_MOTOR			(1 << 0)
#define CIS_CALIB_MOTOR_EEP		(1 << 1)
#define CIS_CALIB_SENSOR		(1 << 2)
#define CIS_CALIB_SENSOR_EEP		(1 << 3)


#define CIS_CALIB_FORCE_SENSOR_EEP	(1 << 7)

// calibration error mask
#define CIS_CAL_ERR_MOTOR_1_OK		(0 << 0)
#define CIS_CAL_ERR_MOTOR_1_MIN		(1 << 0)
#define CIS_CAL_ERR_MOTOR_1_MAX		(2 << 0)
#define CIS_CAL_ERR_MOTOR_1_DIF		(3 << 0)

#define CIS_CAL_ERR_MOTOR_2_OK		(0 << 2)
#define CIS_CAL_ERR_MOTOR_2_MIN		(1 << 2)
#define CIS_CAL_ERR_MOTOR_2_MAX		(2 << 2)
#define CIS_CAL_ERR_MOTOR_2_DIF		(3 << 2)

#define CIS_CAL_ERR_SENSOR_1_OK		(0 << 4)
#define CIS_CAL_ERR_SENSOR_1_MIN	(1 << 4)
#define CIS_CAL_ERR_SENSOR_1_MAX	(2 << 4)
#define CIS_CAL_ERR_SENSOR_1_DIF	(3 << 4)

#define CIS_CAL_ERR_SENSOR_2_OK		(0 << 6)
#define CIS_CAL_ERR_SENSOR_2_MIN	(1 << 6)
#define CIS_CAL_ERR_SENSOR_2_MAX	(2 << 6)
#define CIS_CAL_ERR_SENSOR_2_DIF	(3 << 6)


#endif
//
// End cisGlobals.h
////////////////////////////////////////////////////////////////////////////////
