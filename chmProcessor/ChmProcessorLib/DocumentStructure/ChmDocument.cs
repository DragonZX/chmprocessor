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
using mshtml;
using System.Collections.Generic;
using System.Web;
using System.Text;

namespace ChmProcessorLib.DocumentStructure
{
	/// <summary>
	/// Structured version of a HTML document.
    /// It stores the document itself, and the document sections tree
	/// </summary>
    public class ChmDocument
    {

        /// <summary>
        /// File name where to store the content of the initial part of the document that comes without any
        /// section
        /// </summary>
        public const string INITIALSECTIONFILENAME = "start.htm";

        /// <summary>
        /// Default title for the start of the help content (start of the document out of any section,
        /// entire help content on a single page, etc)
        /// </summary>
        public const string DEFAULTTILE = "Help content";

        /// <summary>
        /// Ultimo nodo insertado en el arbol.
        /// </summary>
        private ChmDocumentNode ultimoInsertado;

        /// <summary>
        /// The original html document
        /// </summary>
        public IHTMLDocument2 IDoc;

        /// <summary>
        /// The root node for the document.
        /// </summary>
        public ChmDocumentNode RootNode;

        /// <summary>
        /// The document index
        /// </summary>
        public ChmDocumentIndex Index;

        /// <summary>
        /// Constructor
        /// TODO: Remove this constructor
        /// </summary>
        public ChmDocument()
        {
            RootNode = new ChmDocumentNode( null , null , null );
            RootNode.HeaderLevel = 0;
        }

        /// <summary>
        /// Constructor
        /// <param name="iDoc">The document to parse</param>
        /// </summary>
        public ChmDocument(IHTMLDocument2 iDoc) : this()
        {
            IDoc = iDoc;
        }

        /// <summary>
        /// Adds a section to the section tree.
        /// The section will be added as child of the last section inserted with a level
        /// higher than then section
        /// </summary>
        /// <param name="nodo">HTML header tag with the title of the section</param>
        /// <param name="ui">Application log. It can be null</param>
        public void InsertarNodo( IHTMLElement node , UserInterface ui ) 
        {
            // Ignorar cabeceras vacias (saltos de linea,etc. ) :
            if( !DocumentProcessor.EsHeader( node ) )
                return;

            int nivel = ChmDocumentNode.HeaderTagLevel( node );
            if( ultimoInsertado == null || nivel == 1 ) 
            {
                ultimoInsertado = new ChmDocumentNode( null, node , ui );
                RootNode.AddChild( ultimoInsertado );
            }
            else 
            {
                ChmDocumentNode nuevoNodo = new ChmDocumentNode( ultimoInsertado, node , ui );
                if( ultimoInsertado.HeaderLevel < nivel )
                    ultimoInsertado.Children.Add( nuevoNodo );
                else 
                {
                    ChmDocumentNode actual = ultimoInsertado.Parent;
                    while( actual != RootNode && actual.HeaderLevel >= nivel )
                        actual = actual.Parent;
                    actual.AddChild( nuevoNodo );
                }
                ultimoInsertado = nuevoNodo;
            }
        }

        /*protected void GenerarArbolDeContenidos( StreamWriter writer , ChmDocumentNode nodo , int NivelMaximoTOC , int nivel ) 
        {
            if( NivelMaximoTOC != 0 && nivel > NivelMaximoTOC )
                return;

            writer.WriteLine( nodo.EntradaArbolContenidos );
            if( nodo.Children.Count > 0 ) 
            {
                writer.WriteLine( "<UL>" );
                foreach( ChmDocumentNode hijo in nodo.Children ) 
                    GenerarArbolDeContenidos( writer , hijo , NivelMaximoTOC , nivel + 1 );
                writer.WriteLine( "</UL>" );
            }
        }*/

        /// <summary>
        /// Saves the table of contents of this tree for a CHM project.
        /// </summary>
        /// <param name="filePath">Path where to store the file.</param>
        /// <param name="MaxTOCLevel">Maximum level of deepth into the tree to save sections. =0 
        /// will save all the sections</param>
        /// <param name="encoding">Encoding to save the file</param>
        /*public void GenerarArbolDeContenidos( string filePath , int MaxTOCLevel , Encoding encoding) 
        {
            StreamWriter writer = new StreamWriter(filePath, false, encoding);
            writer.WriteLine( "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML//EN\">" );
            writer.WriteLine( "<HTML>" );
            writer.WriteLine( "<HEAD>" );
            writer.WriteLine( "<!-- Sitemap 1.0 -->" );
            writer.WriteLine( "</HEAD><BODY>" );
            writer.WriteLine( "<UL>" );
            foreach( ChmDocumentNode hijo in RootNode.Children ) 
                GenerarArbolDeContenidos( writer , hijo , MaxTOCLevel , 1 );
            writer.WriteLine( "</UL>" );
            writer.WriteLine( "</BODY></HTML>" );
            writer.Close();
        }*/

        /// <summary>
        /// Generate a java help table of contents xml file.
        /// </summary>
        /// <param name="writer">File where to store the TOC</param>
        /// <param name="currentNode">Node to process now</param>
        /// <param name="currentLevel">Current deep level of the node into the document tree</param>
        /// <param name="maxLevelTOC">Maximum deep level into the tree to generate the TOC.</param>
        /*public void GenerateJavaHelpTOC(StreamWriter writer, ChmDocumentNode currentNode, int maxLevelTOC, int currentLevel)
        {
            if (maxLevelTOC != 0 && currentLevel > maxLevelTOC)
                return;

            if( currentNode.HeaderTag != null ) 
                writer.WriteLine(currentNode.JavaHelpTOCEntry);
            foreach (ChmDocumentNode child in currentNode.Children)
                GenerateJavaHelpTOC(writer, child, maxLevelTOC, currentLevel + 1);

            if (currentNode.HeaderTag != null && currentNode.Children.Count > 0)
                writer.WriteLine("</tocitem>");
        }*/

        /// <summary>
        /// Generate a java help table of contents xml file.
        /// </summary>
        /// <param name="file">Path of the TOC file to generate.</param>
        /// <param name="maxLevelTOC">Maximum deep level into the tree to generate the TOC.</param>
        /*public void GenerateJavaHelpTOC(string file, int maxLevelTOC)
        {
            StreamWriter writer = new StreamWriter(file, false, Encoding.UTF8);
            writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            writer.WriteLine("<!DOCTYPE toc\n" + 
                "PUBLIC \"-//Sun Microsystems Inc.//DTD JavaHelp TOC Version 2.0//EN\"\n" +
                "\"http://java.sun.com/products/javahelp/toc_2_0.dtd\">");
            writer.WriteLine("<toc version=\"2.0\">");
            foreach (ChmDocumentNode child in RootNode.Children)
                GenerateJavaHelpTOC(writer, child, maxLevelTOC, 1);
            writer.WriteLine("</toc>");
            writer.Close();
        }*/

        // TODO: Move this function to ChmDocumentIndex class
        /*public void GenerarIndice( ChmDocumentIndex index , ChmDocumentNode nodo , int NivelMaximoIndice , int nivel ) 
        {
            if( NivelMaximoIndice != 0 && nivel > NivelMaximoIndice )
                return;

            index.Add( nodo );
            //writer.WriteLine( nodo.EntradaArbolContenidos );
            foreach( ChmDocumentNode hijo in nodo.Children ) 
                GenerarIndice( index , hijo , NivelMaximoIndice , nivel + 1 );
        }*/

        // TODO: Move this function to ChmDocumentIndex class
        /*public ChmDocumentIndex GenerarIndice( int NivelMaximoIndice ) 
        {
            ChmDocumentIndex index = new ChmDocumentIndex();
            foreach( ChmDocumentNode hijo in RootNode.Children ) 
                GenerarIndice( index , hijo , NivelMaximoIndice , 1 );
            return index;
        }*/

        private string GenerarArbolHtml( ChmDocumentNode nodo , int NivelMaximoTOC , int nivel ) 
        {
            if( NivelMaximoTOC != 0 && nivel > NivelMaximoTOC )
                return "";

            string texto = "";
            if( ! nodo.Href.Equals("") ) 
            {
                // Verificar el nodo inicial, que puede no tener titulo:
                string nombre = "";
                if( nodo.HeaderTag != null )
                    nombre = nodo.HeaderTag.innerText;
                else
                    nombre = DEFAULTTILE;
                texto = "<li><a href=\"" + nodo.Href ;
                texto += "\">" + HttpUtility.HtmlEncode( nombre ) + "</a>";
            }

            if( nodo.Children.Count > 0 ) 
            {
                if( NivelMaximoTOC == 0 || nivel < NivelMaximoTOC ) 
                {
                    texto += "\n<ul>\n";
                    foreach( ChmDocumentNode hijo in nodo.Children ) 
                        texto += GenerarArbolHtml( hijo , NivelMaximoTOC , nivel + 1 ) + "\n";
                    texto += "</ul>";
                }
            }
            if( !texto.Equals("") )
                texto += "</li>";
            return texto;
        }

        public string GenerarArbolHtml(int NivelMaximoTOC , string id , string classId ) 
        {
            //string texto = "<ul id=\"contentsTree\" class=\"contentTree\">";
            string texto = "<ul";
            if( id != null )
                texto += " id=\"" + id + "\"";
            if( classId != null )
                texto += " class=\"" + classId + "\"";
            texto += ">\n";

            foreach( ChmDocumentNode hijo in RootNode.Children ) 
                texto += GenerarArbolHtml( hijo , NivelMaximoTOC , 1 ) + "\n";
            texto += "</ul>\n";
            return texto;
        }

        private void ListaArchivosGenerados(List<string> lista, ChmDocumentNode nodo) 
        {
            if( !nodo.DestinationFileName.Equals("") && !lista.Contains(nodo.DestinationFileName) )
                lista.Add( nodo.DestinationFileName);
            foreach( ChmDocumentNode hijo in nodo.Children )
                ListaArchivosGenerados( lista , hijo );
        }

        /// <summary>
        /// Obtiene la lista de archivos HTML que se generaran.
        /// </summary>
        /// <returns>Lista de strings con los nombres de los archivos generados.</returns>
        public List<string> ListaArchivosGenerados() 
        {
            List<string> lista = new List<string>();
            ListaArchivosGenerados( lista , this.RootNode );
            return lista;
        }

        /// <summary>
        /// Searches the first section with a given title. 
        /// The comparation is done without letter case.
        /// </summary>
        /// <param name="sectionTitle">The section title to seach</param>
        /// <returns>The first section of the document with that title. null if no section was
        /// found.</returns>
        public ChmDocumentNode SearchBySectionTitle(string sectionTitle)
        {
            return RootNode.SearchBySectionTitle(sectionTitle);
        }
    }
}
