/* 
 * File:   CTimeOut.cpp
 * Author: manfred
 * 
 * Created on 5. November 2009, 16:07
 * Modified: 2019
 */

////////////////////////////////////////////////////////////////////////////////

#include "CTimeOut.h"
#include <stdlib.h>
#include <string.h>
#include <stdio.h>
#include <time.h>

#pragma pack(1)


////////////////////////////////////////////////////////////////////////////////

#define CTimeOutDebugOut(...) printf(__VA_ARGS__),fflush(stdout)

////////////////////////////////////////////////////////////////////////////////

#define BYTE unsigned char

// #define _RASP_PI

////////////////////////////////////////////////////////////////////////////////

#if USE_CLOCK_GETTIME


CTimeOut::CTimeOut()
{
    clock_gettime (CLOCK_REALTIME, &m_Timer);    
    memcpy(&m_TimeOut, &m_Timer, sizeof(m_StartTimeOut));      
    memcpy(&m_StartTimeOut, &m_Timer, sizeof(m_StartTimeOut));     
}

CTimeOut::~CTimeOut()
{

}


void CTimeOut::StartTimer(void)
{
    clock_gettime (CLOCK_REALTIME, &m_Timer);    
}

long CTimeOut::GetTimerMilliSeconds(void)
{
    long timer;
    struct timespec gettime;
    
    clock_gettime (CLOCK_REALTIME, &gettime);
    
    timer= (gettime.tv_sec - m_Timer.tv_sec) * 1000;
    timer+= (gettime.tv_nsec - m_Timer.tv_nsec) / 1000000;
       
    return timer;
}

void CTimeOut::StartSecTimeOut(int Tout)
{
    clock_gettime (CLOCK_REALTIME, &m_TimeOut);

    memcpy(&m_StartTimeOut, &m_TimeOut, sizeof(m_StartTimeOut));    // Startzeit merken 
    
    m_TimeOut.tv_sec+= Tout;                                        // ganze Sekunden
        
    return ;    
}

void CTimeOut::StartTimeOut(int Tout)
{  
    clock_gettime (CLOCK_REALTIME, &m_TimeOut);
    
    memcpy(&m_StartTimeOut, &m_TimeOut, sizeof(m_StartTimeOut));    // Startzeit merken 
    
    m_TimeOut.tv_sec+= Tout / 1000;				    // Sekunden Anteil
    m_TimeOut.tv_nsec+= ((unsigned long)Tout % 1000L) * 1000000L;   // nano Sek. Anteil
    m_TimeOut.tv_sec+= m_TimeOut.tv_nsec / 1000000000L;		    // Sekundenübertrag
    
    m_TimeOutValue= Tout;
    
    return ;
}

void CTimeOut::SetMilliSec(int Tout)
{
    StartTimeOut(Tout);
}

bool CTimeOut::Elapse(void)
{
    return IsTimeOut();
}

bool CTimeOut::IsTimeOut(void)
{
    bool elapsed= false;
    bool timechanged= false;
    struct timespec gettime;
    
    clock_gettime (CLOCK_REALTIME, &gettime);

    if(m_TimeOut.tv_sec < gettime.tv_sec)
	elapsed= true;
    else
    {
        if(m_TimeOut.tv_sec == gettime.tv_sec && m_TimeOut.tv_nsec <= gettime.tv_nsec)
	    elapsed= true;
    }

    // Negative Zeitänderung abfangen

    if(m_StartTimeOut.tv_sec > gettime.tv_sec)
	elapsed= timechanged= true;
    else
    {
        if(m_StartTimeOut.tv_sec == gettime.tv_sec && m_StartTimeOut.tv_nsec > gettime.tv_nsec)
	    elapsed= timechanged= true;
    }    
 
    if(timechanged)
         CTimeOutDebugOut("\r\nCToutTime Date Changes Error (%d,%d -> %d,%d)\r\n", m_StartTimeOut.tv_sec, m_StartTimeOut.tv_nsec,
                                                                gettime.tv_sec, gettime.tv_nsec);
            
    return elapsed;
}

void CTimeOut::DelayMilliSec(int Delay)
{
    StartTimeOut(Delay);

    while(!Elapse())
        ;
}	

void CTimeOut::GetRealTime(time_t & Seconds, long & NanoSeconds )
{
    struct timespec gettime;
    
    clock_gettime (CLOCK_REALTIME, &gettime);
    
    Seconds= gettime.tv_sec;
    NanoSeconds= gettime.tv_nsec;
}

void CTimeOut::GetTimeStamp(char * Stamp, int Resolution)
{
    time_t sec;
    long nsec;
    int i;
    char str[20];
    long div= 1000000000L;
    
    GetRealTime(sec, nsec);
    
    if(Resolution > 0)
    {
        for(i= 0; i < Resolution; i++)
        {
            div/= 10L;
            str[i]= (nsec / div) % 10 + '0';
        }
        str[i]= 0;
        
        sprintf(Stamp, "%02d:%02d:%02d.%s", (int)((sec/60/60) % 24), (int)((sec/60) % 60), (int)(sec % 60), str);
    }
    else
        sprintf(Stamp, "%02d:%02d:%02d", (int)((sec/60/60) % 24), (int)((sec/60) % 60), (int)(sec % 60));
}    

#define CTIME_T_STAMP_BUF_LEN   50

void CTimeOut::WriteTimeStamp(FILE *file, int Resolution)
{
    char *stamp;
    
    if(file)
    {
        stamp= new char[CTIME_T_STAMP_BUF_LEN];
        if(stamp)
        {
            memset(stamp, 0, CTIME_T_STAMP_BUF_LEN);
            GetTimeStamp(stamp, Resolution);
            fprintf(file, "%s", stamp);
            delete stamp;            
        }   
    }
}


#else	
	
////////////////////////////////////////////////////////////////////////////////	
	
CTimeOut::CTimeOut()
{
    m_TimerTicks= clock();
    m_msDiv= CLOCKS_PER_SEC / 1000L;
    if(m_msDiv < 1)
        m_msDiv= 1;
    struct timespec gettime_now;
    	clock_gettime (CLOCK_REALTIME, &gettime_now);

}

CTimeOut::~CTimeOut()
{

}

void CTimeOut::StartTimeOut(int Tout)
{
        GetTimer();
	m_TimeOut= m_msTimer + Tout;
  	return ;
}

void CTimeOut::SetMilliSec(int Tout)
{
    StartTimeOut(Tout);
}

bool CTimeOut::Elapse(void)
{
    return IsTimeOut();
}


bool CTimeOut::IsTimeOut(void)
{
    bool elapse;

    GetTimer();

#ifdef _RASP_PI
    elapse= ((m_msTimer ^ m_TimeOut) & 0x80000000) ? (m_msTimer & 0x7fffffff) < (m_TimeOut & 0x7fffffff)
                                               : (m_msTimer & 0x7fffffff) > (m_TimeOut & 0x7fffffff);
#else
    elapse= ((m_msTimer ^ m_TimeOut) & 0x8000) ? (m_msTimer & 0x7fff) < (m_TimeOut & 0x7fff)
                                               : (m_msTimer & 0x7fff) > (m_TimeOut & 0x7fff);
#endif
    return elapse;
}

void CTimeOut::GetTimer(void)
{
    clock_t t= clock();
//    unsigned long t= clock();
//    if(t < 0)
//        printf("clock() error\r\n");

//    m_msTimer= (t / m_msDiv) & 0xffff;
#ifdef _RASP_PI
       m_msTimer= (t / m_msDiv) & 0xffffffff;
#else
///    t&= 0xfffffff;
    m_msTimer= (t / m_msDiv) & 0xffff;
#endif       

}

void CTimeOut::DelayMilliSec(int Delay)
{
    StartTimeOut(Delay);

    while(!Elapse())
        ;
}
////////////////////////////////////////////////////////////////////////////////
#endif
////////////////////////////////////////////////////////////////////////////////