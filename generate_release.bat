@echo off

set "rel=2.23"

set "dest=.\releases\comforttan_r%rel%.zip"

echo Zipping OTA release files...

copy .\pollca_v7_proj\pollca_v7\bin\Release\net8.0\linux-arm\publish\pollca_v7 *
powershell -Command "Compress-Archive -Path '.env', 'cardterminal.py', 'cardterminalDisable.py', 'comforttan.py', 'ComWD.py', 'HW_IO.py', 'pollca', 'pollca_v7', 'sim_IO.py' -DestinationPath '%dest%' -Force"

echo Done!
pause