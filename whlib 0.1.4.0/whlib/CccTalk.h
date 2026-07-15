/* 
 * File:   CccTalk.h
 * Author: manfred
 *
 * Created on 4. November 2009, 10:51
 */

#ifndef _CCCTALK_H
#define	_CCCTALK_H

//////////////////////////////////////////////////////////////////////
// Includes

#include "CSerCom.h"
#include <string>
using namespace std;

//////////////////////////////////////////////////////////////////////
// ccTalk defines

#define CCT_OK			0
#define CCT_RX_RDY		(1<<0)

#define CCT_ERR_RX_TOUT		(1<<4)
#define CCT_ERR_RX_CSUM		(1<<5)
#define CCT_ERR_RX_OF		(1<<6)

#define CCT_ERR_TX_ECHO		(1<<8)
#define CCT_ERR_TX_TOUT		(1<<9)

//////////////////////////////////////////////////////////////////////
// ccTalk Frame:
//
//			|des|len|src|hdr|...data...|csum|
//
// Offset     0   1   2   3   4         4+len
//
#define FRM_DES 0
#define FRM_LEN 1
#define FRM_SRC 2
#define FRM_HDR 3
#define FRM_DAT 4
#define FRM_SUM 4

#define FRM_MIN_FRAME_LEN 5
#define FRM_MAX_DATA_LEN 255

//////////////////////////////////////////////////////////////////////
// extendet modes

#define MODE_TWO_WIRE	(1<<0)
#define MODE_DEFAULT	0


//////////////////////////////////////////////////////////////////////
// defines of ccTalk Commands

// Core Commands
#define CCT_FACTORY_SETUP 			255
#define CCT_SIMPLE_POLL				254
#define CCT_ADDRESS_POLL 			253
#define CCT_ADDRESS_CLASH			252
#define CCT_ADDRESS_CHANGE 			251
#define CCT_ADDRESS_RANDOM  			250
#define CCT_REQUEST_POLLING_PRIORITY            249
#define CCT_REQUEST_STATUS                      248
#define CCT_REQUEST_VARIABLE_SET                247
#define CCT_REQUEST_MANUFACTURER_ID             246
#define CCT_REQUEST_CATEGORY_ID			245
#define CCT_REQUEST_PRODUCT_CODE		244
#define CCT_REQUEST_DATA_BASE_VERSION           243
#define CCT_REQUEST_SERIAL_NUMBER		242
#define CCT_REQUEST_SOFTWARE_REVISION   	241
#define CCT_TEST_SOLENOIDS                      240
#define CCT_OPERATE_MOTORS                      239
#define CCT_TEST_OUTPUT_LINES                   238
#define CCT_READ_INPUT_LINES                    237
#define CCT_READ_OPTO_STATES                    236
#define CCT_READ_LAST_CREDIT_ERROR_CODE         235
#define CCT_ISSUE_GUARD_CODE                    234
#define CCT_LATCH_OUTPUT_LINES                  233
#define CCT_PERFORM_SELF_TEST                   232
#define CCT_MODIFY_INHIBIT_STATUS               231
#define CCT_REQUEST_INHIBIT_STATUS              230
#define CCT_READ_BUFFERED_CREDIT                229
#define CCT_MODIFY_MASTER_INHIBIT_STATUS        228
#define CCT_REQUEST_MASTER_INHIBIT_STATUS       227
#define CCT_REQUEST_INSERTION_COUNTER           226
#define CCT_REQUEST_ACCEPT_COUNTER              225
#define CCT_DISPENSE_COINS                      224
#define CCT_DISPENSE_CHANGE                     223
#define CCT_MODIFY_SORTER_OVERRIDE              222
#define CCT_REQUEST_SORTER_OVERRIDE             221
#define CCT_ONE_SHOT_CREDIT                     220
#define CCT_ENTER_NEW_PIN_NUMBER                219
#define CCT_ENTER_PIN_NUMBER                    218
#define CCT_REQUEST_PAYOUT_HIGH_LOW_STATUS      217
#define CCT_REQUEST_DATA_AVAILABILITY           216
#define CCT_READ_DATA_BLOCK                     215
#define CCT_WRITE_DATA_BLOCK                    214
#define CCT_REQUEST_OPTION_FLAGS                213
#define CCT_REQUEST_COIN_POSITION               212
#define CCT_MANAGEMENT_CONTROL                  211
#define CCT_MODIFY_SORTER_PATH                  210
#define CCT_REQUEST_SORTER_PATH                 209
#define CCT_MODIFY_PAYOUT_ABSULUTE_COUNT        208
#define CCT_REQUEST_PAYOUT_ABSULUTE_COUNT       207
#define CCT_EMPTY_PAYOUT                        206
#define CCT_REQUEST_AUDIT_INFO_BLOCK            205
#define CCT_METER_CONTROL                       204
#define CCT_DISPLAY_CONTROL                     203
#define CCT_TEACH_MODE_CONTROL                  202
#define CCT_REQUEST_TEACH_STATUS                201
#define CCT_UPDATE_COIN_DATA                    200
#define CCT_CONFIGURATION_TO_EEPROM             199
#define CCT_COUNTER_TO_EEPROM                   198
#define CCT_CALCULATE_ROM_CHECKSUM		197 	
#define CCT_REQUEST_CREATION_DATE               196
#define CCT_REQUEST_LAST_MODIFICATION_DATE      195
#define CCT_REQUEST_REJECT_COUNTER              194
#define CCT_REQUEST_FRAUD_COUNTER               193
#define CCT_REQUEST_BUILD_CODE			192
#define CCT_KEY_PAD_CONTROL                     191
#define CCT_REQUEST_PAYOUT_STATUS               190
#define CCT_MODIFY_DEFAULT_SORTER_PATH          189
#define CCT_REQUEST_DEFAULT_SORTER_PATH         188
#define CCT_MODIFY_PAYOUT_CAPACITY              187
#define CCT_REQUEST_PAYOUT_CAPACITY             186
#define CCT_MODIFY_COIN_ID                      185
#define CCT_REQUEST_COIN_ID                     184
#define CCT_UPLOAD_WINDOW_DATA                  183
#define CCT_DOWNLOAD_CALIBRATION_INFO           182
#define CCT_MODIFY_SECURITY_SETTING             181
#define CCT_REQUEST_SECURITY_SETTING            180
#define CCT_MODIFY_BANK_SELECT                  179
#define CCT_REQUEST_BANK_SELECT                 178
#define CCT_HANDHELD_FUNCTION                   177
#define CCT_REQUEST_ALARM_COUNTER               176
#define CCT_MODIFY_PAYOUT_FLOAT                 175
#define CCT_REQUEST_PAYOUT_FLOAT                174
#define CCT_REQUEST_THERMISTOR_READING          173
#define CCT_EMERGENCY_STOP                      172
#define CCT_REQUEST_HOPPER_COIN                 171
#define CCT_REQUEST_BASE_YEAR                   170
#define CCT_REQUEST_ADDRESS_MODE                169
#define CCT_REQUEST_HOPPER_DISPENSE_COUNT       168
#define CCT_DISPENSE_HOPPER_COINS               167
#define CCT_REQUEST_HOPPER_STATUS               166
#define CCT_MODIFY_VARIABLE_SET                 165
#define CCT_ENABLE_HOPPER                       164
#define CCT_TEST_HOPPER                         163
#define CCT_MODIFY_INHIBIT_OVERRIDE             162
#define CCT_PUMP_RNG                            161
#define CCT_REQUEST_CIPHER_KEY                  160
#define CCT_READ_BUFFERED_BILL_EVENT            159
#define CCT_MODIFY_BILL_ID                      158
#define CCT_REQUEST_BILL_ID                     157
#define CCT_REQUEST_COUNTRY_SCALING_FACTOR      156
#define CCT_REQUEST_BILL_POSITION               155
#define CCT_ROUTE_BILl                          154
#define CCT_MODIFY_BILL_OPERATING_MODE          153
#define CCT_REQUEST_BILL_OPERATING_MODE         152
#define CCT_TEST_LAMP                           151
#define CCT_REQUEST_INDIVIDUAL_ACCEPT_COUNTER   150
#define CCT_REQUEST_INDIVIDUAL_ERROR_COUNTER    149
#define CCT_READ_OPTO_VOLTAGES                  148
#define CCT_PERFORM_STACKER_CYCLE               147
#define CCT_OPERATE_BI_DIRECTIONAL_MOTORS       146
#define CCT_REQUEST_CURRENCY_REVISION           145
#define CCT_UPLOAD_BILL_TABLE                   144
#define CCT_BEGIN_BILL_TABLE_UPDATE             143
#define CCT_FINISH_BILL_TABLE_UPDATE            142
#define CCT_REQUEST_FIRMWARE_UPGRADE_CAPABILITY 141
#define CCT_UPLOAD_FIRMWARE                     140
#define CCT_BEGIN_FIRMWARE_UPGRADE              139
#define CCT_FINISH_FIRMWARE_UPGRADE             138
#define CCT_SWITSCH_ENCRYPTION_CODE             137
#define CCT_STORE_ENCRYPTION_CODE               136
#define CCT_STE_ACCEPT_LIMIT                    135
#define CCT_DISPENSE_HOPPER_VALUE               134
#define CCT_REQUEST_HOPPER_POLLING_VALUE        133
#define CCT_EMERGENCY_STOP_VALUE                132
#define CCT_REQUEST_HOPPER_COIN_VALUE           131
#define CCT_REQUEST_INDEXED_HOPPER_DISPENSE     130
#define CCT_REad_BARCODE_DATA                   129
#define CCT_REQUEST_MONEY_IN                    128
#define CCT_REQUEST_MONEY_OUT                   127
#define CCT_CLEAR_MONEY_COUNTER                 126
#define CCT_PAY_MONEY_OUT                       125
#define CCT_VERIFY_MONEY_OUT                    124
#define CCT_REQUEST_ACTIVITY_REGISTER           123
#define CCT_REQUEST_ERROR_STATUS                122
#define CCT_PURGE_HOPPER                        121
#define CCT_MODIFY_HOPPER_BALANCE               120
#define CCT_REQUEST_HOPPER_BALANCE              119
#define CCT_MODIFY_CASHBOX_VALUE                118
#define CCT_REQUEST_CASHBOX_VALUE               117
#define CCT_MODIFY_REAL_TIME_CLOCK              116
#define CCT_REQUEST_REAL_TIME_CLOCK             115
#define CCT_REQUEST_USB_ID                      114
// 113 - 104 reserved for future products
#define CCT_EXPANSION_HEADER_4                  103
#define CCT_EXPANSION_HEADER_3                  102
#define CCT_EXPANSION_HEADER_2                  101
#define CCT_EXPANSION_HEADER_1                  100
// 99 -20 Application specific
// 19 - 7 reserved
#define CCT_BUSY                                006
#define CCT_NAK                                 005
#define CCT_REQUEST_COMMS_REVISION		004
#define CCT_CLEAR_COMM_STATUS			003
#define CCT_REQUEST_COMM_STATUS 		002
#define CCT_RESET_DEVIVCE			001
#define CCT_RETURN_MESSAGE                      000

// Quittungs Telegramme
#define CCT_RETURN_MESSAGE  			000
#define CCT_ACK 			 	000
#define CCT_NACK 			 	005
#define CCT_BUSY 			 	006

// Standard Coin Validator Commands as extended Core Commands
#define CCT_ADR_POLL_COUNT  256

#define CCT_BUF_LEN	256


/// Base class for ccTalk bus communication.
////////////////////////////////////////////////////////////////////////////////
/// This class includes base functions for data transfer and data encapsulating.
///
////////////////////////////////////////////////////////////////////////////////

class CccTalk : public CSerCom {

public:
    // constructor / destructor
    CccTalk();
    virtual ~CccTalk();

    // public types
    enum CCT_CSUM_TYPE{
	CCT_CSUM_SIMPLE,
	CCT_CSUM_CRC16,
	CCT_CSUM_UNDEF
    };

    // Basics
    /// @name Initialise Functions
    //@{
    int Open(const char* Port, int MasterAdr= 1, int BaudRate = 9600);
    int Open(int Port, int MasterAdr= 1, int BaudRate = 9600);
    void Close();
    void SetTwoWire(bool TwoWire = true);
    int SetDefaultClientAdr(int Adr);
    //@}

    /// @name Base Functions
    //@{
    int Send(int Addr, int Len, int Header, void * Data);
    int SendEx(int Addr, int Len, int Header, void *Data, int Master, int Csum);

    // Check Sum Calculation
    void AddToCsum(int &Csum, int Data);
    void CalcCrc16(int &Csum, int Data);
    int CalcCsum(int Addr, int Len, int Header, void *Data, int Master);
    void SetCRCType(enum CCT_CSUM_TYPE CRCType);
    void SetCsum(unsigned char *Buffer);
    int ChkCsum(unsigned char *Buffer);

    int GetStatus(bool Clear = true);
    void ClearRxBuffer();
    int RxPolling();
    int Receive(int &Adr, int &Len, int &Header, void * Data, bool Wait = true);
    bool IsRxFrameReady(void);
    void GetReceiveCsum(int &RxCsum, int &CalcCsum);
    //@}

    /// @name ccTalk communication
    //@{
    int SendCommand(int Addr, int Header);
    int SendData(int Addr, int Header, int Len, void *Data);
    int RequestBinDataEx(int Addr, int Header, void *Cmds, int CmdLen, void *Data, int &Datalen);
    int RequestBinData(int Addr, int Header, int &Len, void *Data);
    int RequestASCIIData(int Adrr, int Command, string &Str);
    int RequestASCIIData(int Addr, int Command, char *Buf, int Len= CCT_BUF_LEN);
    //@}

    /// @name Core Commands
    //@{
    int ResetDevice(int Addr);
    int RequestAddressMode(int Addr, unsigned char *Mode);
    int ClearCommsStatus(int Addr);
    int RequestCommsRevision(int Addr, string &Str);
    int RequestCommsStatus(int Addr, unsigned char *Status);
    int RequestCommsStatus(int Addr, int &RxTimeOuts, int &ByteIgnored, int &BadChecksums);
    int RequestCommsRevision(int Addr, unsigned char *Code);
    int RequestSoftwareRevision(int Addr, string &Vers);
    int RequestSerialNumber(int Addr, unsigned long &Serial);
    int RequestBuildCode(int Addr, string &Build);
    int RequestProductCode(int Addr, string &Code);
    int RequestCategoryID(int Addr, string &CatId);
    int RequestManufacturerID(int Addr, string &ManId);
    int AddressPoll(int *PollTbl);
    //@}
    
    // class vars
private:
    enum CCT_CSUM_TYPE m_CsumType;
    int m_LastRxCalcCsum;
    int m_LastRxCsum;
    int m_Status;
    int m_TxLen;
    int m_RxLen;
    unsigned char * m_TxBuffer;
    unsigned char * m_RxBuffer;
    int m_MasterAddress;
    bool m_RxFrameReady;
    int m_RxCnt;
    int m_RxCsum;
    int m_RxPos;
    int m_DefaultClientAdr;
    int m_Mode;
    bool m_Abbort;
};

#endif	/* _CCCTALK_H */

//////////////////////////////////////////////////////////////////////
