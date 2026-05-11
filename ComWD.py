import subprocess
import os
import traceback
import datetime
import asyncio	



async def main():	
  WDlogfilename = "Logs/ComWD.log"
  TEST_INTERVAL_s = 55
  TIMEOUT_min = 5
  batcmd = "ls -l --time-style=full-iso /boot/rc_local.log"
  
  misMatchCnt = 0
  sleepTime = 1
  while True:
    try:
      fileData = subprocess.check_output(batcmd, shell=True)
      now = datetime.datetime.now()
      now_h = now.strftime("%H")
      now_m = now.strftime("%M")
      # check if the files timestamp matches the current time
      searchStringNow = now.strftime("%H:%M") #in '2023-11-02 09:40:28.00000' search for '09:40'
      #searchStringNow = '15:25' TEST ONLY
      if int(now_m) == 0: prevMinute = '00'
      elif int(now_m) <= 10: prevMinute = '0' + str(int(now_m)-1)
      else: prevMinute = str(int(now_m)-1)
      #else: prevMinute = str(int(now_m)-2)#1) TEST ONLY
      searchStringOneMinAgo = now_h + ':' + prevMinute
      if searchStringNow not in str(fileData) and searchStringOneMinAgo not in str(fileData): 
        #print('rc_local.log timestamp not updated') Don't print - it'll update the timestamp of the logfile...
        #print(now.strftime("%H:%M:%S") + ' (' + searchStringOneMinAgo + ')')
        #print(fileData)
        if sleepTime > 1: # Don't count up until we've seen a matching timestamp (avoids contineous reboot if file isn't updated at all
          misMatchCnt = misMatchCnt + 1
        #print('No of consecutive mis-matches: '+str(misMatchCnt), flush=True)
        if misMatchCnt >= (TIMEOUT_min * 60) / TEST_INTERVAL_s:
          # Create a logfile, so that we know what has happened
          if os.path.isfile(WDlogfilename) : WDlogfile = open(WDlogfilename, 'a') # (a)ppend (w)rite
          else : WDlogfile = open(WDlogfilename, 'w') # (a)ppend (w)rite
          LogtimeStamp = now.strftime("%H:%M:%S")
          WDlogfile.write(LogtimeStamp + '\trc_local.log has not been updated for 5 minutes - Rebooting\r\n')
          WDlogfile.close
          print(fileData)
          print('rc_local.log has not been updated for 5 minutes - Rebooting', flush=True)
          # How do we make sure we dont turn off an active cabin?
          os.system("sudo reboot")
      else:
        misMatchCnt = 0
        sleepTime = TEST_INTERVAL_s
    except:
      traceback.print_exc()
    await asyncio.sleep(sleepTime)


if __name__ == "__main__":
  exit = 0
  while exit == 0:
    try:
        print('Starting communication WD...')
        asyncio.run(main())	
    except KeyboardInterrupt:	
        # Exit application because user indicated they wish to exit.	
        # This will have cancelled `main()` implicitly.	
        print("")
        print("User initiated exit. Exiting.", flush=True)	
        exit = 1
    except:
        print('Unhandled exception - Rebooting', flush=True) # Should not happen
        os.system("sudo reboot")
    finally:	
        print("Leaving program...", flush=True)	
