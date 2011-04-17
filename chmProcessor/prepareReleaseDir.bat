COPY chmProcessor.chm ProcesadorHtml\bin\Release
COPY license.txt ProcesadorHtml\bin\Release
MKDIR ProcesadorHtml\bin\Release\webFiles
COPY webFiles ProcesadorHtml\bin\Release\webFiles
MKDIR ProcesadorHtml\bin\Release\webTranslations
COPY webTranslations ProcesadorHtml\bin\Release\webTranslations
copy ProcesadorHtml\dialog-information.png ProcesadorHtml\bin\Release
copy ProcesadorHtml\dialog-error.png ProcesadorHtml\bin\Release
copy searchdb.sql ProcesadorHtml\bin\Release
copy web\chmProcessorDocumentation.pdf ProcesadorHtml\bin\Release

REM PREPARE SEARCH FILES:
MKDIR ProcesadorHtml\bin\Release\searchFiles
MKDIR ProcesadorHtml\bin\Release\searchFiles\Bin
COPY WebFullTextSearch\Bin ProcesadorHtml\bin\Release\searchFiles\Bin
COPY WebFullTextSearch\search.aspx ProcesadorHtml\bin\Release\searchFiles
COPY WebFullTextSearch\search.aspx.cs ProcesadorHtml\bin\Release\searchFiles
COPY WebFullTextSearch\Web.Config ProcesadorHtml\bin\Release\searchFiles

PAUSE
