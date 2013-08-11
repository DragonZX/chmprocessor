using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ChmProcessorLib.DocumentStructure;
using System.Globalization;
using System.Diagnostics;

namespace ChmProcessorLib.Generators
{

    /// <summary>
    /// Tool to create and compile a CHM project
    /// </summary>
    public class ChmGenerator
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
        /// Directy path where is stored the CHM project
        /// </summary>
        private string ChmProjectDirectory;

        /// <summary>
        /// Text encodig to create the CHM project files
        /// </summary>
        private Encoding Encoding;

        /// <summary>
        /// Culture to put into the help workshop project file.
        /// </summary>
        private CultureInfo HelpWorkshopCulture;

        /// <summary>
        /// Structured documento to compile to CHM
        /// </summary>
        private ChmDocument Document;

        /// <summary>
        /// Log generator
        /// </summary>
        private UserInterface UI;

        /// <summary>
        /// Generation settings
        /// </summary>
        private ChmProject Project;

        /// <summary>
        /// List of other file paths to include into the CHM.
        /// The paths on this list must to be relative to the ChmProjectDirectory member directory
        /// </summary>
        private List<string> AdditionalFiles;

        public ChmGenerator(ChmDocument document, string chmProjectDirectory, 
            UserInterface ui, ChmProject project, List<string> additionalFiles)
        {
            this.ChmProjectDirectory = chmProjectDirectory;
            this.Document = document;
            this.UI = ui;
            this.Project = project;
            this.AdditionalFiles = additionalFiles;

            // Get the encoding and culture for the chm:
            try
            {
                this.HelpWorkshopCulture = CultureInfo.GetCultureInfo(project.ChmLocaleID);
            }
            catch (Exception ex)
            {
                UI.log(ex);
                throw new Exception("The locale ID (LCID) " + project.ChmLocaleID + " is not found.", ex);
            }

            try
            {
                this.Encoding = Encoding.GetEncoding(HelpWorkshopCulture.TextInfo.ANSICodePage);
            }
            catch (Exception ex)
            {
                UI.log(ex);
                throw new Exception("The ANSI codepage " + HelpWorkshopCulture.TextInfo.ANSICodePage + " is not found.", ex);
            }
        }

        /// <summary>
        /// The absolute path to the CHM project file
        /// </summary>
        private string ChmProjectPath
        {
            get { return Path.Combine(ChmProjectDirectory, HELPPROJECTFILENAME); }
        }

        public void CreateProject()
        {
            UI.log("Generating table of contents", ConsoleUserInterface.INFO);
            GenerateTOCFile();

            if (UI.CancellRequested())
                return;

            UI.log("Generating index", ConsoleUserInterface.INFO);
            GenerateHelpIndex();

            if (UI.CancellRequested())
                return;

            UI.log("Generating help project", ConsoleUserInterface.INFO);
            GenerateHelpProject();

            if (UI.CancellRequested())
                return;

            // Open or compile the project
            ProcessHelpProject();

        }

        /// <summary>
        /// Saves the table of contents of this tree for a CHM project.
        /// </summary>
        /// <param name="filePath">Path where to store the file.</param>
        private void GenerateTOCFile()
        {
            string filePath = Path.Combine(ChmProjectDirectory, TOCFILENAME);
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

        private void GenerateTOCFile(StreamWriter writer, ChmDocumentNode nodo, int nivel)
        {
            if (Project.MaxHeaderContentTree != 0 && nivel > Project.MaxHeaderContentTree)
                return;

            writer.WriteLine(nodo.EntradaArbolContenidos);
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
            string filePath = Path.Combine(ChmProjectDirectory, INDEXFILENAME);
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
                    writer.WriteLine(node.EntradaArbolContenidos);
            }
            writer.WriteLine("</UL>");
            writer.WriteLine("</BODY></HTML>");
            writer.Close();
        }

        private void GenerateHelpProject()
        {

            // Get the name of the first splitted file:
            string firstTopicFile = string.Empty;
            if (Document.RootNode.Children.Count > 0)
                firstTopicFile = Document.RootNode.Children[0].DestinationFileName;

            string filePath = ChmProjectPath;
            StreamWriter writer = new StreamWriter(filePath, false, Encoding);
            writer.WriteLine("[OPTIONS]");
            writer.WriteLine("Compatibility=1.1 or later");
            writer.WriteLine("Compiled file=" + CHMFILENAME);
            writer.WriteLine("Contents file=toc-generado.hhc");
            writer.WriteLine("Default topic=" + firstTopicFile);
            writer.WriteLine("Display compile progress=No");
            writer.WriteLine("Full-text search=Yes");
            writer.WriteLine("Index file=Index-generado.hhk");
            writer.WriteLine("Language=0x" + Convert.ToString(HelpWorkshopCulture.LCID, 16) + " " + HelpWorkshopCulture.DisplayName);
            writer.WriteLine("Title=" + Project.HelpTitle);
            writer.WriteLine("\r\n[FILES]");
            foreach (string extraFile in AdditionalFiles)
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
            UI.log("Compiling CHM file", ConsoleUserInterface.INFO);

            // The help compiler EXE path:
            string compilerPath = AppSettings.CompilerPath;

            if (!File.Exists(compilerPath))
                throw new Exception("Compiler not found at " + compilerPath + ". Help not generated");
            else
            {
                string proyecto = "\"" + ChmProjectPath + "\"";

                ProcessStartInfo info;
                if (!AppSettings.UseAppLocale)
                    // Run the raw compiler
                    info = new ProcessStartInfo(compilerPath, proyecto);
                else
                {
                    // Run the compiler with AppLocale. Need to compile files with a 
                    // char encoding distinct to the system codepage.
                    // Command line example: C:\Windows\AppPatch\AppLoc.exe "C:\Program Files\HTML Help Workshop\hhc.exe" "A B C" "/L0480"
                    string parameters = "\"" + compilerPath + "\" " + proyecto + " /L" + Convert.ToString(HelpWorkshopCulture.LCID, 16);
                    info = new ProcessStartInfo(AppSettings.AppLocalePath, parameters);
                }

                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.CreateNoWindow = true;

                // Execute the compile process
                Process proceso = Process.Start(info);
                while (!proceso.WaitForExit(1000))
                    UI.LogStream(proceso.StandardOutput, ConsoleUserInterface.INFO);
                UI.LogStream(proceso.StandardOutput, ConsoleUserInterface.INFO);
                UI.LogStream(proceso.StandardError, ConsoleUserInterface.ERRORWARNING);

                string archivoAyudaOrigen = Path.Combine(ChmProjectDirectory, CHMFILENAME);
                if (File.Exists(archivoAyudaOrigen))
                    // Copy the file from the temporally directory to the gift by the user
                    File.Copy(archivoAyudaOrigen, Project.HelpFile, true);
                else
                    throw new Exception("After compiling, the file " + archivoAyudaOrigen + " was not found. Some error happened with the compilation. Try to generate the help project manually");
            }
        }

        /// <summary>
        /// Handle the generated help project.
        /// It can be compiled or openened through Windows sell.
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
                UI.log("Opening CHM project", ConsoleUserInterface.INFO);
                try
                {
                    // Abrir el proyecto de la ayuda
                    Process proceso = new Process();
                    proceso.StartInfo.FileName = ChmProjectPath;
                    proceso.Start();
                }
                catch (Exception ex)
                {
                    UI.log("The project " + ChmProjectPath + " cannot be opened" +
                        ". Do you have installed the Microsoft Help Workshop ?", ConsoleUserInterface.ERRORWARNING);
                    UI.log(ex);
                }
            }
        }

    }
}
