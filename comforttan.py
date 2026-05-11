import time
import datetime
import signal, os
import requests
import subprocess
import logging
import glob
import hashlib # Import hashlib library (md5 method is part of it)
import zipfile
import json
import traceback

import asyncio	
import os	

try:
  import  azure.iot.device
  from azure.iot.device.aio import IoTHubDeviceClient
  print('Azure MQTT imported')
except:
  print ('Installing Azure MQTT')
  os.system("sudo pip install azure-iot-device==2.12.0")
  os.system("sudo reboot")


	
try:
  import board
  import busio
  import digitalio
  from adafruit_mcp230xx.mcp23008 import MCP23008
  import HW_IO
  SIMULATION = False
  RPi_HOME_FOLDER = "/home/comforttan/"
except:
  import sim_IO
  SIMULATION = True
  RPi_HOME_FOLDER = "./"


comforttanVer = "2.22"            # release version for this program
pollcaVer = "2.2"                 # these are PT hardcoded
cardterminalVer = "2.2"           #
if SIMULATION: kernelVer = "sim"  #
else: kernelVer = "6.1"           #


from dotenv import load_dotenv
load_dotenv()
DEVELOPMENT_LOCATION = "32"


NEW_RELEASE_FOLDER = RPi_HOME_FOLDER +'newRel/'

MAX_NO_OFF_CABINS = 10
MAX_NO_OFF_VENDING_SHELFES = 10

SESSION_STATUS_IDLE = 0
SESSION_STATUS_RUNNING = 1
SESSION_STATUS_AWAITING_USER_START = 2

SESSIONSTATUS = [{"cabin" : 0, "sessionStatus" : 0, "sessionTimeLeft_m" : 0} for x in range(MAX_NO_OFF_CABINS)]

#internals:
_startCabin = [0 for x in range(MAX_NO_OFF_CABINS)]
_stopCabin = [0 for x in range(MAX_NO_OFF_CABINS)]
_sessionTime_m = [0 for x in range(MAX_NO_OFF_CABINS)]
_activateShelf = [0 for x in range(MAX_NO_OFF_VENDING_SHELFES)]
_amountToRequest = 0
_autostartTimeout_s = 180
_maxSessionTime_m = 40
_vendTime_s = 1
_doorTime_s = 20
_ActivateCA = 0
_DeActivateCA = 0
_CA_Activated = 0
_CA_ClosingAmount = 9999999

WebHookAnyBedOn = "0.0.0.0"     # Signal to turn off other power consumers
WebHookAllBedsOn = "0.0.0.0"    # For "All taken" sign
WebHookAllBedsOff = "0.0.0.0"   # Turn other power consumers back on
WebHookDoorOpen = "0.0.0.0"     # ex: "WebHookDoorOpen":"http://192.168.2.100/relay/0?turn=on&timer=10",
WebHookDoorClose = "0.0.0.0"    

prevAnyBedActive = False
prevAllBedsActive  = False

CabinsInstalled = MAX_NO_OFF_CABINS
VendingShelfesInstalled = MAX_NO_OFF_VENDING_SHELFES

prevSESSIONSTATUS = [{"cabin" : 0, "sessionStatus" : 0, "sessionTimeLeft_m" : 0} for x in range(MAX_NO_OFF_CABINS)]
CABINctrl = [{"Output" : 0, "UserSwitch" : 0, 'timer_s' : 0} for x in range(MAX_NO_OFF_CABINS)]
VENDINGctrl = [{"state" : False, "Output" : 0, 'timer_s' : 0} for x in range(MAX_NO_OFF_VENDING_SHELFES)]

CAopenTime = time.perf_counter()

timeStampFormat = "%y%m%d-%H:%M:%S.%f"

CoinAcceptorPort = "none"
CardReaderPort = "none"

newReset = True


# Read settings.ini file
with open(RPi_HOME_FOLDER+'settings.ini') as json_file:
    data = json.load(json_file)
    if 'id' in data: locationID = data['id'].strip()
    elif 'location' in data:  locationID = data['location'].strip()
    elif 'lokation' in data:  locationID = data['lokation'].strip()
    else:  locationID = DEVELOPMENT_LOCATION
    if 'host' in data: host = data['host'].strip()
    else: host = os.getenv(DEFAULT_HOST)
    host = host.replace('api/machine/', '') #the remove 'api/machine/' (for backwards compatibility)
    if 'hosturlextension' in data: hostUrlExtension = data['hosturlextension'].strip()
    else: hostUrlExtension = "callback"
    CALLBACK_URL = host + "api/machine/" + locationID + '/' + hostUrlExtension
    if 'logerrors' in data: LogErrors = data['logerrors']
    else: LogErrors = False
    if 'connectionString' in data: CONNECTION_STRING = data['connectionString']
    else: CONNECTION_STRING = os.getenv(DEFAULT_CONNECTION_STRING)
    if 'WebHookAnyBedOn' in data:  WebHookAnyBedOn = data['WebHookAnyBedOn'].strip()     
    if 'WebHookAllBedsOn' in data:  WebHookAllBedsOn = data['WebHookAllBedsOn'].strip()  
    if 'WebHookAllBedsOff' in data:  WebHookAllBedsOff = data['WebHookAllBedsOff'].strip()           
    if 'WebHookDoorOpen' in data:  WebHookDoorOpen = data['WebHookDoorOpen'].strip()           
    if 'WebHookDoorClose' in data:  WebHookDoorClose = data['WebHookDoorClose'].strip()     



# Add a logging handler so we can see the raw communication data
import logging
import logging.config #MQTTtest
import sys
root = logging.getLogger()
if LogErrors :
    logging.basicConfig(format='%(asctime)s %(message)s', datefmt='%Y%m%d %H:%M:%S')
    #logging.config.fileConfig('logging.conf') #MQTTtest
    root.setLevel(logging.ERROR)
    requests_log = logging.getLogger("requests.packages.urllib3")
    requests_log.setLevel(logging.ERROR)
    requests_log.propagate = True
else : 
    logging.basicConfig(format='%(asctime)s %(message)s', datefmt='%H:%M:%S')
    root.setLevel(logging.INFO)
ch = logging.StreamHandler(sys.stdout)
root.addHandler(ch)

   
        
def Log(txt):
    global LogErrors
    if LogErrors :
        LogtimeStamp = datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S.%f")
        Logfilename = RPi_HOME_FOLDER+'Logs/Log'+ datetime.datetime.now().strftime("%Y%m%d") +'.log'
        try:
            if os.path.isfile(Logfilename) : Logfile = open(Logfilename, 'a') # (a)ppend (w)rite
            else : Logfile = open(Logfilename, 'w') # (a)ppend (w)rite
            LogtimeStamp = datetime.datetime.now().strftime("%H:%M:%S.%f")
            Logfile.write(LogtimeStamp + '\t' + txt + '\r\n')
            Logfile.close
            if  locationID == DEVELOPMENT_LOCATION: print(LogtimeStamp + '\t', txt, flush=True)
        except:
            print(LogtimeStamp + '\t', txt, flush=True)



if SIMULATION == False:

  # Test if WD has already been enabled (https://pysselilivet.blogspot.com/2021/10/raspberry-pi-watchdog-made-simple.html)
  try:
    with open('/etc/systemd/system.conf', 'r') as file:
      # read all content of a file
      content = file.read()
      # check if string present in a file
      if 'RuntimeWatchdogSec=11' in content:
        Log('WD already set up')
      else:
        Log('setting up WD')
        os.system("sudo chmod 777 /etc/systemd/system.conf")
        os.system("sudo echo 'RuntimeWatchdogSec=11' >> /etc/systemd/system.conf")
        os.system("sudo echo 'ShutdownWatchdogSec=5min' >> /etc/systemd/system.conf")
        os.system("sudo chmod 644 /etc/systemd/system.conf")
        os.system("sudo reboot")
  except:
    traceback.print_exc()

  # Initialize the I2C bus:
  try:
    i2c = busio.I2C(board.SCL, board.SDA)
  except:
    Log('giving permissions to I2C...')
    os.system("sudo chmod 777 /dev/i2c-1")
    i2c = busio.I2C(board.SCL, board.SDA)

  # Create an instance of MCP23008:
  mcp = MCP23008(i2c)  # MCP23008 - Always present

  # Optionally change the address of the device if you set any of the A0, A1, A2
  # pins.  Specify the new address with a keyword parameter:
  # mcp = MCP23017(i2c, address=0x21)  # MCP23017 w/ A0 set

  # Create an instance of MCP23008 on Aux board #1:
  try:
      mcp1 = MCP23008(i2c, address=0x21) 
      AuxCard1Installed = True
  except: 
      AuxCard1Installed = False
  Log('AuxCard1Installed: ' + str(AuxCard1Installed))

  # Create an instance of MCP23008 on Aux board #2:
  try:
      mcp2 = MCP23008(i2c, address=0x22) 
      AuxCard2Installed = True
  except: 
      AuxCard2Installed = False
  Log('AuxCard2Installed: ' + str(AuxCard2Installed))

  #Setup main door
  if AuxCard2Installed: 
    Door = mcp2.get_pin(7)                   # Relæ 17: Dør til sluse (Relæ trækkes i 20 sek lige så snart der er købt sol, så kan de komme ind ad dør)
    Door.switch_to_output(value=False) 
    doorInstalled = True
  elif SIMULATION: 
    doorInstalled = True
  else:
    doorInstalled = False
  doorState = False
  requestedDoorState = False
  doorTimeLeft = 0
  DOOR_OPENED_MANUALLY = 999
  DOOR_OPENED_MANUALLY_STATUS_SENT = DOOR_OPENED_MANUALLY - 1

else: #SIMULATION
  doorInstalled = True
  doorState = False
  requestedDoorState = False
  doorTimeLeft = 0
  DOOR_OPENED_MANUALLY = 999
  DOOR_OPENED_MANUALLY_STATUS_SENT = DOOR_OPENED_MANUALLY - 1
      


Log("Connecting to IoT Hub...")	
timeToLive_s = 22 * 60 * 60
device_client = IoTHubDeviceClient.create_from_connection_string(CONNECTION_STRING, sastoken_ttl=timeToLive_s)


async def sendLogMessage(msgToSend):
  try:
    await asyncio.wait_for(device_client.send_message(msgToSend), timeout=1)
    Log('Sent: '+msgToSend)
  except:
    Log('Could not send: '+msgToSend)


async def updateVendingStatus(ShelfNo, _vendTime_s):
  timeStamp = '"timeStamp":"'+datetime.datetime.now().strftime(timeStampFormat)+'"'
  await sendLogMessage('{"property":"vendingStatus","shelf":'+format(ShelfNo+1)+',"shelfTimeLeft":'+format(_vendTime_s)+','+timeStamp+'}')
  if SIMULATION: 
    try:
      sim_IO.vend(ShelfNo, _vendTime_s)
    except:
      traceback.print_exc()


def setVendingState(shelf, newState):
  global VENDINGctrl
  if SIMULATION == False: VENDINGctrl[shelf]['Output'].value = newState
  VENDINGctrl[shelf]['state'] = newState
  
def getVendingState(shelf):
  global VENDINGctrl
  return VENDINGctrl[shelf]['state']
  
  
def setDoorState(newState):
  global Door
  global doorState
  doorState = newState
  if SIMULATION == False: Door.value = doorState
  
def getDoorState():
  global doorState
  return doorState
  
async def updateDoorStatus():
  global doorTimeLeft
  timeStamp = '"timeStamp":"'+datetime.datetime.now().strftime(timeStampFormat)+'"'
  await sendLogMessage('{"property":"doorStatus","status":'+format(getDoorState(), 'd')+',"timeLeft":'+format(doorTimeLeft, 'd')+','+timeStamp+'}')
  if SIMULATION: 
    try:
      sim_IO.door(doorTimeLeft, _doorTime_s)
    except:
      traceback.print_exc()

  
def startButtonPressed(CabinNo):
  global CABINctrl
  if SIMULATION == False: return CABINctrl[CabinNo]['UserSwitch'].value == False
  else: return sim_IO.userButton[CabinNo] == True
  
  
async def updateCabinStatus(cabinNo, status, timeLeft, timeToStart):
  timeStamp = '"timeStamp":"'+datetime.datetime.now().strftime(timeStampFormat)+'"'
  await sendLogMessage('{"property":"cabinStatus","cabinNo":'+format(cabinNo)+',"status":'+format(status, 'd')+',"timeAutostart":'+format(timeToStart, '0.0f')+', "timeLeft":'+format(timeLeft)+','+timeStamp+'}')
  if SIMULATION: 
    try:
      sim_IO.cabin(cabinNo, status, timeLeft, timeToStart);
    except:
      traceback.print_exc()



def setCabinState(CabinNo, newState):
  global CABINctrl
  if SIMULATION == False: CABINctrl[CabinNo]['Output'].value = newState
  
  
  

async def DoorHandler():
  # Open / close door (if it is installed)
  global requestedDoorState
  global doorTimeLeft
  global WebHookDoorOpen
  global WebHookDoorClose
  while True:
    if (requestedDoorState != getDoorState()) and doorInstalled:
      if requestedDoorState == False:
        Log("Door closed manually")
        setDoorState(False)
        if WebHookDoorClose != "0.0.0.0": requests.post(WebHookDoorClose)
        doorTimeLeft = 0 
      else:
        setDoorState(True)
        if WebHookDoorOpen != "0.0.0.0": requests.post(WebHookDoorOpen)
        # wait for timeout
        while (doorTimeLeft > 0) and (requestedDoorState == True):
          if doorTimeLeft != DOOR_OPENED_MANUALLY_STATUS_SENT: 
            doorTimeLeft -= 1
            await updateDoorStatus()
          await asyncio.sleep(1)
        # close door
        setDoorState(False)
        if WebHookDoorClose != "0.0.0.0": requests.post(WebHookDoorClose)
        requestedDoorState = False
        doorTimeLeft = 0
        Log("Door closed")
      await updateDoorStatus()
    await asyncio.sleep(1)

      



# https://stackoverflow.com/questions/7243750/download-file-from-web-in-python-3
def download(url, file_name):
    # open in binary mode
    try:
        with open(file_name, "wb") as file:
            # get request
            try:
                response = requests.get(url)
            except:
                Log("error retreiving " + url)
            # write to file
            file.write(response.content)
            file.close()        
    except:
        Log("error opening " + file_name + " for writing")
        







async def sendCurrentConfig():
      global comforttanVer
      global pollcaVer
      global cardterminalVer
      global kernelVer
      global CabinsInstalled
      global VendingShelfesInstalled
      global doorInstalled
      progVer = '"comforttanVer":"'+comforttanVer+'"'
      caVer = '"pollcaVer":"'+pollcaVer+'"'
      cardVer = '"cardterminalVer":"'+cardterminalVer+'"'
      kernVer = '"kernelVer":"'+kernelVer+'"'
      cabinCnt = '"noOfCabins":'+format(CabinsInstalled)
      shelfCnt = '"noOfShelfes":'+format(VendingShelfesInstalled)
      doorCnt = '"doorInstalled":'+format(doorInstalled)
      timeStamp = '"timeStamp":"'+datetime.datetime.now().strftime(timeStampFormat)+'"'
      await sendLogMessage('{"property":"configuration",'+progVer+','+caVer+','+cardVer+','+kernVer+','+cabinCnt+','+shelfCnt+','+doorCnt+','+timeStamp+'}')



#################################
# Commands from backend - begin #
#################################


async def  cardPayment(JSONcontent):
    global _amountToRequest
    _amountToRequest = JSONcontent['requestAmount']
    if JSONcontent['newRequest'] == 0: _amountToRequest = 0
    if _amountToRequest not in range(100000): 
        Log("requestAmount invalid: "+str(_amountToRequest)+" - revert to 0")
        _amountToRequest = 0


async def coinAcceptor(JSONcontent):
    global _ActivateCA
    global _DeActivateCA
    global _CA_ClosingAmount
    if 'activate' in JSONcontent:
      if JSONcontent['activate'] == 1:
        _ActivateCA = 1
        _DeActivateCA = 0
      else: 
        _ActivateCA = 0
        _DeActivateCA = 1
    else: 
      _ActivateCA = 0
      _DeActivateCA = 1
    if 'closingAmount' in JSONcontent: _CA_ClosingAmount = JSONcontent['closingAmount']
    else: _CA_ClosingAmount = 9999999


async def cmdManualDoor(JSONcontent):
    global requestedDoorState
    global doorTimeLeft
    global doorInstalled
    global WebHookDoorOpen
    if (doorInstalled == False) and (WebHookDoorOpen == "0.0.0.0"):
      doorTimeLeft = -1
      await updateDoorStatus()
      Log("Door not installed")
    else:
      # send current status
      await updateDoorStatus()
      # set new state
      requestedDoorState = JSONcontent["activate"]
      if requestedDoorState == 1: doorTimeLeft = DOOR_OPENED_MANUALLY
      else: doorTimeLeft = 0
      Log("Manual door control: " + format(requestedDoorState))


async def  cmdStartSession(JSONcontent):
  global _startCabin
  global _sessionTime_m
  global _stopCabin
  cabinNo = JSONcontent["cabinNo"]-1
  if ((JSONcontent["newState"] == 0) or ('timeSet' in JSONcontent and JSONcontent["timeSet"] == 0) or (('timeSet' in JSONcontent) == False)):
    _stopCabin[cabinNo] = 1
    _startCabin[cabinNo] = 0
    _sessionTime_m[cabinNo] = 0
  elif (JSONcontent["newState"] == 1):
    _stopCabin[cabinNo] = 0
    _startCabin[cabinNo] = 1
    _sessionTime_m[cabinNo] = JSONcontent["timeSet"]


async def  cmdStartVending(JSONcontent):
    global _activateShelf
    _activateShelf[JSONcontent["shelf"]-1] = 1


async def  sendLogFiles(JSONcontent):
    fileToUpload = 'AllLogs.zip'
    filePathToUpload = RPi_HOME_FOLDER
    logType = 'AllLogs'
    # Zip all logfiles
    Log('Zipping to '+filePathToUpload+fileToUpload+'...')
    try:
      with zipfile.ZipFile(filePathToUpload+fileToUpload, 'w', zipfile.ZIP_DEFLATED) as myzip:
        if not SIMULATION:
          myzip.write('/boot/rc_local.log')
          try: myzip.write('/boot/rc_local_1.log')
          except: pass
          try: myzip.write('/boot/rc_local_2.log')
          except: pass
        for dirpath,dirs,files in os.walk('Logs'):
          for f in files:
            fn = os.path.join(dirpath, f)
            myzip.write(fn)
        for dirpath,dirs,files in os.walk('CALogs'):
          for f in files:
            fn = os.path.join(dirpath, f)
            myzip.write(fn)
        for dirpath,dirs,files in os.walk('CardLogs'):
          for f in files:
            fn = os.path.join(dirpath, f)
            myzip.write(fn)
      try:
          with open(filePathToUpload + fileToUpload, "rb") as a_file:
              file_dict = {'file': a_file}
              file_dict = {fileToUpload: a_file}
              global host
              logURL = host + "api/mobile/receive-log/" + format(JSONcontent["object_id"]) + "/" + logType
              Log ('postback URL. '+logURL)
              timeoutVal = 60
              for attempt in range(10):
                  try:
                      response = requests.post(logURL, files=file_dict, timeout=timeoutVal)
                      timeoutVal += 2
                      Log (response.text)
                      timeStamp = '"timeStamp":"'+datetime.datetime.now().strftime(timeStampFormat)+'"'
                      await sendLogMessage('{"property":"logFileStatus","status":0,'+timeStamp+'}')
                  except:
                      eType = sys.exc_info()[0]
                      eValue = sys.exc_info()[1]
                      Log ("error posting log file - %s" % eType)
                      Log ("\tvalue: %s" % eValue )
                      timeStamp = '"timeStamp":"'+datetime.datetime.now().strftime(timeStampFormat)+'"'
                      await sendLogMessage('{"property":"logFileStatus","status":2,'+timeStamp+'}')
                  else:
                      break # no exceptions - break out of attempts
              else: # all attempts failed
                  Log ('all attempts to post log file failed')
                  timeStamp = '"timeStamp":"'+datetime.datetime.now().strftime(timeStampFormat)+'"'
                  await sendLogMessage('{"property":"logFileStatus","status":2,'+timeStamp+'}')
      except:
          Log ('filename error')
          traceback.print_exc()
          timeStamp = '"timeStamp":"'+datetime.datetime.now().strftime(timeStampFormat)+'"'
          await sendLogMessage('{"property":"logFileStatus","status":2,'+timeStamp+'}')
      # remove zipfile
      os.remove(fileToUpload) 
    except:
      # zip error
      Log ('Logfile zip error')
      traceback.print_exc()
      timeStamp = '"timeStamp":"'+datetime.datetime.now().strftime(timeStampFormat)+'"'
      await sendLogMessage('{"property":"logFileStatus","status":3,'+timeStamp+'}')



async def uploadUnzipFile(JSON):
    errorCode = -1
    if ('name' in JSON) and ('path' in JSON) and ('MD5' in JSON): # All 3 must be present
        _fileName = JSON['name']
        _locationPath = JSON['path']
        _MD5 = JSON['MD5']
        _saveFile = RPi_HOME_FOLDER + _fileName
        # download the file and save it
        download(_locationPath, _saveFile)
        # Open,close, read file and calculate MD5 on its contents 
        with open(_saveFile, 'rb') as file_to_check: # https://stackoverflow.com/questions/16874598/how-do-i-calculate-the-md5-checksum-of-a-file-in-python
            # read contents of the file
            data = file_to_check.read()    
            # pipe contents of the file through
            md5_returned = hashlib.md5(data).hexdigest()
        # if MD5 OK: 
        if md5_returned == _MD5:
            Log(" Received :" + _fileName + " correctly")
            try:
                # unzip file (https://stackoverflow.com/questions/3451111/unzipping-files-in-python)
                with zipfile.ZipFile(_saveFile, 'r') as zip_ref:
                    #unzip to temporary "NEW_RELEASE_FOLDER" folder (content of the temporary folder will be moved to home folder on reboot)
                    zip_ref.extractall(NEW_RELEASE_FOLDER)
                if (comforttanVer <= '2.19') and (os.path.exists(NEW_RELEASE_FOLDER+'comforttan.py')): # old releases will not look into NEW_RELEASE_FOLDER, so we have to "help"
                  os.system('sudo cp '+NEW_RELEASE_FOLDER+'comforttan.py .')
                errorCode = 0
            except:
                Log("Error unzipping :" + _saveFile)
                traceback.print_exc()
                errorCode = 3
                # Delete whatever is in the temporary folder so that we dont end up with a "half version"
                try:
                  try:
                    os.system('sudo rm '+NEW_RELEASE_FOLDER +'*')
                  except:
                    pass
                  try:
                    os.system('sudo rmdir '+NEW_RELEASE_FOLDER)
                  except:
                    pass
                  Log("Temporary files removed")
                except:
                  Log("Could not remove temporary files")
        else:
            Log("Error receiving :" + _fileName)
            Log("Received MD5 :" + md5_returned)
            Log("Expected MD5 :" + _MD5)
            errorCode = 1
        os.remove(_saveFile)
    else:
        Log('Error in JSON input')
        errorCode = 1 
    Log("report back - Errorcode: " + str(errorCode))
    timeStamp = '"timeStamp":"'+datetime.datetime.now().strftime(timeStampFormat)+'"'
    await sendLogMessage('{"property":"uploadFileStatus","status":'+format(errorCode)+','+timeStamp+'}') 
    


async def changeSettings(JSONsettings):
    global _maxSessionTime_m
    global _autostartTimeout_s
    global _vendTime_s
    global _doorTime_s
    try:
      _maxSessionTime_m = JSONsettings['maxSessionTime']
      _autostartTimeout_s = JSONsettings['autostartTimeout']
      _vendTime_s = JSONsettings['vendTime']
      _doorTime_s = JSONsettings['doorTime']
      if _doorTime_s not in range(9999): _doorTime_s = 0
      await sendCurrentConfig()
    except:
      traceback.print_exc()
    Log ('_doorTime_s:'+str(_doorTime_s) + ' - _autostartTimeout_s:' +  str(_autostartTimeout_s) + ' - _vendTime_s:' + str(_vendTime_s) + ' - _maxSessionTime_m:' + str(_maxSessionTime_m))



#################################
# Commands from backend - end   #
#################################

def checkUserStart():
  global CabinsInstalled
  global SESSIONSTATUS
  global SESSION_STATUS_AWAITING_USER_START
  global SESSION_STATUS_RUNNING
  global CABINctrl
  global _autostartTimeout_s
  sessionRunning = False
  CabinCnt = 0
  while CabinCnt < CabinsInstalled:
    if (SESSIONSTATUS[CabinCnt]['sessionStatus'] == SESSION_STATUS_AWAITING_USER_START):
      sessionRunning = True
      if startButtonPressed(CabinCnt) or (time.perf_counter() > CABINctrl[CabinCnt]['timer_s'] + _autostartTimeout_s): # if cabin is waiting for user start and (button is pressed or timeout)
        # activate output
        setCabinState(CabinCnt, True)
        CABINctrl[CabinCnt]['timer_s'] = time.perf_counter()
        SESSIONSTATUS[CabinCnt]['sessionStatus'] = SESSION_STATUS_RUNNING
        Log(SESSIONSTATUS[CabinCnt])
    CabinCnt = CabinCnt + 1
  return sessionRunning

def checkTanningTime():
  global CabinsInstalled
  global SESSIONSTATUS
  global SESSION_STATUS_IDLE
  global SESSION_STATUS_RUNNING
  global CABINctrl
  sessionRunning = False
  CabinCnt = 0
  while CabinCnt < CabinsInstalled:
    if (SESSIONSTATUS[CabinCnt]['sessionStatus'] == SESSION_STATUS_RUNNING):
      sessionRunning = True
      if time.perf_counter() > CABINctrl[CabinCnt]['timer_s'] + 60: # one minute has passed
        CABINctrl[CabinCnt]['timer_s'] = time.perf_counter()
        SESSIONSTATUS[CabinCnt]['sessionTimeLeft_m'] = SESSIONSTATUS[CabinCnt]['sessionTimeLeft_m'] - 1
        Log('Cabin '+str(CabinCnt+1) + ' timeleft: ' + str(SESSIONSTATUS[CabinCnt]['sessionTimeLeft_m']))
        if SESSIONSTATUS[CabinCnt]['sessionTimeLeft_m'] <= 0: # Session completed
          # deactivate output
          setCabinState(CabinCnt, False)
          SESSIONSTATUS[CabinCnt]['sessionStatus'] = SESSION_STATUS_IDLE
          Log(SESSIONSTATUS[CabinCnt])
    CabinCnt = CabinCnt + 1
  return sessionRunning
      


def testNewVersion():
  try:
    if os.path.exists(NEW_RELEASE_FOLDER):
      os.system('sudo cp '+NEW_RELEASE_FOLDER +'* '+RPi_HOME_FOLDER)
      Log ('\r\n\r\nUpdating from revision ' + comforttanVer)
      dir_list = os.listdir(NEW_RELEASE_FOLDER)
      Log (dir_list)
      os.system('sudo rm '+NEW_RELEASE_FOLDER +'*')
      os.system('sudo rmdir '+NEW_RELEASE_FOLDER)
      os.system("sudo reboot")
  except:
    traceback.print_exc()
    try:
      Log ('Removing files')
      os.system('sudo rm '+NEW_RELEASE_FOLDER +'*')
    except:
      pass
    try:
      Log ('Removing folder')
      os.system('sudo rmdir '+NEW_RELEASE_FOLDER)
    except:
      pass


def machineInitSim():
  global locationID
  global CALLBACK_URL
  global CONNECTION_STRING
  sim_IO.setLocation(locationID)
  sim_IO.setCallbackURL(CALLBACK_URL)
  sim_IO.setConnectionString(CONNECTION_STRING)
  while sim_IO.btnAppStartPressed == False:
    sim_IO.updateGUI()
  print(sim_IO.btnAppStartPressed)
  locationID = sim_IO.getLocation()
  CALLBACK_URL = sim_IO.getCallbackURL()
  CONNECTION_STRING = sim_IO.getConnectionString()
  



def machineInitHW():
    global MAX_NO_OFF_CABINS 
    global MAX_NO_OFF_VENDING_SHELFES 
    global CABINctrl 
    global VENDINGctrl
    global CoinAcceptorPort
    global CardReaderPort
    global doorState
    global CabinsInstalled
    global VendingShelfesInstalled
    global doorInstalled
    global newReset

    try:
      Log ('\r\n\r\n\r\nStarting comforttan.py revision ' + comforttanVer)
      Log ('Location ID: '+locationID)
      Log ('Callback URL: {0}'.format(CALLBACK_URL))
      Log ('\r\n\r\nInitializing hardware')
      newReset = True
      
      Log ('Cleaning logfiles')
      #delete-all-files-in-a-folder-except-last-n-items
      noOfFilesToKeep = 10
      #import os
      for filename in sorted(os.listdir("Logs"))[:-noOfFilesToKeep]:
          filename_relPath = os.path.join("Logs",filename)
          os.remove(filename_relPath)
      for filename in sorted(os.listdir("CALogs"))[:-noOfFilesToKeep]:
          filename_relPath = os.path.join("CALogs",filename)
          os.remove(filename_relPath)
      for filename in sorted(os.listdir("CardLogs"))[:-noOfFilesToKeep]:
          filename_relPath = os.path.join("CardLogs",filename)
          os.remove(filename_relPath)

      # Map UniPi relays on main board
      CABINctrl[0]['Output'] = mcp.get_pin(7)   # Relæ 1: Kabine 1
      CABINctrl[1]['Output'] = mcp.get_pin(6)   # Relæ 2: Kabine 2
      CABINctrl[2]['Output'] = mcp.get_pin(5)   # Relæ 3: Kabine 3
      CABINctrl[3]['Output'] = mcp.get_pin(4)   # Relæ 4: Kabine 4
      CABINctrl[4]['Output'] = mcp.get_pin(3)   # Relæ 5: Kabine 5
      CABINctrl[5]['Output'] = mcp.get_pin(2)   # Relæ 6: Kabine 6
      VENDINGctrl[0]['Output'] = mcp.get_pin(1) # Relæ 7: Automat hylde 1
      VENDINGctrl[1]['Output'] = mcp.get_pin(0) # Relæ 8: Automat hylde 2

      # Map UniPi relays on Aux board #1
      if AuxCard1Installed: 
          Log('Aux card #1 installed')
          VENDINGctrl[2]['Output'] = mcp1.get_pin(7) # Relæ 9:  Automat hylde 3
          VENDINGctrl[3]['Output'] = mcp1.get_pin(6) # Relæ 10: Automat hylde 4
          VENDINGctrl[4]['Output'] = mcp1.get_pin(5) # Relæ 11: Automat hylde 5
          VENDINGctrl[5]['Output'] = mcp1.get_pin(4) # Relæ 12: Automat hylde 6
          VENDINGctrl[6]['Output'] = mcp1.get_pin(3) # Relæ 13: Automat hylde 7
          VENDINGctrl[7]['Output'] = mcp1.get_pin(2) # Relæ 14: Automat hylde 8
          VENDINGctrl[8]['Output'] = mcp1.get_pin(1) # Relæ 15: Automat hylde 9
          VENDINGctrl[9]['Output'] = mcp1.get_pin(0) # Relæ 16: Automat hylde 10

      # Map UniPi relays on Aux board #2
      if AuxCard2Installed: 
          Log('Aux card #2 installed')
          CABINctrl[6]['Output'] = mcp2.get_pin(6) # Relæ 18: Kabine 7
          CABINctrl[7]['Output'] = mcp2.get_pin(5) # Relæ 19: Kabine 8
          CABINctrl[8]['Output'] = mcp2.get_pin(4) # Relæ 20: Kabine 9
          CABINctrl[9]['Output'] = mcp2.get_pin(3) # Relæ 21: Kabine 10
          #                      = mcp2.get_pin(2) # Relæ 22:
          #                      = mcp2.get_pin(1) # Relæ 23:
          #                      = mcp2.get_pin(0) # Relæ 24:

      n = 0
      if AuxCard2Installed: CabinsInstalled = MAX_NO_OFF_CABINS
      else: CabinsInstalled = 6
      while n < CabinsInstalled:
          CABINctrl[n]['Output'].switch_to_output(value=False)
          n = n + 1

      n = 0
      if AuxCard1Installed: VendingShelfesInstalled = MAX_NO_OFF_VENDING_SHELFES
      else: VendingShelfesInstalled = 2
      while n < VendingShelfesInstalled:
          VENDINGctrl[n]['Output'].switch_to_output(value=False)
          n = n + 1

      #GPIO setup:
      #button = digitalio.DigitalInOut(board.D27) # D27 = UniPi IO3
      #button.direction = digitalio.Direction.INPUT
      #button.pull = digitalio.Pull.UP

      # Map UniPi GPIO's    
      CABINctrl[0]['UserSwitch'] = digitalio.DigitalInOut(board.D4)  # I01 GPIO04
      CABINctrl[1]['UserSwitch'] = digitalio.DigitalInOut(board.D17) # I02 GPIO17
      CABINctrl[2]['UserSwitch'] = digitalio.DigitalInOut(board.D27) # I03 GPIO27
      CABINctrl[3]['UserSwitch'] = digitalio.DigitalInOut(board.D23) # I04 GPIO23
      CABINctrl[4]['UserSwitch'] = digitalio.DigitalInOut(board.D22) # I05 GPIO22
      CABINctrl[5]['UserSwitch'] = digitalio.DigitalInOut(board.D24) # I06 GPIO24
      CABINctrl[6]['UserSwitch'] = digitalio.DigitalInOut(board.D11) # I07 GPIO11
      CABINctrl[7]['UserSwitch'] = digitalio.DigitalInOut(board.D7)  # I08 GPIO07
      CABINctrl[8]['UserSwitch'] = digitalio.DigitalInOut(board.D8)  # I09 GPIO08
      CABINctrl[9]['UserSwitch'] = digitalio.DigitalInOut(board.D9)  # I10 GPIO09
      #                       = digitalio.DigitalInOut(board.D25) # I11 GPIO25
      #                       = digitalio.DigitalInOut(board.D10) # I12 GPIO10
      #                       = digitalio.DigitalInOut(board.D31) # I13 GPIO31
      #                       = digitalio.DigitalInOut(board.D30) # I14 GPIO30
          
      # Setup UserSwitch inputs
      n = 0
      while n < MAX_NO_OFF_CABINS:
          CABINctrl[n]['UserSwitch'].direction = digitalio.Direction.INPUT
          CABINctrl[n]['UserSwitch'].pull = digitalio.Pull.UP
          n = n + 1
      
      CoinAcceptorPort = HW_IO.getCoinAcceptorPort()
      CardReaderPort = HW_IO.getCardReaderPort()

      Log ('\r\n\r\nHardware initialized')

    except:
      traceback.print_exc()



prevDate = datetime.datetime.now().strftime('%d')


async def machineControl():
  global RPi_HOME_FOLDER 
  global SESSION_STATUS_IDLE
  global SESSION_STATUS_AWAITING_USER_START
  global SESSION_STATUS_RUNNING
  global SESSIONSTATUS
  global _startCabin
  global _stopCabin
  global _sessionTime_m
  global _activateShelf
  global _amountToRequest
  global _autostartTimeout_s
  global _maxSessionTime_m 
  global _vendTime_s 
  global _doorTime_s 
  global _ActivateCA 
  global _DeActivateCA 
  global _CA_Activated 
  global _CA_ClosingAmount 
  global prevSESSIONSTATUS 
  global CABINctrl 
  global VENDINGctrl
  global LogErrors
  global CoinAcceptorPort
  global doorInstalled
  global newReset
  global prevDate
  global WebHookAnyBedOn
  global WebHookAllBedsOn
  global WebHookAllBedsOff 
  global WebHookDoorOpen
  global WebHookDoorClose
    
  if newReset:
    # inform backend of reset
    newReset = False
    Log ('New reset at '+datetime.datetime.now().strftime(timeStampFormat))
    timeStamp = '"timeStamp":"'+datetime.datetime.now().strftime(timeStampFormat)+'"'
    await sendLogMessage('{"property":"reset",'+timeStamp+'}')

    await sendCurrentConfig()
    Log ('_doorTime_s:'+str(_doorTime_s) + ' - _autostartTimeout_s:' +  str(_autostartTimeout_s) + ' - _vendTime_s:' + str(_vendTime_s) + ' - _maxSessionTime_m:' + str(_maxSessionTime_m))
    try:
      proc = subprocess.Popen(['python3', 'cardterminalDisable.py'])
    except:
      pass
    CAStatus = -1

  cabinStartIx = 0 # allows users to start more than one cabins at the same time.
  cabinStopIx = 0
  vendIx = 0
  while True:
      if SIMULATION: sim_IO.updateGUI()
      await asyncio.sleep(.1)
      
      # Maybe start or stop a New Session
      
      if _startCabin[cabinStartIx] > 0 : 
          # Grap the new session
          CabinNo = cabinStartIx
          _startCabin[cabinStartIx]  = 0
          Log("Starting a new session on cabin " + str(CabinNo + 1))
          if CabinNo in range(CabinsInstalled):
              SESSIONSTATUS[CabinNo]['cabin'] = CabinNo + 1  
              SESSIONSTATUS[CabinNo]['sessionStatus'] = SESSION_STATUS_AWAITING_USER_START 
              SESSIONSTATUS[CabinNo]['sessionTimeLeft_m'] = _sessionTime_m[cabinStartIx]
              if SESSIONSTATUS[CabinNo]['sessionTimeLeft_m'] > _maxSessionTime_m: SESSIONSTATUS[CabinNo]['sessionTimeLeft_m'] = _maxSessionTime_m
              CABINctrl[CabinNo]['timer_s'] = time.perf_counter() # grap time for UserSwitch timeout
              # Open door (if it is installed)
              if doorInstalled or (WebHookDoorOpen != "0.0.0.0"):
                global requestedDoorState
                global doorTimeLeft
                requestedDoorState = True
                if (doorTimeLeft != DOOR_OPENED_MANUALLY) and (doorTimeLeft != DOOR_OPENED_MANUALLY_STATUS_SENT): doorTimeLeft = _doorTime_s
          else: Log('Invalid CabinNo: ' + str(CabinNo + 1))
      cabinStartIx += 1
      if cabinStartIx >= MAX_NO_OFF_CABINS: cabinStartIx = 0
          
      if _stopCabin[cabinStopIx] > 0 :
          # Grap the session
          CabinNo = cabinStopIx
          _stopCabin[cabinStopIx]  = 0
          Log("Stopping cabin " + str(CabinNo + 1))
          if CabinNo in range(CabinsInstalled):
              # deactivate output
              setCabinState(CabinNo, False)
              SESSIONSTATUS[CabinNo]['sessionStatus'] = SESSION_STATUS_IDLE
              SESSIONSTATUS[CabinNo]['sessionTimeLeft_m'] = 0
              Log(SESSIONSTATUS[CabinNo])
          else: Log('Invalid CabinNo: ' + str(CabinNo + 1))
      cabinStopIx += 1
      if cabinStopIx >= MAX_NO_OFF_CABINS: cabinStopIx = 0

      # Maybe start a vending
      if _activateShelf[vendIx] > 0:
          ShelfNo = vendIx
          _activateShelf[vendIx] = 0
          if ShelfNo in range(VendingShelfesInstalled):
              setVendingState(ShelfNo, True)
              VENDINGctrl[ShelfNo]['timer_s'] = time.perf_counter() # grap time 
              Log("Vending Started on shelf " + str(ShelfNo + 1))
              await updateVendingStatus(ShelfNo, _vendTime_s)
          else: Log('Invalid shelf number: ' + str(ShelfNo + 1))
      vendIx += 1
      if vendIx >= MAX_NO_OFF_VENDING_SHELFES: vendIx = 0
              

      # Maybe request an amount from the card terminal
      if _amountToRequest > 0:
          Log('Request '+str(_amountToRequest)+' from Card terminal')
          #Log('Terminating previous sessions')
          try:
              proc.terminate() #terminate previous session - Preffer terminate() over kill() to be sure that logfile is closed
          except:
              Log('failed terminating card - not running')
              #pass
          _DeActivateCA = 1
          await asyncio.sleep(.1)
          timeStamp = '"timeStamp":"'+datetime.datetime.now().strftime(timeStampFormat)+'"'
          await sendLogMessage('{"property":"cardTerminalStatus","status": 1'+','+timeStamp+'}')
          try:
              proc = subprocess.Popen(['python3', 'cardterminal.py', str(_amountToRequest)])
          except:
              Log ('Error running cardterminal.py');
          _amountToRequest = 0;
          timeStamp = '"timeStamp":"'+datetime.datetime.now().strftime(timeStampFormat)+'"'
          await sendLogMessage('{"property":"cardTerminalStatus","status": 0'+','+timeStamp+'}')
          #Log('we are back')

          
      # Control Coin Acceptor
      global CAopenTime
      logFileTimeStamp = datetime.datetime.now().strftime("%Y%m%d")
      CAlogfilename = RPi_HOME_FOLDER+'CALogs/CALog'+logFileTimeStamp+'.log'
      CAStatus = -1
      if _DeActivateCA > 0:
          _DeActivateCA = 0
          _ActivateCA = 0 # Deactivate has highest priority
          _CA_Activated = 0
          # terminate CA application
          try:
              os.kill(CA_proc.pid, signal.SIGSTOP)
              Log ('CA stopped')
              CAStatus = 0
          except:
              Log ("Could not stop CA - Process is not startet or CA was already stopped")
              CAStatus = 0
          timeStamp = '"timeStamp":"'+datetime.datetime.now().strftime(timeStampFormat)+'"'
          await sendLogMessage('{"property":"coinAcceptorStatus","status": '+format(CAStatus)+','+timeStamp+'}')
      elif _ActivateCA > 0:
          _ActivateCA = 0
          # start CA application
          if CoinAcceptorPort != 'none':
              if _CA_Activated == 0:
                  # First make sure card reader is disabled:
                  try:
                      proc.terminate() #terminate previous session - Preffer terminate() over kill() to be sure that logfile is closed 
                  except:
                      Log('failed terminating card reader')
                  await asyncio.sleep(.1)
                  try:
                      subprocess.Popen(['python3', 'cardterminalDisable.py'])
                  except:
                      Log ('Error launching cardterminalDisable');
                  # Now, lets open the coin aceptor:
                  try:
                      if os.path.isfile(CAlogfilename) : CAlogfile = open(CAlogfilename, 'a') # (a)ppend (w)rite
                      else : CAlogfile = open(CAlogfilename, 'w') # (a)ppend (w)rite
                  except:
                      Log ('CA logfile error')
                      CAlogfile = "None"
                  try:
                      if LogErrors: 
                          CA_proc = subprocess.Popen([RPi_HOME_FOLDER+'pollca', CoinAcceptorPort, CALLBACK_URL, 'Y', str(_CA_ClosingAmount)], stdout=CAlogfile, stderr=CAlogfile)
                      else:
                          CA_proc = subprocess.Popen([RPi_HOME_FOLDER+'pollca', CoinAcceptorPort, CALLBACK_URL, 'N', str(_CA_ClosingAmount)], stdout=CAlogfile, stderr=CAlogfile)
                      _CA_Activated = 1
                      Log ('CA startet')
                      Log ('Coins are logged to: '+CAlogfilename)
                      CAopenTime =  time.perf_counter()
                      CAStatus = 1
                  except:
                      eType = sys.exc_info()[0]
                      eValue = sys.exc_info()[1]
                      Log ("Could not start CA - %s" % eType)
                      Log ("\tvalue: %s" % eValue )
                      _DeActivateCA = 1
                      CAStatus = 2
                      CAlogfile.close()
              else:
                  Log ('CA already activated')
                  CAStatus = 1
          else:
              Log ('No CA available')
              CAStatus = 2
          timeStamp = '"timeStamp":"'+datetime.datetime.now().strftime(timeStampFormat)+'"'
          await sendLogMessage('{"property":"coinAcceptorStatus","status": '+format(CAStatus)+','+timeStamp+'}')
      if (_CA_Activated == 1) and (time.perf_counter() > CAopenTime + 1):
        try:
          with open(CAlogfilename, 'r') as f:
            finalLine = f.readlines()[-2] #find second-last line
            if 'bye' in finalLine:
              Log('amount reached')
              _DeActivateCA = 1
        except:
            traceback.print_exc()
      
          
      # Update changed status to server
      i = 0
      while i < CabinsInstalled:
          if prevSESSIONSTATUS[i]['cabin'] != SESSIONSTATUS[i]['cabin'] or \
             prevSESSIONSTATUS[i]['sessionStatus'] != SESSIONSTATUS[i]['sessionStatus'] or \
             prevSESSIONSTATUS[i]['sessionTimeLeft_m'] != SESSIONSTATUS[i]['sessionTimeLeft_m']:
              # if data has changed since last update - update again
              if (SESSIONSTATUS[i]['sessionStatus'] == SESSION_STATUS_AWAITING_USER_START): autostartTimeLeft = _autostartTimeout_s
              else: autostartTimeLeft = 0
              await updateCabinStatus(i+1, SESSIONSTATUS[i]['sessionStatus'], SESSIONSTATUS[i]['sessionTimeLeft_m'], autostartTimeLeft)
              # and store changes
              prevSESSIONSTATUS[i]['cabin'] = SESSIONSTATUS[i]['cabin']
              prevSESSIONSTATUS[i]['sessionStatus'] = SESSIONSTATUS[i]['sessionStatus']
              prevSESSIONSTATUS[i]['sessionTimeLeft_m'] = SESSIONSTATUS[i]['sessionTimeLeft_m']
          i = i + 1
      
      waitingForUserStart = checkUserStart()          
      tanningTimeRunning = checkTanningTime()
      bedActive = waitingForUserStart or tanningTimeRunning
      #### WebHooks
      global prevAnyBedActive
      global prevAllBedsActive
      if bedActive and not prevAnyBedActive:
        if WebHookAnyBedOn != "0.0.0.0": requests.post(WebHookAnyBedOn)
      elif not bedActive and prevAnyBedActive:
        if WebHookAllBedsOff != "0.0.0.0": requests.post(WebHookAllBedsOff)
      prevAnyBedActive = bedActive
      allOn = True
      for CabinCnt in range(CabinsInstalled):  
        if (SESSIONSTATUS[CabinCnt]['sessionStatus'] != SESSION_STATUS_RUNNING): allOn = False
      if allOn and not prevAllBedsActive:
        if WebHookAllBedsOn != "0.0.0.0": requests.post(WebHookAllBedsOn)
      prevAllBedsActive = allOn
      #### WebHooks
      
#      checkVendingTime()
      vendingActive = False
      ShelfCnt = 0
      while ShelfCnt < VendingShelfesInstalled:
        if getVendingState(ShelfCnt) == True:
          vendingActive = True
          if time.perf_counter() > VENDINGctrl[ShelfCnt]['timer_s'] + _vendTime_s :
            setVendingState(ShelfCnt, False)
            Log('Vending on shelf ' + str(ShelfCnt+1) + ' completed')
            await updateVendingStatus(ShelfNo, 0)
        ShelfCnt = ShelfCnt + 1


      # reboot if logfile becomes too large
      timeNow = datetime.datetime.now().strftime("%H%M%S")
      if timeNow == '020000' or timeNow == '030000' : 
          rcSize = os.path.getsize("/boot/rc_local.log")
          rcSizeMax = 3000000         # r1.16: In idle mode with LogErrors = 1 we gather +250k /h (6M/d) - With LogErrors = 0 expect 60k/h (1M5/d) - r2.04 idle: appr 25k/h
          if rcSize > rcSizeMax : 
              Log ('Log file size: ' + str(rcSize))
              Log ('Max log file size: ' + str(rcSizeMax))
              # make sure all cabins are free  
              allCabinsFree = True
              CabinCnt = 0
              while CabinCnt < CabinsInstalled:
                  if (SESSIONSTATUS[CabinCnt]['sessionStatus'] != SESSION_STATUS_IDLE): allCabinsFree = False
                  CabinCnt = CabinCnt + 1
              if allCabinsFree == False :
                  Log ('A cabin is in use - No reboot')
              else :
                  Log ('Log file exceeded max size - rebooting')
                  os.system("sudo reboot")


      #log date change
      thisDate = datetime.datetime.now().strftime('%d')
      if thisDate != prevDate:
        prevDate = thisDate
  #      Log('The date is now ' + thisDate)
        # cleanup logfiles (sort alphabetical)
        print ('Cleaning logfiles')
        noOfFilesToKeep = 7
        for filename in sorted(os.listdir("Logs"))[:-noOfFilesToKeep]:
            filename_relPath = os.path.join("Logs",filename)
            os.remove(filename_relPath)
        for filename in sorted(os.listdir("CALogs"))[:-noOfFilesToKeep]:
            filename_relPath = os.path.join("CALogs",filename)
            os.remove(filename_relPath)
        for filename in sorted(os.listdir("CardLogs"))[:-noOfFilesToKeep]:
            filename_relPath = os.path.join("CardLogs",filename)
            os.remove(filename_relPath)
          



#######################################
# Azure MQTT specific functions begin #
#######################################


messageExpirationDisabled = False

async def receive_c2d_messages(message):	
  global messageExpirationDisabled
  Log("Message received with payload: {}".format(message.data))
  try:
    try:
      JSONcontent = json.loads(message.data)
      #Log('received data was str')
    except:
      JSONcontent = json.loads(json.dumps(message.data))
      #Log('received data was dict')
    messageHasExpired = False
    if ('expirationTime' in JSONcontent):
      expirationTime = datetime.datetime.strptime(JSONcontent["expirationTime"], timeStampFormat)
      if expirationTime < datetime.datetime.now(): messageHasExpired = True
    elif ('timeStamp' in JSONcontent):
      timeToExpiration = datetime.timedelta(seconds=30) 
      expirationTime = datetime.datetime.strptime(JSONcontent["timeStamp"], timeStampFormat) + timeToExpiration
      if expirationTime < datetime.datetime.now(): messageHasExpired = True
    if JSONcontent["property"] == "diableMessageExpiration": 
      messageExpirationDisabled = True
      Log ("Message expiration is now disabled for the rest of the session...")
    if messageHasExpired and (messageExpirationDisabled == False):
      Log ("Message has expired: {}".format(JSONcontent))
    else:
      Log("Message to parse: {}".format(JSONcontent))
      if JSONcontent["property"] == "cardTerminalControl":
         await cardPayment(JSONcontent)
      elif JSONcontent["property"] == "coinAcceptorControl":
        await coinAcceptor(JSONcontent)
      elif JSONcontent["property"] == "cabinControl":
        await cmdStartSession(JSONcontent)
      elif JSONcontent["property"] == "doorControl":
        await cmdManualDoor(JSONcontent)
      elif JSONcontent["property"] == "vendingControl":
        await cmdStartVending(JSONcontent)
      elif JSONcontent["property"] == "logFileControl":
        await sendLogFiles(JSONcontent)
      elif JSONcontent["property"] == "uploadFileControl":
        await uploadUnzipFile(JSONcontent)
      elif JSONcontent["property"] == "settings":
        await changeSettings(JSONcontent)
      elif JSONcontent["property"] == "reboot":
        Log("Reboot from backend")
        os.system("sudo reboot")
      else:
        Log("Unknown property: "+JSONcontent["property"])
        #TODO send a reply for UnknownProperty
  except:
    Log ("exception: JSON error")
    #TODO send a reply for JSONError
    traceback.print_exc()
	




async def main():	
    #reboot if not able to establish connection within this amount of seconds
    NO_OF_CONNECTION_ATTEMPTS_1min  =  1*60 # min time to wait if no cabins are active
    NO_OF_CONNECTION_ATTEMPTS_40min = 40*60 # max time in case cabins does not become inactive

    #start communication WD
  #  try:
  #    subprocess.Popen(['python3', 'ComWD.py'])
  #  except:
  #    traceback.print_exc()

    if SIMULATION: machineInitSim()
    else: machineInitHW()
    testNewVersion()
    connectionAttempts = 0
    while True:	
        try:	
            # connect the client.
            await device_client.connect()
            # set the message received handler on the client
            device_client.on_message_received = receive_c2d_messages
            Log("Connected to IoT Hub")
            connectionAttempts = 0
            while True:
              await asyncio.gather(
                machineControl(),
                DoorHandler(),
              )
            
        except Exception as e:
            Log (e)
            Log("Unexpected error in main().")
            traceback.print_exc()
            SessionRunning = False
            _waitingForUserStart = checkUserStart()          
            _tanningTimeRunning = checkTanningTime()
            SessionRunning = waitingForUserStart or tanningTimeRunning
            connectionAttempts = connectionAttempts + 1
            if ((connectionAttempts > NO_OF_CONNECTION_ATTEMPTS_1min) and not SessionRunning) or (connectionAttempts > NO_OF_CONNECTION_ATTEMPTS_40min):
              Log ('Gone through ' + str(connectionAttempts) + 'connection attempts - rebooting')
              os.system("sudo reboot")
            
        await asyncio.sleep(1)
        
        
#######################################
# Azure MQTT specific functions end   #
#######################################


    
if __name__ == "__main__":
  exit = 0
  while exit == 0:
    try:	
        asyncio.run(main())	
    except KeyboardInterrupt:	
        # Exit application because user indicated they wish to exit.	
        # This will have cancelled `main()` implicitly.	
        print("")
        print("User initiated exit. Exiting.", flush=True)	
        exit = 1
    except:
        traceback.print_exc()
        print('Unhandled exception - Rebooting', flush=True) # Should not happen
        exit = 1
        os.system("sudo reboot")
    finally:	
        print("Leaving program...", flush=True)	
  
  