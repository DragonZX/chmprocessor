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
using System.Collections;
using System.Text;
using System.Diagnostics;
using System.Management;
using System.Web;
using System.Runtime.InteropServices;
using System.Globalization;
using WebIndexLib;

namespace ChmProcessorLib
{
	/// <summary>
    /// Class to handle manipulations of a HTML / Word file to generate the help
	/// </summary>
    public class DocumentProcessor
    {

        /// <summary>
        /// Name for the help project that will be generated
        /// </summary>
        public static string NOMBREPROYECTO = "help.hhp";

        /// <summary>
        /// Name for the chm file that will be generated (?)
        /// </summary>
        public static string NOMBREARCHIVOAYUDA = "help.chm";

        /// <summary>
        /// Main source file to convert to help.
        /// If there was multiple documents to convert to help, they are joined on this.
        /// </summary>
        private string MainSourceFile;

        /// <summary>
        /// Documento HTML cargado:
        /// </summary>
        private IHTMLDocument2 iDoc;

        /// <summary>
        /// Texto a colocar antes del tag body.
        /// </summary>
        /// 
        private string textoAntesBody;

        /// <summary>
        /// Texto a colocar despues del tag body.
        /// </summary>
        private string textoDespuesBody;

        /// <summary>
        /// HTML title nodes (H1,H2,etc) tree.
        /// </summary>
        private ArbolCapitulos arbol;

        /// <summary>
        /// Html code for the CHM headers
        /// </summary>
        private string HtmlCabecera;

        /// <summary>
        /// Html code for the CHM footers
        /// </summary>
        private string HtmlPie;

        /// <summary>
        /// Lista de archivos y directorios adicionales a añadir al proyecto de ayuda
        /// </summary>
        private ArrayList ArchivosAdicionales;

        //public ArrayList Errores;

        /// <summary>
        /// Indica si el archivo a procesar es un documento word o uno html
        /// </summary>
        private bool esWord;

        /// <summary>
        /// Si esWord = true, indica el directorio temporal donde se genero el 
        /// html del documento word
        /// </summary>
        private string dirHtml;

        /// <summary>
        /// HTML content of the body of the first chapter into the document.
        /// </summary>
        public string FirstChapterContent;

        /// <summary>
        /// HTML code loaded for the web site headers.
        /// </summary>
        public string HtmlHeaderCode;

        /// <summary>
        /// HTML code loaded for the web site footers.
        /// </summary>
        public string HtmlFooterCode;

        /// <summary>
        /// Configuration to generate the help. 
        /// </summary>
        public ChmProject Configuration;

        /// <summary>
        /// Timer to avoid html loading hang ups
        /// </summary>
        System.Windows.Forms.Timer timerTimeout;

        public interface UserInterface 
        {
            void log( string text );
            bool CancellRequested();
        }

        /// <summary>
        /// Handler of the user interface of the generation process. Can be null.
        /// </summary>
        public UserInterface ui;

        private void log( string texto ) 
        {
            if (ui != null)
                ui.log(texto);
        }

        private bool CancellRequested()
        {
            if (ui != null)
                return ui.CancellRequested();
            else
                return false;
        }

        [ComVisible(true), ComImport(),
        Guid("7FD52380-4E07-101B-AE2D-08002B2EC713"),
        InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPersistStreamInit
        {
            void GetClassID([In, Out] ref Guid pClassID);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int IsDirty();
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int Load([In] UCOMIStream pstm);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int Save([In] UCOMIStream pstm, [In,
                MarshalAs(UnmanagedType.Bool)] bool fClearDirty);
            void GetSizeMax([Out] long pcbSize);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int InitNew();
        }

        /// <summary>
        /// Timer to avoid load html file hang up
        /// </summary>
        private void timer_Tick(object sender, System.EventArgs e)
        {
            timerTimeout.Enabled = false;
        }

        /// <summary>
        /// Checks if any source document is open, and join the multiple source Word documents to 
        /// a single file.
        /// </summary>
        /// <param name="msWord">Word instace that will do this work.</param>
        /// <returns>Path to a single Word file containing all source documents. If there is a 
        /// single source file, it will return that file path.</returns>
        private string CheckAndJoinWordSourceFiles(MSWord msWord)
        {

            // Check any source file is open:
            foreach (string sourceFile in Configuration.SourceFiles)
            {
                if (msWord.IsOpen(sourceFile))
                    throw new Exception("The file " + sourceFile + " is already open. You must to close it before generate the help.");
            }

            if (Configuration.SourceFiles.Count == 1)
                return (string) Configuration.SourceFiles[0];

            // Join multiple documents to a temporal file:
            string joinedDocument = Path.GetTempFileName();
            log("Joining documents to a single temporal file : " + joinedDocument);
            msWord.JoinDocuments(Configuration.SourceFiles, joinedDocument);
            return joinedDocument;
        }

        /// <summary>
        /// Open source files, if they are MS Word documents.
        /// It joins and store them to a HTML single file.
        /// </summary>
        /// <param name="msWord">Word instace that will do this work.</param>
        /// <returns>The HTML joined version of the MS Word documents</returns>
        private string ConvertWordSourceFiles(MSWord msWord)
        {

            string singleWordFile = CheckAndJoinWordSourceFiles(msWord);

            log("Convert file " + singleWordFile + " to HTML");
            string nombreArchivo = Path.GetFileNameWithoutExtension(singleWordFile);
            dirHtml = Path.GetTempPath() + Path.DirectorySeparatorChar + nombreArchivo;
            if (Directory.Exists(dirHtml))
                Directory.Delete(dirHtml, true);
            Directory.CreateDirectory(dirHtml);
            string finalFile = dirHtml + Path.DirectorySeparatorChar + nombreArchivo + ".htm";
            msWord.SaveWordToHtml(singleWordFile, finalFile);
            return finalFile;
        }

        /// <summary>
        /// Open source files.
        /// If they are not word, they will be converted to HTML.
        /// </summary>
        private void OpenSourceFiles() 
        {
            MSWord msWord = null;

            try
            {
                //esWord = MSWord.ItIsWordDocument( Configuration.ArchivoOrigen );
                esWord = MSWord.ItIsWordDocument((string)Configuration.SourceFiles[0]);
                dirHtml = null;
                string archivoFinal = Configuration.ArchivoOrigen;
                // Si es un documento word, convertirlo a HTML filtrado
                if (esWord)
                {
                    msWord = new MSWord();
                    archivoFinal = ConvertWordSourceFiles(msWord);
                    // Be sure we have closed word, to avoid overlapping between the html read
                    // and the reading from chmprocessor:
                    msWord.Dispose();
                    msWord = null;
                }

                if (AppSettings.UseTidyOverInput)
                    new TidyParser(ui).Parse(archivoFinal);

                // Prepare loading:
                HTMLDocumentClass docClass = new HTMLDocumentClass();
                IPersistStreamInit ips = (IPersistStreamInit)docClass;
                ips.InitNew();

                // Create a timer, to be sure that HTML file load will not be hang up (Sometime happens)
                timerTimeout = new System.Windows.Forms.Timer();
                timerTimeout.Tick += new System.EventHandler(this.timer_Tick);
                timerTimeout.Interval = 60 * 1000;     // 1 minute
                timerTimeout.Enabled = true;

                // Load the file:
                IHTMLDocument2 docLoader = (mshtml.IHTMLDocument2)docClass.createDocumentFromUrl( archivoFinal , null);
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(1000);

                String currentStatus = docLoader.readyState;
                log("Reading file " + archivoFinal + ". Status: " + currentStatus );
                while (currentStatus != "complete" && timerTimeout.Enabled)
                {
                    System.Windows.Forms.Application.DoEvents();
                    System.Threading.Thread.Sleep(500);
                    String newStatus = docLoader.readyState;
                    if (newStatus != currentStatus)
                    {
                        log("Status: " + newStatus);
                        if (currentStatus == "interactive" && newStatus == "uninitialized")
                        {
                            // fucking shit bug. Try to reload the file:
                            log("Warning. Something wrong happens loading the file. Trying to reopen " + archivoFinal );
                            docClass = new HTMLDocumentClass();
                            ips = (IPersistStreamInit)docClass;
                            ips.InitNew();
                            docLoader = (mshtml.IHTMLDocument2)docClass.createDocumentFromUrl(archivoFinal, null);
                            newStatus = docLoader.readyState;
                            log("Status: " + newStatus);
                        }
                        currentStatus = newStatus;
                    }
                }
                if (!timerTimeout.Enabled)
                    log("Warning: time to load file expired.");
                timerTimeout.Enabled = false;

                // Get a copy of the document:
                HTMLDocumentClass newDocClass = new HTMLDocumentClass();
                iDoc = (IHTMLDocument2)newDocClass;
                object[] txtHtml = { ((IHTMLDocument3)docLoader).documentElement.outerHTML };
                iDoc.writeln(txtHtml);
                try
                {
                    // Needed, otherwise some characters will not be displayed well.
                    iDoc.charset = docLoader.charset;
                }
                catch (Exception ex)
                {
                    log("Warning: Cannot set the charset \"" + docLoader.charset + "\" to the html document. Reason:" + ex.Message);
                }
            }
            finally
            {
                if (msWord != null)
                {
                    msWord.Dispose();
                    msWord = null;
                }
            }
        }

        public DocumentProcessor( ChmProject configuration)
        {
            this.Configuration = configuration;
            this.ArchivosAdicionales = new ArrayList(configuration.ArchivosAdicionales);
        }

        private void AntesYDespuesBody() 
        {
            textoAntesBody = "";
            textoDespuesBody = "";
            bool antes = true;
            IHTMLDocument3 iDoc3 = (IHTMLDocument3) iDoc;
            IHTMLDOMChildrenCollection col = (IHTMLDOMChildrenCollection)iDoc3.childNodes;
            foreach( IHTMLElement e in col )
            {
                if( e is IHTMLCommentElement ) 
                {
                    IHTMLCommentElement com = (IHTMLCommentElement) e;
                    if( antes )
                        textoAntesBody += com.text + "\n";
                    else
                        textoDespuesBody += com.text  + "\n";
                }
                else if( e is IHTMLHtmlElement ) 
                {
                    // Get the HTML tag node.
                    textoAntesBody += "<html ";
                    IHTMLAttributeCollection atrCol = (IHTMLAttributeCollection) ((IHTMLDOMNode)e).attributes;

                    foreach (IHTMLDOMAttribute atr in atrCol)
                    {
                        if( atr.specified )
                            textoAntesBody += atr.nodeName + "=\"" + atr.nodeValue + "\"";
                    }
                    textoAntesBody += " >\n";

                    IHTMLElementCollection colBody = (IHTMLElementCollection)e.children;
                    foreach( IHTMLElement hijo in colBody ) 
                    {
                        if( hijo is IHTMLBodyElement ) 
                            antes = false;
                        else if( antes )
                            textoAntesBody += hijo.outerHTML  + "\n";
                        else
                            textoDespuesBody += hijo.outerHTML  + "\n";
                    }

                    textoDespuesBody += "</html>\n";
                }
                    
            }

        }

        /// <summary>
        /// Modifica un nodo HTML si hace falta
        /// </summary>
        /// <param name="nodo">Un nodo HTML del documento</param>
        /// <param name="parent">Parent node of nodo. Null if nodo has no parents.</param>
        private void PreProcesarNodo( IHTMLElement nodo , IHTMLElement parent ) 
        {
            if( nodo is IHTMLAnchorElement ) 
            {
                IHTMLAnchorElement link = (IHTMLAnchorElement)nodo;
                // Remove the about:blank
                string href = link.href;
                if( href!= null ) 
                {
                    href = href.Replace( "about:blank" , "" ).Replace("about:" , "" );
                    if( href.StartsWith("#") ) 
                    {
                        // Cambiar el enlace interno para que vaya al archivo correspondiente:
                        string safeRef = NodoArbol.ProcesarNombreArchivo(href.Substring(1));
                        NodoArbol nodoArbol = arbol.Raiz.BuscarEnlace( safeRef );
                        if (nodoArbol != null)
                            link.href = nodoArbol.Archivo + "#" + safeRef;
                        else
                        {
                            // Broken link.
                            log("WARNING: Broken link with text: '" + nodo.innerText + "'" );
                            if (parent != null)
                            {
                                String inText = parent.innerText;
                                if (inText != null)
                                {
                                    if (inText.Length > 200)
                                        inText = inText.Substring(0, 200) + "...";
                                    log(" near of text: '" + inText + "'" );
                                }
                            }
                        }
                        
                    }
                }
                else if (link.name != null /*&& link.name.Contains(" ")*/ )
                {
                    string safeName = NodoArbol.ProcesarNombreArchivo(link.name);
                    if (!link.name.Equals(safeName))
                    {
                        // Word bug? i have found names with space characters and other bad things. 
                        // They fail into the CHM:
                        //link.name = link.name.Replace(" ", ""); < NOT WORKS
                        IHTMLDOMNode domNodeParent = (IHTMLDOMNode)nodo.parentElement;
                        string htmlNewNode = "<a name=" + safeName + "></a>";
                        IHTMLDOMNode newDomNode = (IHTMLDOMNode)iDoc.createElement(htmlNewNode);
                        domNodeParent.replaceChild(newDomNode, (IHTMLDOMNode)nodo);
                    }
                }
            }

            IHTMLElementCollection col = (IHTMLElementCollection) nodo.children;
            foreach (IHTMLElement hijo in col)
                PreProcesarNodo(hijo, nodo);
        }

        private IHTMLElement BuscarNodo( IHTMLElement nodo , string tag ) 
        {
            if( nodo.tagName.ToLower().Equals( tag.ToLower() ) )
                return nodo;
            else 
            {
                IHTMLElementCollection col = (IHTMLElementCollection) nodo.children;
                foreach( IHTMLElement hijo in col ) 
                {
                    IHTMLElement encontrado = BuscarNodo( hijo , tag );
                    if( encontrado != null )
                        return encontrado;
                }
                return null;
            }
        }

        private string GetBodyContent(string htmlDocument, bool outputXhtml)
        {
            // Use tidy over the chapter, if it's needed:
            string goodText = "";
            if (AppSettings.UseTidyOverOutput)
                goodText = new TidyParser(ui, outputXhtml).ParseString(htmlDocument);
            else
                goodText = htmlDocument;

            // Extract the body content:
            HTMLDocumentClass docClass = new HTMLDocumentClass();
            IHTMLDocument2 iDocFirstChapter = (IHTMLDocument2)docClass;
            object[] txtHtml = { goodText };
            iDocFirstChapter.write(txtHtml);

            // return the content of the body:
            return iDocFirstChapter.body.innerHTML.Replace("about:blank", "").Replace("about:", "");
        }
        
        private void GuardarDocumentos( string directory , string header , string footer , NodoArbol nodo , ArrayList archivosGenerados , WebIndex indexer ) 
        {
            if( nodo.body != null ) 
            {
                string texto = "";
                if( nodo.body.innerText != null )
                    texto = nodo.body.innerText.Trim();

                if( !texto.Equals("") ) 
                {
                    bool guardar = true;
                    string titulo = "";
                    IHTMLElement seccion = null;

                    seccion = SearchFirstCutNode( nodo.body );
                    if( seccion != null && seccion.innerText != null ) 
                    {
                        titulo = seccion.innerText.Trim() ;
                        if( titulo.Length == 0 )
                            guardar = false;
                    }

                    if( guardar ) 
                    {
                        // hacer un preproceso de TODOS los nodos del cuerpo:
                        IHTMLElementCollection col = (IHTMLElementCollection)nodo.body.children;
                        foreach( IHTMLElement nodoBody in col ) 
                            PreProcesarNodo( nodoBody , null);

                        // Generar el documento a guardar:

                        IHTMLDOMNode domNode = (IHTMLDOMNode)nodo.body;
                        IHTMLElement clonedBody = (IHTMLElement)domNode.cloneNode(true);

                        // Si hay pie o cabecera, añadirlos al body:
                        if (header != null && !header.Equals(""))
                            clonedBody.insertAdjacentHTML("afterBegin", header);
                        if (footer != null && !footer.Equals(""))
                            clonedBody.insertAdjacentHTML("beforeEnd", footer);

                        iDoc.title = titulo;
                        AntesYDespuesBody();

                        string archivo = directory + Path.DirectorySeparatorChar + nodo.Archivo;

                        Encoding encoding = Encoding.GetEncoding( iDoc.charset );
                        StreamWriter writer = new StreamWriter( archivo , false , encoding );
                        writer.WriteLine( textoAntesBody );
                        texto = clonedBody.outerHTML;
                        
                        // Parece que hay un bug por el cual pone about:blank en los links. Quitarlos:
                        texto = texto.Replace( "about:blank" , "" ).Replace("about:" , "" );
                        writer.WriteLine( texto );
                        writer.WriteLine( textoDespuesBody );
                        writer.Close();

                        // Clean the files using Tidy
                        TidyOutputFile(archivo);

                        if (FirstChapterContent == null)
                        {
                            // This is the first chapter of the document. Store it clean, because
                            // we will need after.
                            FirstChapterContent = nodo.body.innerHTML.Replace("about:blank", "").Replace("about:", "");
                        }

                        archivosGenerados.Add( archivo );

                        if (indexer != null)
                            // Store the document at the full text search index:
                            indexer.AddPage(nodo.Archivo, nodo.Title , clonedBody );
                    }
                }
            }

            foreach( NodoArbol hijo in nodo.Hijos ) 
                GuardarDocumentos( directory , header , footer , hijo , archivosGenerados , indexer );
        }

        /// <summary>
        /// Run tidy over the output file, if the configuration says we must to do it.
        /// </summary>
        /// <param name="filePath">Path to the HTML file to repair with tidy.</param>
        private void TidyOutputFile(string filePath)
        {
            if( AppSettings.UseTidyOverOutput )
                new TidyParser(ui).Parse(filePath);
        }

        private void UnificarNodos( NodoArbol nodo ) 
        {
            if( nodo.Nodo != null && nodo.Nodo.innerText != null && nodo.body != null ) 
            {
                // Nodo con cuerpo:

                if( nodo.Nodo.innerText.Trim().Equals( nodo.body.innerText.Trim() ) && 
                    nodo.Hijos.Count > 0 ) 
                {
                    // Nodo vacio y con hijos 
                    NodoArbol hijo = (NodoArbol) nodo.Hijos[0];
                    if( hijo.body != null ) 
                    {
                        // El hijo tiene cuerpo: Unificarlos.
                        nodo.body.insertAdjacentHTML("beforeEnd" , hijo.body.innerHTML);
                        hijo.body = null;
                        //hijo.Archivo = nodo.Archivo;
                        hijo.GuardadoEn(nodo.Archivo);
                    }
                }
            }

            foreach( NodoArbol hijo in nodo.Hijos ) 
                UnificarNodos( hijo );
        }

        private ArrayList GuardarDocumentos(string directory, string header, string footer, WebIndex indexer ) 
        {
            // Intentar unificar nodos que quedarian vacios, con solo el titulo de la seccion:
            foreach( NodoArbol nodo in arbol.Raiz.Hijos ) 
                UnificarNodos( nodo );

            // Recorrer el arbol en busca de nodos con cuerpo
            ArrayList archivosGenerados = new ArrayList();
            foreach (NodoArbol nodo in arbol.Raiz.Hijos)
                GuardarDocumentos(directory , header , footer , nodo, archivosGenerados , indexer );

            return archivosGenerados;
        }

        private void GuardarParte( IHTMLElement nuevoBody ) 
        {
            IHTMLElement seccion = SearchFirstCutNode( nuevoBody );
            NodoArbol nodoAGuardar;
            if( seccion == null )
                // Si es sin seccion, es la primera seccion de todas:
                nodoAGuardar = (NodoArbol) arbol.Raiz.Hijos[0];
            else 
            {
                string aName = "";
                IHTMLAnchorElement a = BuscarNodoA( seccion );
                if( a != null && a.name != null )
                    aName = NodoArbol.ProcesarNombreArchivo( a.name );
                nodoAGuardar = arbol.Raiz.BuscarNodo( seccion , aName );
            }

            if (nodoAGuardar == null)
            {
                string msgError = "Error al buscar el nodo ";
                if (seccion != null)
                    msgError += seccion.innerText;
                else
                    msgError += "<vacio>";
                //Errores.Add(msgError);
            }
            else
            {
                nodoAGuardar.body = nuevoBody;
                nodoAGuardar.BuildListOfContainedANames();  // Store the A name's tags of the body.
            }
        }

        /// <summary>
        /// Vacia el directorio de destino, y copia los archivos adicionales a aquel.
        /// </summary>
        /// <returns>Devuelve la lista de archivos adicionales a incluir en el proyecto de la ayuda</returns>
        private ArrayList GenerarDirDestino( string dirDst ) 
        {
            // Recrear el directorio:
            if( Directory.Exists( dirDst ) )
                Directory.Delete( dirDst , true );
            Directory.CreateDirectory( dirDst );
            
            // Copiar los archivos adicionales
            ArrayList nuevosArchivos = new ArrayList();
            foreach( string arc in ArchivosAdicionales ) 
            //foreach (string arc in Configuration.ArchivosAdicionales) 
            {
                if( Directory.Exists( arc ) )
                {
                    // Its a directory. Copy it:
                    string dst = dirDst + Path.DirectorySeparatorChar + Path.GetFileName( arc );
                    FileSystem.CopyDirectory(arc, dst);
                }
                else if( File.Exists( arc ) ) 
                {
                    string dst = dirDst + Path.DirectorySeparatorChar + Path.GetFileName( arc );
                    File.Copy( arc , dst );
                    nuevosArchivos.Add( Path.GetFileName( arc ) );
                }
            }

            return nuevosArchivos;
        }

        /// <summary>
        /// Extract the style tag of the document to an CSS external file.
        /// </summary>
        /// <returns>Path to the generated CSS file. null if no CSS was generated</returns>
        private string CheckForStyleTags() {
            IHTMLDocument3 iDoc3 = (IHTMLDocument3) iDoc;
            IHTMLDOMChildrenCollection col = (IHTMLDOMChildrenCollection)iDoc3.childNodes;
            IHTMLHeadElement head = null;
            string cssFile = null;
            IHTMLHtmlElement html = null;
            IHTMLElement style = null;

            // Search the HTML tag:
            foreach (IHTMLElement e in col)
            {
                if (e is IHTMLHtmlElement)
                {
                    html = (IHTMLHtmlElement)e;
                    break;
                }
            }

            if (html != null)
            {
                // Search the <HEAD> tag.
                col = (IHTMLDOMChildrenCollection)((IHTMLDOMNode)html).childNodes;
                foreach (IHTMLElement e in col)
                {
                    if (e is IHTMLHeadElement)
                    {
                        head = (IHTMLHeadElement)e;
                        break;
                    }
                }
            }

            if (head != null)
            {
                // Search the first <STYLE> tag:
                col = (IHTMLDOMChildrenCollection)((IHTMLDOMNode)head).childNodes;
                foreach (IHTMLElement e in col)
                {
                    if (e is IHTMLStyleElement)
                    {
                        style = (IHTMLElement)e;
                        break;
                    }
                }
            }

            if (style != null && style.innerHTML != null )
            {
                // Remove comments
                string cssText = style.innerHTML.Replace("<!--", "").Replace("-->", "");
                // Create the CSS file:
                cssFile = Configuration.HelpProjectDirectory + Path.DirectorySeparatorChar + "embeddedstyles.css";
                StreamWriter writer = new StreamWriter(cssFile);
                writer.Write(cssText);
                writer.Close();
                // Replace the node by other including the CSS file.
                IHTMLDOMNode newDomNode = (IHTMLDOMNode)iDoc.createElement("<link rel=\"stylesheet\" type=\"text/css\" href=\"embeddedstyles.css\" >");
                ((IHTMLDOMNode)style).replaceNode(newDomNode);
            }

            return cssFile;
        }

        /// <summary>
        /// Generate a XPS file for the document.
        /// </summary>
        private void BuildXps()
        {
            log("Generating XPS file");
            try
            {
                MSWord word = new MSWord();
                word.SaveWordToXps(Configuration.ArchivoOrigen, Configuration.XpsPath);
            }
            catch (Exception ex)
            {
                log(ex.Message);
                log("Something wrong happened with the XPS generation. Remember you must to have Microsoft Office 2007 and the" +
                        "pdf/xps generation add-in (http://www.microsoft.com/downloads/details.aspx?FamilyID=4D951911-3E7E-4AE6-B059-A2E79ED87041&displaylang=en)");
            }
        }

        private void ExecuteProjectCommandLine()
        {
            log("Executing '" + Configuration.CommandLine.Trim() + "'");
            string strCmdLine = "/C " + Configuration.CommandLine.Trim();
            ProcessStartInfo si = new System.Diagnostics.ProcessStartInfo("CMD.exe", strCmdLine);
            si.CreateNoWindow = false;
            si.UseShellExecute = false;
            si.RedirectStandardOutput = true;
            si.RedirectStandardError = true;
            Process p = new Process();
            p.StartInfo = si;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();
            p.WaitForExit();
            log(output);
            log(error);
        }

        private void ProcessHelpProject(string helpProjectFile)
        {
            if (Configuration.Compile)
            {
                // Due to some strange bug, if we have as current drive a network drive, the generated
                // help dont show the images... So, change it to the system drive:
                string cwd = Directory.GetCurrentDirectory();
                string tempDirectory = Path.GetDirectoryName(Configuration.HelpProjectDirectory);
                Directory.SetCurrentDirectory(tempDirectory);
                Compile(Configuration.ArchivoAyuda, AppSettings.CompilerPath);
                Directory.SetCurrentDirectory(cwd);
            }
            else if (Configuration.AbrirProyecto)
            {
                try
                {
                    // Abrir el proyecto de la ayuda
                    Process proceso = new Process();
                    proceso.StartInfo.FileName = helpProjectFile;
                    proceso.Start();
                }
                catch
                {
                    log("The project " + helpProjectFile + " cannot be opened" +
                        ". Have you installed the Microsoft Help Workshop ?");
                }
            }
        }

        /// <summary>
        /// Generate a PDF file for the document.
        /// </summary>
        private void BuildPdf()
        {
            try
            {
                log("Generating PDF file");
                if (Configuration.PdfGeneration == ChmProject.PdfGenerationWay.OfficeAddin)
                {
                    MSWord word = new MSWord();
                    word.SaveWordToPdf(Configuration.ArchivoOrigen, Configuration.PdfPath);
                }
                else
                {
                    PdfPrinter pdfPrinter = new PdfPrinter();
                    pdfPrinter.ConvertToPdf(Configuration.ArchivoOrigen, Configuration.PdfPath);
                }
            }
            catch (Exception ex)
            {
                log(ex.Message);
                if (Configuration.PdfGeneration == ChmProject.PdfGenerationWay.OfficeAddin)
                    log("Something wrong happened with the PDF generation. Remember you must to have Microsoft Office 2007 and the" +
                        "pdf/xps generation add-in (http://www.microsoft.com/downloads/details.aspx?FamilyID=4D951911-3E7E-4AE6-B059-A2E79ED87041&displaylang=en)");
                else
                    log("Something wrong happened with the PDF generation. Remember you must to have PdfCreator (version 0.9.3 tested only) installed into your computer to " +
                        "generate a PDF file. You can download it from http://www.pdfforge.org/products/pdfcreator/download");
                //throw ex;
            }
        }

        /// <summary>
        /// Generates help products.
        /// </summary>
        /// <returns>Path to help project generated.</returns>
        public string Generate() 
        {

            // Open and process source files
            OpenSourceFiles();

            if (CancellRequested())
                return null;

            if( esWord )
            {
                // Añadir a la lista de archivos adicionales el directorio generado con 
                // los archivos del documento word:
                string[] archivos = Directory.GetDirectories( dirHtml );
                foreach( string archivo in archivos ) 
                    ArchivosAdicionales.Add( archivo );
            }

            if (CancellRequested())
                return null;

            //if( !ArchivoCabecera.Equals("") ) 
            if (!Configuration.ChmHeaderFile.Equals("")) 
            {
                log("Reading header: " + Configuration.ChmHeaderFile);
                // Cargar el html del archivo de cabecera:
                StreamReader reader = new StreamReader(Configuration.ChmHeaderFile);
                HtmlCabecera = reader.ReadToEnd();
                reader.Close();
            }

            if (CancellRequested())
                return null;

            //if( !ArchivoPie.Equals("") ) 
            if (!Configuration.ChmFooterFile.Equals("")) 
            {
                log("Reading footer: " + Configuration.ChmFooterFile);
                // Cargar el html del archivo de cabecera:
                StreamReader reader = new StreamReader(Configuration.ChmFooterFile);
                HtmlPie = reader.ReadToEnd();
                reader.Close();
            }

            if (CancellRequested())
                return null;

            //if (!HtmlHeaderFile.Equals(""))
            if (!Configuration.WebHeaderFile.Equals(""))
            {
                log("Reading header: " + Configuration.WebHeaderFile);
                // Cargar el html del archivo de cabecera:
                StreamReader reader = new StreamReader(Configuration.WebHeaderFile);
                HtmlHeaderCode = reader.ReadToEnd();
                reader.Close();
            }

            if (CancellRequested())
                return null;

            //if (!HtmlFooterFile.Equals(""))
            if (!Configuration.WebFooterFile.Equals(""))
            {
                log("Reading footer: " + Configuration.WebFooterFile);
                // Cargar el html del archivo de cabecera:
                StreamReader reader = new StreamReader(Configuration.WebFooterFile);
                HtmlFooterCode = reader.ReadToEnd();
                reader.Close();
            }

            // Preparar el directorio de destino.
            log("Creating project directory: " + Configuration.HelpProjectDirectory);
            ArrayList listaFinalArchivos = GenerarDirDestino(Configuration.HelpProjectDirectory);

            // Check if there is a <STYLE> tag into the header. If there is, take it out to a CSS file.
            log("Extracting STYLE tags to a CSS file");
            string cssFile = CheckForStyleTags();
            if (cssFile != null)
                listaFinalArchivos.Add(cssFile);

            if (CancellRequested())
                return null;

            // Build the tree structure of chapters.
            log( "Searching sections");
            arbol = new ArbolCapitulos();
            //arbol.AnalizarDocumento( this.nivelCorte , iDoc.body );
            arbol.AnalizarDocumento( Configuration.CutLevel, iDoc.body);

            if (CancellRequested())
                return null;

            log( "Splitting file");
            // newBody is the <body> tag of the current splitted part 
            IHTMLElement newBody = Clone( iDoc.body );
            IHTMLElementCollection col = (IHTMLElementCollection)iDoc.body.children;
            // Traverse root nodes:
            foreach( IHTMLElement nodo in col ) 
            {
                //if( EsHeaderDeCorte( nivelCorte , nodo ) ) 
                if (EsHeaderDeCorte( Configuration.CutLevel, nodo)) 
                {
                    // Found start of a new part: Store the current body part.
                    GuardarParte(newBody);
                    newBody = Clone(iDoc.body);
                    InsertAfter(newBody, nodo);
                }
                else 
                {
                    ArrayList lista = Procesar( nodo );
                    foreach( IHTMLElement hijo in lista ) 
                    {
                        InsertAfter(newBody, hijo);

                        if( lista[ lista.Count - 1 ] != hijo ) 
                        {
                            // Si no es el ultimo, cerrar esta parte y abrir otra.
                            GuardarParte(newBody);
                            newBody = Clone(iDoc.body);
                        }
                    }
                }

                if (CancellRequested())
                    return null;
            }
            GuardarParte(newBody);

            if (CancellRequested())
                return null;

            // Generar los archivos HTML:
            log( "Storing splitted files");
            ArrayList archivosGenerados = GuardarDocumentos(Configuration.HelpProjectDirectory, HtmlCabecera, HtmlPie, null);

            if (CancellRequested())
                return null;

            // Mirar si al final se ha generado el archivo "1.htm". Si no, borrarlo
            // del arbol de archivos:
            string archivo1 = Configuration.HelpProjectDirectory + Path.DirectorySeparatorChar + "1.htm";
            if( ! File.Exists( archivo1) ) 
            {
                arbol.Raiz.Archivo = "";
                arbol.Raiz.Hijos.RemoveAt(0);
            }

            // Obtener el nombre del primer archivo generado:
            string primero = "";
            if( arbol.Raiz.Hijos.Count > 0 )
                primero = ((NodoArbol) arbol.Raiz.Hijos[0]).Archivo;

            if (CancellRequested())
                return null;

            // Generar archivo con arbol de contenidos:
            log( "Generating table of contents");
            arbol.GenerarArbolDeContenidos(Configuration.HelpProjectDirectory + Path.DirectorySeparatorChar + "toc-generado.hhc", Configuration.NivelArbolContenidos);
            
            if (CancellRequested())
                return null;

            // Generar archivo con palabras clave:
            log( "Generating index");
            Index index = arbol.GenerarIndice(Configuration.NivelTemasIndice);
            index.StoreHelpIndex(Configuration.HelpProjectDirectory + Path.DirectorySeparatorChar + "Index-generado.hhk");

            if (CancellRequested())
                return null;

            // Generar el archivo del proyecto de ayuda
            log( "Generating help project");
            string archivoAyuda = Configuration.HelpProjectDirectory + Path.DirectorySeparatorChar + NOMBREPROYECTO;
            GenerarArchivoProyecto( listaFinalArchivos , archivoAyuda , primero );

            if (CancellRequested())
                return null;

            //if( GenerarHtml ) 
            if( Configuration.GenerateWeb )
            {
                // Generar la web con la ayuda:
                log( "Generating web site");
                ProcesarHtml(Configuration.ArchivoOrigen, archivosGenerados, index, cssFile);
            }

            if (CancellRequested())
                return null;

            if (Configuration.GenerateJavaHelp)
            {
                log("Generating Java Help");
                GenerateJavaHelp(archivosGenerados, index, cssFile, Configuration.ArchivoOrigen);
            }

            if( esWord )
                // Era un doc word. Se creo un dir. temporal para guardar el html.
                // Borrar este directorio:
                Directory.Delete( dirHtml , true );

            log( "Project generated");

            return archivoAyuda;
        }

        static public string HtmlEncode(string textToEnconde)
        {
            return HtmlEncode(textToEnconde, true);
        }

        static public string HtmlEncode(string textToEnconde, bool encodeCharactersUpper255 )
        {
            char[] chars = HttpUtility.HtmlEncode(textToEnconde).ToCharArray();
            StringBuilder result = new StringBuilder(textToEnconde.Length + (int)(textToEnconde.Length * 0.1));

            foreach (char c in chars)
            {
                int value = Convert.ToInt32(c);
                if ( (value > 127 && encodeCharactersUpper255) || (value > 127 && value < 256 && !encodeCharactersUpper255 ) )
                    result.AppendFormat("&#{0};", value);
                else
                    result.Append(c);
            }

            return result.ToString();
        }

        public void GeneateSitemap(string webDirectory)
        {
            try {
                string sitemap = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + 
                                 "<urlset xmlns=\"http://www.google.com/schemas/sitemap/0.84\">\n";
                string webBase = this.Configuration.WebBase;
                if( !webBase.EndsWith("/") )
                    webBase += "/";
                if( !webBase.StartsWith("http://") )
                    webBase += "http://";

                string[] htmlFiles = Directory.GetFiles(webDirectory);
                foreach (string file in htmlFiles)
                {
                    string lowerFile = file.ToLower();
                    if (lowerFile.EndsWith(".htm") || lowerFile.EndsWith(".html"))
                    {
                        // Add to the sitemap
                        sitemap += "<url>\n<loc>" + webBase + Path.GetFileName(file) + "</loc>\n<lastmod>";
                        DateTime lastmod = File.GetLastWriteTime( file );
                        sitemap += lastmod.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszzz") + "</lastmod>\n";
                        sitemap += "<changefreq>" + this.Configuration.ChangeFrequency + "</changefreq>\n";
                        sitemap += "</url>\n";
                    }
                }
                sitemap += "</urlset>";

                // Store
                string sitemapFile = webDirectory + Path.DirectorySeparatorChar + "sitemap.xml";
                StreamWriter writer = new StreamWriter( sitemapFile , false , Encoding.UTF8 );
                writer.Write( sitemap );
                writer.Close();
                string sitemapZiped = webDirectory + Path.DirectorySeparatorChar + "sitemap.xml.gz";
                Zip.CompressFile(sitemapFile, sitemapZiped);
                File.Delete(sitemapFile);
            }
            catch( Exception ex ) {
                log( "Error generating the sitemap: " + ex.Message );
            }
        }

        /// <summary>
        /// Generates the help.hs help set xml file for java help
        /// </summary>
        /// <param name="dirJavaHelp">Directory where to generate the help.hs file</param>
        /// <param name="index">Index of topics of the document</param>
        void GenerateJavaHelpSetFile(String dirJavaHelp, Index index)
        {
            StreamWriter writer = new StreamWriter( dirJavaHelp + Path.DirectorySeparatorChar + "help.hs" ,
                false, Encoding.UTF8 );
            writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            writer.WriteLine("<!DOCTYPE helpset\n" + 
                "PUBLIC \"-//Sun Microsystems Inc.//DTD JavaHelp HelpSet Version 2.0//EN\"\n" + 
                "\"http://java.sun.com/products/javahelp/helpset_2_0.dtd\">");
            writer.WriteLine("<helpset version=\"2.0\">");
            writer.WriteLine("<title>" + Configuration.HelpTitle + "</title>");
            writer.WriteLine("<maps><homeID>" + index.FirstTopicTarget + "</homeID><mapref location=\"map.jhm\"/></maps>");
            writer.WriteLine("<view><name>TOC</name><label>Table Of Contents</label><type>javax.help.TOCView</type><data>toc.xml</data></view>");
            writer.WriteLine("<view><name>Index</name><label>Index</label><type>javax.help.IndexView</type><data>index.xml</data></view>");
            writer.WriteLine("<view><name>Search</name><label>Search</label><type>javax.help.SearchView</type><data engine=\"com.sun.java.help.search.DefaultSearchEngine\">JavaHelpSearch</data></view>");
            writer.WriteLine("</helpset>");
            writer.Close();
        }

        

        /// <summary>
        /// Generates a JAR with the java help of the document.
        /// <param name="generatedFiles">List of chapter html files generated for the help</param>
        /// <param name="index">List of topics of the document.</param>
        /// <param name="cssFile">CSS file of the document, if it was generated.</param>
        /// <param name="sourceFile">Path of the document converted to CHM</param>
        /// </summary>
        public void GenerateJavaHelp(ArrayList generatedFiles, Index index, string cssFile, string sourceFile)
        {
            
            // Create a temporal directy to generate the javahelp files:
            String dirJavaHelp = Path.GetTempPath() + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(sourceFile) + "-javahelp";
            log("Copiying files to directory " + dirJavaHelp);
            GenerarDirDestino(dirJavaHelp);

            // Copy the css file if was generated:
            if (cssFile != null)
                File.Copy(cssFile, dirJavaHelp + Path.DirectorySeparatorChar + Path.GetFileName(cssFile));

            // Copy files generated for the CHM help
            foreach (string file in generatedFiles )
            {
                string dstFile = dirJavaHelp + Path.DirectorySeparatorChar + Path.GetFileName(file);
                File.Copy(file, dstFile);
            }

            log("Generating java help xml files");
            // Generate the java help xml files:
            GenerateJavaHelpSetFile(dirJavaHelp, index);
            index.GenerateJavaHelpIndex(dirJavaHelp + Path.DirectorySeparatorChar + "index.xml");
            index.GenerateJavaHelpMapFile(dirJavaHelp + Path.DirectorySeparatorChar + "map.jhm");
            arbol.GenerateJavaHelpTOC(dirJavaHelp + Path.DirectorySeparatorChar + "toc.xml", Configuration.NivelArbolContenidos);

            log("Building the search index");
            log(AppSettings.JavaHelpIndexerPath + " .");
            executeCommandLine(AppSettings.JavaHelpIndexerPath, ".", dirJavaHelp);

            // Build a JAR with the help.
            //java -jar E:\dev\java\javahelp\javahelp2.0\demos\bin\hsviewer.jar -helpset help.jar
            string commandLine = " cvf \"" + Configuration.JavaHelpPath + "\" .";
            string jarPath = AppSettings.JarPath;
            log("Building jar:");
            log(jarPath + " " + commandLine);
            executeCommandLine(jarPath, commandLine, dirJavaHelp);

            Directory.Delete(dirJavaHelp, true);
        }

        /// <summary>
        /// Executes a command line and writes the command output to the log.
        /// </summary>
        /// <param name="exeFile">Path of the executable file to run</param>
        /// <param name="parameters">Parameters of the command line</param>
        /// <param name="workingDirectory">Directory where to run the command line</param>
        public void executeCommandLine(string exeFile, string parameters, string workingDirectory)
        {
            ProcessStartInfo info = new ProcessStartInfo(exeFile, parameters);
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.CreateNoWindow = true;
            info.WorkingDirectory = workingDirectory;

            Process proceso = Process.Start(info);
            while (!proceso.WaitForExit(1000))
                logStream(proceso.StandardOutput);
            logStream(proceso.StandardOutput);
        }

        private string ProcesarHtml(string sourceFile, ArrayList archivosGenerados, Index index, string cssFile ) 
        {
            // Crear el directorio web y copiar archivos adicionales:
            string dirWeb;
            //if( DirectorioWeb.Equals("") )
            if (Configuration.DirectorioWeb.Equals(""))
                dirWeb = Configuration.HelpProjectDirectory + Path.DirectorySeparatorChar + "web";
            else
                //dirWeb = DirectorioWeb;
                dirWeb = Configuration.DirectorioWeb;
            GenerarDirDestino( dirWeb );

            // Copy the css file if was generated:
            if (cssFile != null)
                File.Copy( cssFile , dirWeb + Path.DirectorySeparatorChar + Path.GetFileName(cssFile) );

            // Check if we can copy the generated files or we must to regenerate with other header 
            // Copy generated chapter files:
            //if (ArchivoCabecera.Equals(HtmlHeaderFile) && ArchivoPie.Equals(HtmlFooterFile) && !Configuration.FullTextSearch )
            if (Configuration.ChmHeaderFile.Equals(Configuration.WebHeaderFile) && Configuration.ChmFooterFile.Equals(Configuration.WebFooterFile) && !Configuration.FullTextSearch)
            {
                // Copy files generated for the CHM help
                foreach (string file in archivosGenerados)
                {
                    string archivoDst = dirWeb + Path.DirectorySeparatorChar + Path.GetFileName(file);
                    File.Copy(file, archivoDst);
                }
            }
            else
            {
                // Prepare the indexing database:
                WebIndex indexer = null;
                try
                {
                    if (Configuration.FullTextSearch)
                    {
                        indexer = new WebIndex();
                        string dbFile = dirWeb + Path.DirectorySeparatorChar + "fullsearchdb.db3";
                        string dirTextFiles = dirWeb + Path.DirectorySeparatorChar + "textFiles";
                        indexer.Connect(dbFile);
                        indexer.CreateDatabase(System.Windows.Forms.Application.StartupPath + Path.DirectorySeparatorChar + "searchdb.sql", dirTextFiles);
                        indexer.StoreConfiguration(Configuration.WebLanguage);
                    }

                    // Create new files for the web help:
                    GuardarDocumentos(dirWeb, HtmlHeaderCode, HtmlFooterCode, indexer);
                }
                finally
                {
                    if (indexer != null)
                        indexer.Disconnect();
                }
            }

            // Copy base files for web help:
            string keywordsMeta = "", descriptionMeta = "";
            //if( !WebKeywords.Trim().Equals( "" ) ) 
            if (!Configuration.WebKeywords.Trim().Equals("")) 
                //keywordsMeta = "<meta name=\"keywords\" content=\"" + WebKeywords + "\" >";
                keywordsMeta = "<meta name=\"keywords\" content=\"" + Configuration.WebKeywords + "\" >";
            //if( !WebDescription.Trim().Equals( "" ) ) 
            if (!Configuration.WebDescription.Trim().Equals("")) 
                //descriptionMeta = "<meta name=\"description\" content=\"" + WebDescription + "\" >";
                descriptionMeta = "<meta name=\"description\" content=\"" + Configuration.WebDescription + "\" >";

            // Convert title to windows-1252 enconding:
            string title = HtmlEncode(Configuration.HelpTitle);

            // Generate search form HTML code:
            string textSearch = "";
            if (Configuration.FullTextSearch)
            {
                textSearch = "<form name=\"searchform\" method=\"post\" action=\"search.aspx\" id=\"searchform\" onsubmit=\"doFullTextSearch();return false;\" >\n";
                textSearch += "<p><img src=\"system-search.png\" align=middle alt=\"Search image\" /> <b>%Search Text%:</b><br /><input type=\"text\" id=\"searchText\" style=\"width:80%;\" name=\"searchText\"/>\n";
                textSearch += "<input type=\"button\" value=\"%Search%\" onclick=\"doFullTextSearch();\" id=\"Button1\" name=\"Button1\"/></p>\n";
            }
            else
            {
                textSearch = "<form name=\"searchform\" method=\"post\" action=\"search.aspx\" id=\"searchform\" onsubmit=\"doSearch();return false;\" >\n";
                textSearch += "<p><img src=\"system-search.png\" align=middle alt=\"Search image\" /> <b>%Search Text%:</b><br /><input type=\"text\" id=\"searchText\" style=\"width:80%;\" name=\"searchText\"/><br/>\n";
                textSearch += "<input type=\"button\" value=\"%Search%\" onclick=\"doSearch();\" id=\"Button1\" name=\"Button1\"/></p>\n";
                textSearch += "<select id=\"searchResult\" style=\"width:100%;\" size=\"20\" name=\"searchResult\">\n";
                textSearch += "<option></option>\n";
                textSearch += "</select>\n";
            }
            textSearch += "</form>\n";

            string[] variables = { "%TEXTSEARCH%" , "%TITLE%", "%TREE%", "%TOPICS%", "%FIRSTPAGECONTENT%", 
                "%WEBDESCRIPTION%", "%KEYWORDS%" , "%HEADER%" , "%FOOTER%" };
            //string[] newValues = { textSearch , title, arbol.GenerarArbolHtml(NivelMaximoTOC, "contentsTree", 
            string[] newValues = { textSearch , title, arbol.GenerarArbolHtml(Configuration.NivelArbolContenidos, "contentsTree", 
                "contentTree"), index.GenerateWebIndex(), FirstChapterContent, descriptionMeta, 
                keywordsMeta , HtmlHeaderCode , HtmlFooterCode };
            string baseDir = System.Windows.Forms.Application.StartupPath + Path.DirectorySeparatorChar + "webFiles";
            string[] extensions = { ".htm", ".html" };
            Replacements replacements = new Replacements(variables, newValues);
            string translationFile = System.Windows.Forms.Application.StartupPath + 
                Path.DirectorySeparatorChar + "webTranslations" + Path.DirectorySeparatorChar + 
                Configuration.WebLanguage + ".txt";
            try
            {
                replacements.AddReplacementsFromFile(translationFile);
            }
            catch (Exception ex)
            {
                log("Error opening web translations file" + translationFile + ": " + ex.Message);
            }

            replacements.CopyDirectoryReplaced(baseDir, dirWeb, extensions , AppSettings.UseTidyOverOutput , ui );
            if (Configuration.FullTextSearch)
            {
                // Copy full text serch files:
                string[] aspxExtensions = { ".aspx" };
                string dirSearchFiles = System.Windows.Forms.Application.StartupPath + Path.DirectorySeparatorChar + "searchFiles";
                replacements.CopyDirectoryReplaced(dirSearchFiles, dirWeb, aspxExtensions, false, ui);
            }

            if (Configuration.GenerateSitemap)
                // Generate site map for web indexers (google).
                GeneateSitemap(dirWeb);

            return dirWeb + Path.DirectorySeparatorChar + "index.html";
        }

        /// <summary>
        /// Hace un copia de un archivo a otro, reemplazando un serie de textos por otros.
        /// </summary>
        /// <param name="pathOrigen"></param>
        /// <param name="pathDestino"></param>
        /// <param name="txtAReemplazar"></param>
        /// <param name="nuevoTexto"></param>
        private void CopyReplaced( string pathOrigen , string pathDestino , string[] txtAReemplazar , string[] nuevoTexto ) 
        {
            StreamReader reader = new StreamReader( pathOrigen );
            string texto = reader.ReadToEnd();
            reader.Close();
            //texto = texto.Replace( txtAReemplazar , nuevoTexto );
            for( int i=0; i<txtAReemplazar.Length; i++ ) 
                texto = texto.Replace( txtAReemplazar[i] , nuevoTexto[i] );
            StreamWriter writer = new StreamWriter( pathDestino );
            writer.WriteLine( texto );
            writer.Close();
        }

        private void CopyReplaced(string pathOrigen, string pathDestino, string txtAReemplazar, string nuevoTexto) 
        {
            CopyReplaced(pathOrigen, pathDestino, new string[] { txtAReemplazar } , new string[] { nuevoTexto });
        }

        static public Encoding HelpWorkshopEncoding {
            get
            {
                Encoding enc = Encoding.GetEncoding("windows-1252");
                if (enc == null)
                    enc = Encoding.Default;
                return enc;
            }
        }

        private void GenerarArchivoProyecto( ArrayList archivosAdicinales , string archivo , string temaInicial) 
        {
            StreamWriter writer = new StreamWriter(archivo, false, HelpWorkshopEncoding);
            writer.WriteLine( "[OPTIONS]" );
            writer.WriteLine( "Compatibility=1.1 or later" );
            writer.WriteLine( "Compiled file=" + NOMBREARCHIVOAYUDA );
            writer.WriteLine( "Contents file=toc-generado.hhc" );
            writer.WriteLine( "Default topic=" + temaInicial );
            writer.WriteLine( "Display compile progress=No" );
            writer.WriteLine( "Full-text search=Yes" );
            writer.WriteLine( "Index file=Index-generado.hhk" );
            //writer.WriteLine( "Language=0xc0a Español (alfabetización internacional)" );
            writer.WriteLine( "Language=0x0409 English (UNITED STATES)" );
            writer.WriteLine( "Title=" + Configuration.HelpTitle );
            writer.WriteLine( "\r\n[FILES]" );
            foreach( string archivoAdi in archivosAdicinales )
                writer.WriteLine( archivoAdi );
            ArrayList lista = arbol.ListaArchivosGenerados();
            foreach( string arc in lista )
                writer.WriteLine( arc );
            writer.WriteLine( "\r\n[INFOTYPES]\r\n" );
            writer.Close();
        }

        /// <summary>
        /// Return the first header tag (H1,H2,etc) found on a subtree of the html document 
        /// that will split the document.
        /// </summary>
        /// <param name="root">Root of the html subtree where to search a split</param>
        /// <returns>The first split tag node. null if none was found.</returns>
        private IHTMLElement SearchFirstCutNode( IHTMLElement root ) 
        {
            if (EsHeaderDeCorte(Configuration.CutLevel, root))
                return root;
            else 
            {
                IHTMLElementCollection col = (IHTMLElementCollection)root.children;
                foreach( IHTMLElement e in col ) 
                {
                    IHTMLElement seccion = SearchFirstCutNode( e );
                    if( seccion != null )
                        return seccion;
                }
                return null;
            }
        }

        private IHTMLAnchorElement BuscarNodoA( IHTMLElement raiz ) 
        {
            if( raiz is IHTMLAnchorElement )
                return (IHTMLAnchorElement)raiz;
            else 
            {
                IHTMLElementCollection col = (IHTMLElementCollection)raiz.children;
                foreach( IHTMLElement e in col ) 
                {
                    IHTMLAnchorElement seccion = BuscarNodoA( e );
                    if( seccion != null )
                        return seccion;
                }
                return null;
            }

        }

        /// <summary>
        /// Busca si un arbol contiene un corte de seccion.
        /// </summary>
        /// <param name="nodo">Raiz del arbol en que buscar</param>
        /// <returns>True si el arbol contiene un corte de seccion.</returns>
        private bool SeRompera( IHTMLElement nodo ) 
        {
            return SearchFirstCutNode( nodo ) != null;
        }

        /// <summary>
        /// Clone a node, without their children.
        /// </summary>
        /// <param name="nodo">Node to clone</param>
        /// <returns>Cloned node</returns>
        private IHTMLElement Clone(IHTMLElement nodo ) 
        {
            IHTMLElement e = iDoc.createElement( nodo.tagName );
            IHTMLElement2 e2 = (IHTMLElement2) e;
            e2.mergeAttributes( nodo );
            return e;
        }

        static public bool EsHeader( IHTMLElement nodo ) 
        {
            return nodo is IHTMLHeaderElement && nodo.innerText != null && !nodo.innerText.Trim().Equals("");
        }

        /// <summary>
        /// Comprueba si un nodo es un tag de cabecera de HTML (H1, H2, etc) hasta un nivel.
        /// Tambien comprueba que contenga algun tipo de texto.
        /// </summary>
        /// <param name="NivelMaximo">Nivel maximo del nodo a considerar: 1, 2, etc.</param>
        /// <param name="nodo">Nodo a verificar</param>
        /// <returns>true si el nodo cumple las condiciones</returns>
        static public bool EsHeaderDeCorte( int NivelMaximo , IHTMLElement nodo ) 
        {
            // Si es un nodo Hx y x <= NivelMaximo y contiene texto, es un nodo de corte:
            if( EsHeader(nodo) ) 
            {
                string tagName = nodo.tagName.ToUpper();
                for( int i=1;i<=NivelMaximo; i++ ) 
                {
                    string nombreTag = "H" + i;
                    if( nombreTag.Equals( tagName ) )
                        return true;
                }
            }
            return false;
        }
        
        private void InsertAfter( IHTMLElement destino , IHTMLElement origen ) 
        {
            try 
            {
                /*if (destino is IHTMLTable && origen.tagName.ToUpper().Equals("TBODY") )
                {
                    IHTMLElementCollection col = (IHTMLElementCollection)origen.children;
                    foreach (IHTMLElement e in col)
                        destino.insertAdjacentHTML("beforeEnd", e.outerHTML);
                }
                else*/
                //destino.insertAdjacentHTML( "beforeEnd" , origen.outerHTML );
                ((IHTMLDOMNode)destino).appendChild((IHTMLDOMNode)origen);
            }
            catch( Exception ex ) {
                log("Warning: error adding a child (" + origen.tagName + ") to his parent (" +
                     destino.tagName + "): " + ex.Message);
            }
        }

        private ArrayList Procesar( IHTMLElement nodo ) 
        {
            ArrayList lista = new ArrayList();

            // Mirar si el nodo se va a romper en varios trozos:
            if( SeRompera( nodo ) ) 
            {
                IHTMLElementCollection col = (IHTMLElementCollection)nodo.children;
                IHTMLElement nuevoNodo = Clone( nodo );
                foreach( IHTMLElement e in col ) 
                {

                    if (EsHeaderDeCorte(Configuration.CutLevel, e))
                    {
                        //RegistrarParte( e );
                        // Se encontro un header. Cortar aqui.
                        lista.Add( nuevoNodo );
                        nuevoNodo = Clone( nodo );
                        //nuevoNodo.insertAdjacentHTML( "beforeEnd" , e.outerHTML );
                        InsertAfter( nuevoNodo , e );
                    }
                    else 
                    {
                        ArrayList listaHijos = Procesar( e );
                        foreach( IHTMLElement hijo in listaHijos ) 
                        {
                            InsertAfter( nuevoNodo , hijo );
                            if( listaHijos[ listaHijos.Count - 1 ] != hijo ) 
                            {
                                // Si no es el ultimo, cerrar esta parte y abrir otra.
                                lista.Add( nuevoNodo );
                                nuevoNodo = Clone( nodo );
                            }
                        }
                    }
                }
                lista.Add( nuevoNodo );
            }
            else 
                lista.Add( nodo );

            return lista;
        }

        private void logStream( StreamReader reader ) 
        {
            string linea = reader.ReadLine();
            while( linea != null ) 
            {
                log( linea );
                linea = reader.ReadLine();
            }
        }

        /// <summary>
        /// Compiles the help project file and it's copied to the destination file.
        /// </summary>
        /// <param name="helpFile">Project help project full path.</param>
        /// <param name="compilerPath">Compiler exe (hhc.exe) full path.</param>
        public void Compile( string helpFile , string compilerPath ) 
        {
            log( "Compiling");
            if( ! File.Exists( compilerPath ) )
                throw new Exception("Compiler not found at " + compilerPath + ". Help not generated");
            else 
            {
                string proyecto = "\"" + Configuration.HelpProjectDirectory + Path.DirectorySeparatorChar + NOMBREPROYECTO + "\"";
                ProcessStartInfo info = new ProcessStartInfo( compilerPath , proyecto );
                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;
                info.CreateNoWindow = true;
                Process proceso = Process.Start( info );
                while( ! proceso.WaitForExit( 1000 ) ) 
                    logStream( proceso.StandardOutput );
                logStream( proceso.StandardOutput );

                string archivoAyudaOrigen = Configuration.HelpProjectDirectory + Path.DirectorySeparatorChar + NOMBREARCHIVOAYUDA;
                if( File.Exists( archivoAyudaOrigen ) ) 
                    // Copy the file frrom the temporally directory to the gift by the user
                    File.Copy( archivoAyudaOrigen , helpFile , true );
                else 
                    throw new Exception("Some error happened with the compilation. Try to generate the help project");
            }
        }
    }
}

