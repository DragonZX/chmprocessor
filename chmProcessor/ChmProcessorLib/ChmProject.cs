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
using System.Globalization;
using System.Text;
using ChmProcessorLib.Generators;
using ChmProcessorLib.Log;

namespace ChmProcessorLib
{

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
        /// Name for the WebTemplateName property to use a custom web template
        /// </summary>
        public const string WEBTEMPLATE_CUSTOM = "Custom";

        /// <summary>
        /// Default WebTemplateName property value.
        /// </summary>
        public const string WEBTEMPLATE_DEFAULT = "jQuery";

        /// <summary>
        /// Current version of the file format.
        /// </summary>
        public const double CURRENTFILEVERSION = 1.7;

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
        /// Only applies if Compile = false.
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
        /// File with content to include into the "head" tag of the web (not at chm) html pages.
        /// Usefull to include, as example, the google analytics javascript code.
        /// </summary>
        [FilePathAttribute]
        public String HeadTagFile = string.Empty;

        /// <summary>
        /// Locale identifier (LCID) to use to generate the CHM files.
        /// </summary>
        public int ChmLocaleID = CultureInfo.CurrentCulture.LCID;

        /// <summary>
        /// Name of the web template to use to create the webhelp. 
        /// It's the subdirectory name into "WebFiles" application directory to use for the webhelp.
        /// If WebTemplateName == WEBTEMPLATE_CUSTOM, the property CustomTemplateDirectory contains
        /// a path of the directory with the custom template to use.
        /// </summary>
        public string WebTemplateName = WEBTEMPLATE_DEFAULT;

        /// <summary>
        /// If WebTemplateName == WEBTEMPLATE_CUSTOM, this contains the directory with the webhelp
        /// template to use for this project.
        /// </summary>
        [DirPathAttribute]
        public string CustomTemplateDirectory = string.Empty;

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

        /// <summary>
        /// Get the absolute webhelp template directory for this project. It's based on WebTemplateName
        /// and CustomTemplateDirectory property values
        /// </summary>
        public string TemplateDirectory
        {
            get
            {
                return WebHelpGenerator.GetTemplateDirectory(WebTemplateName, CustomTemplateDirectory);
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
        /// Throws an exception if the source file cannot be added to the source files list.
        /// </summary>
        /// <param name="sourceFilePath">Source file to check</param>
        private void CheckAddSourceFile(string sourceFilePath)
        {
            if (SourceFiles.Count == 0)
                return;

            bool currentListIsHtml = MSWord.IsHtmlDocument(SourceFiles[0]);
            bool fileIsHtml = MSWord.IsHtmlDocument(sourceFilePath);

            if ((currentListIsHtml && !fileIsHtml) || (!currentListIsHtml && fileIsHtml))
                throw new Exception("HTML and Word documents cannot be mixed as source documents");
            if (fileIsHtml)
                throw new Exception("Only one HTML document can be used as source document");
        }

        ///// <summary>
        ///// Verify if a list of source files can be added to the current source files list.
        ///// Currently we cannot mix HTML and Word documents as source files, and only one
        ///// HTML document can be source file. Multiple Word documents can be defined as 
        ///// source documents.
        ///// </summary>
        ///// <param name="currentSourceFiles">Current list of source files</param>
        ///// <param name="newSourceFiles">New files to add to the source files</param>
        ///// <returns>A string with the error message if the new source files cannot be
        ///// added to the source files list. null if the new source files can be added.</returns>
        //static public string CanBeAddedToSourceFiles(List<string> currentSourceFiles, List<string> newSourceFiles)
        //{
        //    bool currentListEmpty = currentSourceFiles.Count == 0;
        //    bool currentListIsHtml = false;
        //    if (!currentListEmpty)
        //        currentListIsHtml = MSWord.IsHtmlDocument((string)currentSourceFiles[0]);
        //    foreach (String file in newSourceFiles)
        //    {
        //        bool fileIsHtml = MSWord.IsHtmlDocument(file);

        //        if (currentListEmpty)
        //        {
        //            currentListEmpty = false;
        //            currentListIsHtml = fileIsHtml;
        //        }
        //        else
        //        {
        //            if ((currentListIsHtml && !fileIsHtml) || (!currentListIsHtml && fileIsHtml))
        //                return "HTML and Word documents cannot be mixed as source documents";
        //            if (fileIsHtml)
        //                return "Only one HTML document can be used as source document";
        //        }
        //    }
        //    return null;
        //}

        /// <summary>
        /// Open a xml file with a ChmProject object serialized.
        /// </summary>
        /// <param name="filePath">Path to xml file to read.</param>
        /// <returns>The ChmProject readed.</returns>
        static public ChmProject Open( string filePath ) 
        {

            // Load and upgrade the xml project document 
            ChmProjectXml xml = new ChmProjectXml();
            xml.Load(filePath);
            xml.UpgradeXml();
            ChmProject cfg = xml.Deserialize();

            // Change relative paths to absolute:
            cfg.MakePathsAbsolute(filePath);

            return cfg;
        }

        /// <summary>
        /// Creates a default project for a HTML / Word file
        /// </summary>
        /// <param name="filePath">Path to the HTML / Word file</param>
        /// <returns>The default project</returns>
        static public ChmProject CreateProjectforHtmlWordFile(string filePath)
        {
            ChmProject defaultProject = new ChmProject();
            defaultProject.AddFileAndProposePaths(filePath);
            defaultProject.MakePathsAbsolute(filePath);
            return defaultProject;
        }

        /// <summary>
        /// Add a file to the source files list and, if its the first file, propose
        /// the default paths for the generated files 
        /// </summary>
        /// <param name="filePath"></param>
        public void AddFileAndProposePaths(string filePath)
        {
            try
            {
                // Convert relative paths to absolute paths:
                filePath = Path.GetFullPath(filePath);
            }
            catch { }

            CheckAddSourceFile(filePath);

            SourceFiles.Add(filePath);

            string firstFile = SourceFiles[0];
            string basePath = Path.GetDirectoryName(firstFile);
            basePath = Path.Combine(basePath, Path.GetFileNameWithoutExtension(firstFile));

            if (string.IsNullOrEmpty(HelpFile))
                HelpFile = basePath + ".chm";

            if (string.IsNullOrEmpty(DestinationProjectDirectory))
                DestinationProjectDirectory = basePath + "-project";

            if (string.IsNullOrEmpty(WebDirectory))
                WebDirectory = basePath + "-web";

            if (string.IsNullOrEmpty(WebDirectory))
                WebDirectory = basePath + ".pdf";

            if (string.IsNullOrEmpty(PdfPath))
                PdfPath = basePath + ".pdf";

            if (string.IsNullOrEmpty(XpsPath))
                XpsPath = basePath + ".xps";

            if (string.IsNullOrEmpty(JavaHelpPath))
                JavaHelpPath = basePath + ".jar";

        }

        /// <summary>
        /// Check if a file is a ChmProject file.
        /// </summary>
        /// <param name="filePath">File path to check</param>
        /// <returns>True if the file is a ChmProject file (i.e, if its extension is "whc"</returns>
        static public bool IsChmProjectFile(string filePath)
        {
            string fileExtension = Path.GetExtension(filePath).ToLower();
            return fileExtension == ".whc";
        }

        /// <summary>
        /// Open a xml file with a ChmProject object serialized OR create a default
        /// ChmProject for a Word/HTML file.
        /// </summary>
        /// <param name="filePath">Path to the ChmProject file or to the Word/Html file</param>
        /// <returns>The ChmProject readed or the default project for the Word/Html file</returns>
        static public ChmProject OpenChmProjectOrWord(string filePath)
        {
            if (IsChmProjectFile(filePath))
                // It's a CHMProcessor file
                return Open(filePath);
            else if (MSWord.IsHtmlDocument(filePath) || MSWord.IsWordDocument(filePath))
                // A word/html file
                return CreateProjectforHtmlWordFile(filePath);
            else
                throw new Exception("Unknown file extension " + Path.GetExtension(filePath));
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
            string directory = Path.GetDirectoryName(filePath);
            if( directory != "" )
                Directory.SetCurrentDirectory(directory);

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
                            try
                            {
                                fi.SetValue(this, Path.GetFullPath(valueInFile.ToString()));
                            }
                            catch
                            {
                                // If the path is invalid, keep it as it is.
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
                                entries[idx] = Path.GetFullPath(entries[idx]);
                            }
                            catch
                            {
                                // If the path is invalid, keep it as it is.
                            }
                        }
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
        /// <param name="filePath">Path to the project file.</param>
        private void MakePathsRelative(string filePath)
        {
            // Calculate absolute paths

            // Set current dir to the project file dir
            string directory = Path.GetDirectoryName(filePath);
            if( directory != "" )
                Directory.SetCurrentDirectory(directory);

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

        /// <summary>
        /// Returns the selected culture for the CHM generation
        /// </summary>
        /// <param name="ui">The user interface</param>
        /// <returns>The CHM culture</returns>
        public CultureInfo GetChmCulture(UserInterface ui)
        {
            // Get the encoding and culture for the chm:
            try
            {
                return CultureInfo.GetCultureInfo(ChmLocaleID);
            }
            catch (Exception ex)
            {
                ui.Log(ex);
                throw new Exception("The locale ID (LCID) " + ChmLocaleID + " was not found.", ex);
            }
        }

        /// <summary>
        /// Returns the selected encoding for the CHM generation
        /// </summary>
        /// <param name="ui">The user interface</param>
        /// <param name="chmCulture">The CHM culture <see cref="GetChmCulture"/></param>
        /// <returns>The CHM encoding</returns>
        static public Encoding GetChmEncoding(UserInterface ui, CultureInfo chmCulture) 
        {
            try
            {
                return Encoding.GetEncoding(chmCulture.TextInfo.ANSICodePage);
            }
            catch (Exception ex)
            {
                ui.Log(ex);
                throw new Exception("The ANSI codepage " + chmCulture.TextInfo.ANSICodePage + " was not found.", ex);
            }
        }

	}
}
