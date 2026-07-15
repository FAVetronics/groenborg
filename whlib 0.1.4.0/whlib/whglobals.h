/* 
 * File:   whglobals.h
 * Author: Manfred Wollny
 *
 * Created on 30. März 2010, 12:09
 */

#ifndef _WHGLOBALS_H
#define	_WHGLOBALS_H

#ifdef	__cplusplus
extern "C" {
#endif

////////////////////////////////////////////////////////////////////////////////

#define ERROR_GROUP_SIZE 100
#define ERROR_GROUP(g) (g*ERROR_GROUP_SIZE)

    enum GLOBAL_ERRORS {
        // serial I/O
        SIO_ERROR = ERROR_GROUP(1),
        SIO_OPEN_ERR,
        SIO_READTOUT_ERR,
        SIO_NOT_OPEN_ERR,
        SIO_SEND_BYTE_ERR,
        SIO_SENDBUFFER_ERR,
        SIO_SENDBUFFER_PAR_ERR,
        SIO_RECEIVE_BYTE_ERR,
        SIO_RECEIVE_BUFFER_ERR
    };

////////////////////////////////////////////////////////////////////////////////
// Macros
// error Handling

#define SetError(e) -e
// insert error loggin here


#ifdef	__cplusplus
}
#endif

#endif	/* _WHGLOBALS_H */

