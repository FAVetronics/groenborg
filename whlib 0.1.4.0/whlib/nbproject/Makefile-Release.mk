#
# Generated Makefile - do not edit!
#
# Edit the Makefile in the project folder instead (../Makefile). Each target
# has a -pre and a -post target defined where you can add customized code.
#
# This makefile implements configuration specific macros and targets.


# Environment
MKDIR=mkdir
CP=cp
CCADMIN=CCadmin
RANLIB=ranlib
CC=gcc
CCC=g++
CXX=g++
FC=
AS=as

# Macros
CND_PLATFORM=GNU-Linux-x86
CND_CONF=Release
CND_DISTDIR=dist

# Include project Makefile
include Makefile

# Object Directory
OBJECTDIR=build/${CND_CONF}/${CND_PLATFORM}

# Object Files
OBJECTFILES= \
	${OBJECTDIR}/ccTalkTwsDevice.o \
	${OBJECTDIR}/ccTalkBillValidatorDevice.o \
	${OBJECTDIR}/MdbDevice.o \
	${OBJECTDIR}/tools.o \
	${OBJECTDIR}/CTimeOut.o \
	${OBJECTDIR}/ccTalkEmpDevice.o \
	${OBJECTDIR}/CSerCom.o \
	${OBJECTDIR}/ccTalkHzwDevice.o \
	${OBJECTDIR}/ccTalkPayoutDevice.o \
	${OBJECTDIR}/CccTalk.o \
	${OBJECTDIR}/MdbDispenserDevice.o \
	${OBJECTDIR}/nomination.o \
	${OBJECTDIR}/ccTalkCCT910Device.o \
	${OBJECTDIR}/ccTalkCCT900Device.o \
	${OBJECTDIR}/MdbCashlessDevice.o \
	${OBJECTDIR}/ccTalkDevice.o \
	${OBJECTDIR}/ccTalkCisDevice.o

# C Compiler Flags
CFLAGS=

# CC Compiler Flags
CCFLAGS=
CXXFLAGS=

# Fortran Compiler Flags
FFLAGS=

# Assembler Flags
ASFLAGS=

# Link Libraries and Options
LDLIBSOPTIONS=

# Build Targets
.build-conf: ${BUILD_SUBPROJECTS}
	${MAKE}  -f nbproject/Makefile-Release.mk dist/Release/GNU-Linux-x86/libwhlib.a

dist/Release/GNU-Linux-x86/libwhlib.a: ${OBJECTFILES}
	${MKDIR} -p dist/Release/GNU-Linux-x86
	${RM} dist/Release/GNU-Linux-x86/libwhlib.a
	${AR} rv ${CND_DISTDIR}/${CND_CONF}/${CND_PLATFORM}/libwhlib.a ${OBJECTFILES} 
	$(RANLIB) dist/Release/GNU-Linux-x86/libwhlib.a

${OBJECTDIR}/ccTalkTwsDevice.o: nbproject/Makefile-${CND_CONF}.mk ccTalkTwsDevice.cpp 
	${MKDIR} -p ${OBJECTDIR}
	${RM} $@.d
	$(COMPILE.cc) -O2 -MMD -MP -MF $@.d -o ${OBJECTDIR}/ccTalkTwsDevice.o ccTalkTwsDevice.cpp

${OBJECTDIR}/ccTalkBillValidatorDevice.o: nbproject/Makefile-${CND_CONF}.mk ccTalkBillValidatorDevice.cpp 
	${MKDIR} -p ${OBJECTDIR}
	${RM} $@.d
	$(COMPILE.cc) -O2 -MMD -MP -MF $@.d -o ${OBJECTDIR}/ccTalkBillValidatorDevice.o ccTalkBillValidatorDevice.cpp

${OBJECTDIR}/MdbDevice.o: nbproject/Makefile-${CND_CONF}.mk MdbDevice.cpp 
	${MKDIR} -p ${OBJECTDIR}
	${RM} $@.d
	$(COMPILE.cc) -O2 -MMD -MP -MF $@.d -o ${OBJECTDIR}/MdbDevice.o MdbDevice.cpp

${OBJECTDIR}/tools.o: nbproject/Makefile-${CND_CONF}.mk tools.cpp 
	${MKDIR} -p ${OBJECTDIR}
	${RM} $@.d
	$(COMPILE.cc) -O2 -MMD -MP -MF $@.d -o ${OBJECTDIR}/tools.o tools.cpp

${OBJECTDIR}/CTimeOut.o: nbproject/Makefile-${CND_CONF}.mk CTimeOut.cpp 
	${MKDIR} -p ${OBJECTDIR}
	${RM} $@.d
	$(COMPILE.cc) -O2 -MMD -MP -MF $@.d -o ${OBJECTDIR}/CTimeOut.o CTimeOut.cpp

${OBJECTDIR}/ccTalkEmpDevice.o: nbproject/Makefile-${CND_CONF}.mk ccTalkEmpDevice.cpp 
	${MKDIR} -p ${OBJECTDIR}
	${RM} $@.d
	$(COMPILE.cc) -O2 -MMD -MP -MF $@.d -o ${OBJECTDIR}/ccTalkEmpDevice.o ccTalkEmpDevice.cpp

${OBJECTDIR}/CSerCom.o: nbproject/Makefile-${CND_CONF}.mk CSerCom.cpp 
	${MKDIR} -p ${OBJECTDIR}
	${RM} $@.d
	$(COMPILE.cc) -O2 -MMD -MP -MF $@.d -o ${OBJECTDIR}/CSerCom.o CSerCom.cpp

${OBJECTDIR}/ccTalkHzwDevice.o: nbproject/Makefile-${CND_CONF}.mk ccTalkHzwDevice.cpp 
	${MKDIR} -p ${OBJECTDIR}
	${RM} $@.d
	$(COMPILE.cc) -O2 -MMD -MP -MF $@.d -o ${OBJECTDIR}/ccTalkHzwDevice.o ccTalkHzwDevice.cpp

${OBJECTDIR}/ccTalkPayoutDevice.o: nbproject/Makefile-${CND_CONF}.mk ccTalkPayoutDevice.cpp 
	${MKDIR} -p ${OBJECTDIR}
	${RM} $@.d
	$(COMPILE.cc) -O2 -MMD -MP -MF $@.d -o ${OBJECTDIR}/ccTalkPayoutDevice.o ccTalkPayoutDevice.cpp

${OBJECTDIR}/CccTalk.o: nbproject/Makefile-${CND_CONF}.mk CccTalk.cpp 
	${MKDIR} -p ${OBJECTDIR}
	${RM} $@.d
	$(COMPILE.cc) -O2 -MMD -MP -MF $@.d -o ${OBJECTDIR}/CccTalk.o CccTalk.cpp

${OBJECTDIR}/MdbDispenserDevice.o: nbproject/Makefile-${CND_CONF}.mk MdbDispenserDevice.cpp 
	${MKDIR} -p ${OBJECTDIR}
	${RM} $@.d
	$(COMPILE.cc) -O2 -MMD -MP -MF $@.d -o ${OBJECTDIR}/MdbDispenserDevice.o MdbDispenserDevice.cpp

${OBJECTDIR}/nomination.o: nbproject/Makefile-${CND_CONF}.mk nomination.cpp 
	${MKDIR} -p ${OBJECTDIR}
	${RM} $@.d
	$(COMPILE.cc) -O2 -MMD -MP -MF $@.d -o ${OBJECTDIR}/nomination.o nomination.cpp

${OBJECTDIR}/ccTalkCCT910Device.o: nbproject/Makefile-${CND_CONF}.mk ccTalkCCT910Device.cpp 
	${MKDIR} -p ${OBJECTDIR}
	${RM} $@.d
	$(COMPILE.cc) -O2 -MMD -MP -MF $@.d -o ${OBJECTDIR}/ccTalkCCT910Device.o ccTalkCCT910Device.cpp

${OBJECTDIR}/ccTalkCCT900Device.o: nbproject/Makefile-${CND_CONF}.mk ccTalkCCT900Device.cpp 
	${MKDIR} -p ${OBJECTDIR}
	${RM} $@.d
	$(COMPILE.cc) -O2 -MMD -MP -MF $@.d -o ${OBJECTDIR}/ccTalkCCT900Device.o ccTalkCCT900Device.cpp

${OBJECTDIR}/MdbCashlessDevice.o: nbproject/Makefile-${CND_CONF}.mk MdbCashlessDevice.cpp 
	${MKDIR} -p ${OBJECTDIR}
	${RM} $@.d
	$(COMPILE.cc) -O2 -MMD -MP -MF $@.d -o ${OBJECTDIR}/MdbCashlessDevice.o MdbCashlessDevice.cpp

${OBJECTDIR}/ccTalkDevice.o: nbproject/Makefile-${CND_CONF}.mk ccTalkDevice.cpp 
	${MKDIR} -p ${OBJECTDIR}
	${RM} $@.d
	$(COMPILE.cc) -O2 -MMD -MP -MF $@.d -o ${OBJECTDIR}/ccTalkDevice.o ccTalkDevice.cpp

${OBJECTDIR}/ccTalkCisDevice.o: nbproject/Makefile-${CND_CONF}.mk ccTalkCisDevice.cpp 
	${MKDIR} -p ${OBJECTDIR}
	${RM} $@.d
	$(COMPILE.cc) -O2 -MMD -MP -MF $@.d -o ${OBJECTDIR}/ccTalkCisDevice.o ccTalkCisDevice.cpp

# Subprojects
.build-subprojects:

# Clean Targets
.clean-conf:
	${RM} -r build/Release
	${RM} dist/Release/GNU-Linux-x86/libwhlib.a

# Subprojects
.clean-subprojects:

# Enable dependency checking
.dep.inc: .depcheck-impl

include .dep.inc
