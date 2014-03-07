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
using HtmlAgilityPack;
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
        /// The HTML source document
        /// </summary>
        private HtmlDocument HtmlDoc;

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
        /// Handler of the user interface of the generation process.
        /// </summary>
        public UserInterface UI;

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

            UI.Log("Joining documents to a single temporal file : " + joinedDocument, ConsoleUserInterface.INFO);
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

            if (UI.CancellRequested())
                return null;

            UI.Log("Convert file " + MainSourceFile + " to HTML", ConsoleUserInterface.INFO);
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

            if( !msWord.SaveWordToHtml(MainSourceFile, finalFile) )
                UI.Log("Warning: There was a time out waiting to close the word document", 1);

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
                IsMSWord = MSWord.IsWordDocument(archivoFinal);
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

                if (UI.CancellRequested())
                    return;

                // TODO: Check if this should be removed.
                if (AppSettings.UseTidyOverInput)
                    new TidyParser(UI).Parse(archivoFinal);

                if (UI.CancellRequested())
                    return;

                // Load the HTML file:
                HtmlDoc = new HtmlDocument();
                HtmlDoc.Load(archivoFinal);

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
        /// <param name="ui">Log for the help generation process</param>
        public DocumentProcessor( ChmProject project , UserInterface ui)
        {
            this.Project = project;
            this.AdditionalFiles = new List<string>(project.ArchivosAdicionales);
            this.UI = ui;
        }

        private void ExecuteProjectCommandLine()
        {
            try
            {
                UI.Log("Executing '" + Project.CommandLine.Trim() + "'", ConsoleUserInterface.INFO);
                string parameters = "/C " + Project.CommandLine.Trim();
                CommandLineExecution cmd = new CommandLineExecution("CMD.exe", parameters, UI);
                // If execution reads std input, create a window for it: Otherwise it can
                // hangup forever.
                cmd.Info.CreateNoWindow = false;
                cmd.Execute();
            }
            catch (Exception ex)
            {
                UI.Log("Error executing command line ", ConsoleUserInterface.ERRORWARNING);
                UI.Log(ex);
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
                UI.Log(ex);
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
                UI.Log(ex);
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
                UI.Log(ex);
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
                UI.Log(ex);
            }
        }

        public void GenerateHelp()
        {
            try
            {

                // Open and process source files
                OpenSourceFiles();

                if (UI.CancellRequested())
                    return;

                if (IsMSWord)
                {
                    // Añadir a la lista de archivos adicionales el directorio generado con 
                    // los archivos del documento word:
                    string[] archivos = Directory.GetDirectories(MSWordHtmlDirectory);
                    foreach (string archivo in archivos)
                        AdditionalFiles.Add(archivo);
                }

                if (UI.CancellRequested())
                    return;

                // Build the tree structure of document titles.
                ChmDocumentParser parser = new ChmDocumentParser(HtmlDoc, this.UI, Project);
                Document = parser.ParseDocument();

                if (UI.CancellRequested())
                    return;

                if (Document.IsEmpty)
                {
                    // If the document is empty, we have finished
                    UI.Log("The document is empty. There is nothing to generate!", ConsoleUserInterface.ERRORWARNING);
                    return;
                }

                // Create decorators. This MUST to be called after the parsing: The parsing replaces the style tag
                // from the document
                PrepareHtmlDecorators();

                if (UI.CancellRequested())
                    return;

                GenerateChm();

                if (Project.GenerateWeb)
                    GenerateWebSite();

                if (UI.CancellRequested())
                    return;

                if (Project.GenerateJavaHelp)
                    GenerateJavaHelp();

                if (UI.CancellRequested())
                    return;

                if (Project.GeneratePdf)
                    GeneratePdf();

                if (UI.CancellRequested())
                    return;

                if (Project.GenerateXps)
                    GenerateXps();

                if (UI.CancellRequested())
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
                UI.Log("Error: " + ex.Message, ConsoleUserInterface.ERRORWARNING);
                UI.Log(ex);
                throw new Exception("General error: " + ex.Message, ex); ;
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
            ChmDecorator.OutputEncoding = ChmProject.GetChmEncoding( UI, Project.GetChmCulture(UI) );

            // Web html files will be UTF-8:
            WebDecorator.ui = this.UI;
            WebDecorator.MetaDescriptionValue = Project.WebDescription;
            WebDecorator.MetaKeywordsValue = Project.WebKeywords;
            WebDecorator.OutputEncoding = Encoding.UTF8;
            WebDecorator.UseTidy = true;

            if (!Project.ChmHeaderFile.Equals(""))
            {
                UI.Log("Reading chm header: " + Project.ChmHeaderFile, ConsoleUserInterface.INFO);
                ChmDecorator.HeaderHtmlFile = Project.ChmHeaderFile;
            }

            if (UI.CancellRequested())
                return;

            if (!Project.ChmFooterFile.Equals(""))
            {
                UI.Log("Reading chm footer: " + Project.ChmFooterFile, ConsoleUserInterface.INFO);
                ChmDecorator.FooterHtmlFile = Project.ChmFooterFile;
            }

            if (UI.CancellRequested())
                return;

            if (Project.GenerateWeb && !Project.WebHeaderFile.Equals(""))
            {
                UI.Log("Reading web header: " + Project.WebHeaderFile, ConsoleUserInterface.INFO);
                WebDecorator.HeaderHtmlFile = Project.WebHeaderFile;
            }

            if (UI.CancellRequested())
                return;

            if (Project.GenerateWeb && !Project.WebFooterFile.Equals(""))
            {
                UI.Log("Reading web footer: " + Project.WebFooterFile, ConsoleUserInterface.INFO);
                WebDecorator.FooterHtmlFile = Project.WebFooterFile;
            }

            if (UI.CancellRequested())
                return;

            if (Project.GenerateWeb && !Project.HeadTagFile.Equals(""))
            {
                UI.Log("Reading <header> include: " + Project.HeadTagFile, ConsoleUserInterface.INFO);
                WebDecorator.HeadIncludeFile = Project.HeadTagFile;
            }

            if (UI.CancellRequested())
                return;

            // Prepare decorators for use. Do it after extract style tags:
            WebDecorator.PrepareHtmlPattern(HtmlDoc);
            ChmDecorator.PrepareHtmlPattern(HtmlDoc);
        }

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
                UI.Log(ex);
            }
        }

    }
}

