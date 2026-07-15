////////////////////////////////////////////////////////////////////////////////
// twsglobal.h
//
// Decription:	Globals für TWS 100
//
// Prefix: 
//
//
// Create:		03.12.2009	M.Wollny	
//
// Modify:
//
//
////////////////////////////////////////////////////////////////////////////////
// 
// Copyright: (c) 2009 wh Münzprüfer Berlin GmbH
//                     14167 Berlin      
//					   Teltower Damm 276	 
//					   www.whberlin.de  	 
//
////////////////////////////////////////////////////////////////////////////////
//
#ifndef TWS_GLOBALS_HEADER
#define TWS_GLOBALS_HEADER
//
////////////////////////////////////////////////////////////////////////////////
// Globale Defines

// Münzkontrolle in der Kammer
enum TWS_COIN_CTRL{
	TWS_COIN_EMPTY,                 // No Coin / no change
	TWS_COIN_MODE_NO_EJECT,		// keine Ausgabe
	TWS_COIN_MODE_EJECT,		// eject into correspondig slot 
	TWS_COIN_MODE_CASH,		// eject to main cash
	TWS_COIN_MODE_REJECT,		// reject
	TWS_COIN_MODE_ERROR,		// error in box
};

#define TWS_COIN_DEFAULT TWS_COIN_EMPTY

////////////////////////////////////////////////////////////////////////////////
// Mechnische Konfiguration 
#define SLOT_ITEMS		10
#define MAGAZINE_ITEMS	52	

////////////////////////////////////////////////////////////////////////////////
// Status / Betriebszustand Flags
// für Header TWS_CCT_GET_STATUS
//  
enum TWS_STATUS {
	TWS_STAT_POWER_ON,		// Power On	Initialisierung
	TWS_STAT_RESTART,		// Restart Initialisierung
	TWS_STAT_READY,			// Zur Einzahlung bereit, 
	TWS_STAT_EMPTY,			// leer und zur Einzahlung bereit
	TWS_STAT_FULL,			// Magazin voll
	TWS_STAT_INJECT,		// Münzen Einzahlung aktiv
	TWS_STAT_FLUSH,			// Münzen Auszahlung in die Hopper
	TWS_STAT_EJECT,			// Münzen Auszahlung in definierten Schacht
	TWS_STAT_STOP,			// Münzen Auszahlung angehalten

	// Master-Funktionen  
	TWS_STAT_CASHIN,			// Zur Einzahlung bereit
	TWS_STAT_CASHOUT,		// bei der Auszahlung

	TWS_STAT_ERROR= 100,
	TWS_STAT_PWON_ERROR,	// Fehler beim Initialisieren
	TWS_STAT_INJECT_ERROR,	// Fehler beim Münzeinwurf
	TWS_STAT_EJECT_ERROR,	// Fehler beim Münzauswurf
	TWS_STAT_EMR_ERROR,		// Fehler beim EMR, geht nicht in Ruhe, Timeout	

	// Master Mode	
	TWS_STAT_EMP_ERROR,		// Fehler beim EMP
	TWS_STAT_CIS_ERROR,		// Fehler beim CIS
	TWS_STAT_ES_ERROR,		// Fehler beim Schlitzsperre (Münzsignal liegt permanent an, TimeOut)
}; 

// ZustandsFlags im Statustelegramm
#define TWS_FLG_MAGAZINE_EMTY 	(1<<0)
#define TWS_FLG_MAGAZINE_FULL	(1<<1)
#define TWS_FLG_MAGNET_ACTIVE 	(1<<2)
#define TWS_FLG_MOTOR_ACTIVE 	(1<<3)
#define TWS_FLG_RESERVED_1 		(1<<4)

// Peripherie
#define TWS_FLG_EMR_ACTIVE 		(1<<4)
#define TWS_FLG_ES_COIN 		(1<<5)
#define TWS_FLG_ES_OPEN 		(1<<6)
#define TWS_FLG_RESERVED_2 		(1<<7)

// Master Mode
#define TWS_FLG_CIS_ACTIVE 		(1<<0)
#define TWS_FLG_CIS_OPEN 		(1<<0)
#define TWS_FLG_EMP_LOCKED 		(1<<0)
#define TWS_FLG_PAYOUT_ACTIVE 	(1<<0)

// Fehlermaske
// Fehler bei der Systemüberwachung
#define TWS_ERR_WDOG			(1<<0)
#define TWS_ERR_SYSTEM			(1<<1)
#define TWS_ERR_MAGAZINE_SENSOR	(1<<2)
#define TWS_ERR_MOTOR_SENSOR	(1<<2)
#define TWS_ERR_SLOT_SENSOR		(1<<2)
// Ablaufüberwachung
#define TWS_ERR_MOTOR_TIMEOUT	(1<<2)
#define TWS_ERR_MAGNET_TIMEOUT	(1<<2)
#define TWS_ERR_FULL_OF			(1<<2)
#define TWS_ERR_COIN_MISSING	(1<<2)


#define TWS_ERR_EMR				(1<<2)




////////////////////////////////////////////////////////////////////////////////
// ccTalk Device Telegramme
//

////////////////////////////////////////
// Standard Telegramme
//


////////////////////////////////////////
// TWS 100 Spezifisch

#define TWS_CCT_BASE	20

enum TWS_CCT_HEADER {
	TWS_CCT_RESTART= TWS_CCT_BASE,	// Rücksetzen der Ablaufsteuerung, Verhalten wie Power On
	TWS_CCT_GET_STATUS,		// Status der Münzsortierung
						//	Parameter: -
						// 	Return:
						//          1.Byte: Status entsprehend "enum TWS_STATUS"
						//          2.Byte:	Zustandsflags
						//          3+4.Byte: Fehlerflags, werden unterdrückt wenn keine Fehler anliegen
						//		 
	TWS_CCT_INJECT_COIN,		// Münze eingeworfen
						// 	Parameter:
						//          1.Byte: Münz-Id (1..16)
						//          2.Byte: Mode entsprechend "enum TWS_COIN_CTRL"
						//	Return: 
						//          Anzahl Münzen 1-50, 0 TWS Voll, < 0 Fehler
					//
	TWS_CCT_FLUSH_COINS,            // Auswerfen aller Münzen in die vorgegebenen Schacht Positionen
					//
	TWS_CCT_EJECT_COINS,		// Auswerfen aller Münzen in angebenen Schacht
                                                //  Parameter:
						//      1. Byte: Zielschacht
						//	0= Hauptkasse	(ohne Paramter)
						//	1-8 Schacht 1-8, kann per Parameter blockiert werden
						//				9= Rückgabe		(>= 9)
						//			2. Byte: 1= Alle Kammern auswerfen, unabhüngig ob mit oder ohen Münze
						//		Return: -
						//	
	TWS_CCT_STOP,				// Auswerfen anhalten
						//
	TWS_CCT_CONTINUE,			// Auswerfen weiter laufen lassen
						//
	TWS_CCT_ABORT,				// Auswerfen abbrechen
						//
	TWS_CCT_REQUEST_COIN_COUNT,		// Anzahl Münzen im Magazin (0-50)
						//
	TWS_CCT_REQUEST_MAGAZIN_COINS,          // Liste der Münzen im Magazin ab Einwurfposition
						// 		Parameter:
						//			1.Byte: erste Magazinposition
						// 			2.Byte: Anzahl Münzen
						//			(n+1). Byte: Münz ID
						//			(n+2). Byte: Sortier Mode
						//		Return: -
						//
	TWS_CCT_MODIFY_MAGAZIN_COINS,           // Änderung der Münzzuordnung und Sortier-Mode in einer Magazin-Kammer
						//		Parameter:
						//			1.Byte: Position im Magazin
						//			2.Byte: Münz-ID
						//			3.Byte: Sortier Mode
						//		Return: -
						//
	TWS_CCT_REQUEST_SORTER_EJECTS,	// Anzahl der ausgeworfenen Münzen pro Slot
						//		Parameter:
						// 			1 Byte: 1= Zühler lüschen
			 			//		Return:
						// 			1. Byte:   Hauptkasse 
						//			2-9. Byte: 1-8 Schacht
						//			10 Byte:   Rückgabe
						//  Hinweis:
						//		die Zühler werden intern als unsigned int geführt. Wenn grüüer 255
						//		wird 255 geliefert und beim Lüschen 255 abgezugen. Mit einer weiteren
						//		Abfrage wird die Testmenge gemeldet
						// 		
											

	// Peripheriegerüte ohne CIS/CIF
	//
	TWS_CCT_EMR,					// EMR- aktivieren
	TWS_CCT_ES_OPEN,				// Schlitzsperre üffnen

	TWS_SUB_COMMAND=	99
};



////////////////////////////////////////////////////////////////////////////////
// Bitzuodnung für Lichtschanken in Header TWS_CMD_GET_LB
//

#define TWS_CCT_LB_SLOT_1		(1 << 0)	
#define TWS_CCT_LB_SLOT_2		(1 << 1)	
#define TWS_CCT_LB_SLOT_3		(1 << 2)	
#define TWS_CCT_LB_SLOT_4		(1 << 3)	
#define TWS_CCT_LB_SLOT_5		(1 << 4)	
#define TWS_CCT_LB_SLOT_6		(1 << 5)	
#define TWS_CCT_LB_SLOT_7		(1 << 6)	
#define TWS_CCT_LB_SLOT_8		(1 << 7)	

#define TWS_CCT_LB_SLOT_CASH	(1 << 9)	
#define TWS_CCT_LB_SLOT_REJECT	(1 << 10)	

#define TWS_CCT_MAGAZINE_INDEX 	(1 << 11)
#define TWS_CCT_MOTOR_INDEX 	(1 << 12)


#endif
//
////////////////////////////////////////////////////////////////////////////////


