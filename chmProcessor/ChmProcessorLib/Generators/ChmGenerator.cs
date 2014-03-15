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
using System.Collections.Generic;
using System.Text;
using System.IO;
using ChmProcessorLib.DocumentStructure;
using System.Globalization;
using System.Diagnostics;
using ChmProcessorLib.Log;

namespace ChmProcessorLib.Generators
{

    /// <summary>
    /// Tool to create and compile a CHM project from a document
    /// </summary>
    public class ChmGenerator : ContentDirectoryGenerator
    {

        /// <summary>
        /// File name for generated table of contents file
        /// </summary>
        private const string TOCFILENAME = "toc-generated.hhc";

        /// <summary>
        /// File name for generated index file
        /// </summary>
        private const string INDEXFILENAME = "Index-generated.hhk";

        /// <summary>
        /// Name for the help project file
        /// </summary>
        private const string HELPPROJECTFILENAME = "help.hhp";

        /// <summary>
        /// Name for the chm file that will be generated
        /// TODO: Dont use this. Use the name stored into the chmproject "Project"
        /// </summary>
        private const string CHMFILENAME = "help.chm";

        /// <summary>
        /// Text encodig to create the CHM project files
        /// </summary>
        private Encoding Encoding;

        /// <summary>
        /// Culture to put into the help workshop project file.
        /// </summary>
        private CultureInfo HelpWorkshopCulture;

        /// <summary>
        /// List of other file paths to include into the CHM.
        /// The paths on this list must to be relative to the ChmProjectDirectory member directory
        /// </summary>
        private List<string> AdditionalFiles;

        public ChmGenerator(ChmDocument document, UserInterface ui, ChmProject project, 
            List<string> additionalFiles, HtmlPageDecorator decorator)
            : base(document, ui, project, decorator)
        {
            this.Document = document;
            this.UI = ui;
            this.Project = project;
            this.AdditionalFiles = additionalFiles;

            // Get the encoding and culture for the chm:
            this.HelpWorkshopCulture = project.GetChmCulture(UI);
            this.Encoding = ChmProject.GetChmEncoding(UI, this.HelpWorkshopCulture);
        }

        /// <summary>
        /// The absolute path to the CHM project file
        /// </summary>
        private string ChmProjectPath
        {
            get { return Path.Combine(Project.HelpProjectDirectory, HELPPROJECTFILENAME); }
        }

        /// <summary>
        /// Generates and compiles the CHM file
        /// </summary>
        public void Generate()
        {

            // Create directory, content files and additional files
            List<string> relativePaths = CreateDestinationDirectory(Project.HelpProjectDirectory, AdditionalFiles);
            CreateHelpContentFiles(Project.HelpProjectDirectory);

            UI.Log("Generating table of contents", ChmLogLevel.INFO);
            GenerateTOCFile();

            if (UI.CancellRequested())
                return;

            UI.Log("Generating index", ChmLogLevel.INFO);
            GenerateHelpIndex();

            if (UI.CancellRequested())
                return;

            UI.Log("Generating help project", ChmLogLevel.INFO);
            GenerateHelpProject(relativePaths);

            if (UI.CancellRequested())
                return;

            // Open or compile the project
            ProcessHelpProject();

        }

        /// <summary>
        /// Saves the table of contents of this tree for a CHM project.
        /// </summary>
        private void GenerateTOCFile()
        {
            string filePath = Path.Combine(Project.HelpProjectDirectory, TOCFILENAME);
            StreamWriter writer = new StreamWriter(filePath, false, Encoding);
            writer.WriteLine("<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML//EN\">");
            writer.WriteLine("<HTML>");
            writer.WriteLine("<HEAD>");
            writer.WriteLine("<!-- Sitemap 1.0 -->");
            writer.WriteLine("</HEAD><BODY>");
            writer.WriteLine("<UL>");
            foreach (ChmDocumentNode hijo in Document.RootNode.Children)
                GenerateTOCFile(writer, hijo , 1);
            writer.WriteLine("</UL>");
            writer.WriteLine("</BODY></HTML>");
            writer.Close();
        }

        private string TOCEntry(ChmDocumentNode node)
        {
            string title = node.HtmlEncodedTitle;
            // Remove line breaks on title: They broke the attribute value and the CHM compiler 
            // doest not recognize it
            title = title.Replace("\r\n", " ").Replace("\n", " ");

            string texto = "<LI> <OBJECT type=\"text/sitemap\">\n" +
                "     <param name=\"Name\" value=\"" +
                title +
                "\">\n" +
                "     <param name=\"Local\" value=\"" + node.Href;
            texto += "\">\n" + "     </OBJECT>\n";
            return texto;
        }

        private void GenerateTOCFile(StreamWriter writer, ChmDocumentNode nodo, int nivel)
        {
            if (Project.MaxHeaderContentTree != 0 && nivel > Project.MaxHeaderContentTree)
                return;

            writer.WriteLine(/*nodo.EntradaArbolContenidos*/ TOCEntry(nodo) );
            if (nodo.Children.Count > 0)
            {
                writer.WriteLine("<UL>");
                foreach (ChmDocumentNode hijo in nodo.Children)
                    GenerateTOCFile(writer, hijo, nivel + 1);
                writer.WriteLine("</UL>");
            }
        }

        /// <summary>
        /// Store the HHK file for the help project with the topics index.
        /// </summary>
        /// <param name="fileName">Path where to save the file</param>
        /// <param name="encoding">Encoding used to write the file</param>
        private void GenerateHelpIndex()
        {
            string filePath = Path.Combine(Project.HelpProjectDirectory, INDEXFILENAME);
            StreamWriter writer = new StreamWriter(filePath, false, Encoding);
            writer.WriteLine("<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML//EN\">");
            writer.WriteLine("<HTML>");
            writer.WriteLine("<HEAD>");
            writer.WriteLine("<!-- Sitemap 1.0 -->");
            writer.WriteLine("</HEAD><BODY>");
            writer.WriteLine("<UL>");
            foreach (ChmDocumentNode node in Document.Index)
            {
                if (!node.Href.Equals(""))
                    writer.WriteLine( TOCEntry(node) );
            }
            writer.WriteLine("</UL>");
            writer.WriteLine("</BODY></HTML>");
            writer.Close();
        }

        private void GenerateHelpProject(List<string> additionalFilesRelativePaths)
        {

            // Get the name of the first splitted file:
            string firstTopicFile = string.Empty;
            //if (Document.RootNode.Children.Count > 0)
            //    firstTopicFile = Document.RootNode.Children[0].DestinationFileName;
            ChmDocumentNode firstTopicNode = Document.FirstNodeWithContent;
            if (firstTopicNode != null && !string.IsNullOrEmpty(firstTopicNode.DestinationFileName))
                firstTopicFile = firstTopicNode.DestinationFileName;

            string filePath = ChmProjectPath;
            StreamWriter writer = new StreamWriter(filePath, false, Encoding);
            writer.WriteLine("[OPTIONS]");
            writer.WriteLine("Compatibility=1.1 or later");
            writer.WriteLine("Compiled file=" + CHMFILENAME);
            writer.WriteLine("Contents file=" + TOCFILENAME);
            writer.WriteLine("Default topic=" + firstTopicFile);
            writer.WriteLine("Display compile progress=No");
            writer.WriteLine("Full-text search=Yes");
            writer.WriteLine("Index file=" + INDEXFILENAME);
            writer.WriteLine("Language=0x" + Convert.ToString(HelpWorkshopCulture.LCID, 16) + " " + HelpWorkshopCulture.DisplayName);
            writer.WriteLine("Title=" + Project.HelpTitle);
            writer.WriteLine("\r\n[FILES]");
            foreach (string extraFile in additionalFilesRelativePaths)
                writer.WriteLine(extraFile);
            List<string> lista = Document.ListaArchivosGenerados();
            foreach (string arc in lista)
                writer.WriteLine(arc);
            writer.WriteLine("\r\n[INFOTYPES]\r\n");
            writer.Close();
        }

        /// <summary>
        /// Compiles the help project file and it's copied to the destination file.
        /// </summary>
        private void Compile()
        {
            UI.Log("Compiling CHM file", ChmLogLevel.INFO);

            // The help compiler EXE path:
            string compilerPath = AppSettings.CompilerPath;

            if (!File.Exists(compilerPath))
                throw new Exception("Compiler not found at " + compilerPath + ". Help not generated");
            else
            {
                string proyecto = "\"" + ChmProjectPath + "\"";

                // TODO: Use DocumentProcessor.ExecuteCommandLine to make this execution:

                CommandLineExecution cmd;
                if (!AppSettings.UseAppLocale)
                    // Run the raw compiler
                    cmd = new CommandLineExecution(compilerPath, proyecto, UI);
                else
                {
                    // Run the compiler with AppLocale. Need to compile files with a 
                    // char encoding distinct to the system codepage.
                    //string parameters = "\"" + compilerPath + "\" " + proyecto + " /L" + Convert.ToString(HelpWorkshopCulture.LCID, 16);
                    string parameters = "\"" + compilerPath + "\" " + proyecto + " /L" + Convert.ToString(HelpWorkshopCulture.TextInfo.LCID, 16);
                    cmd = new CommandLineExecution(AppSettings.AppLocalePath, parameters, UI);
                }
                cmd.Execute();

                string archivoAyudaOrigen = Path.Combine(Project.HelpProjectDirectory, CHMFILENAME);
                if (File.Exists(archivoAyudaOrigen))
                {
                    // Be sure the destination directory exists
                    string destinationDirectory = Path.GetDirectoryName(Project.HelpFile);
                    if (!Directory.Exists(destinationDirectory))
                        Directory.CreateDirectory(destinationDirectory);

                    // Copy the file from the temporally directory to destination
                    File.Copy(archivoAyudaOrigen, Project.HelpFile, true);
                }
                else
                    throw new Exception("After compiling, the file " + archivoAyudaOrigen + " was not found. Some error happened with the compilation. Try to generate the help project manually");
            }
        }

        /// <summary>
        /// Handle the generated help project.
        /// It can be compiled or opened through Windows sell.
        /// </summary>
        private void ProcessHelpProject()
        {
            if (Project.Compile)
            {
                // Due to some strange bug, if we have as current drive a network drive, the generated
                // help dont show the images... So, change it to the system drive:
                string cwd = Directory.GetCurrentDirectory();
                string tempDirectory = Path.GetDirectoryName(Project.HelpProjectDirectory);
                Directory.SetCurrentDirectory(tempDirectory);
                Compile();
                Directory.SetCurrentDirectory(cwd);
            }
            else if (Project.OpenProject)
            {
                UI.Log("Opening CHM project", ChmLogLevel.INFO);
                try
                {
                    // Abrir el proyecto de la ayuda
                    Process proceso = new Process();
                    proceso.StartInfo.FileName = ChmProjectPath;
                    proceso.Start();
                }
                catch (Exception ex)
                {
                    UI.Log("The project " + ChmProjectPath + " cannot be opened" +
                        ". Do you have installed the Microsoft Help Workshop ?", ChmLogLevel.ERROR);
                    UI.Log(ex);
                }
            }
        }

    }
}
