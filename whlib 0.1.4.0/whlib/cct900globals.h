////////////////////////////////////////////////////////////////////////////////
// cct900globals.h
//
//	(c)  wh Münzprüfer Berlin GmbH
//              14167 Berlin
//		Teltower Damm 276
//
//	Create:	17.03.2011	Manfred Wollny
//
//	Doc:	Global Constants
//
////////////////////////////////////////////////////////////////////////////////

#ifndef _CCT900GLOBALS_H
#define	_CCT900GLOBALS_H

////////////////////////////////////////////////////////////////////////////////
// ccTalk Header & Fags
//

#define CCT_CCT900_BASE 3

#define CCT_CCT900_MODIFY_ESCROW_STATE          135
#define CCT_CCT900_START_MOTOR_REJECT           133
#define CCT_CCT900_MODIFY_ANTI_PIN_STATUS       132
#define CCT_CCT900_REQUEST_PERIPHERAL_STATUS    131
#define CCT_CCT900_CLEAR_UPTIME_COUNTER         124
#define CCT_CCT900_REQUEST_UPTIME_COUNTER       123
#define CCT_CCT900_MDB_COMMUNICATION            122
#define CCT_CCT900_MDB_SEND_BREAK               121

// MDB Communicationen Status
// returned by "MDB_Communuication" command
#define CCT_CCT900_MDB_DATA_BLOCK_RECEIVED      (1<<0)
#define CCT_CCT900_MDB_CHECK_SUM_ERROR          (1<<1)
#define CCT_CCT900_MDB_RECEIVE_TIMEOUT          (1<<2)
#define CCT_CCT900_MDB_BREAK_ACTIVE             (1<<3)

// Escrow State Flags
// Byte 0
#define CCT900_ESCROW_CLOSE             0
#define CCT900_ESCROW_CASH              1
#define CCT900_ESCROW_RETURN            2
// Byte 1
#define CCT900_ESCROW_STEADY            0

// Anti Pin Status
// Modify
#define CCT900_ANTI_PIN_DISABLE         0
#define CCT900_ANTI_PIN_AUTO            1
#define CCT900_ANTI_PIN_OPEN            2

// Request Peripheral Status
// Byte 0
#define CCT900_ESCROW_STATUS_CLOSE      0
#define CCT900_ESCROW_STATUS_CASH       1
#define CCT900_ESCROW_STATUS_RETURN     2
// Byte 1
#define CCT900_ESCROW_SWITCH_OPEN       0
// Byte 2 == Modify FLAGS
//#define CCT900_ANTI_PIN_DISABLE         0
//#define CCT900_ANTI_PIN_AUTO            1
//#define CCT900_ANTI_PIN_OPEN            2
// Byte 3
#define CCT900_ANTI_PIN_STATUS_COIN     0
#define CCT900_ANTI_PIN_STATUS_OPEN     1
#define CCT900_ANTI_PIN_STATUS_STRING   2
// Byte 4
#define CCT900_MOTOR_REJECT_IDLE        1

////////////////////////////////////////////////////////////////////////////////

#endif	/* _CCT900GLOBALS_H */
//
// End cct900globals.h
////////////////////////////////////////////////////////////////////////////////

