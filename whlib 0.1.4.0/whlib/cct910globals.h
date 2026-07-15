////////////////////////////////////////////////////////////////////////////////
// cct910globals.h
//
//	(c)  wh M�nzpr�fer Berlin GmbH
//              14167 Berlin
//		Teltower Damm 276
//
//	Create:	17.03.2011	Manfred Wollny
//
//	Doc:	Global Constants
//
////////////////////////////////////////////////////////////////////////////////

#ifndef _CCT910GLOBALS_H
#define	_CCT910GLOBALS_H

////////////////////////////////////////////////////////////////////////////////
// ccTalk Header & Fags
//

#define CCT_CCT900_BASE 3

/*	CCT 00 commands
#define CCT_CCT900_MODIFY_ESCROW_STATE          135
#define CCT_CCT900_START_MOTOR_REJECT           133
#define CCT_CCT900_MODIFY_ANTI_PIN_STATUS       132
#define CCT_CCT900_REQUEST_PERIPHERAL_STATUS    131
#define CCT_CCT900_CLEAR_UPTIME_COUNTER         124
#define CCT_CCT900_REQUEST_UPTIME_COUNTER       123
#define CCT_CCT900_MDB_COMMUNICATION            122
#define CCT_CCT900_MDB_SEND_BREAK               121

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

  */
// 120-117 reserved
#define CCT_CCT910_SETUP_SERIAL_PORT			116
#define CCT_CCT910_SERIAL_COMMUNICATION			115
#define CCT_CCT910_SERIAL_SEND_BREAK			114
#define CCT_CCT910_GET_SERIAL_LINES			113
#define CCT_CCT910_SET_SERIAL_LINES			112
// 111 reserved
#define CCT_CCT910_REQUEST_FEATURES			110
#define CCT_CCT910_REQUEST_IO_PORT_USAGE		109
#define CCT_CCT910_SET_LED_FLASHING			108
#define CCT_CCT910_SET_SINGLE_LED			107
#define CCT_CCT910_SET_ALL_LED				106
#define CCT_CCT910_REQUEST_SWITCH_STATE			105
#define CCT_CCT910_CONFIGURE_IO_PORT			104
#define CCT_CCT910_READ_IO_PORT				103
#define CCT_CCT910_WRITE_IO_PORT			102
// 104-100 reserved

////////////////////////////////////////////////////////////////////////////////

#define CCT910_CONF_IO_PORTS 8

////////////////////////////////////////////////////////////////////////////////

#endif	/* _CCT910GLOBALS_H */
//
// End cct910globals.h
////////////////////////////////////////////////////////////////////////////////

