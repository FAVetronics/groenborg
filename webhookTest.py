import asyncio
import requests


WebHookBedTimeLeft = "http://192.168.1.151/decCnt="
timeLeftWebHooks = []

async def test():
    CabinCnt = 0
    while CabinCnt < 2:
        timeLeftWebHooks.append(CabinCnt+1)
        CabinCnt += 1
    for _cabinNo in timeLeftWebHooks:
        try:
            await asyncio.get_running_loop().run_in_executor(None, lambda n=_cabinNo: requests.get(WebHookBedTimeLeft + str(n), timeout=2))
        except Exception as e:
            print(f"WebHookBedTimeLeft failed: {e}")


if __name__ == "__main__":
    asyncio.run(test())