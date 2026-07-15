////////////////////////////////////////////////////////////////////////////////
// File:   ccTalkPayoutDevice.h
// Author: Manfred Wollny
// Copyright 2010:
//              wh Münzprüfer Berlin GmbH
//              Teltower Damm 276
//              D-10707 Berlin
//              Germany
//              info@whberlin.de
//
// Created: 2. August 2010 13:42
//
////////////////////////////////////////////////////////////////////////////////



#ifndef _CCTALKPAYOUTDEVICE_H
#define	_CCTALKPAYOUTDEVICE_H

#include "ccTalkDevice.h"

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




/// Class for payout devices
////////////////////////////////////////////////////////////////////////////////
/// Manage hopper devices

class ccTalkPayoutDevice : public CccTalkDevice
{
public:
    ccTalkPayoutDevice();
    virtual ~ccTalkPayoutDevice();

public:
    /// Encryption mode
    enum ENCRYPTION_MODE {
        ENCRYPT_UNDEF,
        ENCRYPT_NONE,
        ENCRYPT_SERIAL
    };


    /// @name management functions
    //@{gg
    int ModifyPayoutAbsoluteCount(unsigned int Count);
    int ModifyPayoutAbsoluteCount(int Hopper, unsigned int Count);
    int RequestPayoutAbsoluteCount(unsigned int &Count);
    int RequestPayoutAbsoluteCount(int Hopper, unsigned int &Count);
    int RequestPayoutCapacity(int Hopper, unsigned int &Count);
    int RequestPayoutCapacity(unsigned int &Count);
    int ModifyPayoutCapacity(int Hopper, unsigned int Count);
    int ModifyPayoutCapacity(unsigned int Count);
    int ModifyPayoutFloat(unsigned int Level);
    int ModifyPayoutFloat(int Hopper, unsigned int Level);
    int RequestPayoutFloat(unsigned int &Level);
    int RequestPayoutFloat(int Hopper, unsigned int &Level);
    int RequestHopperDispenseCount(unsigned int &CoinCount);
    int ModifyVariableSet(unsigned char * VarSet, int Len);
    int RequestHopperCoinValue(char *Id, int &Value);
    //@}

    /// @name control functions
    //@{
    int EmergencyStop(unsigned int &Coins);
    int DispenseHopperCoins(unsigned char *DispenseData, int Len, unsigned int &Event);
    int RequestHopperStatus(int &EventCounter, int &Remaining, int &Paid, int &Unpaid);
    int EnableHopper(bool Enable= true);
    int RequestHopperPollingValue(int &EventCounter, int &Remaining, int &Paid, int &Unpaid);
    int EmergencyStopValue(int &Remaining);
    int RequestIndexedHopperDispenseCount(int Coin,  unsigned int *Count);
    //@}

    /// @name encryption functions
    //@{
    int PumpRng(unsigned char * Vars, int Len);
    int RequestCipherKey(unsigned char *Key, int &Len);
    //@}

    /// @name enhanced functions
    //@{
    int SetEncryptionMode(enum ENCRYPTION_MODE Encryption);
    int Open(int Adr, CccTalk *ccTalk, enum ENCRYPTION_MODE Encryption= ENCRYPT_UNDEF);
    int Dispense(int Count, enum ENCRYPTION_MODE Encryption= ENCRYPT_UNDEF);

    //@}

    /// @name extended functions using string class
    //@{
#if USE_STRING
    int RequestHopperCoin(string &CoinStr);
#endif
    //@}

private:
    enum ENCRYPTION_MODE m_Encrytion;

};

#endif	/* _CCTALKPAYOUTDEVICE_H */

// End ccTalkPayoutDevice.h
////////////////////////////////////////////////////////////////////////////////

