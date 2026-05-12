@echo off

set "rel=2.23"

set "dest=C:\Path\To\Archive\comforttan_r'%rel%'.zip"

echo Zipping specific files...

powershell -Command "Compress-Archive -Path 'cardterminal.py', 'cardterminalDisable.py', 'comforttan.py', 'ComWD.py', 'HW_IO.py', 'pollca', 'pollca_v7', 'sim_IO.py' -DestinationPath '%dest%' -Force"

echo Done!
pause