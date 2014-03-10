
REM ------------------------------------------------
REM COPY NEEDED FILES TO DEBUG DIRECTORY (WIN)
REM ------------------------------------------------

MKDIR ChmProcessor\bin
MKDIR ChmProcessor\bin\x86
MKDIR ChmProcessor\bin\x86\Release

COPY doc\chmProcessor.chm ChmProcessor\bin\x86\Release
COPY license.txt ChmProcessor\bin\x86\Release

REM **************************
REM COPY WEBFILES
REM **************************
MKDIR ChmProcessor\bin\x86\Release\webFiles
XCOPY webFiles ChmProcessor\bin\x86\Release\webFiles /E /Y


MKDIR ChmProcessor\bin\x86\Release\webTranslations
COPY webTranslations ChmProcessor\bin\x86\Release\webTranslations
copy ChmProcessor\dialog-information.png ChmProcessor\bin\x86\Release
copy ChmProcessor\dialog-error.png ChmProcessor\bin\x86\Release
copy searchdb.sql ChmProcessor\bin\x86\Release
copy doc\web\chmProcessorDocumentation.pdf ChmProcessor\bin\x86\Release
copy libraries\tidy.exe ChmProcessor\bin\x86\Release

REM PREPARE SEARCH FILES:
MKDIR ChmProcessor\bin\x86\Release\searchFiles
MKDIR ChmProcessor\bin\x86\Release\searchFiles\Bin
COPY WebFullTextSearch\Bin ChmProcessor\bin\x86\Release\searchFiles\Bin
COPY WebFullTextSearch\search.aspx ChmProcessor\bin\x86\Release\searchFiles
COPY WebFullTextSearch\search.aspx.cs ChmProcessor\bin\x86\Release\searchFiles
COPY WebFullTextSearch\Web.Config ChmProcessor\bin\x86\Release\searchFiles


REM ------------------------------------------------
REM COPY NEEDED FILES TO DEBUG DIRECTORY (CMD)
REM ------------------------------------------------


MKDIR ChmProcessorCmd\bin
MKDIR ChmProcessorCmd\bin\Release

REM **************************
REM COPY WEBFILES
REM **************************
MKDIR ChmProcessorCmd\bin\Release\webFiles
XCOPY webFiles ChmProcessorCmd\bin\Release\webFiles /E /Y

MKDIR ChmProcessorCmd\bin\Release\webTranslations
COPY webTranslations ChmProcessorCmd\bin\Release\webTranslations
copy searchdb.sql ChmProcessorCmd\bin\Release
copy libraries\tidy.exe ChmProcessorCmd\bin\Release

REM PREPARE SEARCH FILES:
MKDIR ChmProcessorCmd\bin\Release\searchFiles
MKDIR ChmProcessorCmd\bin\Release\searchFiles\Bin
COPY WebFullTextSearch\Bin ChmProcessorCmd\bin\Release\searchFiles\Bin
COPY WebFullTextSearch\search.aspx ChmProcessorCmd\bin\Release\searchFiles
COPY WebFullTextSearch\search.aspx.cs ChmProcessorCmd\bin\Release\searchFiles
COPY WebFullTextSearch\Web.Config ChmProcessorCmd\bin\Release\searchFiles

PAUSE
