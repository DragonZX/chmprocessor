COPY chmProcessor.chm ProcesadorHtml\bin\Debug
COPY license.txt ProcesadorHtml\bin\Debug
MKDIR ProcesadorHtml\bin\Debug\webFiles
COPY webFiles ProcesadorHtml\bin\Debug\webFiles
MKDIR ProcesadorHtml\bin\Debug\webTranslations
COPY webTranslations ProcesadorHtml\bin\Debug\webTranslations
copy ProcesadorHtml\dialog-information.png ProcesadorHtml\bin\Debug
copy ProcesadorHtml\dialog-error.png ProcesadorHtml\bin\Debug
copy searchdb.sql ProcesadorHtml\bin\Debug
copy web\chmProcessorDocumentation.pdf ProcesadorHtml\bin\Debug

REM PREPARE SEARCH FILES:
MKDIR ProcesadorHtml\bin\Debug\searchFiles
MKDIR ProcesadorHtml\bin\Debug\searchFiles\Bin
COPY WebFullTextSearch\Bin ProcesadorHtml\bin\Debug\searchFiles\Bin
COPY WebFullTextSearch\search.aspx ProcesadorHtml\bin\Debug\searchFiles
COPY WebFullTextSearch\search.aspx.cs ProcesadorHtml\bin\Debug\searchFiles
COPY WebFullTextSearch\Web.Config ProcesadorHtml\bin\Debug\searchFiles

PAUSE
