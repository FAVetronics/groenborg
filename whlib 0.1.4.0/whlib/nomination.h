/*
 * nomination.h
 *
 * Created: 21.11.2011 15:48:10
 *  Author: WOLLNY
 */ 


////////////////////////////////////////////////////////////////////////////////

#ifndef NOMINATION_H_
#define NOMINATION_H_

////////////////////////////////////////////////////////////////////////////////

#define NOM_ID_LEN 2
#define NOM_LIST_LEN 16
#define COIN_ID_STR_LEN 6
#define BILL_ID_STR_LEN 7

class Nomination {
protected:
	struct NOMINATION_ID {
		unsigned char Currency[NOM_ID_LEN];
		int Value;
#ifdef NOMINATION_POINT	
		char Point;
#endif
	}  m_Nomination; 
	
public:
	Nomination() {};
	~Nomination() {};	

	void FindCountryCode(unsigned short Code, unsigned char *Currency);
	char SetNomination(unsigned short CountryCode, unsigned short Value, unsigned short Scalefactor= 1);			
	char SetNomination(unsigned char * NomString, char IdLen, unsigned int ScaleFactor= 1, char Point= 0);	
	int GetValue(void);
	void SetValue(int Value);

	void GetCurrency(unsigned char *Nom1, unsigned char *Nom2);
#ifdef NOMINATION_POINT
	void GetDisplayStr(char * Dsp, char &Len, char Point= '.');		
#endif
};




#endif /* NOMINATION_H_ */
//
////////////////////////////////////////////////////////////////////////////////
