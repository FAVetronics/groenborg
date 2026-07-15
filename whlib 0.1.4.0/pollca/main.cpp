////////////////////////////////////////////////////////////////////////////////
//
// File:        main.cpp
//
// Author:      Manfred Wollny
// Copyright:   (c) 2010
//              wh Muenzpuefer Berlin GmbH
//              Teltower Damm 276
//              D-14167 Berlin
//              www.whberlin.de
//
// Created:     02.08.2010 (mw)
// Versions:    v b.01  27.08.2010  initial beta release
//
////////////////////////////////////////////////////////////////////////////////

// includes

#include <stdio.h>
#include <time.h>
#include <stdlib.h>
#include <string.h>
#include <ctype.h>
#include <unistd.h> // sleep()
#include <fcntl.h>
#include <sys/signal.h>
#include <sys/types.h>
#include "CSerCom.h"
#include "CccTalk.h"
#include "CTimeOut.h"
#include "ccTalkEmpDevice.h"

#include <stdio.h>
#include <curl/curl.h>
#include <math.h>



////////////////////////////////////////////////////////////////////////////////
// defines macros
//
////////////////////////////////////////////////////////////////////////////////


#define DEF_PORT_NAME "/dev/ttyUSB0"
//#define DEF_PORT_NAME "/dev/ttyS0"
#define FRM_PORT_NAME "/dev/ttyS%d"
#define DEF_DEVICE_ADR 002

#define DEF_COIN_EANBLE 0xffff

#define LEN_PORT_NAME 20	// /dev/ttyUSB0

#define DEF_CALLBACK_URL "localhost:5000/coinacceptor/local"
#define LEN_CALLBACK_URL 100

#define LEN_EXTENDED_LOGING 2

////////////////////////////////////////////////////////////////////////////////
// Locals
////////////////////////////////////////////////////////////////////////////////

#define CallbackURL_LENGTH	LEN_CALLBACK_URL + 1
char CallbackURL[CallbackURL_LENGTH] = DEF_CALLBACK_URL;

char ExtendedLoging[LEN_EXTENDED_LOGING] = "n";
	
int ClosingAmount_x100 = 9999999; //99.999,99
int ReceivedAmount_x100 = 0;

int _transaction_id = 0;
string _reference = "";

////////////////////////////////////////////////////////////////////////////////
// text
////////////////////////////////////////////////////////////////////////////////

#define t_Title     "ccTalk - Poll coin acceptor - "
#define t_Version   "version b.01\n\r"
#define t_CopyRight "(c) wh Muenzpruefer GmbH Berlin - 2010\n\r"
#define t_Line      "-------------------------------------------------------------------------------\n\r"



/// Collection of coin acceptor subfunctions.
/// Demonstrate building a dervated class with application optimized functions


class CoinAcceptor : public ccTalkEmpDevice {

public:
    CoinAcceptor(){
        m_EventCount= -1;
    };
    ~CoinAcceptor(){
        Close();
    };

private:
    int m_EventCount;

    // local vars for using as single device solution
    CccTalk m_ccTalk;
    static char m_Portname[];
    static int m_Address;

public:

    /// use this methode for single device solution
    /// \param none
    /// \return error code 0= OK
    int OpenCoinAcceptor() {
        int error;
        error = m_ccTalk.Open(m_Portname);
        if (!error)
        {
            error= Open(m_Address, &m_ccTalk);
        }
        return error;
    }


    /// print coin acceptors device IDs
    /// \param none
    /// \return error code 0= OK
    void PrintIds() {
        std::string str;
        printf("Address:          %03d \n\r", GetDeviceAddress());
        RequestManufacturerID(str);
        printf("ManufacturerID:   %s\n\r", str.data());
        RequestProductCode(str);
        printf("ProductCode:      %s\n\r", str.data());
        RequestCategoryID(str);
        printf("CategoryID:       %s\n\r", str.data());
        RequestSoftwareRevision(str);
        printf("SoftwareRevision: %s\n\r", str.data());
        RequestBuildCode(str);
        printf("BuildCode:        %s\n\r", str.data());  
    };



	char* GetfTime (void)
	{
/*	  time_t rawtime;
	  struct tm * timeinfo;
	  char buffer [80];
	  time (&rawtime);
	  timeinfo = localtime (&rawtime);
	  //printf ("%s", asctime(timeinfo));
	  strftime (buffer,80,"%Y-%m-%d %H:%M:%S.%f",timeinfo);
	  puts (buffer);*/
	  
	  static char buffer[32];
	  int millisec;
	  struct tm* tm_info;
	  struct timeval tv;

	  gettimeofday(&tv, NULL);

	  millisec = lrint(tv.tv_usec/1000.0); // Round to nearest millisec
	  if (millisec>=1000) { // Allow for rounding up to nearest second
		millisec -=1000;
		tv.tv_sec++;
	  }

	  tm_info = localtime(&tv.tv_sec);

	  char timePart[32];
	  strftime(timePart, sizeof(timePart), "%Y:%m:%d %H:%M:%S", tm_info);
	  snprintf(buffer, sizeof(buffer), "%s.%06d", timePart, millisec);
	  return &buffer[0];
	}


	void PrintTime (void)
	{
	  printf("%s\r\n", GetfTime());
	}




    /// list all coins with IDs, names and inhibit status
    /// \param none
    /// \return none
    void GetAllCoinIds()
    {
        int i;
        char name[20];
        char ccode[10];
        unsigned short mask;
        printf("Coin  Id  Name     Enable\r\n");
        RequestInhibitStatus(mask);
        for(i= 0; i < CCT_MAX_VALUES; i++)
        {
            RequestCoinCountryCode(i, ccode, true);
            RequestCoinName(i, name, ',', true);
            printf(" %2d   %s  %s  %s\r\n", i+CCT_EMP_COIN_OFFSET , ccode, name, (mask & 1 << i) ? "enabled" : "disabled");
        }
    }




	void POST_Coinval(int iValueToPOST)
	{
		CURL *curl;
		CURLcode res;
		long http_code;
		char jsonBuf[512];
		snprintf(jsonBuf, sizeof(jsonBuf), "{\"method\":\"coinAcceptor\", \"data\":{\"amountreceived\" : %d, \"transactionID\" : %d, \"reference\" : \"%s\", \"DateTime\": \"%s\"}}", iValueToPOST, _transaction_id, _reference.c_str(), GetfTime());
		std::string jsonObj(jsonBuf);
		printf("This: %s\r\n", jsonObj.c_str());
		
		struct curl_slist *slist1;
		slist1 = NULL;
		slist1 = curl_slist_append(slist1, "Content-Type: application/json");

		curl_global_init(CURL_GLOBAL_ALL);
		curl = curl_easy_init();

		if(curl){
			curl_easy_setopt(curl, CURLOPT_URL, CallbackURL);//insert your url here");
			curl_easy_setopt(curl, CURLOPT_HTTPAUTH, (long)CURLAUTH_BASIC);
			curl_easy_setopt(curl, CURLOPT_USERPWD, "root:");
			curl_easy_setopt(curl, CURLOPT_CUSTOMREQUEST, "POST");
			curl_easy_setopt(curl, CURLOPT_POSTFIELDS, jsonObj.c_str());
			curl_easy_setopt(curl, CURLOPT_NOPROGRESS, 1L);
			curl_easy_setopt(curl, CURLOPT_USERAGENT, "curl/7.38.0");
			curl_easy_setopt(curl, CURLOPT_HTTPHEADER, slist1);
			curl_easy_setopt(curl, CURLOPT_MAXREDIRS, 50L);
			curl_easy_setopt(curl, CURLOPT_TCP_KEEPALIVE, 1L);

			//enable to spit out information for debugging
			if (ExtendedLoging[0] == 'Y') curl_easy_setopt(curl, CURLOPT_VERBOSE,1L); 

			res = curl_easy_perform(curl);

			if (res != CURLE_OK){
				fprintf(stderr, "curl_easy_perform() failed: %s\n", curl_easy_strerror(res) );
				printf("URL: %s\r\n",CallbackURL);
				printf("JSON: %s\r\n",jsonObj.c_str());
			}

			printf("\nget http return code\n");
			curl_easy_getinfo(curl, CURLINFO_RESPONSE_CODE, &http_code);
			printf("http code: %lu\n", http_code );


			curl_easy_cleanup(curl);
			curl_slist_free_all(slist1);
			curl_global_cleanup();
		}
		else
		{
			printf("Error initializing curl...");
		}
	}




    /// extendend polling using advanced functions also for formating strings
    /// \param none
    /// \return error code 0= OK
    int ExtendedPollCoins() {
        int ret;
        int evt= 0;
        int coin, err;
		double coinval;
		int coinval_x100; // max amount: 21.474.836,47
		int attempts = 3;

        do {
			ret= PollNext(evt, coin, err);
			if(evt) {
				RequestCoinValue(coin-CCT_EMP_COIN_OFFSET, coinval);
				if(coin) 
				{
					PrintTime();
					coinval_x100 = (int)(coinval*100);
	//				printf("Coin  No.: %2d    Value: %f   for server: %d\r\n", coin, coinval, coinval_x100);
					printf("Coin  No.: %2d    Value: %f\r\n", coin, coinval);
					POST_Coinval(coinval_x100);
					ReceivedAmount_x100 += coinval_x100;
				}
				else
					printf("Coin error %03d\r\n", err);
			}
			attempts --;
		} while ((ret != 0) && (attempts > 0));
		// Posible errorcodes for ret:
		// #define CCT_OK                0
		// #define CCT_RX_RDY           (1<<0)        // Receiver not ready
		// #define CCT_ERR_RX_TOUT      (1<<4)        // Receiver timeout
		// #define CCT_ERR_RX_CSUM      (1<<5)        // Checksum error in received data blocl
		// #define CCT_ERR_RX_OF        (1<<6)        // Data length error (?)
		// #define CCT_ERR_TX_ECHO      (1<<8)        // Missing local echo
		// #define CCT_ERR_TX_TOUT      (1<<9)        // Transceiver timeout		
		if (ret != 0) printf("Poll error %03d\r\n", ret);
        return ret;
    }

	

	

   int ClearOldCoins() {
        int ret;
        int evt= 0;
        int coin, err;
        char str[255];
		bool CoinsInBuffer = true;

		while (CoinsInBuffer)
		{
			ret= PollNext(evt, coin, err);
			if(evt) {
				RequestCoinValueStr(coin-CCT_EMP_COIN_OFFSET, str, ',');
				if(coin) 
				{
					printf("Buffered Coin  No.: %2d    Value: %s\n\r", coin, str);
				}
				else
					printf("Error %03d\r\n", err);
			}
			else CoinsInBuffer = false;
		}

        return ret;
    }



	

	

};



char m_Portname[]= "/dev/ttyUSB0";
int m_Address= 2;




////////////////////////////////////////////////////////////////////////////////
// main
////////////////////////////////////////////////////////////////////////////////

int main(int Parm_Count, char *Parms[])
{
    int error;
    CccTalk cctalk; // ccTalk port driver
    CoinAcceptor acceptor;
    int ttyno;
    int deviceadr= DEF_DEVICE_ADR;
    int enablemask= DEF_COIN_EANBLE;
    char portname[LEN_PORT_NAME] = DEF_PORT_NAME;

	setbuf(stdout, NULL);
	string version =  "2.3";
	acceptor.PrintTime();
	printf("FAVetronics version: %s\r\n", version.c_str());

    if (Parm_Count > 1) // Get serial port name or tty number
    {
        if (isdigit(Parms[1][0]))
        {
            sscanf(Parms[1], "%d", &ttyno);
            sprintf(portname, FRM_PORT_NAME, ttyno);
        } else
        {
            snprintf(portname, sizeof(portname), "%s", Parms[1]);
        }
    }
    if (Parm_Count > 2) // Get callback URL
	{
        snprintf(CallbackURL, sizeof(CallbackURL), "%s", Parms[2]);
	}
	printf("Callback is set to: %s \n",CallbackURL);
    if (Parm_Count > 3) // Get ExtendedLogin
    {
        snprintf(ExtendedLoging, sizeof(ExtendedLoging), "%s", Parms[3]);
	}
	printf("ExtendedLoging is set to: %s \n",ExtendedLoging);
    if (Parm_Count > 4) // Get ClosingAmount
    {
		const std::string snum = Parms[4];
		ClosingAmount_x100 = std::stoi(snum);
	}
	printf("Closing amount set to: %d \n",ClosingAmount_x100);

    if (Parm_Count > 5)
    {
		const std::string snum = Parms[5];
		_transaction_id = std::stoi(snum);
	}
	printf("Transaction ID: %d \n",_transaction_id);

    if (Parm_Count > 6)
	{
        _reference = std::string(Parms[6]);
	}
	printf("Reference: %s \n",_reference.c_str());

    // print header
    printf("%s", t_Title);
    printf("%s", t_Version);
    printf("%s", t_CopyRight);
    printf("%s", t_Line);

    printf("uses serial port: %s\n\r", portname);
    printf(t_Line);

    /// open port and device
    error = cctalk.Open(portname);

    if (!error)
    {
        acceptor.Open(deviceadr, &cctalk);

        // print device IDs
        acceptor.PrintIds();
        printf(t_Line);

        // enable coins
        acceptor.ModifyInhibitStatus(enablemask);

        // print Coin Names and IDs
        acceptor.GetAllCoinIds();
        printf(t_Line);

		acceptor.ClearOldCoins();
		
		// signal that we are ready by sending received = 0
		acceptor.POST_Coinval(0);
		
        // main loop polling acceptor
        while(!error && (ReceivedAmount_x100 < ClosingAmount_x100))
        {
			error= acceptor.ExtendedPollCoins();
			if(error == EMP_CCT_ERR_LOST_EVENTS)
			{
				printf("Warning: Events lost\r\n");
				error= 0;
			}
        }

        // finsh
        if(error)
            printf("Error %d on polling device!\n\r", error);

		if (ReceivedAmount_x100 >= ClosingAmount_x100)
            printf("Requested %d - Received %d\n\r", ClosingAmount_x100, ReceivedAmount_x100);
		
        acceptor.Close();
    }
    else
    {
        printf("can't open %s \n\r<return>\n\n", portname);
    }

    printf("\n\rbye\n\r");

    exit(0);
}



// main.cpp

////////////////////////////////////////////////////////////////////////////////



