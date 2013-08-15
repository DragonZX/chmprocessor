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

namespace ChmProcessorLib.Generators
{
    /// <summary>
    /// Tool to create a JavaHelp jar file from a document.
    /// TODO: The project directory creation, the topic pages generation and decoration, and the copy
    /// TODO: of additional files should be done here.
    /// </summary>
    public class JavaHelpGenerator : ContentDirectoryGenerator
    {

        /// <summary>
        /// Name for the index file
        /// </summary>
        private const string INDEXFILENAME = "index.xml";

        /// <summary>
        /// Name for the map file
        /// </summary>
        private const string MAPFILENAME = "map.jhm";

        /// <summary>
        /// Name for the table of contents file
        /// </summary>
        private const string TOCFILENAME = "toc.xml";

        /// <summary>
        /// Name for the help set file
        /// </summary>
        private const string HELPSETFILENAME = "help.hs";

        /// <summary>
        /// Directory where all the java help files will be created
        /// </summary>
        public string JavaHelpDirectoryGeneration;

        public JavaHelpGenerator(string mainSourceFile, ChmDocument document, UserInterface ui, 
            ChmProject project, HtmlPageDecorator decorator) 
            : base(document, ui, project, decorator)
        {
            JavaHelpDirectoryGeneration = Path.Combine( Path.GetTempPath() , Path.GetFileNameWithoutExtension(mainSourceFile) )+ 
                "-javahelp";
        }

        public void Generate(List<string> additionalFiles)
        {

            // Create directory, content files and additional files
            CreateDestinationDirectory(JavaHelpDirectoryGeneration, additionalFiles);
            CreateHelpContentFiles(JavaHelpDirectoryGeneration);

            UI.log("Generating java help xml files", ConsoleUserInterface.INFO);
            GenerateJavaHelpSetFile();
            GenerateJavaHelpIndex();
            GenerateJavaHelpMapFile();
            GenerateJavaHelpTOC();

            UI.log("Building the search index", ConsoleUserInterface.INFO);
            UI.log(AppSettings.JavaHelpIndexerPath + " .", ConsoleUserInterface.INFO);
            DocumentProcessor.ExecuteCommandLine(AppSettings.JavaHelpIndexerPath, ".", JavaHelpDirectoryGeneration, UI);

            // Build a JAR with the help.
            //java -jar E:\dev\java\javahelp\javahelp2.0\demos\bin\hsviewer.jar -helpset help.jar
            string commandLine = " cvf \"" + Project.JavaHelpPath + "\" .";
            string jarPath = AppSettings.JarPath;
            UI.log("Building jar:", ConsoleUserInterface.INFO);
            UI.log(jarPath + " " + commandLine, ConsoleUserInterface.INFO);
            DocumentProcessor.ExecuteCommandLine(jarPath, commandLine, JavaHelpDirectoryGeneration, UI);

            // Remove the temporal directory
            Directory.Delete(JavaHelpDirectoryGeneration, true);
        }

        private string FirstTopicTarget
        {
            get
            {
                foreach (ChmDocumentNode node in Document.Index)
                {
                    if (!node.Href.Equals(""))
                        return JavaHelpTarget(node);
                }
                return ChmDocument.DEFAULTTILE;
            }
        }

        /// <summary>
        /// Generates the help.hs help set xml file for java help
        /// </summary>
        private void GenerateJavaHelpSetFile()
        {
            string fileName = Path.Combine(JavaHelpDirectoryGeneration, HELPSETFILENAME);
            // TODO: Translate the labels with web translation files:
            StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8);
            writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            writer.WriteLine("<!DOCTYPE helpset\n" +
                "PUBLIC \"-//Sun Microsystems Inc.//DTD JavaHelp HelpSet Version 2.0//EN\"\n" +
                "\"http://java.sun.com/products/javahelp/helpset_2_0.dtd\">");
            writer.WriteLine("<helpset version=\"2.0\">");
            writer.WriteLine("<title>" + Project.HelpTitle + "</title>");
            writer.WriteLine("<maps><homeID>" + this.FirstTopicTarget + "</homeID><mapref location=\"" + MAPFILENAME + "\"/></maps>");
            writer.WriteLine("<view><name>TOC</name><label>Table Of Contents</label><type>javax.help.TOCView</type><data>" + TOCFILENAME + "</data></view>");
            writer.WriteLine("<view><name>Index</name><label>Index</label><type>javax.help.IndexView</type><data>" + INDEXFILENAME + "</data></view>");
            writer.WriteLine("<view><name>Search</name><label>Search</label><type>javax.help.SearchView</type><data engine=\"com.sun.java.help.search.DefaultSearchEngine\">JavaHelpSearch</data></view>");
            writer.WriteLine("</helpset>");
            writer.Close();
        }

        private string JavaHelpTarget(ChmDocumentNode node)
        {
            return node.HtmlEncodedTitle;
        }

        /// <summary>
        /// Tag for a java help index file of this section.
        /// </summary>
        private string JavaHelpIndexEntry(ChmDocumentNode node)
        {
            return "<indexitem text=\"" + node.HtmlEncodedTitle + "\" target=\"" + JavaHelpTarget(node) + "\" />";
        }

        /// <summary>
        /// Store the xml file for the java help with the index of topics.
        /// </summary>
        private void GenerateJavaHelpIndex()
        {
            string fileName = Path.Combine(JavaHelpDirectoryGeneration, INDEXFILENAME);
            StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8);
            writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            writer.WriteLine("<!DOCTYPE index\n" +
                "PUBLIC \"-//Sun Microsystems Inc.//DTD JavaHelp Index Version 1.0//EN\"\n" +
                "\"http://java.sun.com/products/javahelp/index_2_0.dtd\">");
            writer.WriteLine("<index version=\"2.0\">");
            foreach (ChmDocumentNode node in Document.Index)
            {
                if (!node.Href.Equals(""))
                    writer.WriteLine(/*node.JavaHelpIndexEntry*/ JavaHelpIndexEntry(node) );
            }
            writer.WriteLine("</index>");
            writer.Close();
        }

        /// <summary>
        /// Tag for a java help map file of this section.
        /// </summary>
        public string JavaHelpMapEntry(ChmDocumentNode node)
        {
            return "<mapID target=\"" + JavaHelpTarget(node) + "\" url=\"" + node.Href + "\" />";
        }

        /// <summary>
        /// Generates the map xml file for java help
        /// </summary>
        private void GenerateJavaHelpMapFile()
        {
            string fileName = Path.Combine(JavaHelpDirectoryGeneration, MAPFILENAME);
            StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8);
            writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            writer.WriteLine("<!DOCTYPE map\n" +
                "PUBLIC \"-//Sun Microsystems Inc.//DTD JavaHelp Map Version 1.0//EN\"\n" +
                "\"http://java.sun.com/products/javahelp/map_1_0.dtd\">");
            writer.WriteLine("<map version=\"1.0\">");
            foreach (ChmDocumentNode node in Document.Index)
            {
                if (!node.Href.Equals(""))
                    writer.WriteLine( JavaHelpMapEntry(node) );
            }
            writer.WriteLine("</map>");
            writer.Close();
        }

        /// <summary>
        /// Generate a java help table of contents xml file.
        /// </summary>
        /// <param name="writer">File where to store the TOC</param>
        /// <param name="currentNode">Node to process now</param>
        /// <param name="currentLevel">Current deep level of the node into the document tree</param>
        /// <param name="maxLevelTOC">Maximum deep level into the tree to generate the TOC.</param>
        private void GenerateJavaHelpTOC(StreamWriter writer, ChmDocumentNode currentNode, int currentLevel)
        {
            if (Project.MaxHeaderContentTree != 0 && currentLevel > Project.MaxHeaderContentTree)
                return;

            if (currentNode.HeaderTag != null)
            {
                //writer.WriteLine(currentNode.JavaHelpTOCEntry);
                String entry = "<tocitem text=\"" + currentNode.HtmlEncodedTitle + "\" target=\"" + JavaHelpTarget(currentNode) + "\"";
                if (currentNode.Children.Count == 0)
                    entry += " />";
                else
                    entry += ">";
                writer.WriteLine(entry);
            }

            foreach (ChmDocumentNode child in currentNode.Children)
                GenerateJavaHelpTOC(writer, child, currentLevel + 1);

            if (currentNode.HeaderTag != null && currentNode.Children.Count > 0)
                writer.WriteLine("</tocitem>");
        }

        /// <summary>
        /// Generate a java help table of contents xml file.
        /// </summary>
        private void GenerateJavaHelpTOC()
        {
            string fileName = Path.Combine(JavaHelpDirectoryGeneration, TOCFILENAME);
            StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8);
            writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            writer.WriteLine("<!DOCTYPE toc\n" +
                "PUBLIC \"-//Sun Microsystems Inc.//DTD JavaHelp TOC Version 2.0//EN\"\n" +
                "\"http://java.sun.com/products/javahelp/toc_2_0.dtd\">");
            writer.WriteLine("<toc version=\"2.0\">");
            foreach (ChmDocumentNode child in Document.RootNode.Children)
                GenerateJavaHelpTOC(writer, child, 1);
            writer.WriteLine("</toc>");
            writer.Close();
        }

    }
}
