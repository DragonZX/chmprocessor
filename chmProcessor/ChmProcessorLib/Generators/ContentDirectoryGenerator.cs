using System;
using System.Collections.Generic;
using System.Text;
using ChmProcessorLib.DocumentStructure;
using System.IO;
using WebIndexLib;

namespace ChmProcessorLib.Generators
{

    /// <summary>
    /// Base class for generators that create a directory with the help content.
    /// It handles the creation of the directory, and the copy and decoration of the HTML help content 
    /// files.
    /// </summary>
    public abstract class ContentDirectoryGenerator
    {

        /// <summary>
        /// The document to convert to help
        /// </summary>
        protected ChmDocument Document;

        /// <summary>
        /// The logger
        /// </summary>
        protected UserInterface UI;

        /// <summary>
        /// The generation settings
        /// </summary>
        protected ChmProject Project;

        /// <summary>
        /// The HTML help content files generator 
        /// </summary>
        protected HtmlPageDecorator Decorator;

        /// <summary>
        /// Optional tool to generate the index of the help content. It can be null if its not needed.
        /// The content of the indexer will be generated when the content files are stored into the 
        /// destination directory.
        /// </summary>
        public WebIndex Indexer;

        protected ContentDirectoryGenerator(ChmDocument document, UserInterface ui,
            ChmProject project, HtmlPageDecorator decorator)
        {
            this.Document = document;
            this.UI = ui;
            this.Project = project;
            this.Decorator = decorator;
        }

        /// <summary>
        /// Creates or re-creates the destination directory, and it copies the additional files to it
        /// <param name="additionalFiles">List of absolute paths to additional files to copy to the
        /// directory</param>
        /// </summary>
        /// <returns>The list of copied files, with a path relative to the destination directory</returns>
        protected List<string> CreateDestinationDirectory(string dirDst, List<string> additionalFiles)
        {
            UI.log("Creating directory " + dirDst, ConsoleUserInterface.INFO);
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

            Directory.CreateDirectory(dirDst);

            // Copiar los archivos adicionales
            List<string> nuevosArchivos = new List<string>();
            foreach (string arc in additionalFiles)
            {
                if (Directory.Exists(arc))
                {
                    // Its a directory. Copy it:
                    string dst = Path.Combine( dirDst, Path.GetFileName(arc) );
                    FileSystem.CopyDirectory(arc, dst);
                    // TODO: Should we return here all recursivelly copied files?
                }
                else if (File.Exists(arc))
                {
                    string fileName = Path.GetFileName(arc);
                    string dst = Path.Combine( dirDst, fileName );
                    File.Copy(arc, dst);
                    nuevosArchivos.Add(fileName);
                }
            }

            return nuevosArchivos;
        }

        /// <summary>
        /// Creates on the destination directory the HTML help content files.
        /// </summary>
        /// <param name="destinationDirectory">Directory where to create the content files</param>
        /// <returns>List of the created file names, without directory</returns>
        protected List<string> CreateHelpContentFiles(string destinationDirectory)
        {
            UI.log("Generating help content files", ConsoleUserInterface.INFO);
            return Document.SaveContentFiles(destinationDirectory, Decorator, Indexer);
        }

    }
}
