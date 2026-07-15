/*
 * nomination.cpp
 *
 * Created: 21.11.2011 15:46:11
 *  Author: WOLLNY
 */ 

////////////////////////////////////////////////////////////////////////////////

#include "stdlib.h"
#include "ctype.h"
#include "string.h"
#include "nomination.h"

////////////////////////////////////////////////////////////////////////////////
/* [CurrencyCodes]
CC0000=Token|...|000|TK
CC1978=Euro |EUR|1978|EU
CC1234=Duckburg Dollar |DBD|1234|DB
CC1953=Pacific Franc |XPF|1953|PF
CC1840=US Dollar |USD|1840|US
CC1124=Canadian Dollar |CAD|1124|CA
CC1826=Pound Sterling |GBP|1826|GB
CC1036=Australian Dollar |AUD|1036|AU
CC1208=Danish Krone |DKK|1208|DK
CC1703=Slovakian Crown |SKK|1703|SK
CC1578=Norvegian Crown |NOK|1578|NO
CC1752=Swedish Crown |SEK|1752|SE
CC1756=Swiss Franc |CHF|1756|CH
CC1214=Dominican Peso |DOP|1214|DO
CC1702=Singapore Dollar|SGD|1702|SG
CC1348=Hungarian Forint|HUF|1348|HU
CC1980=Ukrainian Hryvnia |UAH|1980|UA
CC1060=Bermudian Dollar |BMD|1060|BM
CC1810=Russian Ruble |RUB|1810|RU
CC1170=Colombian Peso |COP|1170|CO
CC1191=Croatian Kuna |HRK|1191|HR
CC1554=New Zealand Dollar |NZD|1554|NZ
CC1203=Czech Koruna |CZK|1203|CZ
CC1901=Taiwan Dollar |TWD|1901|TW
CC1400=Jordanian Dinar |JOD|1400|JO
CC1484=Mexican Peso |MXN|1484|MX
CC1985=Polish Zloty |PLN|1985|PL
CC1376=Israeli Sheqel |ILS|1376|IL
CC1480=Mauritius Rupee |MUR|1480|MU
CC1705=Slovenian Tolar |SIT|1705|SI
CC1428=Latvian Lats |LVL|1428|LV
CC1440=Lithuanian Litas |LTL|1440|LT
CC1233=Estonian Kroon |EEK|1233|EE
CC1196=Cyprus Pound |CYP|1196|CY
CC1792=Turkish Lira - Old |TRL|1792|TR
CC1949=Turkish Lira - New |TRY|1949|TR
CC1032=Argentine Peso |ARS|1032|AR
CC1446=Macau Pataca |MOP|1446|MO
CC1356=Indian Rupee |INR|1356|IN
CC1784=United Arab Emirates Dirham |AED|1784|AE
CC1642=Romanian Leu - Old |ROL|1642|RO
CC1946=Romanian Leu - New |RON|1946|RO
CC1398=Kazakhstan Tenge |KZT|1398|KZ
CC1710=South African Rand |ZAR|1710|ZA
CC1048=Bahraini Dinar |BHD|1048|BH|3
CC1352=Iceland Krona |ISK|1352|IS
CC1512=Rial Omani |OMR|1512|OM|3
CC1788=Tunisian Dinar |TND|1788|TN|3
CC1977=Bosnia-Herzegovina Convert. Marks |BAM|1977|BA
CC1398=Kazakhstan Tenge |KZT|1398|KZ
CC1634=Qatari Rial |QAR|1634|QA
CC1100=Bulgarian Lev - Old |BGL|1100|BG
CC1975=Bulgarian Lev - New |BGN|1975|BG
CC1504=Moroccan Dirham |MAD|1504|MA
CC1344=Hong Kong Dollar |HKD|1344|HK
CC1604=Peruvian Nuevo Sol |PEN|1604|PE
CC1470=Maltese Lira |MTL|1470|MT
CC1422=Lebanese Pound |LBP|1422|LB
CC1012=Algerian Dinar |DZD|1012|DZ
CC1392=Japanese Yen |JPY|1392|JP|0
CC1590=Panama Balboa|PAB|1590|PA
CC1156=Chinese Yuan Renminbi |CNY|1156|CN
CC1953=New Caledonian Franc |XPF|1953|XP|0
CC1008=Albanian Lek |ALL|1008|AL
CC1458=Malaysian Ringgit |MYR|1458|MY
CC1152=Chilean Peso |CLP|1152|CL
CC1388=Jamaican Dollar |JMD|1388|JM
CC1986=Brazilian Real |BRL|1986|BR
CC1170=Colombian Peso |COP|1170|CO
CC1608=Philippine Peso |PHP|1608|PH
CC1404=Kenyan Shilling |KES|1404|KE
CC1807=Macedonian Denar |MKD|1807|MK
CC1031=Azerbaijanian Manat - New |AZN|1944|AZ
CC1764=Thailandian Baht |THB|1764|TH
CC1242=Fiji Dollar |FJD|1242|FJ
CC1068=Bolivian Boliviano |BOB|1068|BO
CC1188=Costa Rican Colon |CRC|1188|CR
CC1532=Antillian Guilder |ANG|1532|AN
CC1516=Namibia Dollar |NAD|1516|NA
CC1981=Georgian Lari |GEL|1981|GE
CC1818=Egyptian Pound |EGP|1818|EG
CC1288=Ghanaian Cedi - Old |GHC|1288|GH
CC1936=Ghanaian Cedi - New |GHS|1936|GH
CC1800=Uganda Shilling |UGX|1800|UG
CC1646=Rwanda Franc |RWF|1646|RW
CC1760=Syrian Pound |SYP|1760|SY
CC1682=Saudi Riyal |SAR|1682|SA 
CC1891=Serbia and Monten. Dinar |YUM|1891|YU
CC1941=Serbian Dinar|RSD|1941|RS
CC1136=Cayman Islands Dollar|KYD|1136|KY
CC1410=South Korean Won|KRW|1410|KR|0
CC1408=North Korean Won|KPW|1408|KP
CC1364=Iranian Rial|IRR|1364|IR
CC1051=Armenian Dram|AMD|1051|AM
CC1360=Indonesian Rupiah|IDR|1360|ID 
CC1320=Guatemalan Quetzal|GTQ|1320|GT
CC1943=Mozambique Metical|MZN|1943|MZ
CC1934=Turkmenian Manat - New|TMT|1934|TM
CC1862=Venezuelan Bolívar |VEB|1862|VE
CC1937=Venezuelan Bolívar Fuerte|VEF|1937|VE
CC1950=CFA-Franc BEAC|XAF|1950|XA
*/
/*
Eurozone                                            EUR                       vorhanden         
Schweiz                                               CHF                       vorhanden
Großbritannien                                GBP                       vorhanden
Ungarn                                                HUF                       vorhanden         
Rumänien                                          RON                      vorhanden         
Polen                                                   PLN                       vorhanden
Tschechien                                        CZK                        vorhanden
Litauen                                                LTL                         vorhanden
Letland                                                LVL                        vorhanden
Bulgarien                                            BGN                      vorhanden
Kroatien                                              HRK                       vorhanden
Serbien                                               RSD                       vorhanden
Bosnien-Herzegowina                  BAM                     vorhanden         
Mazedonien                                     MKD                     
Albanien                                             ALL
Türkei                                                  TRY
Schweden                                         SEK                        vorhanden         
Norwegen                                         NOK                      vorhanden
Dänemark                                          DKK                       vorhanden
Russland                                             RUB                       vorhanden
Ukraine                                               UAH                      vorhanden
Weißrussland                                   BYR                        vorhanden
Aserbaidschan                                 AZN
Kasachstan                                        KZT
Georgien                                            GEL

*/

struct COUNTRY_CODE {
	unsigned short Code;
	char Currency[3];
} volatile g_CountryCodeTbl[]= {
	{1978, "EU"},			// Euro
	{1985, "PL"},			// Polish Zloty		
	{1203, "CZ"},			// Czech Koruna
	{1578, "NO"},			// Norvegian Crown
	{1348, "HU"},			// Hungarian Forint
	{1428, "LV"},			// Latvian Lats
	{1977, "BA"},			// Bosnia-Herzegovina Convert
	{1826, "GB"},			// Pound Sterling
	{1233, "EE"},			// Estonian Kroon
	{1810, "RU"},			// Russian Ruble alt 
	{1643, "RU"},		    // Russian Ruble neu  
	{1100, "BG"},			// Bulgarian Lev - Old
	{1975, "BG"},			// Bulgarian Lev - Old
	{1974, "BY"},			// Weißrussland (Belarus)
	{1756, "CH"},			// Swiss Franc
	{1440, "LT"},			// Lithuanian Litas
	{1191, "HR"},			// Croatian Kuna
	{1941, "RS"},			// Serbische Dinar
	{1752, "SE"},			// Swedish Crown
	{1208, "DK"},			// Danish Krone
	{1642, "RO"},			// Romanian Leu - Old
	{1946, "RO"},			// Romanian Leu - New 
	{1980, "UA"},			// Ukrainian Hryvnia	

// ab CWE Version 0.01.03.05	10.04.2013
	{1807, "MK"},			// Mazedonien			MKD	CC1807=Macedonian Denar |MKD|1807|MK                     
	{1008, "AL"},			// Albanien				ALL	CC1008=Albanian Lek |ALL|1008|AL
	{1792, "TR"},			// Türkei				TRY	CC1792=Turkish Lira - Old |TRL|1792|TR
	{1949, "TR"},			// Türkei				TRY	CC1949=Turkish Lira - New |TRY|1949|TR
	{1944, "AZ"},			// Aserbaidschan        AZN	CC1031=Azerbaijanian Manat - New |AZN|1944|AZ
	{1398, "KZ"},			// Kasachstan           KZT CC1398=Kazakhstan Tenge |KZT|1398|KZ
	{1981, "GE"},			// Georgien             GEL CC1981=Georgian Lari |GEL|1981|GE

// ab 0.01.04.00 vom 01.07.2013
	{   7, "RU"},			// Russian Ruble
	{1051, "AM"},			// Armenian Dram
	{1498, "MD"},			// Leu, Moldawien
	{1934, "TM"},			// Turkmenistan    
	{1860, "UZ"},			// Usbekistan      
	{1417, "KS"},			// Kirgisistan     
	{1972, "TD"},			// Tadschikistan   

	{0, "TK"}				// Token
};




void Nomination::FindCountryCode(unsigned short Code, unsigned char *Currency)
{
	unsigned char i;
	for(i= 0; g_CountryCodeTbl[i].Code; i++)
	{
		if(g_CountryCodeTbl[i].Code == Code)
			break;
	}
	Currency[0]= g_CountryCodeTbl[i].Currency[0];
	Currency[1]= g_CountryCodeTbl[i].Currency[1];
} 

char Nomination::SetNomination(unsigned short CountryCode, unsigned short Value, unsigned short Scalefactor)
{
	memset(&m_Nomination, 0, sizeof(m_Nomination));
	FindCountryCode(CountryCode, m_Nomination.Currency);
	m_Nomination.Value= Value*Scalefactor;
	return 0;
}

	
char Nomination::SetNomination(unsigned char * NomString, char IdLen, unsigned int ScaleFactor, char Point)
{
	char err= 0;
	unsigned long val= 0;
	unsigned int mul= 1;
	unsigned int div= 1; // 1000;
	unsigned int dec= 1;
	unsigned char i;
	char decade= 0;

	//for(i= 0; i < IdLen - 3; i++ )	
	//	div*= 10;

	// Kommastelle geht nicht in Nomnalwert ein, dient nur zur Kommapositon in einem Display 
	// for(i= 0; i < Point; i++ )	
	// 	dec*= 10;
		
	memset(&m_Nomination, 0, sizeof(m_Nomination));
#ifdef NOMINATION_POINT
	m_Nomination.Point= Point;		
#endif
	if(isalpha(NomString[0]) && isalpha(NomString[1]))
	{
		m_Nomination.Currency[0]= NomString[0];		
		m_Nomination.Currency[1]= NomString[1];
		
		for(i= 2; i < IdLen; i++)
		{
			if(isdigit(NomString[i]))				
			{
				val*= 10;
				if(mul > 1)
					mul/= 10;
//				div/= 10;
				val+= NomString[i] - '0';
			}					
			else
			{
				switch(NomString[i])
				{
					case 'k':
					case 'K':	
						mul= 1000; 
						decade= 3;
						break;
					case 'm':
					case 'M':	
						mul= 1000000L; 
						decade= 6;
						break;
					case '.':
					case ',':	
						div= 100L; 
						decade= 6;
						break;
					default:	break;
				}
			}					
		}
	//	div= (div < 1) ? 1 : div;
//		m_Nomination.Value= val*mul*ScaleFactor/div/dec;
		m_Nomination.Value= val*mul*ScaleFactor/div;
	}		
	else
	{
		err= 1;
	}					
	return err;	
}
	
int Nomination::GetValue(void)
{
	return m_Nomination.Value;		
}


void Nomination::SetValue(int Value)
{
	m_Nomination.Value= Value;		
}


void Nomination::GetCurrency(unsigned char *Nom1, unsigned char *Nom2)
{
	*Nom1= m_Nomination.Currency[0];		
	*Nom2= m_Nomination.Currency[1];		
}
	


////////////////////////////////////////////////////////////////////////////////
// Optionnale Formatierung eines Anzeigetextes
//
#ifdef NOMINATION_POINT

void Nomination::GetDisplayStr(char *Dsp, char &Len, char Point)
{
	int val;
	int dec;
	unsigned char i;
	char str[15];
	char len;

	if(m_Nomination.Point > 0)
	{
		memset(str, 0, sizeof(str));

		dec= 1;
		for(i= 0; i < m_Nomination.Point; i++)
			dec*= 10;

		val= m_Nomination.Value	/ dec;
		
		itoa(val, str, 10);
		str[strlen(str)]= Point;
		val= m_Nomination.Value % dec;
		
		for(i= 0; i < m_Nomination.Point; i++)
		{
			dec/=10;
			len= strlen(str);
			str[len]= (val / dec) % 10 + '0';
		}
	}
	else
	{
		itoa(m_Nomination.Value, str, 10);
	}

	Len= Len < (char)strlen(str) ? Len : strlen(str);
	strncpy(Dsp, str, Len);
	Dsp[Len]= 0;
}
#endif

//
// end nomination.cpp
////////////////////////////////////////////////////////////////////////////////
