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
using System.IO;
using mshtml;
using System.Web;
using WebIndexLib;

namespace ChmProcessorLib.DocumentStructure
{
    /// <summary>
    /// The referente to a section (a H1, H2, etc tag) of the HTML document.
    /// </summary>
    public class ChmDocumentNode : IComparable
    {
        /// <summary>
        /// Header tag (h1,h2...) for this node.
        /// It will be null for the root node AND the initial part of the document without any title.
        /// TODO: This member should be private
        /// </summary>
        public IHTMLElement HeaderTag;

        /// <summary>
        /// Name, without the directory, of the html file name of where this node will be stored, 
        /// after the document will be splitted.
        /// Until the document is not splitted, it will be null.
        /// </summary>
        public string DestinationFileName;

        /// <summary>
        /// Children nodes of this node (subsections)
        /// </summary>
        public List<ChmDocumentNode> Children;

        /// <summary>
        /// The parent node of this. It will be null for the root node.
        /// </summary>
        public ChmDocumentNode Parent;

        /// <summary>
        /// The node header level (H1 -> 1 , H2 -> 2, etc ).
        /// Will be zero for the root
        /// </summary>
        public int HeaderLevel;

        /// <summary>
        /// Anchor names (a name="thisisthename" tags) that can be used to reference this node.
        /// </summary>
        private List<string> AnchorNames;

        /// <summary>
        /// Used when we need to create a custom anchor for a node. It happens when the node
        /// does not have any anchor on the HTML document.
        /// In this case we create a custom anchor with name "NODOxxxx" where xxxx is a number.
        /// This number is get from this counter.
        /// </summary>
        private static int LatestCustomAnchorNumber = 0;

        /// <summary>
        /// The main anchor name (a name="mainname" tag) for this node.
        /// Its the first name of the AnchorNames list. 
        /// It will be an empty string for the root node.
        /// </summary>
        public string MainAnchorName 
        {
            get 
            {
                if( AnchorNames.Count > 0 )
                    return AnchorNames[0];
                return string.Empty;
            }
        }

        /// <summary>
        /// If this node will be the root for a splitted part of the original HTML document,
        /// this is the HTML body tag for this part. Otherwise, it will be null.
        /// </summary>
        public IHTMLElement SplittedPartBody;

        /// <summary>
        /// If SplittedPartBody is not null, this is the list with all the anchor names contained into the body of 
        /// this node. Those names are the "name" property
        /// of the tag A (a name="foo"). They are used to change internal links into the document.
        /// If SplittedPartBody is null, this will be null too.
        /// </summary>
        private List<string> DescendantAnchorNames;

        /// <summary>
        /// Builds the member listOfContainedANames, with all the A name tags contained into the body member.
        /// TODO: Probably this should be private
        /// </summary>
        public void BuildListOfContainedANames()
        {
            if (DescendantAnchorNames == null)
                DescendantAnchorNames = new List<string>();
            else
                DescendantAnchorNames.Clear();

            if (SplittedPartBody != null)
                BuildListOfContainedANames(SplittedPartBody);
        }

        /// <summary>
        /// Builds the member listOfContainedANames, with all the A name tags contained into the body member.
        /// It does a recursive search for A tags.
        /// </summary>
        private void BuildListOfContainedANames(IHTMLElement e)
        {
            if (e is IHTMLAnchorElement)
            {
                IHTMLAnchorElement link = (IHTMLAnchorElement)e;
                if (link.name != null)
                    DescendantAnchorNames.Add(link.name);
            }
            // Do recursive search
            IHTMLElementCollection col = (IHTMLElementCollection)e.children;
            foreach (IHTMLElement child in col)
                BuildListOfContainedANames(child);
        }

        /// <summary>
        /// Changes a file name file to other safe for references into the help.
        /// Non digits or letters characters will be removed.
        /// </summary>
        /// <param name="filename">Original file name</param>
        /// <returns>Safe version of the file name</returns>
        static public string ToSafeFilename( string filename ) 
        {
            if (filename == null)
                return "";

            string newName = "";
            int i;
            for (i = 0; i < filename.Length; i++)
            {
                char c = filename[i];
                if (c == '-' || c == '_' || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || 
                    c == '.' || ( c >= '0' && c <= '9' ) )
                    newName += c;
            }
            return newName;
        }

        /// <summary>
        /// Genera el nombre del archivo que contendra este nodo.
        /// </summary>
        /// <param name="Cnt">Numero secuencial que se ha de dar al nodo</param>
        /// <returns>El nombre del archivo HTML que contendria al nodo</returns>
        public string NombreArchivo( int NumSeccion ) 
        {
            string nombre;
            if( HeaderTag == null )
                nombre = NumSeccion + ".htm";
            else
                nombre = NumSeccion + "_" + HeaderTag.innerText.Trim() + ".htm";
            return ToSafeFilename( nombre );
        }

        /// <summary>
        /// Returns the level number for a header tag (H1, H2, etc)
        /// </summary>
        /// <param name="nodo">HTML tag to check</param>
        /// <returns>The header tag level. 1 if its null</returns>
        static public int HeaderTagLevel( IHTMLElement nodo ) 
        {
            if( nodo == null )
                return 1;
            else
                return Int32.Parse( nodo.tagName.Substring( 1 ) );
        }

        /// <summary>
        /// Adds a "a name="nodeName" tag as child of a node.
        /// </summary>
        /// <param name="node">Parent node where to add the a tag.</param>
        /// <param name="ui">Application log. It can be null.</param>
        /// <param name="nodeName">Value of the "name" attribute of the "a" tag added</param>
        static private void AddARefTagToNode( IHTMLElement node, UserInterface ui , string nodeName ) {

            try
            {
                IHTMLDocument2 doc = (IHTMLDocument2)node.document;
                IHTMLAnchorElement aTagElement = (IHTMLAnchorElement) doc.createElement("<A NAME='" + nodeName + "'></A>");
                ((IHTMLDOMNode)node).appendChild( (IHTMLDOMNode)aTagElement );

            }
            catch (Exception ex)
            {
                if (ui != null)
                    ui.log(new Exception("There was an error trying to add the tag <a name=\"" +
                        nodeName + "\" /> to the node " + node.outerHTML + " (wrong HTML syntax?). If " +
                        "the source document is HTML, try to add manually an <a> tag manually. " +
                        "The application needs a node of this kind on each section title " +
                        "to make links to point it", ex));
            }
        }

        /// <summary>
        /// Tree section node constructor
        /// </summary>
        /// <param name="parent">Parent section of the section to create. null if the node to create is the root section.</param>
        /// <param name="node">HTML header tag for this section</param>
        /// <param name="ui">Application log. It can be null</param>
        public ChmDocumentNode(ChmDocumentNode parent, IHTMLElement node, UserInterface ui) 
        {
            this.Parent = parent;
            this.HeaderTag = node;
            Children = new List<ChmDocumentNode>();
            HeaderLevel = HeaderTagLevel( node );
            DestinationFileName = "";
                
            // Guardar la lista de los todos las referencias de este nodo ( nodos <A> con la propiedad "name")
            AnchorNames = new List<string>();
            if( node != null ) 
            {
                IHTMLElementCollection col = (IHTMLElementCollection) node.children;
                foreach( IHTMLElement hijo in col ) 
                {
                    if (hijo is IHTMLAnchorElement)
                    {
                        // Remove empty spaces, because they will fail into the CHM. 
                        // The anchors to this will be replace too after.
                        //listaANames.Add( ((IHTMLAnchorElement)hijo).name.Replace( " " , "" ) );
                        string processedName = ToSafeFilename( ((IHTMLAnchorElement)hijo).name );
                        if (processedName == null || processedName.Trim() == "")
                            // It seems on HTML 5 and XHTML <a id="foo"> is used...
                            processedName = ToSafeFilename( hijo.id );
                        if( processedName != null && processedName.Trim() != "" )
                            AnchorNames.Add(processedName);
                    }
                }
                if( AnchorNames.Count == 0 ) 
                {
                    // Si no tiene ningun nombre, darle uno artificial:
                    int numero = LatestCustomAnchorNumber++;
                    string nombreNodo = "NODO" + numero.ToString().Trim();
                    AddARefTagToNode(node, ui, nombreNodo);
                    AnchorNames.Add(nombreNodo);
                }
            }
        }

        /// <summary>
        /// Appends a child to this node
        /// </summary>
        /// <param name="node">Child node to add</param>
        public void AddChild( ChmDocumentNode node ) 
        {
            node.Parent = this;
            Children.Add( node );
        }

        /// <summary>
        ///  The plain text title of the section. Its not HTML encoded.
        /// </summary>
        public String Title 
        {
            get 
            {
                string name = "";
                if (HeaderTag != null)
                {
                    name = HeaderTag.innerText;
                    if (name != null)
                        /// Remove spaces for the right ordering on the topics list.
                        name = name.Trim();
                }
                else
                    name = ChmDocument.DEFAULTTILE;
                return name;
            }
        }

        /// <summary>
        /// The HTML encoded title of this node
        /// </summary>
        public string HtmlEncodedTitle 
        {
            get 
            {
                //return DocumentProcessor.HtmlEncode( Title );
                return HttpUtility.HtmlEncode(Title);
            }
        }

        /// <summary>
        /// Relative URL for this section. 
        /// As example "achilipu.htm#anchorname"
        /// </summary>
        public string Href 
        {
            get 
            {
                string enlace = "";
                if( HeaderTag != null ) 
                {
                    enlace = HttpUtility.HtmlEncode( Path.GetFileName( DestinationFileName ) );
                    if( ! MainAnchorName.Equals("") )
                        enlace += "#" + MainAnchorName;
                }
                return enlace;
            }
        }

        /// <summary>
        /// Destino de un href para hacer referencia a este capitulo. P.ej. "aa.htm#xxx".
        /// No convierte los caracteres no ascii a su codigo html.
        /// </summary>
        public string HrefNoCodificado 
        {
            get 
            {
                string enlace = "";
                if( HeaderTag != null ) 
                {
                    enlace = Path.GetFileName( DestinationFileName );
                    if( ! MainAnchorName.Equals("") )
                        enlace += "#" + MainAnchorName;
                }
                return enlace;
            }
        }

        /// <summary>
        /// The html tag A to reference this chapter.
        /// </summary>
        public string ATag 
        {
            get 
            {
                return "<a href=\"" + Href + "\">" + HtmlEncodedTitle + "</a>";
            }
        }

        /// <summary>
        /// Busca recursivamente en el arbol un nodo HTML que tenga un tag A con un cierto name.
        /// </summary>
        /// <param name="aName">name del tag A a buscar</param>
        /// <returns>El nodo encontrado con este name. null si no se encuentra</returns>
        public ChmDocumentNode BuscarEnlace( string aName ) 
        {
            if( this.AnchorNames.Contains( aName ) || ( this.DescendantAnchorNames != null && this.DescendantAnchorNames.Contains( aName ) ) )
                return this;
            else
            {
                foreach( ChmDocumentNode hijo in Children ) 
                {
                    ChmDocumentNode resultado = hijo.BuscarEnlace( aName );
                    if( resultado != null )
                        return resultado;
                }
            }
            return null;
        }

        /// <summary>
        /// Busca recursivamente un nodo HTML dentro del arbol de capitulos
        /// </summary>
        /// <param name="element">El nodo HTML a buscar</param>
        /// <returns>El nodo que lo contiene. Null, si no se encontro.</returns>
        public ChmDocumentNode BuscarNodo( IHTMLElement element , string aNameElement ) 
        {

            // Mirar si es el mismo nodo:
            if( this.HeaderTag != null && element != null ) 
            {

                if( ! aNameElement.Equals("") ) 
                {
                    if( this.AnchorNames.Contains( aNameElement ) )
                        return this;
                }
                else 
                {
                    // Para evitar el error del about:blank en los src de las imagenes:
                    string t1 = HeaderTag.outerHTML.Replace("about:blank" , "" ).Replace("about:" , "" );
                    string t2 = element.outerHTML.Replace("about:blank" , "" ).Replace("about:" , "" );
                    if( t1.Equals(t2) )
                        return this;
                }
            }
            // Sino , buscar en los hijos:
            foreach( ChmDocumentNode hijo in Children ) 
            {
                ChmDocumentNode resultado = hijo.BuscarNodo( element , aNameElement );
                if( resultado != null )
                    return resultado;
            }
            return null;
        }

        /// <summary>
        /// Stores on the node and their descendants the file name where this section will be saved.
        /// </summary>
        /// <param name="filename">Name of the HTML file where this section will be stored</param>
        public void StoredAt( string filename ) 
        {
            this.DestinationFileName = filename;
            foreach( ChmDocumentNode hijo in Children ) 
                hijo.StoredAt( filename );
        }

        /// <summary>
        /// Replaces the file where is stored this node and their descendants by other.
        /// </summary>
        /// <param name="newFile">Name of the new file where its stored</param>
        public void ReplaceFile(string newFile)
        {
            ReplaceFile(DestinationFileName, newFile);
        }

        /// <summary>
        /// Replaces the file where is stored this node and their descendants by other.
        /// </summary>
        /// <param name="oldFile">Old name of the file. Only nodes with this file will be replaced</param>
        /// <param name="newFile">Name of the new file where its stored</param>
        private void ReplaceFile(string oldFile, string newFile)
        {
            if (DestinationFileName != null && DestinationFileName.Equals(oldFile))
                DestinationFileName = newFile;
            foreach (ChmDocumentNode child in Children)
                child.ReplaceFile(oldFile, newFile);
        }

        /// <summary>
        /// Searches the first descendant section of this with a given title. 
        /// The comparation is done without letter case.
        /// </summary>
        /// <param name="sectionTitle">The section title to seach</param>
        /// <returns>The first section of the document with that title. null if no section was
        /// found.</returns>
        public ChmDocumentNode SearchBySectionTitle(string sectionTitle)
        {
            if (this.Title.ToLower() == sectionTitle.ToLower())
                return this;
            foreach (ChmDocumentNode child in Children)
            {
                ChmDocumentNode result = child.SearchBySectionTitle(sectionTitle);
                if (result != null)
                    return result;
            }
            return null;
        }

        // DONT REMOVE THIS COMMENT BY NOW. ITS THE INTRINCATED ORIGINAL SOURCE TO SAVE FILES
        /*
        private void GuardarDocumentos(string directory, HtmlPageDecorator decorator, ChmDocumentNode nodo, List<string> archivosGenerados, WebIndex indexer) 
        {
            if( nodo.SplittedPartBody != null ) 
            {
                string texto = "";
                if( nodo.SplittedPartBody.innerText != null )
                    texto = nodo.SplittedPartBody.innerText.Trim();

                if( !texto.Equals("") ) 
                {
                    bool guardar = true;
                    string titulo = "";
                    IHTMLElement seccion = null;

                    seccion = SearchFirstCutNode( nodo.SplittedPartBody );
                    if( seccion != null && seccion.innerText != null ) 
                    {
                        titulo = seccion.innerText.Trim() ;
                        if( titulo.Length == 0 )
                            guardar = false;
                    }

                    if( guardar ) 
                    {
                        // Save the section, adding header, footers, etc:
                        string filePath = directory + Path.DirectorySeparatorChar + nodo.DestinationFileName;
                        decorator.ProcessAndSavePage(nodo.SplittedPartBody, filePath, nodo.Title);

                        if (FirstChapterContent == null)
                        {
                            // This is the first chapter of the document. Store it clean, because
                            // we will need after.
                            FirstChapterContent = nodo.SplittedPartBody.innerHTML.Replace("about:blank", "").Replace("about:", "");
                        }

                        archivosGenerados.Add(filePath);

                        if (indexer != null)
                            // Store the document at the full text search index:
                            indexer.AddPage(nodo.DestinationFileName, nodo.Title, nodo.SplittedPartBody);
                            
                    }
                }
            }

            foreach( ChmDocumentNode hijo in nodo.Children ) 
                GuardarDocumentos( directory , decorator , hijo , archivosGenerados , indexer );
        }
        */

        /// <summary>
        /// Saves the splitted content of this node into a file, if it has any.
        /// </summary>
        /// <param name="directoryDstPath">Directory path where the content files will be stored</param>
        /// <param name="decorator">Tool to generate and decorate the HTML content files</param>
        /// <param name="indexer">Tool to index the saved content files. It can be null, if the content
        /// does not need to be indexed.</param>
        /// <returns>The content file name saved. Is this node has no content, it returns null</returns>
        public string SaveContent(string directoryDstPath, HtmlPageDecorator decorator, WebIndex indexer)
        {
            if (SplittedPartBody == null)
                return null;

            string texto = "";
            if (SplittedPartBody.innerText != null)
                texto = SplittedPartBody.innerText.Trim();

            // TODO: This never should happen... The parser joins empty sections...
            if (string.IsNullOrEmpty(texto))
                return null;

            // Save the section, adding header, footers, etc:
            string filePath = Path.Combine( directoryDstPath, DestinationFileName );
            decorator.ProcessAndSavePage(SplittedPartBody, filePath, Title);

            if (indexer != null)
                // Store the document at the full text search index:
                indexer.AddPage(DestinationFileName, Title, SplittedPartBody);

            return DestinationFileName;
        }

        public int CompareTo(object obj)
        {
            if( ! ( obj is ChmDocumentNode ) )
                return 0;
            ChmDocumentNode nodo = (ChmDocumentNode) obj;
            return String.CompareOrdinal( Title.ToLower() , nodo.Title.ToLower() );
        }

    }

}
