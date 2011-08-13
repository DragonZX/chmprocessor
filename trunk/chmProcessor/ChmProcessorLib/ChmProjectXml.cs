using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ChmProcessorLib 
{
    /// <summary>
    /// Class to load and upgrade the XML from a ChmProcessor project file.
    /// </summary>
    class ChmProjectXml : XmlDocument
    {

        /// <summary>
        /// Deserializes this xml document and convert it to a ChmProject object.
        /// </summary>
        /// <returns>The ChmProject object readed from the xml.</returns>
        public ChmProject Deserialize()
        {
            XmlReader reader = new XmlNodeReader(this);
            XmlSerializer serializer = new XmlSerializer(typeof(ChmProject));
            ChmProject cfg = (ChmProject)serializer.Deserialize(reader);
            reader.Close();
            return cfg;
        }

        /// <summary>
        /// Creates and adds a node to a node parent
        /// </summary>
        /// <param name="parent">Parent where to add the node</param>
        /// <param name="name">Name of the new node</param>
        /// <param name="value">Text contents of the new node</param>
        /// <returns>The node added</returns>
        private XmlElement CreateXmlNode(XmlNode parent, string name, string value)
        {
            XmlElement newNode = CreateElement(name);
            newNode.InnerText = value;
            parent.AppendChild(newNode);
            return newNode;
        }

        /// <summary>
        /// Convert a xml node used to serialize an ArrayList object to other that serializes a
        /// List&lt;String&gt; object
        /// </summary>
        /// <param name="root">Parent of the node to convert</param>
        /// <param name="nodeName">Name of the node to convert</param>
        private void ConvertArrayListToStringList(XmlNode root, string nodeName)
        {
            XmlElement oldListNode = root[nodeName];
            XmlElement newListNode = CreateElement(nodeName);
            foreach (XmlElement oldNode in oldListNode.ChildNodes)
                CreateXmlNode(newListNode, "string", oldNode.InnerText);

            root.RemoveChild(oldListNode);
            root.AppendChild(newListNode);
        }

        /// <summary>
        /// Checks the version of the readed project xml, and make changes to upgrade it to
        /// the current version.
        /// </summary>
        public void UpgradeXml()
        {
            XmlNode root = this["Configuracion"];
            if (root == null)
                throw new ArgumentException("The file is not a ChmProcessor project (Configuracion not found)");

            XmlElement versionNode = root["ConfigurationVersion"];
            if (versionNode == null)
                // Should not happen...
                throw new ArgumentException("The file has an unknown format");

            double version = XmlConvert.ToDouble(versionNode.InnerText);
            if (version < 1.3)
            {
                /*cfg.WebHeaderFile = cfg.ChmHeaderFile;
                cfg.WebFooterFile = cfg.ChmFooterFile;
                cfg.ChangeFrequency = FrequencyOfChange.monthly;
                cfg.GenerateSitemap = false;
                cfg.WebLanguage = "English";
                cfg.FullTextSearch = false;*/
                CreateXmlNode(root, "WebHeaderFile", this["ChmHeaderFile"].InnerText);
                CreateXmlNode(root, "WebFooterFile", this["ChmFooterFile"].InnerText);
                CreateXmlNode(root, "ChangeFrequency", "monthly");
                CreateXmlNode(root, "GenerateSitemap", XmlConvert.ToString(false));
                CreateXmlNode(root, "WebLanguage", "English");
                CreateXmlNode(root, "FullTextSearch", XmlConvert.ToString(false));
            }

            if (version < 1.4)
            {
                /*cfg.PdfGeneration = PdfGenerationWay.PdfCreator;
                cfg.GenerateXps = false;
                cfg.XpsPath = "";
                cfg.GenerateJavaHelp = false;
                cfg.JavaHelpPath = "";*/
                CreateXmlNode(root, "PdfGeneration", "PdfCreator");
                CreateXmlNode(root, "GenerateXps", XmlConvert.ToString(false));
                CreateXmlNode(root, "XpsPath", "");
                CreateXmlNode(root, "GenerateJavaHelp", XmlConvert.ToString(false));
                CreateXmlNode(root, "JavaHelpPath", "");
            }

            if (version < 1.5)
            {
                /*cfg.SourceFiles = new ArrayList();
                cfg.SourceFiles.Add(cfg.ArchivoOrigen);*/
                XmlElement singleSourceFileNode = root["ArchivoOrigen"];
                string sourceFile = singleSourceFileNode.InnerText;
                root.RemoveChild(singleSourceFileNode);

                XmlElement sourceFiles = CreateElement("SourceFiles");
                root.AppendChild(sourceFiles);
                XmlElement newSourceFileNode = CreateXmlNode(sourceFiles, "anyType", sourceFile);
                newSourceFileNode.SetAttribute("xsi:type", "xsd:string");
            }

            if (version < 1.6)
            {
                //SourceFiles and ArchivosAdicionales has changed type from ArrayList to List<String>
                //So, previous source files were serialized as <anyType xsi:type="xsd:string">c:\file path</anyType>
                //and now are <string>c:\file path</string>
                ConvertArrayListToStringList(root, "SourceFiles");
                ConvertArrayListToStringList(root, "ArchivosAdicionales");

                // Code to include into the <head> tag of the web html files.
                CreateXmlNode(root, "HeadTagFile", "");
            }

            // Set the current version:
            versionNode.InnerText = XmlConvert.ToString(ChmProject.CURRENTFILEVERSION);

        }
    }
}
