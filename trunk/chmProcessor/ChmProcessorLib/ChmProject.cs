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
using System.Xml.Serialization;
using System.IO;

namespace ChmProcessorLib
{
	/// <summary>
	/// A chmProcessor project file.
	/// </summary>
    [XmlRoot(ElementName="Configuracion")]  // To keep compatibility with 1.4 version.
	public class ChmProject
	{

        /// <summary>
        /// Current version of the file format.
        /// </summary>
        public const double CURRENTFILEVERSION = 1.5;

        /// <summary>
        /// Ways to generate the PDF file: With PdfCreator, or with the Office 2007 add-in.
        /// </summary>
        public enum PdfGenerationWay { PdfCreator, OfficeAddin }

        /// <summary>
        /// DEPRECATED. Source file name for the help content.
        /// This is deprecated until 1.5 version. Now we can use a list of source files.
        /// Use SourceFiles member instead.
        /// </summary>
        public string ArchivoOrigen;

        /// <summary>
        /// List of source files (MS Word and HTML) to generate the help.
        /// From 1.5 version.
        /// </summary>
        public ArrayList SourceFiles = new ArrayList();

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
        [XmlElement(ElementName = "DirectorioDestino")] // To keep compatibility with 1.4 version.
        public string DestinationProjectDirectory = "";

        /// <summary>
        /// Path to the file with the HTML to put on the header of the CHM html files.
        /// If its equals to "", no header will be put.
        /// </summary>
        [XmlElement(ElementName = "HtmlCabecera")] // To keep compatibility with 1.4 version.
        public string ChmHeaderFile = "";

        /// <summary>
        /// Path to the file with the HTML to put on the footer of the CHM html files.
        /// If its equals to "", no footer will be put.
        /// </summary>
        [XmlElement(ElementName = "HtmlPie")] // To keep compatibility with 1.4 version.
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

        public ArrayList ArchivosAdicionales;
        public bool AbrirProyecto;
        public string DirectorioWeb;
        public int NivelArbolContenidos;
        public int NivelTemasIndice;
        public string ArchivoAyuda;
        public string CommandLine;
        public bool GeneratePdf;
        public string PdfPath;
        public string WebKeywords;
        public string WebDescription;
        public string WebHeaderFile;
        public string WebFooterFile;
        public bool GenerateSitemap;
        public string WebBase;
        public enum FrequencyOfChange { always, hourly, daily, weekly, monthly, yearly, never };
        public FrequencyOfChange ChangeFrequency;
        public string WebLanguage;
        public double ConfigurationVersion;

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
        public string XpsPath;

        /// <summary>
        /// True if we should generate a Java Help jar.
        /// </summary>
        public bool GenerateJavaHelp;

        /// <summary>
        /// If GenerateJavaHelp = true, path where to generate the jar.
        /// </summary>
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
                    string nombreArchivo = Path.GetFileNameWithoutExtension(ArchivoAyuda);
                    directory = Path.GetTempPath() + Path.DirectorySeparatorChar + nombreArchivo + "-project";
                }
                else
                    directory = DestinationProjectDirectory;
                return directory;
            }
        }

		public ChmProject()
		{
            ArchivosAdicionales = new ArrayList();
            ConfigurationVersion = CURRENTFILEVERSION;
            ChangeFrequency = FrequencyOfChange.monthly;
            WebLanguage = "English";
            PdfGeneration = PdfGenerationWay.OfficeAddin;
		}

        public void Guardar( string archivo ) 
        {
            StreamWriter writer = new StreamWriter(archivo);
            XmlSerializer serializador = new XmlSerializer( typeof(ChmProject) );
            serializador.Serialize( writer , this );
            writer.Close();
        }

        /// <summary>
        /// Return true if the source documents are MS Word files.
        /// Return false if the source file list is empty.
        /// </summary>
        public bool WordSourceDocuments
        {
            get
            {
                if (SourceFiles.Count == 0)
                    return false;
                return MSWord.ItIsWordDocument( (string) SourceFiles[0] );
            }
        }

        static public ChmProject Abrir( string archivo ) 
        {
            StreamReader reader = null;
            try 
            {
                reader = new StreamReader( archivo );
                XmlSerializer serializador = new XmlSerializer( typeof(ChmProject) );
                ChmProject cfg = (ChmProject)serializador.Deserialize(reader);
                // Project format upgrade:
                if (cfg.ConfigurationVersion < 1.3)
                {
                    cfg.WebHeaderFile = cfg.ChmHeaderFile;
                    cfg.WebFooterFile = cfg.ChmFooterFile;
                    cfg.ChangeFrequency = FrequencyOfChange.monthly;
                    cfg.GenerateSitemap = false;
                    cfg.WebLanguage = "English";
                    cfg.FullTextSearch = false;
                }
                if (cfg.ConfigurationVersion < 1.4)
                {
                    cfg.PdfGeneration = PdfGenerationWay.PdfCreator;
                    cfg.GenerateXps = false;
                    cfg.XpsPath = "";
                    cfg.GenerateJavaHelp = false;
                    cfg.JavaHelpPath = "";
                }

                if (cfg.ConfigurationVersion < 1.5)
                {
                    cfg.SourceFiles = new ArrayList();
                    cfg.SourceFiles.Add(cfg.ArchivoOrigen);
                }

                cfg.ConfigurationVersion = CURRENTFILEVERSION;
                return cfg;
            }
            finally 
            {
                if( reader != null )
                    reader.Close();
            }
        }

	}
}
