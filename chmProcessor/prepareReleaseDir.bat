
MKDIR ChmProcessor\bin
MKDIR ChmProcessor\bin\Release

COPY doc\chmProcessor.chm ChmProcessor\bin\Release
COPY license.txt ChmProcessor\bin\Release
MKDIR ChmProcessor\bin\Release\webFiles
COPY webFiles ChmProcessor\bin\Release\webFiles
MKDIR ChmProcessor\bin\Release\webTranslations
COPY webTranslations ChmProcessor\bin\Release\webTranslations
copy ChmProcessor\dialog-information.png ChmProcessor\bin\Release
copy ChmProcessor\dialog-error.png ChmProcessor\bin\Release
copy searchdb.sql ChmProcessor\bin\Release
copy doc\web\chmProcessorDocumentation.pdf ChmProcessor\bin\Release
copy libraries\tidy.exe ChmProcessor\bin\Release

REM PREPARE SEARCH FILES:
MKDIR ChmProcessor\bin\Release\searchFiles
MKDIR ChmProcessor\bin\Release\searchFiles\Bin
COPY WebFullTextSearch\Bin ChmProcessor\bin\Release\searchFiles\Bin
COPY WebFullTextSearch\search.aspx ChmProcessor\bin\Release\searchFiles
COPY WebFullTextSearch\search.aspx.cs ChmProcessor\bin\Release\searchFiles
COPY WebFullTextSearch\Web.Config ChmProcessor\bin\Release\searchFiles

PAUSE
