@echo off
setlocal

rem Propose the version from comforttanVer in comforttan.py so the zip name matches the program
set "rel="
for /f tokens^=2^ delims^=^" %%v in ('findstr /b /c:"comforttanVer =" comforttan.py') do set "rel=%%v"
if not defined rel set /p "rel=Could not read comforttanVer from comforttan.py - enter release version: "

:confirm
echo.
echo Release version: %rel%
set "dest=.\releases\comforttan_r%rel%.zip"
if exist "%dest%" echo WARNING: %dest% already exists and will be overwritten
choice /c YN /m "Build %dest%"
if errorlevel 2 (
  set /p "rel=Enter release version: "
  goto confirm
)

echo Zipping OTA release files...

copy .\pollca_v7_proj\pollca_v7\bin\Release\net8.0\linux-arm\publish\pollca_v7 *
powershell -Command "Compress-Archive -Path 'cardterminal.py', 'cardterminalDisable.py', 'comforttan.py', 'ComWD.py', 'HW_IO.py', 'pollca', 'pollca_v7', 'sim_IO.py' -DestinationPath '%dest%' -Force"

echo Done!
pause
