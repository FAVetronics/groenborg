/*
 * CWEtools.cpp
 *
 * Created: 21.03.2012 17:24:37
 *  Author: WOLLNY
 */ 

//////////////////////////////////////////////////////////////////////////////// 
#include "stdlib.h"


//////////////////////////////////////////////////////////////////////////////// 
// Globale Hilfsfunktionen 

void SwapShort(void *Value)
{
	union {
		unsigned short s;
		unsigned char  b[2]; 
	} temp;
	unsigned char b;
			
	temp.s= *(unsigned short*)Value;
	b= temp.b[0];
	temp.b[0]= temp.b[1];
	temp.b[1]= b;
	*(unsigned short*)Value= temp.s;
}

unsigned short Bcd2Short(unsigned char *Bcd)
{
	unsigned short val;
	
	val= Bcd[1] & 0x0f;
	val+= (unsigned short)((Bcd[1] >> 4) & 0x0f) * 10;
	val+= (unsigned short)(Bcd[0] & 0x0f) * 100;
	val+= (unsigned short)((Bcd[0] >> 4) & 0x0f) * 1000;	 	
	
	return val;
}

//////////////////////////////////////////////////////////////////////////////// 
