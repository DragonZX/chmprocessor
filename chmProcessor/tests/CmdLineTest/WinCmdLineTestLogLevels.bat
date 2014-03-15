REM Test log levels for command line execution

..\..\ChmProcessorCmd\bin\Debug\ChmProcessorCmd.exe ..\StandardTest\test.WHC /g /y /e /l4 > logDebug.txt

..\..\ChmProcessorCmd\bin\Debug\ChmProcessorCmd.exe ..\StandardTest\test.WHC /g /y /e /l3 > logInfo.txt

..\..\ChmProcessorCmd\bin\Debug\ChmProcessorCmd.exe ..\StandardTest\test.WHC /g /y /e /l2 > logWarnings.txt

..\..\ChmProcessorCmd\bin\Debug\ChmProcessorCmd.exe ..\StandardTest\test.WHC /g /y /e /l1 > logErrors.txt
