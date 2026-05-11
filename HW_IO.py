import serial.tools.list_ports # Scan USB ports
import os


# Scan USB ports
CoinAcceptorPort = "none"
CardReaderPort = "none"

def getCardReaderPort():
  global CardReaderPort
  ports = list(serial.tools.list_ports.comports())
  for p in ports:
      print (p)
      if ("USB" in p.device) and not ("EMP" in p.description): 
          CardReaderPort = p.device
  print ('CardReaderPort: ' + CardReaderPort)
  return CardReaderPort


def getCoinAcceptorPort():
  global CoinAcceptorPort
  ports = list(serial.tools.list_ports.comports())
  for p in ports:
      print (p)
      if "EMP" in p.description:
          CoinAcceptorPort = p.device
  print ('CoinAcceptorPort: ' + CoinAcceptorPort)
  # set permissions
  os.system("sudo chmod 777 " + CoinAcceptorPort)
  return CoinAcceptorPort

