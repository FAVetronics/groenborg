/* 
 * File:   ccTalkEmpDevice.h
 * Author: manfred
 *
 * Created on 20. April 2010, 14:41
 */

#ifndef _CCTALKEMPDEVICE_H
#define	_CCTALKEMPDEVICE_H

#include "ccTalkDevice.h"

#ifndef USE_QT
#define USE_QT      0
#endif

#define USE_STRING  1

#if USE_STRING
#include <string>
#endif

#if USE_QT
#include <QtGui/QApplication>
#endif

////////////////////////////////////////////////////////////////////////////////
// ccTalk Comms


// wh spezific header

#define EMP_CCT_COIN_PRECISION              100
#define EMP_CCT_COIN_PRECISION_SUB_MODIFY   3
#define EMP_CCT_COIN_PRECISION_SUB_REQUEST  4


// Constants / Mask Defintions

#define EMP_CCT_REQUEST_POLLING_PRIORITY_LEN 2


// Test Solenoids (240)
#define EMP_CCT_TEST_SOLENOID_ACCEPT	(1<<0)
#define EMP_CCT_TEST_SOLENOID_1             (1<<1)
#define EMP_CCT_TEST_SOLENOID_2             (1<<2)
#define EMP_CCT_TEST_SOLENOID_3             (1<<3)
#define EMP_CCT_TEST_SOLENOID_ALL		0xf

// Credit or Error
#define EMP_CCT_READ_BUFFERED_CREDIT_LEN 11
#define EMP_CCT_BUFFERED_CREDITS_QUEUE   5

// Teaching
#define EMP_CCT_TEACH_STAT_ABORTED          252
#define EMP_CCT_TEACH_STAT_ERROR            253
#define EMP_CCT_TEACH_STAT_BUSY             254
#define EMP_CCT_TEACH_STAT_COMPLETE         255

// Coin Id
#define CCT_EMP_COIN_OFFSET                 1
#define EMP_CCT_COIN_ID_LEN                 6
#define EMP_CCT_COUNTRY_CODE_LEN            2
#define EMP_CCT_COIN_VAL_STR_LEN            15
#define EMP_CCT_COIN_NAME_LEN               15

#define EMP_CCT_MAX_COINS                   16


#define EMP_CCT_ERR_LOST_EVENTS             100


/// Device class for coin acceptors
////////////////////////////////////////////////////////////////////////////////
/// Defines the ccTalk communickation functions for coin acceptors. Also spezial 
/// functions for coin handling ar included

class ccTalkEmpDevice : public CccTalkDevice
{
public:
    ccTalkEmpDevice();
    virtual ~ccTalkEmpDevice();


    typedef struct BUFFERED_CREDIT {
        unsigned char Count;
        struct {
            unsigned char Coin;
            unsigned char Error;
        } Credit[EMP_CCT_BUFFERED_CREDITS_QUEUE];
    } CreditBuffer;


private:
    int m_LastPollCounter;
    int m_LastPollEvent;
    CreditBuffer m_BufferdCredits;

public:
    /// @name Base Functions (overloaded)
    //@{
    void Close(void);
    //@}


    /// @name Coin acceptor commands
    //@{
    int RequestPollingPriority(int &Unit, int &Value);  // 249
    int RequestStatus(int &Status);                     // 248
    int TestSolenoids(int Mask);                        // 240
    int TestOutputLines(int Mask);                      // 238
    int LatchOutputLines(int Mask);                     // 233
    int ReadBufferedCredit(int &Count, unsigned char *Credits);     // 229
    int ReadBufferedCredit(struct BUFFERED_CREDIT *Credits= NULL);  // 229
    int ModifySorterOverrideStatus(unsigned short Mask); // 222
    int RequestSorterOverrideStatus(unsigned short &Mask); // 221
    int RequestCoinPosition(int Coin, unsigned short &Mask); // 212
    int ModifySorterPath(int Coin, int Path);           // 210
    int RequestSorterPath(int Coin, int &Path);         // 209
    int RequestRejectCounter(long &Counter);            // 194
    int RequestThermistorReading(int &Degrees);         // 173
    //@}

    int TeachModeControl(int Coin);                     // 202
    int TeachModeControl(int Coin, int Orientation);    // 202
    int RequestTeachStatus(int &Coins, int &Status, bool Abbort= false);     // 201


#if USE_QT
    /// @name extended functions using Qt library
    //@{
    QString CoinTotalStr(int Coin);
    QString CoinTotalSumStr(int First= 0, int Last= EMP_CCT_MAX_COINS);
    //@}
#endif



    // wh spezific ccTalk functions
    /// @name wh specific function for coin acceptors EMPxxx
    //@{
    int ModifyCoinPrecision(const unsigned char *Buffer);
    int RequestCoinPrecision(unsigned char *Buffer);
    //@}

public:
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            
    /// @name extended and management functions
    //@{    int ModifyCoinPrecision(const unsigned char *Buffer);
    int Poll(int &EventCount, CreditBuffer *Credits);
    int PollNext(int &EventCount, int &Coin, int &ErrorCode);

   
    int IncrementCoinCounter(int Coin);
    void ClearAllCoinCounter(void);
    void ClearCoinCounter(int Coin);

    int CoinCounter(int Coin);
    double CoinTotal(int Coin);
    int CoinSum(int First= 0, int Last= EMP_CCT_MAX_COINS);
    double CoinTotalSum(int First= 0, int Last= EMP_CCT_MAX_COINS);
    //@}

};

#endif	/* _CCTALKEMPDEVICE_H */

