////////////////////////////////////////////////////////////////////////////////
/* 
 * File:   ccTalkCisDevice.h
 * Author: manfred
 *
 * Created on 13. April 2010, 11:54
 */

#include "ccTalkDevice.h"
#include "cisglobals.H"

#ifndef USE_QT
#define USE_QT      1
#endif

#define USE_STRING  1

#if USE_STRING
#include <string>
#endif

#if USE_QT
#include <QtGui/QApplication>
#endif

#ifndef _CCTALKCISDEVICE_H
#define	_CCTALKCISDEVICE_H



/// Device class for coin feeder CIS 100
////////////////////////////////////////////////////////////////////////////////
/// Defines the ccTalk communickation functions for coin feeders
////////////////////////////////////////////////////////////////////////////////


class ccTalkCisDevice : public CccTalkDevice  {
public:
    ccTalkCisDevice();
 //   ccTalkCisDevice(const ccTalkCisDevice& orig);
    virtual ~ccTalkCisDevice();

    /// @name General functions
    //@{
    int Restart();
    int GetStatus(int &Status, unsigned char &Flags, unsigned short &ErrFlags);

    int EjectOneCoin();
    int EjectCoins(int Coins);

    int GeneralLock();
    int Unlock();

    int ForceEject();
    int Reverse();
    //@}

    /// @name commands for connected devices CIF 100, EMR 100
    //@{
    int CifMagnetOpen();
    int CifMagnetClose();
    int EmrActivate();
    ///@}

    /// @name extended functions
    //@{
    int CapCommand(int Command, int *Status);
    int Calibrate(int Cmd, void *Data, int BufLen);
    //@}

    #if USE_STRING
    /// @name extended functions for debug and management
    //@{
    string GetStatusFlagsString(int Flags);
    string GetErrorMaskString(int Errors);
    //@}
    #endif
protected:
    struct CalValuesStruct {char bytes; } m_CalibData;
};

#endif	/* _CCTALKCISDEVICE_H */

////////////////////////////////////////////////////////////////////////////////
