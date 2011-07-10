
MKDIR ChmProcessor\bin
MKDIR ChmProcessor\bin\Debug

COPY doc\chmProcessor.chm ChmProcessor\bin\Debug
COPY license.txt ChmProcessor\bin\Debug
MKDIR ChmProcessor\bin\Debug\webFiles
COPY webFiles ChmProcessor\bin\Debug\webFiles
MKDIR ChmProcessor\bin\Debug\webTranslations
COPY webTranslations ChmProcessor\bin\Debug\webTranslations
copy ChmProcessor\dialog-information.png ChmProcessor\bin\Debug
copy ChmProcessor\dialog-error.png ChmProcessor\bin\Debug
copy searchdb.sql ChmProcessor\bin\Debug
copy doc\web\chmProcessorDocumentation.pdf ChmProcessor\bin\Debug
copy libraries\tidy.exe ChmProcessor\bin\Debug

REM PREPARE SEARCH FILES:
MKDIR ChmProcessor\bin\Debug\searchFiles
MKDIR ChmProcessor\bin\Debug\searchFiles\Bin
COPY WebFullTextSearch\Bin ChmProcessor\bin\Debug\searchFiles\Bin
COPY WebFullTextSearch\search.aspx ChmProcessor\bin\Debug\searchFiles
COPY WebFullTextSearch\search.aspx.cs ChmProcessor\bin\Debug\searchFiles
COPY WebFullTextSearch\Web.Config ChmProcessor\bin\Debug\searchFiles

PAUSE
