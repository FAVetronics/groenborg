/* 
 * File:   CTimeOut.h
 * Author: manfred
 *
 * Created on 5. November 2009, 16:07
 */
////////////////////////////////////////////////////////////////////////////////

#ifndef _CTIMEOUT_H
#define	_CTIMEOUT_H

#include <time.h>
#include <stdio.h>
////////////////////////////////////////////////////////////////////////////////

#ifndef USE_CLOCK_GETTIME
#define USE_CLOCK_GETTIME 1
#endif

////////////////////////////////////////////////////////////////////////////////

class CTimeOut
{
public:
    CTimeOut();
    virtual ~CTimeOut();

    void StartTimeOut(int Tout);
    void StartSecTimeOut(int Tout);
    bool IsTimeOut();
    void SetMilliSec(int Tout);
    bool Elapse(void);
    void DelayMilliSec(int Delay);

    void StartTimer(void);
    long GetTimerMilliSeconds(void);

    static void GetRealTime(time_t & Seconds, long & NanoSeconds );
    static void GetTimeStamp(char * Stamp, int Resolution= 3);
    static void WriteTimeStamp(FILE *file= stdout, int Resolution= 3);
    
#if !USE_CLOCK_GETTIME
    void GetTimer(void);
#endif

    
private:
    
#if USE_CLOCK_GETTIME
    struct timespec m_StartTimeOut;
    struct timespec m_TimeOut;
    struct timespec m_Timer;
    short m_TimerTicks;
    short m_msTimer;
    short m_msDiv;
    long m_TimeOutValue;
#else
    clock_t m_TimeOut;
    clock_t m_TimerTicks;
    clock_t m_msTimer;
    clock_t m_msDiv;
    //long m_TimeOut;
//    struct timespec m_LastTime; 
    //long int m_LastTime;
    //long int m_TimeOut;
#endif
    
};

#endif	/* _CTIMEOUT_H */
////////////////////////////////////////////////////////////////////////////////
