/* 
 * File:   ccTalkEmpDevice.cpp
 * Author: manfred
 * 
 * Created on 20. April 2010, 14:41
 */

#include "ccTalkEmpDevice.h"

#define EMP_COIN_INDEX_ERR -1

#define CHECK_IS_OPEN()    {if(!IsOpen()) return SetError(SIO_OPEN_ERR);}
#define CHECK_COIN_INDEX(a,b)  {if((a)<0 || (a)>=EMP_CCT_MAX_COINS) return (b);}
#define CHECK_COIN_INDEX_NR(a) {if((a)<0 || (a)>=EMP_CCT_MAX_COINS) return ;}

ccTalkEmpDevice::ccTalkEmpDevice()
{
    memset(m_ValDataList, 0, sizeof(m_ValDataList));
    m_BaseYear= 2000;   // as default for wh Coinselectors 
                        // set to zero 
                        // for automaticaly requesting on Request...Date
    m_LastPollCounter= 0;
    m_LastPollEvent= 0;
    memset(&m_BufferdCredits,0 , sizeof(m_BufferdCredits));
}


ccTalkEmpDevice::~ccTalkEmpDevice()
{
    Close();
}


//////////////////////////////////////////////////////////////////////
/// Overloaded Close() function resets local members
/// \param RevStr reference to string class
/// \return error code, 0= no error

void ccTalkEmpDevice::Close(void)
{
   memset(m_ValDataList, 0, sizeof(m_ValDataList));
   m_BaseYear= 2000;
   m_LastPollCounter= 0;
   m_LastPollEvent= 0;
   CccTalkDevice::Close();
}


//////////////////////////////////////////////////////////////////////
/// Returns polling priority (249)
/// \param Unit refenece to returned multiplier for the value
/// \param Value reference to returned value
/// \return error code, 0= no error

int ccTalkEmpDevice::RequestPollingPriority(int &Unit, int &Value) // 249
{
    int err;
    int len= EMP_CCT_REQUEST_POLLING_PRIORITY_LEN;
    unsigned char buf[EMP_CCT_REQUEST_POLLING_PRIORITY_LEN];

    CHECK_IS_OPEN();

    err= m_ccTalk->RequestBinData(m_DeviceAdr, EMP_CCT_REQUEST_POLLING_PRIORITY_LEN, len, &buf);
    if(!err)
    {
        Unit= buf[0];
        Value= buf[1];
    }
    return err;
}

//////////////////////////////////////////////////////////////////////
/// Request coin acceptor status
/// \param Status refenece to returned status
/// \return error code, 0= no error

int ccTalkEmpDevice::RequestStatus(int &Status)                     // 248
{
    int err;
    int len= 1;

    CHECK_IS_OPEN();

    Status= 0;

    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_REQUEST_STATUS, len, &Status);

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Test soloids of the device. The bitmask indicates which soloids to operate.
/// \param Mask Bit 0= accept gate, 1... sorter
/// \return error code, 0= no error

int ccTalkEmpDevice::TestSolenoids(int Mask)                        // 240
{
    int err;

    CHECK_IS_OPEN();

    err= m_ccTalk->SendData(m_DeviceAdr, CCT_TEST_SOLENOIDS, 1, &Mask);

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Test output lines
/// \param Mask Bitmask for pulsin lines, product depending
/// \return error code, 0= no error
int ccTalkEmpDevice::TestOutputLines(int Mask)                      // 238
{
    int err;

    CHECK_IS_OPEN();

    err= m_ccTalk->SendData(m_DeviceAdr, CCT_TEST_OUTPUT_LINES, 1, &Mask);

    return err;
}

//////////////////////////////////////////////////////////////////////
/// Set latched output lines
/// \param Mask for latch outputs
/// \return error code, 0= no error

int ccTalkEmpDevice::LatchOutputLines(int Mask)                     // 233
{
    int err;

    CHECK_IS_OPEN();

    err= m_ccTalk->SendData(m_DeviceAdr, CCT_LATCH_OUTPUT_LINES, 1, &Mask);

    return err;
}



////////////////////////////////////////////////////////////////////////////////
/// Return the last 5 events of credit or error codes. This information queue
/// allows a host polling the device with low cycle rate
/// \param Credits pointer to the receive buffer
///  - 1. Byte counter 1...255. \n
///    This counter is reset to zero only at power on or at ResetDevice() command
///  - 5x 2 Bytes Queue
///  -- 1. Byte coin 0= no coin accepded error accured,  1-16 Coin
///  -- 2. Byte error code 0= none (coin accepted) or error code
/// \return error code, 0= no error

int ccTalkEmpDevice::ReadBufferedCredit(int &Count, unsigned char *Credits)     // 229
{
    int err;

    CHECK_IS_OPEN();

    err= ReadBufferedCredit();
    if(!err)
    {
        Count= m_BufferdCredits.Count;
        memcpy(Credits, m_BufferdCredits.Credit, sizeof(m_BufferdCredits.Credit));
    }
    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Return the last 5 events of credit or error codes. This information queue
/// allows a host polling the device with low cycle rate
/// \param Credits pointer structure BUFFERED_CREDIT, see ReadBufferedCredit(int, unsigened char)
/// \return error code, 0= no error

int ccTalkEmpDevice::ReadBufferedCredit(CreditBuffer *Credits)  // 229
{
    int err;

    CHECK_IS_OPEN();

    int buflen= sizeof(m_BufferdCredits);

    if(Credits == NULL)
        Credits= &m_BufferdCredits;

    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_READ_BUFFERED_CREDIT, buflen, Credits);

    return err;
}


////////////////////////////////////////////////////////////////////////////////
/// Override the sorter path
/// \param Mask 8 bit mask overiding the sorter path
///   - 0= override to default
///   - 1= no changes, normal sorting
/// \return error code, 0= no error

int ccTalkEmpDevice::ModifySorterOverrideStatus(unsigned short Mask) // 222
{
    int err;

    CHECK_IS_OPEN();

    err= m_ccTalk->SendData(m_DeviceAdr, CCT_MODIFY_SORTER_OVERRIDE, 2, &Mask);

    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Return the sorter override Status, see ModifySorterOverrideStatus()
/// \param Mask reference to return 8 bit mask overiding the sorter status
///   - 0= override to default
///   - 1= no changes, normal sorting
/// \return error code, 0= no error

int ccTalkEmpDevice::RequestSorterOverrideStatus(unsigned short &Mask) // 221
{
    int err;
    int buflen= sizeof(unsigned short);

    CHECK_IS_OPEN();

    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_REQUEST_SORTER_OVERRIDE, buflen, &Mask);

    return err;
}


////////////////////////////////////////////////////////////////////////////////
/// Return the sorter override Status, see ModifySorterOverrideStatus()
/// \param Mask reference to return 8 bit mask overiding the sorter status
///   - 0= override to default
///   - 1= no changes, normal sorting
/// \return error code, 0= no error

int ccTalkEmpDevice::RequestCoinPosition(int Coin, unsigned short &Mask) // 212
{
    int err;
    int buflen= sizeof(unsigned short);
    char cmd= Coin+CCT_EMP_COIN_OFFSET;

    CHECK_IS_OPEN();
    CHECK_COIN_INDEX(Coin,EMP_COIN_INDEX_ERR);

    err= m_ccTalk->RequestBinDataEx(m_DeviceAdr, CCT_REQUEST_COIN_POSITION, &cmd, 1, &Mask, buflen);

    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Set new sorter path for a coin
/// \param Coin coin Index (zero based)
/// \param Path new sorter path
/// \return error code, 0= no error

int ccTalkEmpDevice::ModifySorterPath(int Coin, int Path)           // 210
{
    int err = 0;
    unsigned char buf[2];

    CHECK_IS_OPEN();
    CHECK_COIN_INDEX(Coin, EMP_COIN_INDEX_ERR);

    buf[0]= Coin+CCT_EMP_COIN_OFFSET;
    buf[1]= Path;

    err= m_ccTalk->SendData(m_DeviceAdr, CCT_MODIFY_SORTER_PATH, 2, buf);

    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Return the sorter path of a coin
/// \param Coin coin Index (zero based)
/// \param Path reference to the return value of the sorter path
/// \return error code, 0= no error

int ccTalkEmpDevice::RequestSorterPath(int Coin, int &Path)         // 209
{
    int err;
    int buflen= 1;
    char cmd= Coin+CCT_EMP_COIN_OFFSET;
    Path= 0;

    CHECK_IS_OPEN();
    CHECK_COIN_INDEX(Coin, EMP_COIN_INDEX_ERR);

    err= m_ccTalk->RequestBinDataEx(m_DeviceAdr, CCT_REQUEST_SORTER_PATH, &cmd, 1, &Path, buflen);

    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// not impemented

int ccTalkEmpDevice::TeachModeControl(int Coin)                     // 202
{
    int err = 0;

    CHECK_IS_OPEN();
    CHECK_COIN_INDEX(Coin, EMP_COIN_INDEX_ERR);
    Coin= 0;

    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// not impemented

int ccTalkEmpDevice::TeachModeControl(int Coin, int Orientation)    // 202
{
    int err = 0;

    CHECK_IS_OPEN();
    CHECK_COIN_INDEX(Coin, EMP_COIN_INDEX_ERR);
    Coin= 0;
    Orientation= 0;

    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// not impemented

int ccTalkEmpDevice::RequestTeachStatus(int &Coins, int &Status, bool Abbort)     // 201
{
    int err = 0;

    CHECK_IS_OPEN();

    
    Coins= 0;
    Status= 0;
    Abbort= false;

    return err;
}




////////////////////////////////////////////////////////////////////////////////
/// Return counter of rejected coins
/// \param Counter reference to returned counter value unsigned max 24 Bit
/// \return error code, 0= no error

int ccTalkEmpDevice::RequestRejectCounter(long &Counter)            // 194
{
    int err;
    int buflen= 3;

    CHECK_IS_OPEN();

    Counter= 0;
    err= m_ccTalk->RequestBinData(m_DeviceAdr, CCT_REQUEST_REJECT_COUNTER, buflen, &Counter);

    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Return thermister value in degree Celsius
/// \param Degress reference to returned degree value, range is -128...+127
/// \return error code, 0= no error

int ccTalkEmpDevice::RequestThermistorReading(int &Degrees)        // 173
{
    int err = 0;

    CHECK_IS_OPEN();

    Degrees= 20;

    return err;
}


// wh spezific functions

int ccTalkEmpDevice::ModifyCoinPrecision(const unsigned char *Buffer)
{
    int err = 0;
    unsigned buf[1];

    CHECK_IS_OPEN();
    buf[0]= *Buffer;         // dummy

    return err;
}

int ccTalkEmpDevice::RequestCoinPrecision(unsigned char *Buffer)
{
    int err = 0;

    CHECK_IS_OPEN();
    *Buffer= 0;         // dummy
    return err;
}

////////////////////////////////////////////////////////////////////////////////
// Extended Functions

////////////////////////////////////////////////////////////////////////////////
/// Poll EMP for accepted coins or errors. This funciotn return a counter of new
/// events in the CrediBuffer queue. For that it implzic calls the
/// ReadBufferedCredit() funcktin and calulate the count of new events in the queue
/// \param EventCount reference to returned new events counter if zero non new event
/// occured
/// \param CreditBuffer reference to struct with event queue, see also function
/// ReadBufferedCredit()
/// \return error code, 0= no error

int ccTalkEmpDevice::Poll(int &EventCount, CreditBuffer *Credits)
{
    int err;
    EventCount= 0;
    int coincnt;
    CHECK_IS_OPEN();

    err= ReadBufferedCredit(Credits);

    if(!err)
    {
        coincnt= Credits->Count;
        if(coincnt == 0)            // Reset
	{
            m_LastPollCounter= 0;
	}
	else
	{
            if(m_LastPollCounter > coincnt)
		EventCount= coincnt + 255 - m_LastPollCounter;
            else
		EventCount= coincnt - m_LastPollCounter;
            m_LastPollCounter= coincnt;
        }
    }

    if(EventCount > EMP_CCT_BUFFERED_CREDITS_QUEUE)
    {
        err= EMP_CCT_ERR_LOST_EVENTS;
        EventCount %= EMP_CCT_BUFFERED_CREDITS_QUEUE;
    }
    return err;
}

////////////////////////////////////////////////////////////////////////////////
/// Poll EMP for accepted coins or errors. Use these function to get separted
/// events received from the coin accepter. It returns the next event (index of
/// accepted coin or deteded error code) places in the buffered queue. If the
/// queue is empty the function Poll() ist impizit called. \n
/// Call these function to get the next event as long as no new event is signed.
/// \param EventCount reference to returned new events counter. If zero non new
/// event occured.
/// \param Coin reference to returned coin index. If zero no coin is accepted.
/// \param ErrorCode reference to returned error code. If zero nor error occured.
/// \return error code, 0= no error

int ccTalkEmpDevice::PollNext(int &EventCount, int &Coin, int &ErrorCode)
{
    int err= 0;

    CHECK_IS_OPEN();

    EventCount= 0;
    CHECK_IS_OPEN();

    if(m_LastPollEvent == 0)    // no events - request new data
    {
        err= Poll(m_LastPollEvent, &m_BufferdCredits);
        if(err)
        {
            m_LastPollEvent= 0;
            EventCount= 0;
            return err;
        }
    }

    if(m_LastPollEvent > 0)     // events in buffer ?
    {
        Coin= m_BufferdCredits.Credit[m_LastPollEvent-1].Coin;
        ErrorCode= m_BufferdCredits.Credit[m_LastPollEvent-1].Error;
        EventCount= m_LastPollEvent;
        m_LastPollEvent--;
    }

    return err;
}


int ccTalkEmpDevice::IncrementCoinCounter(int Coin)
{
    CHECK_COIN_INDEX(Coin,0);

    return m_ValDataList[Coin].Count++;
}

void ccTalkEmpDevice::ClearCoinCounter(int Coin)
{
    CHECK_COIN_INDEX_NR(Coin);

    m_ValDataList[Coin].Count= 0;
    m_ValDataList[Coin].Total= 0.0;
}

void ccTalkEmpDevice::ClearAllCoinCounter(void)
{
    int i;

    for(i= 0; i < EMP_CCT_MAX_COINS; i++)
    {
        m_ValDataList[i].Count= 0;
        m_ValDataList[i].Total= 0.0;
    }
}

int ccTalkEmpDevice::CoinCounter(int Coin)
{
    CHECK_COIN_INDEX(Coin,0);

    return m_ValDataList[Coin].Count;
}

double ccTalkEmpDevice::CoinTotal(int Coin)
{
    CHECK_COIN_INDEX(Coin,0);

    return m_ValDataList[Coin].Total;
}

int ccTalkEmpDevice::CoinSum(int First, int Last)
{
    int i;
    int sum= 0;

    CHECK_COIN_INDEX(First,0);
    CHECK_COIN_INDEX(Last,0);

    for(i= First; i < Last; i++)
        sum+= m_ValDataList[i].Count;

    return sum;
}

double ccTalkEmpDevice::CoinTotalSum(int First, int Last)
{
    int i;
    double sum= 0.0;

    CHECK_COIN_INDEX(First,0.0);
    CHECK_COIN_INDEX(Last,0.0);

    for(i= First; i < Last; i++)
        sum+= m_ValDataList[i].Total;

    return sum;
}

#if USE_QT
QString ccTalkEmpDevice::CoinTotalStr(int Coin)
{
    QString str;

    CHECK_COIN_INDEX(Coin,"");

    str.sprintf(m_ValDataList[Coin].CountryCode.Format, m_ValDataList[Coin].Total);

    return str;
}

QString ccTalkEmpDevice::CoinTotalSumStr(int First, int Last)
{
    QString str;

    CHECK_COIN_INDEX(First,"");
    CHECK_COIN_INDEX(Last,"");

    str.sprintf(m_ValDataList[First].CountryCode.Format, CoinTotalSum(First, Last));

    return str;
}
#endif