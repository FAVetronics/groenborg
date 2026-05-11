# invoke for test: python3 cardterminal.py 5678 /dev/ttyUSB1

import serial
from time import sleep
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


PROGRAM_VERSION = '2.2'

TRANSACTION_SUCCEEDED = 0
TRANSACTION_FAILED = 1
TRANSACTION_TIMEOUT = 2
NO_REPLY_FROM_TERMINAL = 3
TERMINAL_BUSY = 4



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



AmountToRequest_centi = 1
#Command line arguments
if len(sys.argv) > 1:
	AmountToRequest_centi = int(sys.argv[1])

serialPort = HW_IO.getCardReaderPort()
try:
  ser = serial.Serial (serialPort, 57600, timeout=30)    #Open port
except:
  print('giving permissions to I2C...')
  os.system("sudo chmod 777 " + serialPort)
  ser = serial.Serial (serialPort, 57600, timeout=30)    #try again



# Read ini file
import json

with open(RPi_HOME_FOLDER+'settings.ini') as json_file:
	data = json.load(json_file)
	if 'id' in data: id = data['id'].strip()
	elif 'location' in data:  id = data['location'].strip()
	elif 'lokation' in data:  id = data['lokation'].strip()
	else:  id = "32"
	if 'host' in data: host = data['host'].strip()
	else: host = "https://comforttanpay.com/api/machine/"
	host = host.replace('api/machine/', '') # Remove 'api/machine/' (for backwards compatibility)
	if 'hosturlextension' in data: hostUrlExtension = data['hosturlextension'].strip()
	else: hostUrlExtension = "callback"
	if 'logerrors' in data: LogErrors = data['logerrors']
	else: LogErrors = False
CALLBACK_URL = host + 'api/machine/' + id + '/' + hostUrlExtension



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




def InitializePayterTerminal():
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
	elif (received_data[4] != 0x04): # not disabled
		Log ("Disable ")
		txdata = 0xCC, 0x02, 0x3C, 0x11, 0x1B
		received_data = NoData
		ser.write(txdata)
		ExpectedAnswerLenght = 6
		received_data = ser.read(ExpectedAnswerLenght)
#		Log ('Answer: ' + str(tuple(received_data)))
		if len(received_data) != ExpectedAnswerLenght: Log ("Timeout")
		elif (veryfi_checksum(received_data) == False): Log ('Checksum error')
		elif (received_data[3] != 0x00) and (received_data[4] != 0x00) : Log ("Failed")
#		else: Log ('OK\r\n')


	Log ("Get status")
	txdata = 0xCC, 0x02, 0x3C, 0x26
	txdata += calc_checksum(txdata),
	received_data = NoData
	ser.write(txdata)
	ExpectedAnswerLenght = 7
	received_data = ser.read(ExpectedAnswerLenght)
#	Log ('Answer: ' + str(tuple(received_data)))


	Log ("reset ")
	txdata = 0xCC, 0x02, 0x3C, 0x55, 0x5F
	received_data = NoData
	ser.write(txdata)
	ExpectedAnswerLenght = 6
	received_data = ser.read(ExpectedAnswerLenght)
#	Log ('Answer: ' + str(tuple(received_data)))
	if len(received_data) != ExpectedAnswerLenght: Log ("Timeout")
	elif (veryfi_checksum(received_data) == False): Log ('Checksum error')
	elif (received_data[3] != 0x00) or (received_data[4] != 0x00) : Log ("Failed")
#	else: Log ('OK\r\n')

	Log ("syncronize ")
	txdata = 0xCC, 0x02 , 0x3C , 0x24 , 0x2E
	received_data = NoData
	ser.write(txdata)
	ExpectedAnswerLenght = 5
	received_data = ser.read(ExpectedAnswerLenght)
#	Log ('Answer: ' + str(tuple(received_data)))
	if len(received_data) != ExpectedAnswerLenght: Log ("Timeout")
	elif (veryfi_checksum(received_data) == False): Log ('Checksum error')
	elif (received_data[3] != 0x24) : Log ("Failed")
#	else: Log ('OK\r\n')

	Log ("Select protocol ")
	txdata = 0xCC, 0x07, 0x3C, 0x13, 0x02, 0x00, 0x00, 0x00, 0x00, 0x24 # Protocol 2 - NO TXN update
	received_data = NoData
	ser.write(txdata)
	ExpectedAnswerLenght = 10
	received_data = ser.read(ExpectedAnswerLenght)
#	Log ('Answer: ' + str(tuple(received_data)))
	if len(received_data) != ExpectedAnswerLenght: Log ("Timeout")
	elif (veryfi_checksum(received_data) == False): Log ('Checksum error')
	elif (received_data[3] != 0x13) and (received_data[4] != 0x02) : Log ("Failed")
#	else: Log ('OK\r\n')

	Log ("Setup ")
	txdata = 0xCC, 0x04, 0x3C ,0x23 ,0x00 ,0x10, 0x3F
	received_data = NoData
	ser.write(txdata)
	ExpectedAnswerLenght = 6
	received_data = ser.read(ExpectedAnswerLenght)
#	Log ('Answer: ' + str(tuple(received_data)))
	if len(received_data) != ExpectedAnswerLenght: Log ("Timeout")
	elif (veryfi_checksum(received_data) == False): Log ('Checksum error')
	elif (received_data[3] != 0x00) and (received_data[4] != 0x00) : Log ("Failed")
#	else: Log ('OK\r\n')

	Log ("Enable ")
	txdata = 0xCC, 0x02, 0x3C, 0x12, 0x1C
	received_data = NoData
	ser.write(txdata)
	ExpectedAnswerLenght = 6
	received_data = ser.read(ExpectedAnswerLenght)
#	Log ('Answer: ' + str(tuple(received_data)))
	if len(received_data) != ExpectedAnswerLenght: Log ("Timeout")
	elif (veryfi_checksum(received_data) == False): Log ('Checksum error')
	elif (received_data[3] != 0x00) and (received_data[4] != 0x00) : Log ("Failed")
#	else: Log ('OK\r\n')


		
if __name__ == "__main__":
	Log ('PVP client - release '+PROGRAM_VERSION)
	Log ('Card reader connected to ' + serialPort)
	Log ('Callback URL: ' + CALLBACK_URL)
	InitializePayterTerminal()
	# get status to make sure that we are ready
	txdata = 0xCC, 0x02, 0x3C, 0x26, 0x30
	Log ("get status ")
	received_data = NoData
	ser.write(txdata)
	ExpectedAnswerLenght = 7
	received_data = ser.read(ExpectedAnswerLenght)
	if len(received_data) != ExpectedAnswerLenght: 
		Log ("get status timeout - Exiting")
		exit()
	elif received_data[4] != 0x08:
		if received_data[4] > 0x0F: Log ("TERMINAL_BUSY - Exiting")
		else: Log (str(received_data[4])+" - not ready - Exiting")
		exit()
		
	Log ('Request amount: ' + str(AmountToRequest_centi))
	Header = 0xCC, 0x08, 0x3C, 0x34
	ItemNo = 0x00, 0x00, 0x00, 0x01
	AmountHi = (math.floor(AmountToRequest_centi / 256)),
	AmountLo = (AmountToRequest_centi % 256),
	packet =  Header + AmountHi + AmountLo + ItemNo
	packet += calc_checksum(packet),
	ser.write(packet)

	Log ('Wait for Answer..')
	ExpectedAnswerLenght = 5
	received_data = ser.read(ExpectedAnswerLenght)
#	Log ('Answer: ' + str(tuple(received_data)))
	amountReceived = 0
	if len(received_data) != ExpectedAnswerLenght: 
		Log ("Request amount timeout")
		error = TRANSACTION_TIMEOUT
	elif (veryfi_checksum(received_data) == False): 
		Log ('Checksum error')
		error = TRANSACTION_FAILED
	elif (received_data[3] == 0x31): 
		Log ('Vend approved')
		error = TRANSACTION_SUCCEEDED
		amountReceived = AmountToRequest_centi
	elif (received_data[3] == 0x32): 
		Log ('Vend denied')
		error = TRANSACTION_FAILED
	elif (received_data[3] == 0x33): 
		Log ('Vend failed')
		error = TRANSACTION_FAILED
	else: 
		Log ('Unknown error')
		error = TRANSACTION_FAILED
	#command = {"method":"cardTerminal", "data":{"amountReceived" : amountReceived, "error" : error}}
	#Log (command)
	timeoutVal = 5
	for attempt in range(10):
		try:
			timeStamp = datetime.datetime.now().strftime("%Y:%m:%d %H:%M:%S.%f")
			rply = requests.post(CALLBACK_URL, json = {"method":"cardTerminal", "data":{"amountReceived" : amountReceived, "error" : error, "DateTime":timeStamp}}, timeout=timeoutVal)
			Log (rply.text)
		except:
			Log ('error posting CardTerminalResult')
		else:
			break # no exceptions - break out of attempts
	else: # all attempts failed
		Log ('all attempts to post CardTerminalError failed')

	if error == TRANSACTION_SUCCEEDED:
		Log ("Vend success")
		received_data = NoData
		txdata = 0xCC, 0x02, 0x3C, 0x35, 0x3F
	else:
		Log ("Vend has failed")
		received_data = NoData
		txdata = 0xCC, 0x02, 0x3C, 0x33, 0x3D
	ser.write(txdata)
	ExpectedAnswerLenght = 6
	received_data = ser.read(ExpectedAnswerLenght)
	if len(received_data) != ExpectedAnswerLenght: Log ("success / failed timeout") # should not occour
	
	Log ("session complete")
	received_data = NoData
	txdata = 0xCC, 0x02, 0x3C, 0x22, 0x2C
	ser.write(txdata)
	ExpectedAnswerLenght = 5
	received_data = ser.read(ExpectedAnswerLenght)
	#Log ('Answer: ' + str(tuple(received_data)))
	if len(received_data) != ExpectedAnswerLenght: Log ("session complete timeout") # should not occour

	Log ("Disable ")
	txdata = 0xCC, 0x02, 0x3C, 0x11, 0x1B
	received_data = NoData
	ser.write(txdata)
	ExpectedAnswerLenght = 6
	received_data = ser.read(ExpectedAnswerLenght)
	#Log ('Answer: ' + str(tuple(received_data)))
	if len(received_data) != ExpectedAnswerLenght: Log ("Timeout")
	elif (veryfi_checksum(received_data) == False): Log ('Checksum error')
	elif (received_data[3] != 0x00) and (received_data[4] != 0x00) : Log ("Failed")
#	else: Log ('OK\r\n')

	ser.close()

	Log("\r\n")
