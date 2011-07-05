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
using System.IO;
using System.Text;

namespace ChmProcessorLib
{
	/// <summary>
	/// Topics index of the help
	/// </summary>
	public class Index
	{

        ArrayList entriesList;

        private class Entry 
        {
        }

		public Index()
		{
            entriesList = new ArrayList();
		}

        public void AddNode( NodoArbol node ) 
        {
            entriesList.Add( node );
        }

        /// <summary>
        /// Generate a HTML select tag, with the topics index of the help.
        /// </summary>
        /// <returns>The select tag with the topics.</returns>
        public string GenerateWebIndex() 
        {
            entriesList.Sort();
            string index = "<select id=\"topicsList\" style=\"width:100%;\" size=\"20\" onclick=\"topicOnClick();\" ondblclick=\"topicSelected();\" >\n";
            foreach (NodoArbol node in entriesList)
            {
                string href = node.Href;
                if( !href.Equals("") )
                    index += "<option value=\"" + node.Href + "\">" + node.EncodedName + "</option>\n";
            }
            index += "</select>\n";
            return index;
        }

        /// <summary>
        /// Store the HHK file for the help project with the topics index.
        /// </summary>
        /// <param name="fileName">Path where to save the file</param>
        /// <param name="encoding">Encoding used to write the file</param>
        public void StoreHelpIndex( string fileName , Encoding encoding) 
        {
            StreamWriter writer = new StreamWriter( fileName , false , encoding );
            writer.WriteLine( "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML//EN\">" );
            writer.WriteLine( "<HTML>" );
            writer.WriteLine( "<HEAD>" );
            //writer.WriteLine("<HEAD>");
            writer.WriteLine( "<!-- Sitemap 1.0 -->" );
            writer.WriteLine( "</HEAD><BODY>" );
            writer.WriteLine( "<UL>" );
            foreach (NodoArbol node in entriesList)
            {
                if (!node.Href.Equals(""))
                    writer.WriteLine(node.EntradaArbolContenidos);
            }
            writer.WriteLine( "</UL>" );
            writer.WriteLine( "</BODY></HTML>" );
            writer.Close();
        }

        /// <summary>
        /// Store the xml file for the java help with the index of topics.
        /// </summary>
        /// <param name="fileName">Path of the java help index file name</param>
        public void GenerateJavaHelpIndex(string fileName)
        {
            StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8 );
            writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            writer.WriteLine("<!DOCTYPE index\n" + 
                "PUBLIC \"-//Sun Microsystems Inc.//DTD JavaHelp Index Version 1.0//EN\"\n" +
                "\"http://java.sun.com/products/javahelp/index_2_0.dtd\">");
            writer.WriteLine("<index version=\"2.0\">");
            foreach (NodoArbol node in entriesList)
            {
                if (!node.Href.Equals(""))
                    writer.WriteLine(node.JavaHelpIndexEntry);
            }
            writer.WriteLine("</index>");
            writer.Close();
        }

        /// <summary>
        /// Generates the map xml file for java help
        /// </summary>
        /// <param name="dirJavaHelp">Path of the java help map file name</param>
        public void GenerateJavaHelpMapFile(String fileName)
        {
            StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8);
            writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            writer.WriteLine("<!DOCTYPE map\n" +
                "PUBLIC \"-//Sun Microsystems Inc.//DTD JavaHelp Map Version 1.0//EN\"\n" +
                "\"http://java.sun.com/products/javahelp/map_1_0.dtd\">");
            writer.WriteLine("<map version=\"1.0\">");
            foreach (NodoArbol node in entriesList)
            {
                if (!node.Href.Equals(""))
                    writer.WriteLine(node.JavaHelpMapEntry);
            }
            writer.WriteLine("</map>");
            writer.Close();
        }

        /// <summary>
        /// Name of the java help target for the first section on the index.
        /// </summary>
        public string FirstTopicTarget
        {
            get
            {
                foreach (NodoArbol node in entriesList)
                {
                    if (!node.Href.Equals("")) 
                        return node.JavaHelpTarget;
                }
                return "Inicio";
            }
        }

    }
}
