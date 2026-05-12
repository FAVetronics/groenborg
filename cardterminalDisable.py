import serial
import math
import requests
import sys
import datetime
import os

try:
  import HW_IO
  RPi_HOME_FOLDER = "/home/comforttan/"
  SIMULATION = False
except:
  print('cannot simulate card reader yet')
  RPi_HOME_FOLDER = "./"
  SIMULATION = True


PROGRAM_VERSION = '1.0'


NoData = 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF



def Log(txt):
	print(str(datetime.datetime.now()) + '\t', txt, flush=True)
	timeStamp = datetime.datetime.now().strftime("%Y%m%d")
	logfilename = RPi_HOME_FOLDER+'CardLogs/cardLog'+timeStamp+'.log'
	try:
		if os.path.isfile(logfilename) : logfile = open(logfilename, 'a') # (a)ppend
		else : logfile = open(logfilename, 'w') # (w)rite
	except:
		os.system("sudo chmod 777 "+logfilename)
		if os.path.isfile(logfilename) : logfile = open(logfilename, 'a') # (a)ppend
		else : logfile = open(logfilename, 'w') # (w)rite
	logfile.write(str(datetime.datetime.now()) + '\t' + txt + "\r\n")
	logfile.close()



serialPort = HW_IO.getCardReaderPort()
if serialPort == "none":
  Log('No card terminal connected (CardReaderPort=none) - exiting cardterminalDisable.py')
  sys.exit(0)
try:
  ser = serial.Serial (serialPort, 57600, timeout=30)    #Open port
except:
  Log('Could not open card terminal serial port - retrying after chmod')
  os.system("sudo chmod 777 " + serialPort)
  ser = serial.Serial (serialPort, 57600, timeout=30)    #try again



def calc_checksum(s):
    sum = 0
    for c in s:
        sum += c
    #sum = sum - s[len(s)-1] # Remove last byte
    sum = sum % 256
    #return '%2X' % (sum & 0xFF) don't know what this does
    return sum

def veryfi_checksum(s):
    sum = 0
    for c in s:
        sum += c
    sum = sum - s[len(s)-1] # Remove last byte
    sum = sum % 256
    chk = s[len(s)-1] # isolate last byte
    if sum == chk : return True
    else: return False



		
if __name__ == "__main__":
	Log ('Disable card terminal - release '+PROGRAM_VERSION)
	Log ('Card reader connected to ' + serialPort)

	Log ("Get status")
	txdata = 0xCC, 0x02, 0x3C, 0x26
	txdata += calc_checksum(txdata),
	received_data = NoData
	ser.write(txdata)
	ExpectedAnswerLenght = 7
	received_data = ser.read(ExpectedAnswerLenght)
#	Log ('Answer: ' + str(tuple(received_data)))
	if len(received_data) != ExpectedAnswerLenght: Log ("Timeout")
	elif (veryfi_checksum(received_data) == False): Log ('Checksum error')
	elif (received_data[5] != 0x00): # there's a session active
		Log ("Session complete")
		txdata = 0xCC, 0x02, 0x3C, 0x22, 0x2C
		received_data = NoData
		ser.write(txdata)
		ExpectedAnswerLenght = 5
		received_data = ser.read(ExpectedAnswerLenght)
#		Log ('Answer: ' + str(tuple(received_data)))
		if len(received_data) != ExpectedAnswerLenght: Log ("Timeout")
		elif (veryfi_checksum(received_data) == False): Log ('Checksum error')
		elif (received_data[3] != 0x25) : Log ("Failed")
#		else: Log ('OK\r\n')

	Log ("Disable ")
	txdata = 0xCC, 0x02, 0x3C, 0x11, 0x1B
	received_data = NoData
	ser.write(txdata)
	ExpectedAnswerLenght = 6
	received_data = ser.read(ExpectedAnswerLenght)
#	#Log ('Answer: ' + str(tuple(received_data)))
	if len(received_data) != ExpectedAnswerLenght: Log ("Timeout")
	elif (veryfi_checksum(received_data) == False): Log ('Checksum error')
	elif (received_data[3] != 0x00) and (received_data[4] != 0x00) : Log ("Failed")
#	else: Log ('OK\r\n')

	Log ("reset ")
	txdata = 0xCC, 0x02, 0x3C, 0x55, 0x5F
	received_data = NoData
	ser.write(txdata)
	ExpectedAnswerLenght = 6
	received_data = ser.read(ExpectedAnswerLenght)
	#Log ('Answer: ' + str(tuple(received_data)))
	if len(received_data) != ExpectedAnswerLenght: Log ("Timeout")
	elif (veryfi_checksum(received_data) == False): Log ('Checksum error')
	elif (received_data[3] != 0x00) or (received_data[4] != 0x00) : Log ("Failed")
#	else: Log ('OK\r\n')

	ser.close()

	Log("\r\n")
