
MKDIR ChmProcessor\bin
MKDIR ChmProcessor\bin\x86
MKDIR ChmProcessor\bin\x86\Release

COPY doc\chmProcessor.chm ChmProcessor\bin\x86\Release
COPY license.txt ChmProcessor\bin\x86\Release
MKDIR ChmProcessor\bin\x86\Release\webFiles
COPY webFiles ChmProcessor\bin\x86\Release\webFiles
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

PAUSE
