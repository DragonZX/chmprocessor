
REM ------------------------------------------------
REM COPY NEEDED FILES TO DEBUG DIRECTORY (WIN)
REM ------------------------------------------------

MKDIR ChmProcessor\bin
MKDIR ChmProcessor\bin\x86
MKDIR ChmProcessor\bin\x86\Debug

COPY doc\chmProcessor.chm ChmProcessor\bin\x86\Debug
COPY license.txt ChmProcessor\bin\x86\Debug

REM **************************
REM COPY WEBFILES
REM **************************
MKDIR ChmProcessor\bin\x86\Debug\webFiles
XCOPY webFiles ChmProcessor\bin\x86\Debug\webFiles /E /Y

MKDIR ChmProcessor\bin\x86\Debug\webTranslations
COPY webTranslations ChmProcessor\bin\x86\Debug\webTranslations
copy ChmProcessor\dialog-information.png ChmProcessor\bin\x86\Debug
copy ChmProcessor\dialog-error.png ChmProcessor\bin\x86\Debug
copy searchdb.sql ChmProcessor\bin\x86\Debug
copy doc\web\chmProcessorDocumentation.pdf ChmProcessor\bin\x86\Debug
copy libraries\tidy.exe ChmProcessor\bin\x86\Debug

REM PREPARE SEARCH FILES:
MKDIR ChmProcessor\bin\x86\Debug\searchFiles
MKDIR ChmProcessor\bin\x86\Debug\searchFiles\Bin
COPY WebFullTextSearch\Bin ChmProcessor\bin\x86\Debug\searchFiles\Bin
COPY WebFullTextSearch\search.aspx ChmProcessor\bin\x86\Debug\searchFiles
COPY WebFullTextSearch\search.aspx.cs ChmProcessor\bin\x86\Debug\searchFiles
COPY WebFullTextSearch\Web.Config ChmProcessor\bin\x86\Debug\searchFiles


REM ------------------------------------------------
REM COPY NEEDED FILES TO DEBUG DIRECTORY (CMD)
REM ------------------------------------------------

MKDIR ChmProcessorCmd\bin
MKDIR ChmProcessorCmd\bin\Debug

REM **************************
REM COPY WEBFILES
REM **************************
MKDIR ChmProcessorCmd\bin\Debug\webFiles
XCOPY webFiles ChmProcessorCmd\bin\Debug\webFiles /E /Y

MKDIR ChmProcessorCmd\bin\Debug\webTranslations
COPY webTranslations ChmProcessorCmd\bin\Debug\webTranslations
copy searchdb.sql ChmProcessorCmd\bin\Debug
copy libraries\tidy.exe ChmProcessorCmd\bin\Debug

REM PREPARE SEARCH FILES:
MKDIR ChmProcessorCmd\bin\Debug\searchFiles
MKDIR ChmProcessorCmd\bin\Debug\searchFiles\Bin
COPY WebFullTextSearch\Bin ChmProcessorCmd\bin\Debug\searchFiles\Bin
COPY WebFullTextSearch\search.aspx ChmProcessorCmd\bin\Debug\searchFiles
COPY WebFullTextSearch\search.aspx.cs ChmProcessorCmd\bin\Debug\searchFiles
COPY WebFullTextSearch\Web.Config ChmProcessorCmd\bin\Debug\searchFiles

PAUSE
