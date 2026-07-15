/* 
 * File:   ccTalkHzwDevice.h
 * Author: manfred
 *
 * Created on 23. Juni 2010, 10:46
 */

#include "ccTalkDevice.h"

#ifndef _CCTALKHZWDEVICE_H
#define	_CCTALKHZWDEVICE_H

#define HZW_CCT_REQUEST_COIN_COUNT  90
#define HZW_CCT_TARA                93

/// Devices class for HZW 100.
////////////////////////////////////////////////////////////////////////////////
///

class ccTalkHzwDevice : public CccTalkDevice
{
public:
    ccTalkHzwDevice();
   // ccTalkHzwDevice(const ccTalkHzwDevice& orig);
    virtual ~ccTalkHzwDevice();

    enum HZW_REQUEST_COUNT {
        HZW_DEFAULT,            // default
        HZW_ADC,
        HZW_COINS,
        HZW_WEIGHT
    };

    int Tara(void);
    int RequestCointCount(unsigned int &Count, unsigned int &Coin, enum HZW_REQUEST_COUNT Mode= HZW_DEFAULT);

private:

};

#endif	/* _CCTALKHZWDEVICE_H */

