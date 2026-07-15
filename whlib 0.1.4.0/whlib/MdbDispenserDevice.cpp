////////////////////////////////////////////////////////////////////////////////
// File:   MdbDispenserDevice.cpp
// Author: manfred
// Copyright 2010:
//              wh Münzprüfer Berlin GmbH
//              Teltower Damm 276
//              D-10707 Berlin
//              Germany
//              info@whberlin.de
//
// Created: 5. November 2014 10:41
//
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
// Includes
#define	_MDBDispenserDEVICE_CPP_SRC

#include "stdlib.h"
#include "stdio.h"
#include "string.h"
#include "MdbDispenserDevice.h"

////////////////////////////////////////////////////////////////////////////////
// Macros
//

#define CHECK_IS_OPEN()    {if(!IsOpen()) return SetError(SIO_OPEN_ERR);}

// for debugging
#define MDB_CL_DEBUG_OUTPUT(a,b,c,d) fprintf(stderr,(a),(b),(c),(d))

////////////////////////////////////////////////////////////////////////////////
// Constructor / Destructor
//


////////////////////////////////////////////////////////////////////////////////
// Constructor / Destructor

MdbDispenserDevice::MdbDispenserDevice()
{
}

MdbDispenserDevice::~MdbDispenserDevice()
{
}

//////////////////////////////////////////////////////////////////////
/// Overwrites parent function
/// \return error code, 0= no error

int MdbDispenserDevice::Reset(void)
{

    CHECK_IS_OPEN();

    memset(&m_SetupResponse, 0,  sizeof(m_SetupResponse));

    return MdbDevice::Reset();
}

//////////////////////////////////////////////////////////////////////
/// Setup reads the configuration of the dispenser device
/// Device response card reader features is stored class intern
/// in m_SetupResponse.
/// \param none
/// \return error code, 0= no error

int MdbDispenserDevice::Setup(void)
{
    int err;
    int len;

    CHECK_IS_OPEN();

    // clear buffers
    memset(&m_SetupResponse, 0,  sizeof(m_SetupResponse));

    len= sizeof(MdbDispenserSetupResponseData);
    err= SendCommand(MDB_CMD_DP_SETUP, NULL,
                     0,
                     (unsigned char*)&m_SetupResponse, len);

    return err;
}




// End MdbDispenserDevice.cpp
////////////////////////////////////////////////////////////////////////////////
