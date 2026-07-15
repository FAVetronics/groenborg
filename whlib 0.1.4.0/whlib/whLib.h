/* 
 * File:   whLib.h  - main header file for documentation main page
 * Author: manfred
 *
 * Created on 5. August 2010, 13:54
 */

#ifndef _WHLIB_H
#define	_WHLIB_H

/*! \mainpage whLib - ccTalk SDK
 *
 * \section intro_sec 1 Introduction
 * The whLib is the main part of the wh ccTalk SDK. It is a implementation of
 * ccTalk communications protocol on LINUX- PC based platforms. A port for embedded LINUX
 * applications can be easily made by adapting the base communication class CSerCom
 * \n
 * The whLib is compiled and tested on a LINUX PC (UBUNTU 9) only.
 *
 * \section licence_sec 2 Licence agreement
 *
 * \subsection scope_sec 2.1 Scope
 * The licence agreement applies to all source code files decribed in this
 * documentation and to the example sources included.
 *
 * \subsection uasge_sec 2.2 Usage
 * You may use the source codes for the development of free and/or commercially
 * sold applications. \n
 * You may also modify the codes.
 *
 * \subsection dist_sec 2.3 Distribution
 * You are not allowed to distribute the original source code or any source code
 * derived from it.
 *
 * \subsection asis_sec 2.4 The AS IS paragraph
 * Please note that wh Münzprüfer Berlin GmbH hereby states that this package is
 * provided "AS IS" and without warranties of any kind, either expressed or implied, including,
 * but not without limitation, the implied warranties of merchantability and
 * fitness for a particular purpose. In other words, we accept no liability for
 * any damage that may result from using wh Münzprüfer Berlin GmbH codes or
 * programs that use wh Münzprüfer Berlin GmbH source codes. \n
 * We do not warrant that the functions contained in the source codes will meet
 * your requirementsor that the software operation will be uninterrupted or error
 * free. \n In no event will wh Münzprüfer Berlin GmbH be liable to you for any
 * damages, including any lost profits, lost savings or other incidental or
 * consequential damages arising out of the use or inability to use these codes,
 * or for any claim by any other party. The entire risk as to the results and
 * performance of the codes is assumed by you. \n

 * \subsection cr_sec 2.5 Copyright
 * (c) Copyright 2010-2015 \n
 * wh Münzprüfer Berlin GmbH \n
 * Teltower Damm 276 \n
 * D-14167 Berlin \n
 * Germany
 * \n
 * http://www.whberlin.de \n
 * info@whberlin.de \n
 *
 * \section install_sec 3 Installation
 *
 * \subsection env_sec 3.1 Environment
 * Tested environment:
 *   - LINUX PC with UBUNTU 9 
 *   - GCC \n
 * For example and test tools: \n
 *   - Netbeans 6.7 IDE
 *   - Qt 4.0 library
 *
 * \subsection driver_sec 3.2 Hardware driver
 * In order to use the  EMP 800.14 or one of the ccTalk- Hubs CCT 900 or 910
 * the driver for the FT232 needs to be installed. The FT232 is an USB to
 * serial controller. From kernel version 2.4.2 the driver
 * is included in most of distrubutions. \n
 * Please refer to
 * http://www.ftdichip.com
 *
 * \subsection gcc_sec 3.3 GCC compiler and tools
 * You may use your own developmet tools or you can coose to follow the
 * steps below to install the development environment used by whMünzprüfer
 * \n
 * In Case no development tools are installed yet, follow these steps:
 *
 *   - sudo apt-get install build-essential
 *
 * In case of compiler errors like\n
 * „cannot find –llibxxxx“ \n
 * you have to install the expected libraries manualy:
 *
 *  - sudo apt-get install libfreetype6-dev /n
 *  - sudo apt-get install libavahi-gobject-dev
 *  - sudo apt-get install libXrender-dev
 *  - sudo apt-get install libfontconfig-dev
 *  - sudo apt-get install libXext-dev
 *  - sudo apt-get install libSM-dev
 *
 * For terminal applications using functions like "getch()" the header file
 * „curses.h“ has to be included. The library „curses“ needs to be installed too:
 *
 *  - sudo apt-get install libncurses-dev
 *
 *
 * \subsection qt_sec 3.4 Netbeans and Qt Installation
 * The icluded test application "ccTalkTest" and other examples are developed with the Qt
 * library.
 * The used IDE is Netbeans and QtDesigner.\n
 *
 *  - chmod u+x qt-sdk-linux-x86-opensource-2009.04.1.bin
 *  - qt-sdk-linux-x86-opensource-2009.04.1.bin
 *
 * \section development_sec 3 Development
 *
 * \subsection predef_sec 3.1 Predefintions
 * With constant definitions \n
 * 
 *   - ##define USE_QT 1
 *   - ##define USE_STRING 1
 *
 * You can select which extended funktions to use. For code optimation you
 * can switch off compiling these functions.
 *
 * \section examples_sec 4 Examples - short description
 * Followings example applications are included in this SDK.
 *   - cctlist - Application listing all found devices attached to the ccTalk
 * bus with their IDs. Using the CccTalk class.
 *   - pollca - Application to open / initialise a coin selector, list all
 *  accepted coins and received error codes. Using the ccTalkEmpDevice class.
 *   - cct900 - Application to test CCT 900/910 basic functions. Using the
 *  the ccTalkCCT900Device class.
 *   - mdbcashless -  Application to test the MDB Device Class for Cashless 
 *  devices using the MdbCashlessDevice class and the ccTalkCCT900Device class.
 *   - pollbv - Application to open / initialise a billvalidator, list all
 *  accepted bills and received error codes. Using the ccTalkBillValidatorDevice class.
 *
 * \section history_sec 5 History
 *
 * \subsection v0_1_0_0_sec 5.1 Version 0.1.0.0
 * Intitial version. The following device types are implemented:
 *   - base device type
 *   - coin acceptor        
 *   - payouts              
 *   - coin separator       
 *   - bill validator       
 *
 * \subsection v0_1_0_1_sec 5.2 Version 0.1.0.1
 * minor extentions / changes in
 * New:
 *   - ccTalkDevice::GetDeviceAddress()
 *
 * \subsection v0_1_1_0_sec 5.3 Version 0.1.1.0
 * New device class for the CCT 900:
 *   - class ccTalkCCT900Device
 *   - class MdbDevice
 *   - demo program cct900
 *
 * \subsection v0_1_2_0_sec 5.4 Version 0.1.2.0
 * New device class for the CCT 910 derivated from CCT 900 class:
 *   - class ccTalkCCT910Device
 *
 * \subsection v0_1_2_1_sec 5.5 Version 0.1.2.1
 * minor fixes
 *
 * \subsection v0_1_2_2_sec 5.5 Version 0.1.2.2
 * minor fixes
 *   - Pointer mismatch in ccTalkPayoutDevice::DispenseHopperCoins(...)
 *
 * \subsection v0_1_3_0_sec 5.6 Version 0.1.3.0
 * minor fixes \n
 * New device class to communicate with a MDB device via a CCT 900/910
 * device.
 *   - class MdbCashlessDevice\n
 *   - demo program to test this new devices class\n
 *
 * \subsection v0_1_4_0_sec 5.7 Version 0.1.4.0
 * minor fixes \n
 * Extended ccTalk checksum calculation CRC16\n
 *   - implemented for ccTalkBillValidatorDevice\n
 *   - demo program pollbv for testing\n
 *  */

#endif	/* _WHLIB_H */

