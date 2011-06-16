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
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Xml;

namespace ChmProcessorLib
{

    /*internal abstract class PathAttribute : Attribute
    {
    }*/

    /// <summary>
    /// Attribute for fields that store directory path.
    /// </summary>
    internal class DirPathAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute for fields that store file path.
    /// </summary>
    internal class FilePathAttribute : Attribute
    {
    }

	/// <summary>
	/// A chmProcessor project file.
	/// </summary>
    [XmlRoot(ElementName="Configuracion")]  // To keep compatibility with 1.4 version.
	public class ChmProject
	{

        /// <summary>
        /// Current version of the file format.
        /// </summary>
        public const double CURRENTFILEVERSION = 1.6;

        /// <summary>
        /// Ways to generate the PDF file: With PdfCreator, or with the Office 2007 add-in.
        /// </summary>
        public enum PdfGenerationWay { PdfCreator, OfficeAddin }

        /// <summary>
        /// Frequency of change of the help web site.
        /// </summary>
        public enum FrequencyOfChange { always, hourly, daily, weekly, monthly, yearly, never };

        /*
        /// <summary>
        /// DEPRECATED. Source file name for the help content.
        /// This is deprecated until 1.5 version. Now we can use a list of source files.
        /// Use SourceFiles member instead.
        /// </summary>
        public string ArchivoOrigen = null;
        */

        /// <summary>
        /// List of source files (MS Word and HTML) to generate the help.
        /// From 1.5 version.
        /// </summary>
        [FilePathAttribute]
        public List<string> SourceFiles = new List<string>();

        /// <summary>
        /// True if we must to generate a compiled CHM file. False if we must generate the 
        /// help project source.
        /// </summary>
        [XmlElement(ElementName = "Compilar")] // To keep compatibility with 1.4 version.
        public bool Compile = true;

        /// <summary>
        /// If "Compile" = true, directory where will be generated the help project directory.
        /// If its false, the project will be generated on a temporal directory
        /// <see cref="HelpProjectDirectory"/>
        /// </summary>
        [XmlElement(ElementName = "DirectorioDestino"), DirPathAttribute] // To keep compatibility with 1.4 version.
        public string DestinationProjectDirectory = "";

        /// <summary>
        /// Path to the file with the HTML to put on the header of the CHM html files.
        /// If its equals to "", no header will be put.
        /// </summary>
        [XmlElement(ElementName = "HtmlCabecera"), FilePathAttribute] // To keep compatibility with 1.4 version.
        public string ChmHeaderFile = "";

        /// <summary>
        /// Path to the file with the HTML to put on the footer of the CHM html files.
        /// If its equals to "", no footer will be put.
        /// </summary>
        [XmlElement(ElementName = "HtmlPie"), FilePathAttribute] // To keep compatibility with 1.4 version.
        public string ChmFooterFile = "";

        /// <summary>
        /// Help main title.
        /// This text will appear as title on CHM, jar and web titles.
        /// </summary>
        [XmlElement(ElementName = "TituloAyuda")] // To keep compatibility with 1.4 version.
        public string HelpTitle = "";

        /// <summary>
        /// Cut header level.
        /// HTML / word header used to split the document. 2 means "Title 2" on Word and H2 tag
        /// on HTML. A zero value means that the document will not be splitted.
        /// </summary>
        [XmlElement(ElementName = "NivelCorte")] // To keep compatibility with 1.4 version.
        public int CutLevel;

        /// <summary>
        /// Should we generate a web site for the help?
        /// </summary>
        [XmlElement(ElementName = "GenerarWeb")] // To keep compatibility with 1.4 version.
        public bool GenerateWeb;

        /// <summary>
        /// Path list to additional files and directories that must to be included on the help file.
        /// (tonib: ArchivosAdicionales can contain files AND directories, so [FilePathAttribute] does not apply.
        /// This is assuming that only contains files, but its working...
        /// So I left it like this.)
        /// </summary>
        [FilePathAttribute]
        public List<string> ArchivosAdicionales = new List<string>();

        /// <summary>
        /// Only applies if Compile = true.
        /// If OpenProject is true, after the generation, the help project will be 
        /// opened through Windows shell.
        /// </summary>
        [XmlElement(ElementName = "AbrirProyecto")] // To keep compatibility with 1.4 version.
        public bool OpenProject;

        /// <summary>
        /// Only applies if GenerateWeb = true.
        /// The directory where the web site will be generated.
        /// </summary>
        [XmlElement(ElementName = "DirectorioWeb"), DirPathAttribute] // To keep compatibility with 1.4 version.
        public string WebDirectory = "";

        /// <summary>
        /// Maximum header level that will be included on the content tree of the help.
        /// =0 means all headers will be included.
        /// </summary>
        [XmlElement(ElementName = "NivelArbolContenidos")] // To keep compatibility with 1.4 version.
        public int MaxHeaderContentTree;

        /// <summary>
        /// Maximum header level that will be included on the index of the help.
        /// =0 means all headers will be included.
        /// </summary>
        [XmlElement(ElementName = "NivelTemasIndice")] // To keep compatibility with 1.4 version.
        public int MaxHeaderIndex;

        /// <summary>
        /// Only applies if Compile = true.
        /// Path to the CHM help file that will be generated.
        /// </summary>
        [XmlElement(ElementName = "ArchivoAyuda"), FilePathAttribute] // To keep compatibility with 1.4 version.
        public string HelpFile;

        /// <summary>
        /// Command line that will be executed after the help generation.
        /// If is null or empty anything will be executed.
        /// </summary>
        public string CommandLine = "";

        /// <summary>
        /// If true, a PDF file will be generated with the help content.
        /// </summary>
        public bool GeneratePdf;

        /// <summary>
        /// Only applies if GeneratePdf = true.
        /// Path where will be generated the PDF file.
        /// </summary>
        [FilePathAttribute]
        public string PdfPath = "";

        /// <summary>
        /// Only applies if GenerateWeb = true.
        /// Meta tag "keywords" value to put on generated web pages.
        /// </summary>
        public string WebKeywords = "";

        /// <summary>
        /// Only applies if GenerateWeb = true.
        /// Meta tag "description" value to put on generated web pages.
        /// </summary>
        public string WebDescription = "";

        /// <summary>
        /// Only applies if GenerateWeb = true.
        /// Path to HTML file with the content to put on header of each generated
        /// web page of the help.
        /// If its equals to "", no header will be put.
        /// </summary>
        [FilePathAttribute]
        public string WebHeaderFile = "";

        /// <summary>
        /// Only applies if GenerateWeb = true.
        /// Path to HTML file with the content to put on footer of each generated
        /// web page of the help.
        /// If its equals to "", no footer will be put.
        /// </summary>
        [FilePathAttribute]
        public string WebFooterFile = "";

        /// <summary>
        /// Only applies if GenerateWeb = true.
        /// If true a sitemap for google will be generated.
        /// </summary>
        public bool GenerateSitemap;

        /// <summary>
        /// Only applies if GenerateWeb = true and GenerateSitemap = true.
        /// URL base for the help web site.
        /// </summary>
        public string WebBase;

        /// <summary>
        /// Only applies if GenerateWeb = true and GenerateSitemap = true.
        /// Frequency of change of the help web site.
        /// </summary>
        public FrequencyOfChange ChangeFrequency;

        /// <summary>
        /// Only applies if GenerateWeb = true.
        /// Name of the language used on the help. Must to be equal to the name
        /// of a file on the webTranslations directory.
        /// </summary>
        public string WebLanguage;

        /// <summary>
        /// Current version of this help project.
        /// </summary>
        public double ConfigurationVersion;

        /// <summary>
        /// Only applies if GenerateWeb = true.
        /// If true a ASP.NET application will be generated on the help web site
        /// to make full text searches.
        /// </summary>
        public bool FullTextSearch;

        /// <summary>
        /// Ways to generate the PDF file: With PdfCreator, or with the Office 2007 add-in.
        /// </summary>
        public PdfGenerationWay PdfGeneration = PdfGenerationWay.OfficeAddin;

        /// <summary>
        /// True if we should generate a XPS file with the document.
        /// </summary>
        public bool GenerateXps;

        /// <summary>
        /// Absolute path of the xps file to generate, if GenerateXps = true.
        /// </summary>
        [FilePathAttribute]
        public string XpsPath;

        /// <summary>
        /// True if we should generate a Java Help jar.
        /// </summary>
        public bool GenerateJavaHelp;

        /// <summary>
        /// If GenerateJavaHelp = true, path where to generate the jar.
        /// </summary>
        [FilePathAttribute]
        public string JavaHelpPath;

        /// <summary>
        /// The directory where will be generated the help project.
        /// </summary>
        public string HelpProjectDirectory
        {
            get
            {
                string directory;
                if (Compile)
                {
                    // Help project will be generated on a temporal directory.
                    string nombreArchivo = Path.GetFileNameWithoutExtension(HelpFile);
                    directory = Path.GetTempPath() + Path.DirectorySeparatorChar + nombreArchivo + "-project";
                }
                else
                    directory = DestinationProjectDirectory;
                return directory;
            }
        }

		public ChmProject()
		{
            ArchivosAdicionales = new List<string>();
            ConfigurationVersion = CURRENTFILEVERSION;
            ChangeFrequency = FrequencyOfChange.monthly;
            WebLanguage = "English";
            PdfGeneration = PdfGenerationWay.OfficeAddin;
		}

        /// <summary>
        /// Stores the project as an xml file.
        /// </summary>
        /// <param name="archivo">Path where to store the xml file. 
        /// By convention, file extension should be ".whc"</param>
        public void Save( string filePath ) 
        {
            if( AppSettings.SaveRelativePaths )
                MakePathsRelative(filePath);
            StreamWriter writer = new StreamWriter(filePath);
            XmlSerializer serializador = new XmlSerializer( typeof(ChmProject) );
            serializador.Serialize( writer , this );
            writer.Close();
            if ( AppSettings.SaveRelativePaths )
                MakePathsAbsolute(filePath);
        }

        /// <summary>
        /// Verify if a list of source files can be added to the current source files list.
        /// Currently we cannot mix HTML and Word documents as source files, and only one
        /// HTML document can be source file. Multiple Word documents can be defined as 
        /// source documents.
        /// </summary>
        /// <param name="currentSourceFiles">Current list of source files</param>
        /// <param name="newSourceFiles">New files to add to the source files</param>
        /// <returns>A string with the error message if the new source files cannot be
        /// added to the source files list. null if the new source files can be added.</returns>
        static public string CanBeAddedToSourceFiles(ArrayList currentSourceFiles, ArrayList newSourceFiles)
        {
            bool currentListEmpty = currentSourceFiles.Count == 0;
            bool currentListIsHtml = false;
            if (!currentListEmpty)
                currentListIsHtml = MSWord.IsHtmlDocument((string)currentSourceFiles[0]);
            foreach (String file in newSourceFiles)
            {
                bool fileIsHtml = MSWord.IsHtmlDocument(file);

                if (currentListEmpty)
                {
                    currentListEmpty = false;
                    currentListIsHtml = fileIsHtml;
                }
                else
                {
                    if ((currentListIsHtml && !fileIsHtml) || (!currentListIsHtml && fileIsHtml))
                        return "HTML and Word documents cannot be mixed as source documents";
                    if (fileIsHtml)
                        return "Only one HTML document can be used as source document";
                }
            }
            return null;
        }

        /// <summary>
        /// Open a xml file with a ChmProject object serialized.
        /// </summary>
        /// <param name="archivo">Path to xml file to read.</param>
        /// <returns>The ChmProject readed.</returns>
        static public ChmProject Open( string filePath ) 
        {

            // Load and upgrade the xml project document 
            ChmProjectXml xml = new ChmProjectXml();
            xml.Load(filePath);
            xml.UpgradeXml();
            ChmProject cfg = xml.Deserialize();

            // Change relative paths to absolute:
            //if (AppSettings.SaveRelativePaths) < do it always
                cfg.MakePathsAbsolute(filePath);

            return cfg;
        }

        /// <summary>
        /// Change any directory or file relative path stored at the project to make it absolute, 
        /// using the file project file path as the reference.
        /// Author is Jozsef Bekes
        /// </summary>
        /// <param name="filePath">Path to the project file</param>
        private void MakePathsAbsolute(string filePath)
        {
            // Calculate absolute paths
            // Set current dir to the project file dir
            System.IO.Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(filePath));

            // go through all fields which are marked with PathAttribute
            FieldInfo[] fields = this.GetType().GetFields();
            foreach (FieldInfo fi in fields)
            {
                if (fi.GetCustomAttributes(typeof(DirPathAttribute), false).Length > 0 ||
                    fi.GetCustomAttributes(typeof(FilePathAttribute), false).Length > 0)
                {
                    if (fi.GetValue(this) is String)
                    {
                        object valueInFile = fi.GetValue(this);
                        if (valueInFile != null && valueInFile.ToString().Length > 0)
                        {
                            fi.SetValue(this, System.IO.Path.GetFullPath(valueInFile.ToString()));
                        }
                    }
                    else if (fi.GetValue(this) is List<string>)
                    {
                        List<string> entries = (List<string>)fi.GetValue(this);
                        for (int idx = 0; idx < entries.Count; ++idx)
                        {
                            entries[idx] = System.IO.Path.GetFullPath(entries[idx]);
                        }
                        //fi.SetValue(this, System.IO.Path.GetFullPath(fi.GetValue(this).ToString()));
                    }
                    else
                    {
                        Debug.Assert(false, "PathAttribute should only be applied to a field that is of string or List<string> type");
                    }
                }
            }
        }
        
        /// <summary>
        /// Change any directory or file path stored at the project to make it relative to the 
        /// path of the project file.
        /// Author is Jozsef Bekes.
        /// </summary>
        /// <param name="filePath">Path to the project fil.</param>
        private void MakePathsRelative(string filePath)
        {
            // Calculate absolute paths
            // Set current dir to the project file dir
            System.IO.Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(filePath));

            // go through all fields which are marked with PathAttribute
            FieldInfo[] fields = this.GetType().GetFields();
            foreach (FieldInfo fi in fields)
            {
                if (fi.GetCustomAttributes(typeof(DirPathAttribute), false).Length > 0 ||
                    fi.GetCustomAttributes(typeof(FilePathAttribute), false).Length > 0)
                {
                    FileSystem.FSObjType tgtType = FileSystem.FSObjType.eAuto;
                    if (fi.GetCustomAttributes(typeof(FilePathAttribute), false).Length > 0)
                    {
                        tgtType = FileSystem.FSObjType.eFile;
                    }
                    else if (fi.GetCustomAttributes(typeof(DirPathAttribute), false).Length > 0)
                    {
                        tgtType = FileSystem.FSObjType.eDir;
                    }
                    else
                    {
                        Debug.Assert(false, "unexpected that this branch executes");
                    }


                    if (fi.GetValue(this) is String)
                    {
                        object valueInFile = fi.GetValue(this);
                        if (valueInFile != null && valueInFile.ToString().Length > 0)
                        {
                            try
                            {
                                fi.SetValue(this, FileSystem.GetRelativePath(FileSystem.FSObjType.eFile,
                                                                       filePath,
                                                                       tgtType,
                                                                       valueInFile.ToString()));
                            }
                            catch
                            {
                                // If files dont have a common root, keep the absolute path.
                            }
                        }
                    }
                    else if (fi.GetValue(this) is List<string>)
                    {
                        List<string> entries = (List<string>)fi.GetValue(this);
                        for (int idx = 0; idx < entries.Count; ++idx)
                        {
                            try
                            {
                                entries[idx] = FileSystem.GetRelativePath(FileSystem.FSObjType.eFile, filePath,
                                                                     tgtType, entries[idx]);
                            }
                            catch
                            {
                                // If files dont have a common root, keep the absolute path.
                            }
                        }
                        //fi.SetValue(this, System.IO.Path.GetFullPath(fi.GetValue(this).ToString()));
                    }
                    else
                    {
                        Debug.Assert(false, "PathAttribute should only be applied to a field that is of string or List<string> type");
                    }
                }
            }
        }

	}
}
