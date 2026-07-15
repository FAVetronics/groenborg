// ccTalkTwsDevice.h: clase for TWS 100 Device from wh Münzprüfer GmbH
//
//////////////////////////////////////////////////////////////////////

#ifndef _CCTALKTWSDEVICE_H
#define _CCTALKTWSDEVICE_H

//////////////////////////////////////////////////////////////////////

#include "ccTalkDevice.h"
#include "twsglobals.h"


//////////////////////////////////////////////////////////////////////
/// class for TWS 100 device - stillunder developing
////////////////////////////////////////////////////////////////////////////////
/// This class is still under developing. Many actual funktions are not included
/// jet. But it can useful as a base class for own expansions.
///
class CccTalkTwsDevice : public CccTalkDevice  
{
public:
    CccTalkTwsDevice();
    virtual ~CccTalkTwsDevice();

/// @name Base - Commands
//@{
    int Restart(void);
    int GetStatus(int &Status, unsigned char &Flags, unsigned short &ErrFlags);
//@}

/// @name Coin Flow - Commands
//@{
    int EjectCoins(int Slot, bool EjectEmpty);
    int InjectCoin(int Coin, int Mode= 0);
    int StopEject(void);
    int ContinueEject(void);
    int AbortEject(void);
    int FlushCoins(void);
//@}

/// @name Extended Coin Management - Commands
//@{
    int ModifyMagazineCoin(int Pos, int CoinId, int Mode);
    int RequestSorterEjects(int *Counter, bool ResetCounter);
    int RequestMagazineCoins(int &First, int &Count, unsigned char *Data, int DataBytes);
    int RequestCoinCount(int &Count);
//@}
};

#endif // _CCTALKTWSDEVICE_H
