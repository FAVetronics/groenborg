from tkinter import ttk
import tkinter as tk
from tkinter.messagebox import showinfo

# root window
root = tk.Tk()
#root.geometry('480x720')
root.title('Comforttan simulator')


######### settings.ini ##########

Location = tk.StringVar()
lblLocation = ttk.Label(root, text="Location ID: ")
lblLocation.grid(column=0, row=0, columnspan=1, padx=1, pady=1, sticky="W")
txtLocation = ttk.Entry(root, textvariable=Location)
txtLocation.grid(column=1, row=0, columnspan=1, padx=1, pady=1, sticky="W")

callbackURL = tk.StringVar()
lblcallbackURL = ttk.Label(root, text="Callback URL: ")
lblcallbackURL.grid(column=0, row=1, columnspan=1, padx=1, pady=1, sticky="W")
txtCallbackURL = ttk.Entry(root, textvariable=callbackURL)
txtCallbackURL.grid(column=1, row=1, columnspan=2, padx=1, pady=1, sticky="W")

connectinString = tk.StringVar()
lblConnectinString = ttk.Label(root, text="Connection String: ")
lblConnectinString.grid(column=0, row=2, columnspan=1, padx=1, pady=1, sticky="W")
txtconnectinString = ttk.Entry(root, textvariable=connectinString)
txtconnectinString.grid(column=1, row=2, columnspan=3, padx=1, pady=1, sticky="W")


btnAppStartPressed = False
def startApplication():
  global btnAppStartPressed
  btnAppStart['text'] = "Application started"
  btnAppStart.state(['disabled'])
  btnAppStartPressed = True
  
btnAppStart = ttk.Button(root, text='Start application',command=startApplication) 
btnAppStart.grid(column=0, row=3, padx=1, pady=1, sticky=tk.E)


def setLocation(locationID):
  Location.set(format (locationID))
def setCallbackURL(CALLBACK_URL):
  callbackURL.set(CALLBACK_URL)
def setConnectionString(CONNECTION_STRING):
  connectinString.set(CONNECTION_STRING)
def getLocation():
  return Location.get()
def getCallbackURL():
  return callbackURL.get()
def getConnectionString():
  return connectinString.get()

    


########## Card terminal ##########
########## Coin acceptor ##########


########## Door ##########

lblDoor = ttk.Label(root, text='Door state')
lblDoor.grid(column=0, row=4, columnspan=1, padx=1, pady=10, sticky="W")

def door(doorTimeLeft, _doorTime_s):
  if doorTimeLeft > _doorTime_s: lblDoor['text'] = "Door is opened manually"
  elif doorTimeLeft == 0: lblDoor['text'] = "Door is closed"
  else: 
    lblDoor['text'] = 'Door is opend for '+format(doorTimeLeft)+' sec.' 



########## Cabins ##########

NO_OFF_CABINS = 10

userButton = [False for x in range(NO_OFF_CABINS)]

def startCabin(cabinNo):
  global userButton
  print('User start on cabin '+format(cabinNo+1))
  userButton[cabinNo] = True

lblCabin = [ttk.Label(root) for x in range(NO_OFF_CABINS)]
start_button = [ttk.Button(root, text='Start now',command=lambda c = x: startCabin(c)) for x in range(NO_OFF_CABINS)]
for n in range(NO_OFF_CABINS):
  lblCabin[n]['text'] = 'Cabin '+format(n+1)+': Off'
  lblCabin[n].grid(column=0, row=n+5, columnspan=1, padx=1, pady=2, sticky="W")
  start_button[n].grid(column=5, row=n+5, padx=1, pady=1, sticky=tk.E)
  start_button[n].state(['disabled'])
  userButton[n] = False
  
def cabin(cabinNo, active, timeLeft, timeToStart):
  if not active: 
    lblCabin[cabinNo]['text'] = 'Cabin '+format(cabinNo+1)+': Off'
    start_button[cabinNo].state(['disabled'])
  elif timeToStart > 0: 
    lblCabin[cabinNo]['text'] = 'Cabin '+format(cabinNo+1)+' starting in '+format(timeToStart)+' seconds'
    start_button[cabinNo].state(['!disabled'])
  else: 
    lblCabin[cabinNo]['text'] = 'Cabin '+format(cabinNo+1)+' active for '+format(timeLeft)+' minutes'
    start_button[cabinNo].state(['disabled'])
    userButton[cabinNo] = False



########## Vending ##########

NO_OFF_SHELFES = 10

lblShelf = [ttk.Label(root) for x in range(NO_OFF_SHELFES)]
for n in range(NO_OFF_SHELFES):
  lblShelf[n]['text'] = 'Shelf '+format(n+1)+': inactive'
  lblShelf[n].grid(column=0, row=n+17, columnspan=1, padx=1, pady=2, sticky="W")
  
def vend(shelfNo, timeLeft):
  if timeLeft > 0: 
    lblShelf[shelfNo]['text'] = 'Shelf '+format(shelfNo+1)+' running for '+format(timeLeft)+' seconds'
  else: 
    lblShelf[shelfNo]['text'] = 'Shelf '+format(shelfNo+1)+' inactive'


########## Settings ##########
########## Configuration ##########

#


def updateGUI():
    root.update_idletasks()
    root.update()
    