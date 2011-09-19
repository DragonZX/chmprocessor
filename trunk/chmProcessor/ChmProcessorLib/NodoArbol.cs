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
using mshtml;
using System.Collections;
using System.IO;
using System.Web;

namespace ChmProcessorLib
{
    /// <summary>
    /// Descripcion de un nodo del arbol de capitulos.
    /// </summary>
    public class NodoArbol : IComparable
    {
        /// <summary>
        /// Nodo del documento del que ha salido este. ojo puede ser null.
        /// </summary>
        public IHTMLElement Nodo;

        /// <summary>
        /// Archivo final al que va la seccion tras trocear el documento.
        /// </summary>
        public string Archivo;

        /// <summary>
        /// Secciones internas de este capitulo.
        /// </summary>
        public ArrayList Hijos;

        /// <summary>
        /// El nodo padre
        /// </summary>
        public NodoArbol Padre;

        /// <summary>
        /// El nivel del header (H1 -> 1 , H2 -> 2, etc )
        /// </summary>
        public int Nivel;

        /// <summary>
        /// La lista de nombres con que se puede hacer referencia a este nodo
        /// </summary>
        private ArrayList listaANames;

        /// <summary>
        /// Contador del ultimo numero dado a un tag A name="NODOxxxx" , donde la xxx es el
        /// numero. Se usa para asignar un nombre a los nodos que no lo tienen.
        /// </summary>
        private static int UltimoNumeroAname = 0;

        /// <summary>
        /// El nombre del identificador del tag A (a name="xxx") que se usa
        /// para enlazar con este nodo. Puede ser nulo. Tener en cuenta
        /// que no incluye el nombre del archivo (Archivo)
        /// </summary>
        public string aNamePrincipal 
        {
            get 
            {
                if( listaANames.Count > 0 )
                    return (string)listaANames[0];
                else
                    return "";
            }
        }

        /// <summary>
        /// Cuerpo de lo que sera este capitulo
        /// </summary>
        public IHTMLElement body;

        /// <summary>
        /// List with all the names contained into the body of this node. Those names are the "name" property
        /// of the tag A (a name="foo"). They are used to change internal links into the document.
        /// </summary>
        private ArrayList listOfContainedANames;

        /// <summary>
        /// Builds the member listOfContainedANames, with all the A name tags contained into the body member.
        /// </summary>
        public void BuildListOfContainedANames()
        {
            if (listOfContainedANames == null)
                listOfContainedANames = new ArrayList();
            else
                listOfContainedANames.Clear();

            if (body != null)
                BuildListOfContainedANames(body);
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
                    listOfContainedANames.Add(link.name);
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
            if( Nodo == null )
                nombre = NumSeccion + ".htm";
            else
                nombre = NumSeccion + "_" + Nodo.innerText.Trim() + ".htm";
            return ToSafeFilename( nombre );
        }

        static public int NivelNodo( IHTMLElement nodo ) 
        {
            if( nodo == null )
                return 1;
            else
                return Int32.Parse( nodo.tagName.Substring( 1 ) );
        }

        /// <summary>
        /// Tree section node constructor
        /// </summary>
        /// <param name="parent">Parent section of the section to create. null if the node to create is the root section.</param>
        /// <param name="node">HTML header tag for this section</param>
        /// <param name="ui">Application log. It can be null</param>
        public NodoArbol(NodoArbol parent, IHTMLElement node, UserInterface ui) 
        {
            this.Padre = parent;
            this.Nodo = node;
            Hijos = new ArrayList();
            Nivel = NivelNodo( node );
            Archivo = "";
                
            // Guardar la lista de los todos las referencias de este nodo ( nodos <A> con la propiedad "name")
            listaANames = new ArrayList();
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
                            listaANames.Add(processedName);
                    }
                }
                if( listaANames.Count == 0 ) 
                {
                    // Si no tiene ningun nombre, darle uno artificial:
                    int numero = UltimoNumeroAname++;
                    string nombreNodo = "NODO" + numero.ToString().Trim();
                    string tagA = "<a name=\"" + nombreNodo + "\">";
                    try
                    {
                        node.insertAdjacentHTML("afterBegin", tagA);
                        listaANames.Add(nombreNodo);
                    }
                    catch (Exception ex)
                    {
                        if( ui != null )
                            ui.log( new Exception("There was an error trying to add the tag " + 
                                tagA + " to the node " + node.outerHTML + " (wrong HTML syntax?). If " + 
                                "the source document is HTML, try to add manually an <a> tag manually. " + 
                                "The application needs a node of this kind on each section title " + 
                                "to make links to point it", ex ) );
                    }
                }
            }
        }

        public void NuevoHijo( NodoArbol nodo ) 
        {
            nodo.Padre = this;
            Hijos.Add( nodo );
        }

        /// <summary>
        ///  The text title of the section. Its not HTML encoded safe.
        /// </summary>
        public String Name 
        {
            get 
            {
                string name = "";
                if (Nodo != null)
                {
                    name = Nodo.innerText;
                    if (name != null)
                        /// Remove spaces for the right ordering on the topics list.
                        name = name.Trim();
                }
                else
                    name = ArbolCapitulos.DEFAULTTILE;
                return name;
            }
        }

        /// <summary>
        /// The HTML encoded title of this chapter / section.
        /// </summary>
        public string EncodedName 
        {
            get 
            {
                return DocumentProcessor.HtmlEncode( Name );
            }
        }

        public string EntradaArbolContenidos 
        {
            get 
            {
                string nombre = "";
                if( Nodo != null )
                    nombre = Nodo.innerText;
                else
                    nombre = ArbolCapitulos.DEFAULTTILE;

                string texto = "<LI> <OBJECT type=\"text/sitemap\">\n" +
                    "     <param name=\"Name\" value=\"" + 
                    //DocumentProcessor.HtmlEncode( nombre , false ) + 
                    this.EncodedName + 
                    "\">\n" + 
                    "     <param name=\"Local\" value=\"" +  Href;
                texto += "\">\n" + "     </OBJECT>\n";
                return texto;
            }
        }

        #region Java Help

        /// <summary>
        /// Tag for a java help index file of this section.
        /// </summary>
        public string JavaHelpIndexEntry
        {
            get
            {
                /*string name = "";
                if (Nodo != null)
                    name = Nodo.innerText;
                else
                    name = "Start";*/

                //return "<indexitem text=\"" + name + "\" target=\"" + JavaHelpTarget + "\" />";
                return "<indexitem text=\"" + EncodedName + "\" target=\"" + JavaHelpTarget + "\" />";
            }
        }

        /// <summary>
        /// Tag for a java help map file of this section.
        /// </summary>
        public string JavaHelpMapEntry
        {
            get
            {
                /*string name = "";
                if (Nodo != null)
                    name = Nodo.innerText;
                else
                    name = "Start";*/
                return "<mapID target=\"" + JavaHelpTarget + "\" url=\"" + Href + "\" />";
            }
        }

        /// <summary>
        /// Tag for a java help TOC file of this section.
        /// </summary>
        public string JavaHelpTOCEntry
        {
            get
            {
                String entry = "<tocitem text=\"" + EncodedName + "\" target=\"" + JavaHelpTarget + "\"";
                if (Hijos.Count == 0)
                    entry += " />";
                else
                    entry += ">";
                return entry;
            }
        }

        /// <summary>
        /// Name of the java help target for this section
        /// </summary>
        public string JavaHelpTarget
        {
            get
            {
                return EncodedName;
            }
        }

        #endregion

        /// <summary>
        /// Destino de un href para hacer referencia a este capitulo. P.ej. "aa.htm#xxx"
        /// </summary>
        public string Href 
        {
            get 
            {
                string enlace = "";
                if( Nodo != null ) 
                {
                    enlace = DocumentProcessor.HtmlEncode( Path.GetFileName( Archivo ) );
                    if( ! aNamePrincipal.Equals("") )
                        enlace += "#" + aNamePrincipal;
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
                if( Nodo != null ) 
                {
                    enlace = Path.GetFileName( Archivo );
                    if( ! aNamePrincipal.Equals("") )
                        enlace += "#" + aNamePrincipal;
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
                return "<a href=\"" + Href + "\">" + EncodedName + "</a>";
            }
        }

        /// <summary>
        /// Busca recursivamente en el arbol un nodo HTML que tenga un tag A con un cierto name.
        /// </summary>
        /// <param name="aName">name del tag A a buscar</param>
        /// <returns>El nodo encontrado con este name. null si no se encuentra</returns>
        public NodoArbol BuscarEnlace( string aName ) 
        {
            if( this.listaANames.Contains( aName ) || ( this.listOfContainedANames != null && this.listOfContainedANames.Contains( aName ) ) )
                return this;
            else
            {
                foreach( NodoArbol hijo in Hijos ) 
                {
                    NodoArbol resultado = hijo.BuscarEnlace( aName );
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
        public NodoArbol BuscarNodo( IHTMLElement element , string aNameElement ) 
        {

            // Mirar si es el mismo nodo:
            if( this.Nodo != null && element != null ) 
            {

                if( ! aNameElement.Equals("") ) 
                {
                    if( this.listaANames.Contains( aNameElement ) )
                        return this;
                }
                else 
                {
                    // Para evitar el error del about:blank en los src de las imagenes:
                    string t1 = Nodo.outerHTML.Replace("about:blank" , "" ).Replace("about:" , "" );
                    string t2 = element.outerHTML.Replace("about:blank" , "" ).Replace("about:" , "" );
                    if( t1.Equals(t2) )
                        return this;
                }
            }
            // Sino , buscar en los hijos:
            foreach( NodoArbol hijo in Hijos ) 
            {
                NodoArbol resultado = hijo.BuscarNodo( element , aNameElement );
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
            this.Archivo = filename;
            foreach( NodoArbol hijo in Hijos ) 
                hijo.StoredAt( filename );
        }

        /// <summary>
        /// Replaces the file where is stored this node and their descendants by other.
        /// </summary>
        /// <param name="newFile">Name of the new file where its stored</param>
        public void ReplaceFile(string newFile)
        {
            ReplaceFile(Archivo, newFile);
        }

        /// <summary>
        /// Replaces the file where is stored this node and their descendants by other.
        /// </summary>
        /// <param name="oldFile">Old name of the file. Only nodes with this file will be replaced</param>
        /// <param name="newFile">Name of the new file where its stored</param>
        private void ReplaceFile(string oldFile, string newFile)
        {
            if (Archivo != null && Archivo.Equals(oldFile))
                Archivo = newFile;
            foreach (NodoArbol child in Hijos)
                child.ReplaceFile(oldFile, newFile);
        }

        /// <summary>
        /// Searches the first descendant section of this with a given title. 
        /// The comparation is done without letter case.
        /// </summary>
        /// <param name="sectionTitle">The section title to seach</param>
        /// <returns>The first section of the document with that title. null if no section was
        /// found.</returns>
        public NodoArbol SearchBySectionTitle(string sectionTitle)
        {
            if (this.Name.ToLower() == sectionTitle.ToLower())
                return this;
            foreach (NodoArbol child in Hijos)
            {
                NodoArbol result = child.SearchBySectionTitle(sectionTitle);
                if (result != null)
                    return result;
            }
            return null;
        }

        #region IComparable members

        public int CompareTo(object obj)
        {
            if( ! ( obj is NodoArbol ) )
                return 0;
            NodoArbol nodo = (NodoArbol) obj;
            return String.CompareOrdinal( Name.ToLower() , nodo.Name.ToLower() );
        }

        #endregion

    }

}
