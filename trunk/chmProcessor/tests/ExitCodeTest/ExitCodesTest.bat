@echo off

REM --------- Test exit codes

REM --------- Command line exe

..\..\ChmProcessorCmd\bin\Debug\ChmProcessorcmd.exe ..\Test1.doc /g /y /e > OKLog.txt
if %ERRORLEVEL% EQU 0 echo OK file returned 0

..\..\ChmProcessorCmd\bin\Debug\ChmProcessorcmd.exe WarningProject.WHC /g /y /e > WarningLog.txt
if %ERRORLEVEL% EQU 1 echo Warning file returned 1

..\..\ChmProcessorCmd\bin\Debug\ChmProcessorcmd.exe WrongProject.WHC /g /y /e > ErrorLog.txt
if %ERRORLEVEL% EQU 2 echo Error file returned 2

REM --------- Win exe

..\..\ChmProcessor\bin\x86\Debug\ChmProcessor.exe ..\Test1.doc /g /y /e > OKLog.txt
if %ERRORLEVEL% EQU 0 echo OK file returned 0

..\..\ChmProcessor\bin\x86\Debug\ChmProcessor.exe WarningProject.WHC /g /y /e > WarningLog.txt
if %ERRORLEVEL% EQU 1 echo Warning file returned 1

..\..\ChmProcessor\bin\x86\Debug\ChmProcessor.exe WrongProject.WHC /g /y /e > ErrorLog.txt
if %ERRORLEVEL% EQU 2 echo Error file returned 2


PAUSE
