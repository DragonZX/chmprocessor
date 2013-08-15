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
        /// Name for the help project that will be generated
        /// </summary>
        public static string NOMBREPROYECTO = "help.hhp";

        /// <summary>
        /// Name for the chm file that will be generated (?)
        /// </summary>
        public static string NOMBREARCHIVOAYUDA = "help.chm";

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
        /// HTML title nodes (H1,H2,etc) tree.
        /// </summary>
        private ChmDocument tree;

        /// <summary>
        /// Decorator for pages for the generated CHM 
        /// </summary>
        private HtmlPageDecorator chmDecorator = new HtmlPageDecorator();

        /// <summary>
        /// Decorator for pages for the generated web site and the JavaHelp file.
        /// </summary>
        private HtmlPageDecorator webDecorator = new HtmlPageDecorator();

        /// <summary>
        /// Lista de archivos y directorios adicionales a añadir al proyecto de ayuda
        /// </summary>
        private List<string> ArchivosAdicionales;

        /// <summary>
        /// Indica si el archivo a procesar es un documento word o uno html
        /// </summary>
        private bool esWord;

        /// <summary>
        /// Si esWord = true, indica el directorio temporal donde se genero el 
        /// html del documento word
        /// </summary>
        private string dirHtml;

        /// <summary>
        /// HTML content of the body of the first chapter into the document.
        /// </summary>
        private string FirstChapterContent;

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
        /// Handler of the user interface of the generation process. Can be null.
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
            dirHtml = Path.GetTempPath() + Path.DirectorySeparatorChar + nombreArchivo;
            if (Directory.Exists(dirHtml))
                Directory.Delete(dirHtml, true);
            else if (File.Exists(dirHtml))
                File.Delete(dirHtml);
            Directory.CreateDirectory(dirHtml);

            // Rename the file to a save name. If there is spaces, for example, 
            // links to embedded images into the document are not found.
            //string finalFile = dirHtml + Path.DirectorySeparatorChar + nombreArchivo + ".htm";
            string finalFile = dirHtml + Path.DirectorySeparatorChar + ChmDocumentNode.ToSafeFilename(nombreArchivo) + ".htm";

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
                esWord = MSWord.ItIsWordDocument(archivoFinal);
                dirHtml = null;
                // Si es un documento word, convertirlo a HTML filtrado
                if (esWord)
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
            this.ArchivosAdicionales = new List<string>(project.ArchivosAdicionales);
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

        private IHTMLElement BuscarNodo( IHTMLElement nodo , string tag ) 
        {
            if( nodo.tagName.ToLower().Equals( tag.ToLower() ) )
                return nodo;
            else 
            {
                IHTMLElementCollection col = (IHTMLElementCollection) nodo.children;
                foreach( IHTMLElement hijo in col ) 
                {
                    IHTMLElement encontrado = BuscarNodo( hijo , tag );
                    if( encontrado != null )
                        return encontrado;
                }
                return null;
            }
        }

        private List<string> GuardarDocumentos(string directory, HtmlPageDecorator decorator, WebIndex indexer) 
        {
            return tree.SaveContentFiles(directory, decorator, indexer);
        }

        /// <summary>
        /// Vacia el directorio de destino, y copia los archivos adicionales a aquel.
        /// </summary>
        /// <returns>Devuelve la lista de archivos adicionales a incluir en el proyecto de la ayuda</returns>
        private List<string> GenerarDirDestino(string dirDst) 
        {
            // Recrear el directorio:
            try
            {
                if (Directory.Exists(dirDst))
                    Directory.Delete(dirDst, true);
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting directory: " + dirDst + ". Is it on use?", ex);
            }

            Directory.CreateDirectory( dirDst );
            
            // Copiar los archivos adicionales
            List<string> nuevosArchivos = new List<string>();
            foreach( string arc in ArchivosAdicionales ) 
            {
                if( Directory.Exists( arc ) )
                {
                    // Its a directory. Copy it:
                    string dst = dirDst + Path.DirectorySeparatorChar + Path.GetFileName( arc );
                    FileSystem.CopyDirectory(arc, dst);
                }
                else if( File.Exists( arc ) ) 
                {
                    string dst = dirDst + Path.DirectorySeparatorChar + Path.GetFileName( arc );
                    File.Copy( arc , dst );
                    nuevosArchivos.Add( Path.GetFileName( arc ) );
                }
            }

            return nuevosArchivos;
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

        public void GenerateHelp()
        {
            try
            {
                // Generate help project and java help:
                Generate();

                // TODO: If the chm has been compiled, remove the project directory from the temporal directory.
                ChmGenerator chmGenerator = new ChmGenerator(tree, UI, Project, ArchivosAdicionales, chmDecorator);
                chmGenerator.Generate();

                if (CancellRequested())
                    return;

                // PDF:
                if (Project.GeneratePdf)
                {
                    //BuildPdf();
                    PdfGenerator pdfGenerator = new PdfGenerator(MainSourceFile, UI, Project);
                    pdfGenerator.Generate();
                }

                if (CancellRequested())
                    return;

                // XPS:
                if (Project.GenerateXps)
                {
                    //BuildXps();
                    XpsGenerator xpsGenerator = new XpsGenerator(MainSourceFile, UI, Project);
                    xpsGenerator.Generate();
                }

                if (CancellRequested())
                    return;

                // Execute command line:
                if (Project.CommandLine != null && !Project.CommandLine.Trim().Equals(""))
                    ExecuteProjectCommandLine();
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
            chmDecorator.ui = this.UI;
            // use the selected encoding:
            chmDecorator.OutputEncoding = helpWorkshopEncoding;

            // Web html files will be UTF-8:
            webDecorator.ui = this.UI;
            webDecorator.MetaDescriptionValue = Project.WebDescription;
            webDecorator.MetaKeywordsValue = Project.WebKeywords;
            webDecorator.OutputEncoding = Encoding.UTF8;
            webDecorator.UseTidy = true;

            if (!Project.ChmHeaderFile.Equals(""))
            {
                log("Reading chm header: " + Project.ChmHeaderFile, ConsoleUserInterface.INFO);
                chmDecorator.HeaderHtmlFile = Project.ChmHeaderFile;
            }

            if (CancellRequested())
                return;

            if (!Project.ChmFooterFile.Equals(""))
            {
                log("Reading chm footer: " + Project.ChmFooterFile, ConsoleUserInterface.INFO);
                chmDecorator.FooterHtmlFile = Project.ChmFooterFile;
            }

            if (CancellRequested())
                return;

            if (Project.GenerateWeb && !Project.WebHeaderFile.Equals(""))
            {
                log("Reading web header: " + Project.WebHeaderFile, ConsoleUserInterface.INFO);
                webDecorator.HeaderHtmlFile = Project.WebHeaderFile;
            }

            if (CancellRequested())
                return;

            if (Project.GenerateWeb && !Project.WebFooterFile.Equals(""))
            {
                log("Reading web footer: " + Project.WebFooterFile, ConsoleUserInterface.INFO);
                webDecorator.FooterHtmlFile = Project.WebFooterFile;
            }

            if (CancellRequested())
                return;

            if (Project.GenerateWeb && !Project.HeadTagFile.Equals(""))
            {
                log("Reading <header> include: " + Project.HeadTagFile, ConsoleUserInterface.INFO);
                webDecorator.HeadIncludeFile = Project.HeadTagFile;
            }

            if (CancellRequested())
                return;

            // Prepare decorators for use. Do it after extract style tags:
            webDecorator.PrepareHtmlPattern((IHTMLDocument3)iDoc);
            chmDecorator.PrepareHtmlPattern((IHTMLDocument3)iDoc);
        }

        /// <summary>
        /// Generates help products.
        /// </summary>
        private void Generate() 
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

            // Preparar el directorio de destino.
            //log("Creating project directory: " + Project.HelpProjectDirectory, ConsoleUserInterface.INFO);
            //List<string> listaFinalArchivos = GenerarDirDestino(Project.HelpProjectDirectory);

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

            /*if (CancellRequested())
                return;*/

            // Generar los archivos HTML:
            /*log("Storing splitted files", ConsoleUserInterface.INFO);
            List<string> archivosGenerados = GuardarDocumentos(Project.HelpProjectDirectory, chmDecorator, null);

            if (CancellRequested())
                return;

            // Check if the file for content without title was created. If not, remove it from the files tree.
            string archivo1 = Path.Combine(Project.HelpProjectDirectory , ChmDocument.INITIALSECTIONFILENAME);
            if( ! File.Exists( archivo1) ) 
            {
                tree.RootNode.DestinationFileName = "";
                tree.RootNode.Children.RemoveAt(0);
            }*/

            // Obtener el nombre del primer archivo generado:
            string primero = "";
            if( tree.RootNode.Children.Count > 0 )
                primero = ((ChmDocumentNode) tree.RootNode.Children[0]).DestinationFileName;

            if (CancellRequested())
                return;

            if( Project.GenerateWeb )
            {
                // Generar la web con la ayuda:
                log("Generating web site", ConsoleUserInterface.INFO);
                GenerateWebSite(tree.Index);
            }

            if (CancellRequested())
                return;

            if (Project.GenerateJavaHelp)
            {
                log("Generating Java Help", ConsoleUserInterface.INFO);
                GenerateJavaHelp(tree.Index);
            }

            if( esWord )
                // Era un doc word. Se creo un dir. temporal para guardar el html.
                // Borrar este directorio:
                Directory.Delete( dirHtml , true );

            log("Project generated", ConsoleUserInterface.ERRORWARNING);

        }

        /*private void GeneateSitemap(string webDirectory)
        {
            try {
                string sitemap = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + 
                                 "<urlset xmlns=\"http://www.google.com/schemas/sitemap/0.84\">\n";
                string webBase = this.Project.WebBase;
                if( !webBase.EndsWith("/") )
                    webBase += "/";
                if( !webBase.StartsWith("http://") )
                    webBase += "http://";

                string[] htmlFiles = Directory.GetFiles(webDirectory);
                foreach (string file in htmlFiles)
                {
                    string lowerFile = file.ToLower();
                    if (lowerFile.EndsWith(".htm") || lowerFile.EndsWith(".html"))
                    {
                        // Add to the sitemap
                        sitemap += "<url>\n<loc>" + webBase + Path.GetFileName(file) + "</loc>\n<lastmod>";
                        DateTime lastmod = File.GetLastWriteTime( file );
                        sitemap += lastmod.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszzz") + "</lastmod>\n";
                        sitemap += "<changefreq>" + this.Project.ChangeFrequency + "</changefreq>\n";
                        sitemap += "</url>\n";
                    }
                }
                sitemap += "</urlset>";

                // Store
                string sitemapFile = webDirectory + Path.DirectorySeparatorChar + "sitemap.xml";
                StreamWriter writer = new StreamWriter( sitemapFile , false , Encoding.UTF8 );
                writer.Write( sitemap );
                writer.Close();
                string sitemapZiped = webDirectory + Path.DirectorySeparatorChar + "sitemap.xml.gz";
                Zip.CompressFile(sitemapFile, sitemapZiped);
                File.Delete(sitemapFile);
            }
            catch( Exception ex ) {
                log("Error generating the sitemap: " + ex.Message, ConsoleUserInterface.ERRORWARNING);
                log(ex);
            }
        }*/     

        /// <summary>
        /// Generates a JAR with the java help of the document.
        /// <param name="generatedFiles">List of chapter html files generated for the help</param>
        /// <param name="index">List of topics of the document.</param>
        /// </summary>
        private void GenerateJavaHelp(ChmDocumentIndex index)
        {

            JavaHelpGenerator jhGenerator = new JavaHelpGenerator(MainSourceFile, tree, UI, Project, chmDecorator);

            /*
            log("Copiying files to directory " + jhGenerator.JavaHelpDirectoryGeneration, ConsoleUserInterface.INFO);
            GenerarDirDestino(jhGenerator.JavaHelpDirectoryGeneration);

            // Write HTML help content files to the destination directory
            GuardarDocumentos(jhGenerator.JavaHelpDirectoryGeneration, chmDecorator, null);
            */

            jhGenerator.Generate(ArchivosAdicionales);
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

        /// <summary>
        /// Generated the help web site 
        /// </summary>
        /// <param name="index">Index help information</param>
        private void GenerateWebSite(ChmDocumentIndex index) 
        {
            try
            {
                /*
                // Crear el directorio web y copiar archivos adicionales:
                string dirWeb;
                if (Project.WebDirectory.Equals(""))
                    dirWeb = Project.HelpProjectDirectory + Path.DirectorySeparatorChar + "web";
                else
                    dirWeb = Project.WebDirectory;
                GenerarDirDestino(dirWeb);

                // Copy the css file if was generated:
                //if (cssFile != null)
                //    File.Copy(cssFile, dirWeb + Path.DirectorySeparatorChar + Path.GetFileName(cssFile));

                // Prepare the indexing database:
                WebIndex indexer = null;
                try
                {
                    if (Project.FullTextSearch)
                    {
                        indexer = new WebIndex();
                        string dbFile = dirWeb + Path.DirectorySeparatorChar + "fullsearchdb.db3";
                        string dirTextFiles = dirWeb + Path.DirectorySeparatorChar + "textFiles";
                        indexer.Connect(dbFile);
                        indexer.CreateDatabase(System.Windows.Forms.Application.StartupPath + Path.DirectorySeparatorChar + "searchdb.sql", dirTextFiles);
                        indexer.StoreConfiguration(Project.WebLanguage);
                    }

                    // Create new files for the web help:
                    GuardarDocumentos(dirWeb, webDecorator, indexer);
                }
                finally
                {
                    if (indexer != null)
                        indexer.Disconnect();
                }

                // HTML save version of the title:
                string htmlTitle = HttpUtility.HtmlEncode(Project.HelpTitle);

                // Generate search form HTML code:
                string textSearch = "";
                if (Project.FullTextSearch)
                {
                    textSearch = "<form name=\"searchform\" method=\"post\" action=\"search.aspx\" id=\"searchform\" onsubmit=\"doFullTextSearch();return false;\" >\n";
                    textSearch += "<p><img src=\"system-search.png\" align=middle alt=\"Search image\" /> <b>%Search Text%:</b><br /><input type=\"text\" id=\"searchText\" style=\"width:80%;\" name=\"searchText\"/>\n";
                    textSearch += "<input type=\"button\" value=\"%Search%\" onclick=\"doFullTextSearch();\" id=\"Button1\" name=\"Button1\"/></p>\n";
                }
                else
                {
                    textSearch = "<form name=\"searchform\" method=\"post\" action=\"search.aspx\" id=\"searchform\" onsubmit=\"doSearch();return false;\" >\n";
                    textSearch += "<p><img src=\"system-search.png\" align=middle alt=\"Search image\" /> <b>%Search Text%:</b><br /><input type=\"text\" id=\"searchText\" style=\"width:80%;\" name=\"searchText\"/><br/>\n";
                    textSearch += "<input type=\"button\" value=\"%Search%\" onclick=\"doSearch();\" id=\"Button1\" name=\"Button1\"/></p>\n";
                    textSearch += "<select id=\"searchResult\" style=\"width:100%;\" size=\"20\" name=\"searchResult\">\n";
                    textSearch += "<option></option>\n";
                    textSearch += "</select>\n";
                }
                textSearch += "</form>\n";

                // The text placements for web files:
                string[] variables = { "%TEXTSEARCH%" , "%TITLE%", "%TREE%", "%TOPICS%", "%FIRSTPAGECONTENT%", 
                    "%WEBDESCRIPTION%", "%KEYWORDS%" , "%HEADER%" , "%FOOTER%" , "%HEADINCLUDE%" };
                string[] newValues = { textSearch , htmlTitle, tree.GenerarArbolHtml(Project.MaxHeaderContentTree, "contentsTree", 
                    "contentTree"), index.GenerateWebIndex(), FirstChapterContent, 
                    webDecorator.MetaDescriptionTag , webDecorator.MetaKeywordsTag ,
                    webDecorator.HeaderHtmlCode , webDecorator.FooterHtmlCode , webDecorator.HeadIncludeHtmlCode };

                Replacements replacements = new Replacements(variables, newValues);

                // Load translation files.
                string translationFile = System.Windows.Forms.Application.StartupPath +
                    Path.DirectorySeparatorChar + "webTranslations" + Path.DirectorySeparatorChar +
                    Project.WebLanguage + ".txt";
                try
                {
                    replacements.AddReplacementsFromFile(translationFile);
                }
                catch (Exception ex)
                {
                    log("Error opening web translations file" + translationFile + ": " + ex.Message, ConsoleUserInterface.ERRORWARNING);
                    log(ex);
                }

                // Copy web files replacing text
                string baseDir = System.Windows.Forms.Application.StartupPath + Path.DirectorySeparatorChar + "webFiles";
                replacements.CopyDirectoryReplaced(baseDir, dirWeb, MSWord.HTMLEXTENSIONS, AppSettings.UseTidyOverOutput, UI, webDecorator.OutputEncoding);

                // Copy full text search files replacing text:
                if (Project.FullTextSearch)
                {
                    // Copy full text serch files:
                    string dirSearchFiles = System.Windows.Forms.Application.StartupPath + Path.DirectorySeparatorChar + "searchFiles";
                    replacements.CopyDirectoryReplaced(dirSearchFiles, dirWeb, MSWord.ASPXEXTENSIONS, false, UI, webDecorator.OutputEncoding);
                }

                if (Project.GenerateSitemap)
                    // Generate site map for web indexers (google).
                    GeneateSitemap(dirWeb);
                */

                WebHelpGenerator webHelpGenerator = new WebHelpGenerator(tree, UI, Project, webDecorator);
                webHelpGenerator.Generate(ArchivosAdicionales);

            }
            catch (Exception ex)
            {
                log(ex);
            }
        }

        /// <summary>
        /// Return the first header tag (H1,H2,etc) found on a subtree of the html document 
        /// that will split the document.
        /// TODO: Join this function and ChmDocumentParser.SearchFirstCutNode
        /// </summary>
        /// <param name="root">Root of the html subtree where to search a split</param>
        /// <returns>The first split tag node. null if none was found.</returns>
        private IHTMLElement SearchFirstCutNode( IHTMLElement root ) 
        {
            if (IsCutHeader(root))
                return root;
            else 
            {
                IHTMLElementCollection col = (IHTMLElementCollection)root.children;
                foreach( IHTMLElement e in col ) 
                {
                    IHTMLElement seccion = SearchFirstCutNode( e );
                    if( seccion != null )
                        return seccion;
                }
                return null;
            }
        }

        // TODO: Join this function with the same on ChmDocumentParser
        static public bool EsHeader( IHTMLElement nodo ) 
        {
            return nodo is IHTMLHeaderElement && nodo.innerText != null && !nodo.innerText.Trim().Equals("");
        }

        /// <summary>
        /// Checks if a node is a HTML header tag (H1, H2, etc) upper or equal to the cut level for the
        /// project (Project.CutLevel).
        /// Also checks if it contains some text.
        /// TODO: Join this function with the same on ChmDocumentParser
        /// </summary>
        /// <param name="node">HTML node to check</param>
        /// <returns>true if the node is a cut header</returns>
        public bool IsCutHeader( IHTMLElement node ) {
            return IsCutHeader(Project.CutLevel, node);
        }

        /// <summary>
        /// Checks if a node is a HTML header tag (H1, H2, etc) upper or equal to the cut level.
        /// Also checks if it contains some text.
        /// </summary>
        /// <param name="MaximumLevel">Maximum level the level is accepted as cut level.</param>
        /// <param name="node">HTML node to check</param>
        /// <returns>true if the node is a cut header</returns>
        static public bool IsCutHeader( int MaximumLevel , IHTMLElement node ) 
        {
            // If its a Hx node and x <= MaximumLevel, and it contains text, its a cut node:
            if( EsHeader(node) ) 
            {
                string tagName = node.tagName.ToUpper();
                for( int i=1;i<=MaximumLevel; i++ ) 
                {
                    string nombreTag = "H" + i;
                    if( nombreTag.Equals( tagName ) )
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Logs the content of a stream
        /// </summary>
        /// <param name="reader">Log with the content to read.</param>
        /// <param name="level">Level of the stream</param>
        private void LogStream(StreamReader reader, int logLevel) 
        {
            if (UI != null)
                UI.LogStream(reader, logLevel);
        }

    }
}

