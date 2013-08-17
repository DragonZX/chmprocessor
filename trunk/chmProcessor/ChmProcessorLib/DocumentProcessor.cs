/* 
 * chmProcessor - Word converter to CHM
 * Copyright (C) 2008 Toni Bennasar Obrador
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using mshtml;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Management;
using System.Web;
using System.Runtime.InteropServices;
using System.Globalization;
using WebIndexLib;
using ChmProcessorLib.DocumentStructure;
using ChmProcessorLib.Generators;

namespace ChmProcessorLib
{

	/// <summary>
    /// Class to handle manipulations of a HTML / Word file to generate the help
    /// TODO: Calls to UserInterface.log outside this class (on Generators classes) does not store
    /// TODO: the generated exceptions on the "GenerationExceptions" member. They must to do it.
	/// </summary>
    public class DocumentProcessor
    {

        /// <summary>
        /// Main source file to convert to help.
        /// If there was multiple documents to convert to help, they are joined on this.
        /// </summary>
        private string MainSourceFile;

        /// <summary>
        /// Documento HTML cargado:
        /// </summary>
        private IHTMLDocument2 iDoc;

        /// <summary>
        /// HTML document structure
        /// </summary>
        private ChmDocument Document;

        /// <summary>
        /// Decorator for pages for the generated CHM 
        /// </summary>
        private HtmlPageDecorator ChmDecorator = new HtmlPageDecorator();

        /// <summary>
        /// Decorator for pages for the generated web site and the JavaHelp file.
        /// </summary>
        private HtmlPageDecorator WebDecorator = new HtmlPageDecorator();

        /// <summary>
        /// Lista de archivos y directorios adicionales a añadir al proyecto de ayuda
        /// </summary>
        private List<string> AdditionalFiles;

        /// <summary>
        /// Indica si el archivo a procesar es un documento word o uno html
        /// </summary>
        private bool IsMSWord;

        /// <summary>
        /// Si IsMSWord = true, indica el directorio temporal donde se genero el 
        /// html del documento word
        /// </summary>
        private string MSWordHtmlDirectory;

        /// <summary>
        /// Project to generate the help. 
        /// </summary>
        public ChmProject Project;

        /// <summary>
        /// List of exceptions catched on the generation process.
        /// </summary>
        public List<Exception> GenerationExceptions = new List<Exception>();

        /// <summary>
        /// Timer to avoid html loading hang ups
        /// </summary>
        private System.Windows.Forms.Timer timerTimeout;

        /// <summary>
        /// Handler of the user interface of the generation process.
        /// </summary>
        public UserInterface UI;

        /// <summary>
        /// Encoding to write the help workshop project files.
        /// </summary>
        private Encoding helpWorkshopEncoding;

        /// <summary>
        /// Culture to put into the help workshop project file.
        /// </summary>
        private CultureInfo helpWorkshopCulture;

        /// <summary>
        /// Should we replace / remove broken links?
        /// It gets its value from <see cref="AppSettings.ReplaceBrokenLinks"/>
        /// </summary>
        private bool replaceBrokenLinks;

        private void log(string texto, int logLevel) 
        {
            if (UI != null)
                UI.log(texto, logLevel);
        }

        /// <summary>
        /// Stores an exception into the log.
        /// </summary>
        /// <param name="exception">Exception to log</param>
        private void log(Exception exception)
        {
            GenerationExceptions.Add(exception);
            if (UI != null)
                UI.log(exception);
        }

        private bool CancellRequested()
        {
            if (UI != null)
                return UI.CancellRequested();
            else
                return false;
        }

        [ComVisible(true), ComImport(),
        Guid("7FD52380-4E07-101B-AE2D-08002B2EC713"),
        InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPersistStreamInit
        {
            void GetClassID([In, Out] ref Guid pClassID);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int IsDirty();
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int Load([In] UCOMIStream pstm);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int Save([In] UCOMIStream pstm, [In,
                MarshalAs(UnmanagedType.Bool)] bool fClearDirty);
            void GetSizeMax([Out] long pcbSize);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int InitNew();
        }

        /// <summary>
        /// Timer to avoid load html file hang up
        /// </summary>
        private void timer_Tick(object sender, System.EventArgs e)
        {
            timerTimeout.Enabled = false;
        }

        /// <summary>
        /// Checks if any source document is open, and join the multiple source Word documents to 
        /// a single file.
        /// </summary>
        /// <param name="msWord">Word instace that will do this work.</param>
        /// <returns>Path to a single Word file containing all source documents. If there is a 
        /// single source file, it will return that file path.</returns>
        private string CheckAndJoinWordSourceFiles(MSWord msWord)
        {

            // Check any source file is open:
            foreach (string sourceFile in Project.SourceFiles)
            {
                if (msWord.IsOpen(sourceFile))
                    throw new Exception("The file " + sourceFile + " is already open. You must to close it before generate the help.");
            }

            if (Project.SourceFiles.Count == 1)
                return (string) Project.SourceFiles[0];

            // Join multiple documents to a temporal file:
            string joinedDocument = Path.GetTempFileName();
            // Add DOC extension:
            joinedDocument += ".doc";

            log("Joining documents to a single temporal file : " + joinedDocument, ConsoleUserInterface.INFO);
            msWord.JoinDocuments(Project.SourceFiles.ToArray(), joinedDocument);
            return joinedDocument;
        }

        /// <summary>
        /// Open source files, if they are MS Word documents.
        /// It joins and store them to a HTML single file.
        /// </summary>
        /// <param name="msWord">Word instace that will do this work.</param>
        /// <returns>The HTML joined version of the MS Word documents</returns>
        private string ConvertWordSourceFiles(MSWord msWord)
        {

            MainSourceFile = CheckAndJoinWordSourceFiles(msWord);

            if (CancellRequested())
                return null;

            log("Convert file " + MainSourceFile + " to HTML", ConsoleUserInterface.INFO);
            string nombreArchivo = Path.GetFileNameWithoutExtension(MainSourceFile);
            MSWordHtmlDirectory = Path.GetTempPath() + Path.DirectorySeparatorChar + nombreArchivo;
            if (Directory.Exists(MSWordHtmlDirectory))
                Directory.Delete(MSWordHtmlDirectory, true);
            else if (File.Exists(MSWordHtmlDirectory))
                File.Delete(MSWordHtmlDirectory);
            Directory.CreateDirectory(MSWordHtmlDirectory);

            // Rename the file to a save name. If there is spaces, for example, 
            // links to embedded images into the document are not found.
            //string finalFile = dirHtml + Path.DirectorySeparatorChar + nombreArchivo + ".htm";
            string finalFile = MSWordHtmlDirectory + Path.DirectorySeparatorChar + ChmDocumentNode.ToSafeFilename(nombreArchivo) + ".htm";

            msWord.SaveWordToHtml(MainSourceFile, finalFile);
            return finalFile;
        }

        /// <summary>
        /// Open source files.
        /// If they are Word, they will be converted to HTML.
        /// </summary>
        private void OpenSourceFiles() 
        {
            MSWord msWord = null;

            try
            {
                string archivoFinal = (string)Project.SourceFiles[0];
                IsMSWord = MSWord.ItIsWordDocument(archivoFinal);
                MSWordHtmlDirectory = null;
                // Si es un documento word, convertirlo a HTML filtrado
                if (IsMSWord)
                {
                    msWord = new MSWord();
                    archivoFinal = ConvertWordSourceFiles(msWord);

                    // Be sure we have closed word, to avoid overlapping between the html read
                    // and the reading from chmprocessor:
                    msWord.Dispose();
                    msWord = null;
                }
                else
                    // There is a single source HTML file.
                    MainSourceFile = (string)Project.SourceFiles[0];

                if (CancellRequested())
                    return;

                // TODO: Check if this should be removed.
                if (AppSettings.UseTidyOverInput)
                    new TidyParser(UI).Parse(archivoFinal);

                if (CancellRequested())
                    return;

                // Prepare loading:
                HTMLDocumentClass docClass = new HTMLDocumentClass();
                IPersistStreamInit ips = (IPersistStreamInit)docClass;
                ips.InitNew();

                // Create a timer, to be sure that HTML file load will not be hang up (Sometime happens)
                timerTimeout = new System.Windows.Forms.Timer();
                timerTimeout.Tick += new System.EventHandler(this.timer_Tick);
                timerTimeout.Interval = 60 * 1000;     // 1 minute
                timerTimeout.Enabled = true;

                // Load the file:
                IHTMLDocument2 docLoader = (mshtml.IHTMLDocument2)docClass.createDocumentFromUrl( archivoFinal , null);
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(1000);

                String currentStatus = docLoader.readyState;
                log("Reading file " + archivoFinal + ". Status: " + currentStatus, ConsoleUserInterface.INFO);
                while (currentStatus != "complete" && timerTimeout.Enabled)
                {
                    System.Windows.Forms.Application.DoEvents();
                    System.Threading.Thread.Sleep(500);
                    String newStatus = docLoader.readyState;
                    if (newStatus != currentStatus)
                    {
                        log("Status: " + newStatus, ConsoleUserInterface.INFO );
                        if (currentStatus == "interactive" && newStatus == "uninitialized")
                        {
                            // fucking shit bug. Try to reload the file:
                            log("Warning. Something wrong happens loading the file. Trying to reopen " + archivoFinal, ConsoleUserInterface.INFO);
                            docClass = new HTMLDocumentClass();
                            ips = (IPersistStreamInit)docClass;
                            ips.InitNew();
                            docLoader = (mshtml.IHTMLDocument2)docClass.createDocumentFromUrl(archivoFinal, null);
                            newStatus = docLoader.readyState;
                            log("Status: " + newStatus, ConsoleUserInterface.INFO);
                        }
                        currentStatus = newStatus;
                    }
                }
                if (!timerTimeout.Enabled)
                    log("Warning: time to load file expired.", ConsoleUserInterface.ERRORWARNING);
                timerTimeout.Enabled = false;

                // Get a copy of the document:
                // TODO: Check why is needed a copy... We cannot work with the original loaded file?
                HTMLDocumentClass newDocClass = new HTMLDocumentClass();
                iDoc = (IHTMLDocument2)newDocClass;
                object[] txtHtml = { ((IHTMLDocument3)docLoader).documentElement.outerHTML };
                iDoc.writeln(txtHtml);
                try
                {
                    // Needed, otherwise some characters will not be displayed well.
                    iDoc.charset = docLoader.charset;
                }
                catch (Exception ex)
                {
                    log("Warning: Cannot set the charset \"" + docLoader.charset + "\" to the html document. Reason:" + ex.Message, ConsoleUserInterface.ERRORWARNING);
                    log(ex);
                }
            }
            finally
            {
                if (msWord != null)
                {
                    msWord.Dispose();
                    msWord = null;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">Data about the document to convert to help.</param>
        public DocumentProcessor( ChmProject project )
        {
            this.Project = project;
            this.AdditionalFiles = new List<string>(project.ArchivosAdicionales);
            this.replaceBrokenLinks = AppSettings.ReplaceBrokenLinks;
            try
            {
                this.helpWorkshopCulture = CultureInfo.GetCultureInfo(project.ChmLocaleID);
            }
            catch (Exception ex)
            {
                log(ex);
                throw new Exception("The locale ID (LCID) " + project.ChmLocaleID + " is not found.", ex);
            }

            try
            {
                this.helpWorkshopEncoding = Encoding.GetEncoding(helpWorkshopCulture.TextInfo.ANSICodePage);
            }
            catch (Exception ex)
            {
                log(ex);
                throw new Exception("The ANSI codepage " + helpWorkshopCulture.TextInfo.ANSICodePage + " is not found.", ex);
            }
        }

        private void ExecuteProjectCommandLine()
        {
            try
            {
                log("Executing '" + Project.CommandLine.Trim() + "'", ConsoleUserInterface.INFO);
                // TODO: Do a call to ExecuteCommandLine to run this execution.
                string strCmdLine = "/C " + Project.CommandLine.Trim();
                ProcessStartInfo si = new System.Diagnostics.ProcessStartInfo("CMD.exe", strCmdLine);
                si.CreateNoWindow = false;
                si.UseShellExecute = false;
                si.RedirectStandardOutput = true;
                si.RedirectStandardError = true;
                Process p = new Process();
                p.StartInfo = si;
                p.Start();
                string output = p.StandardOutput.ReadToEnd();
                string error = p.StandardError.ReadToEnd();
                p.WaitForExit();
                log(output, ConsoleUserInterface.INFO);
                log(error, ConsoleUserInterface.ERRORWARNING);
            }
            catch (Exception ex)
            {
                log("Error executing command line ", ConsoleUserInterface.ERRORWARNING);
                log(ex);
            }
        }

        /// <summary>
        /// Generates the CHM project, and compile it optionally
        /// </summary>
        private void GenerateChm()
        {
            try
            {
                // TODO: If the chm has been compiled, remove the project directory from the temporal directory.
                ChmGenerator chmGenerator = new ChmGenerator(Document, UI, Project, AdditionalFiles, ChmDecorator);
                chmGenerator.Generate();
            }
            catch (Exception ex)
            {
                UI.log(ex);
            }
        }

        private void GeneratePdf()
        {
            try
            {
                PdfGenerator pdfGenerator = new PdfGenerator(MainSourceFile, UI, Project);
                pdfGenerator.Generate();
            }
            catch (Exception ex)
            {
                log(ex);
            }
        }

        private void GenerateXps()
        {
            try
            {
                XpsGenerator xpsGenerator = new XpsGenerator(MainSourceFile, UI, Project);
                xpsGenerator.Generate();
            }
            catch (Exception ex)
            {
                log(ex);
            }
        }

        /// <summary>
        /// Generated the help web site 
        /// </summary>
        private void GenerateWebSite()
        {
            try
            {
                WebHelpGenerator webHelpGenerator = new WebHelpGenerator(Document, UI, Project, WebDecorator);
                webHelpGenerator.Generate(AdditionalFiles);
            }
            catch (Exception ex)
            {
                log(ex);
            }
        }

        public void GenerateHelp()
        {
            try
            {

                // Open and process source files
                OpenSourceFiles();

                if (CancellRequested())
                    return;

                if (IsMSWord)
                {
                    // Añadir a la lista de archivos adicionales el directorio generado con 
                    // los archivos del documento word:
                    string[] archivos = Directory.GetDirectories(MSWordHtmlDirectory);
                    foreach (string archivo in archivos)
                        AdditionalFiles.Add(archivo);
                }

                if (CancellRequested())
                    return;

                // Build the tree structure of document titles.
                ChmDocumentParser parser = new ChmDocumentParser(iDoc, this.UI, Project);
                Document = parser.ParseDocument();

                if (CancellRequested())
                    return;

                // Create decorators. This MUST to be called after the parsing: The parsing replaces the style tag
                // from the document
                PrepareHtmlDecorators();

                if (CancellRequested())
                    return;

                GenerateChm();

                if (Project.GenerateWeb)
                    GenerateWebSite();

                if (CancellRequested())
                    return;

                if (Project.GenerateJavaHelp)
                    GenerateJavaHelp();

                if (CancellRequested())
                    return;

                if (Project.GeneratePdf)
                    GeneratePdf();

                if (CancellRequested())
                    return;

                if (Project.GenerateXps)
                    GenerateXps();

                if (CancellRequested())
                    return;

                // Execute command line:
                if (Project.CommandLine != null && !Project.CommandLine.Trim().Equals(""))
                    ExecuteProjectCommandLine();

                if (IsMSWord)
                    // Era un doc word. Se creo un dir. temporal para guardar el html.
                    // Borrar este directorio:
                    Directory.Delete(MSWordHtmlDirectory, true);

            }
            catch (Exception ex)
            {
                log("Error: " + ex.Message, ConsoleUserInterface.ERRORWARNING);
                log(ex);
                throw;
            }
        }

        /// <summary>
        /// Configure decorators to add headers, footer, metas and other stuff to the generated
        /// web pages. Call this after do any change on the original page
        /// </summary>
        private void PrepareHtmlDecorators() 
        {

            // CHM html files will use the encoding specified by the user:
            ChmDecorator.ui = this.UI;
            // use the selected encoding:
            ChmDecorator.OutputEncoding = helpWorkshopEncoding;

            // Web html files will be UTF-8:
            WebDecorator.ui = this.UI;
            WebDecorator.MetaDescriptionValue = Project.WebDescription;
            WebDecorator.MetaKeywordsValue = Project.WebKeywords;
            WebDecorator.OutputEncoding = Encoding.UTF8;
            WebDecorator.UseTidy = true;

            if (!Project.ChmHeaderFile.Equals(""))
            {
                log("Reading chm header: " + Project.ChmHeaderFile, ConsoleUserInterface.INFO);
                ChmDecorator.HeaderHtmlFile = Project.ChmHeaderFile;
            }

            if (CancellRequested())
                return;

            if (!Project.ChmFooterFile.Equals(""))
            {
                log("Reading chm footer: " + Project.ChmFooterFile, ConsoleUserInterface.INFO);
                ChmDecorator.FooterHtmlFile = Project.ChmFooterFile;
            }

            if (CancellRequested())
                return;

            if (Project.GenerateWeb && !Project.WebHeaderFile.Equals(""))
            {
                log("Reading web header: " + Project.WebHeaderFile, ConsoleUserInterface.INFO);
                WebDecorator.HeaderHtmlFile = Project.WebHeaderFile;
            }

            if (CancellRequested())
                return;

            if (Project.GenerateWeb && !Project.WebFooterFile.Equals(""))
            {
                log("Reading web footer: " + Project.WebFooterFile, ConsoleUserInterface.INFO);
                WebDecorator.FooterHtmlFile = Project.WebFooterFile;
            }

            if (CancellRequested())
                return;

            if (Project.GenerateWeb && !Project.HeadTagFile.Equals(""))
            {
                log("Reading <header> include: " + Project.HeadTagFile, ConsoleUserInterface.INFO);
                WebDecorator.HeadIncludeFile = Project.HeadTagFile;
            }

            if (CancellRequested())
                return;

            // Prepare decorators for use. Do it after extract style tags:
            WebDecorator.PrepareHtmlPattern((IHTMLDocument3)iDoc);
            ChmDecorator.PrepareHtmlPattern((IHTMLDocument3)iDoc);
        }

        /// <summary>
        /// Generates help products.
        /// </summary>
        /*private void Generate() 
        {

            // Open and process source files
            OpenSourceFiles();

            if (CancellRequested())
                return;

            if( esWord )
            {
                // Añadir a la lista de archivos adicionales el directorio generado con 
                // los archivos del documento word:
                string[] archivos = Directory.GetDirectories( dirHtml );
                foreach( string archivo in archivos ) 
                    ArchivosAdicionales.Add( archivo );
            }

            if (CancellRequested())
                return;

            // Build the tree structure of document titles.
            ChmDocumentParser parser = new ChmDocumentParser(iDoc, this.UI, Project);
            tree = parser.ParseDocument();

            if (CancellRequested())
                return;

            // Create decorators. This MUST to be called after the parsing: The parsing replaces the style tag
            // from the document
            PrepareHtmlDecorators();

            if (CancellRequested())
                return;

            if( Project.GenerateWeb )
            {
                // Generar la web con la ayuda:
                log("Generating web site", ConsoleUserInterface.INFO);
                GenerateWebSite();
            }

            if (CancellRequested())
                return;

            if (Project.GenerateJavaHelp)
                GenerateJavaHelp();

            if( esWord )
                // Era un doc word. Se creo un dir. temporal para guardar el html.
                // Borrar este directorio:
                Directory.Delete( dirHtml , true );

            log("Project generated", ConsoleUserInterface.ERRORWARNING);

        }*/

        /// <summary>
        /// Generates a JAR with the java help of the document.
        /// <param name="generatedFiles">List of chapter html files generated for the help</param>
        /// <param name="index">List of topics of the document.</param>
        /// </summary>
        private void GenerateJavaHelp()
        {
            try
            {
                JavaHelpGenerator jhGenerator = new JavaHelpGenerator(MainSourceFile, Document, UI, Project, ChmDecorator);
                jhGenerator.Generate(AdditionalFiles);
            }
            catch (Exception ex)
            {
                UI.log(ex);
            }
        }

        /// <summary>
        /// Executes a command line and writes the command output to the log.
        /// </summary>
        /// <param name="exeFile">Path of the executable file to run</param>
        /// <param name="parameters">Parameters of the command line</param>
        /// <param name="workingDirectory">Directory where to run the command line</param>
        static public void ExecuteCommandLine(string exeFile, string parameters, string workingDirectory, UserInterface ui)
        {
            ProcessStartInfo info = new ProcessStartInfo(exeFile, parameters);
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.CreateNoWindow = true;
            info.WorkingDirectory = workingDirectory;

            Process proceso = Process.Start(info);
            while (!proceso.WaitForExit(1000))
                ui.LogStream(proceso.StandardOutput, ConsoleUserInterface.INFO);
            ui.LogStream(proceso.StandardOutput, ConsoleUserInterface.INFO);
        }

        /// <summary>
        /// Executes a command line and writes the command output to the log.
        /// </summary>
        /// <param name="exeFile">Path of the executable file to run</param>
        /// <param name="parameters">Parameters of the command line</param>
        /// <param name="workingDirectory">Directory where to run the command line</param>
        private void ExecuteCommandLine(string exeFile, string parameters, string workingDirectory)
        {
            ExecuteCommandLine(exeFile, parameters, workingDirectory, this.UI);
        }

    }
}

