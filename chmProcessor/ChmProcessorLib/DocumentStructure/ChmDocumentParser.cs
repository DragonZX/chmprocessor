using System;
using System.Collections.Generic;
using System.Text;
using mshtml;

namespace ChmProcessorLib.DocumentStructure
{

    /// <summary>
    /// Tool to create a ChmDocument from a HTML document.
    /// </summary>
    public class ChmDocumentParser
    {
        /// <summary>
        /// The document we are currently parsing
        /// </summary>
        private ChmDocument Document;

        /// <summary>
        /// Log to generate messages.
        /// </summary>
        private UserInterface UI;

        /// <summary>
        /// Information about how to split the document (and other things not used here)
        /// </summary>
        private ChmProject Project;

        /// <summary>
        /// Latest header node inserted on the document
        /// </summary>
        private ChmDocumentNode LastedNodeInserted;

        /// <summary>
        /// True if the parse process must clear broken links
        /// </summary>
        private bool ReplaceBrokenLinks;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="iDoc">The HTML document to parse</param>
        /// <param name="ui">Log generation object</param>
        /// <param name="project">Information about how to split the document</param>
        public ChmDocumentParser(IHTMLDocument2 iDoc, UserInterface ui, ChmProject project)
        {
            // Create the empty document:
            Document = new ChmDocument(iDoc);
            UI = ui;
            Project = project;
            ReplaceBrokenLinks = AppSettings.ReplaceBrokenLinks;
        }

        /// <summary>
        /// Makes a parse of the headers structure of the document
        /// </summary>
        /// <returns>The parsed dococument</returns>
        public ChmDocument ParseDocument()
        {
            UI.log("Searching sections", ConsoleUserInterface.INFO);

            // Build a node for the content without an initial title
            ChmDocumentNode noSectionStart = new ChmDocumentNode(Document.RootNode, null, UI);
            Document.RootNode.Children.Add(noSectionStart);

            // Parse recursivelly the document headers structure by headers sections
            ParseHeaderStructure(Document.IDoc.body);

            if (UI.CancellRequested())
                return null;

            // By default, all document goes to the section without any title
            Document.RootNode.StoredAt(ChmDocument.INITIALSECTIONFILENAME);

            // Now assign filenames where will be stored each section.
            int cnt = 2;
            foreach (ChmDocumentNode hijo in Document.RootNode.Children)
                SplitFilesStructure(hijo, ref cnt);

            if (UI.CancellRequested())
                return null;

            // Split the document content:
            UI.log("Splitting file", ConsoleUserInterface.INFO);
            // TODO: This method content and all descendants are pure crap: Make a rewrite
            SplitContent();

            if (UI.CancellRequested())
                return null;

            // Join empty nodes:
            UI.log("Joining empty document sections", ConsoleUserInterface.INFO);
            JoinEmptyNodes();

            if (UI.CancellRequested())
                return null;

            // Change internal document links to point to the splitted files. Optionally repair broken links.
            UI.log("Changing internal links", ConsoleUserInterface.INFO);
            ChangeInternalLinks(Document.RootNode);

            if (UI.CancellRequested())
                return null;

            // Create the document index
            UI.log("Creating document index", ConsoleUserInterface.INFO);
            CreateDocumentIndex();

            // Extract the embedded CSS styles of the document:
            UI.log("Extracting CSS STYLE header tags", ConsoleUserInterface.INFO);
            CheckForStyleTags();

            return Document;
        }

        /// <summary>
        /// Make a recursive seach of all HTML header nodes into the document.
        /// </summary>
        /// <param name="currentNode">Current HTML node on the recursive search</param>
        private void ParseHeaderStructure(IHTMLElement currentNode)
        {
            if (UI.CancellRequested())
                return;

            if (currentNode is IHTMLHeaderElement)
                AddHeaderNode(currentNode);

            IHTMLElementCollection col = (IHTMLElementCollection)currentNode.children;
            foreach (IHTMLElement hijo in col)
                ParseHeaderStructure(hijo);
        }

        /// <summary>
        /// Adds a section to the section tree.
        /// The section will be added as child of the last section inserted with a level
        /// higher than then section
        /// TODO: Remove LastedNodeInserted member and put a parameter with the chapters current path on this function
        /// </summary>
        /// <param name="nodo">HTML header tag with the title of the section</param>
        /// <param name="ui">Application log. It can be null</param>
        private void AddHeaderNode(IHTMLElement node)
        {
            // Ignore empty headers (line breaks, etc)
            if (!DocumentProcessor.EsHeader(node))
                return;

            int headerLevel = ChmDocumentNode.HeaderTagLevel(node);
            if (LastedNodeInserted == null || headerLevel == 1)
            {
                // Add a document main section 
                LastedNodeInserted = new ChmDocumentNode(null, node, UI);
                Document.RootNode.AddChild(LastedNodeInserted);
            }
            else
            {
                // And a subsection
                ChmDocumentNode newNode = new ChmDocumentNode(LastedNodeInserted, node, UI);
                if (LastedNodeInserted.HeaderLevel < headerLevel)
                    // Its a child section of the last inserted node
                    LastedNodeInserted.Children.Add(newNode);
                else
                {
                    // Its a sibling node of the last inserted node.
                    ChmDocumentNode actual = LastedNodeInserted.Parent;
                    while (actual != Document.RootNode && actual.HeaderLevel >= headerLevel)
                        actual = actual.Parent;
                    actual.AddChild(newNode);
                }
                LastedNodeInserted = newNode;
            }
        }

        /// <summary>
        /// Splits recursivelly the nodes on the document by the cut level on the chmprocessor project.
        /// Each splitted node will store its own file.
        /// </summary>
        /// <param name="node">Node to check recursivelly</param>
        /// <param name="Cnt">Counter to assign unique file names</param>
        private void SplitFilesStructure(ChmDocumentNode node, ref int Cnt)
        {
            if (node.HeaderTag != null && DocumentProcessor.IsCutHeader(Project.CutLevel, node.HeaderTag))
                node.StoredAt(node.NombreArchivo(Cnt++));

            foreach (ChmDocumentNode hijo in node.Children)
                SplitFilesStructure(hijo, ref Cnt);
        }

        /// <summary>
        /// Returns true if the tag is a non empty (=containts some text) header (h1, h2, etc)
        /// </summary>
        /// <param name="tag">Tag to check</param>
        /// <returns>True if its a non empty header tag</returns>
        static private bool EsHeader(IHTMLElement tag)
        {
            return tag is IHTMLHeaderElement && tag.innerText != null && !tag.innerText.Trim().Equals("");
        }

        /// <summary>
        /// Checks if a node is a HTML header tag (H1, H2, etc) upper or equal to the cut level
        /// defined by the project settings
        /// Also checks if it contains some text.
        /// </summary>
        /// <param name="node">HTML node to check</param>
        /// <returns>true if the tag is a non empty cut header</returns>
        private bool IsCutHeader(IHTMLElement node)
        {
            // If its a Hx node and x <= MaximumLevel, and it contains text, its a cut node:
            if (!EsHeader(node))
                return false;

            string tagName = node.tagName.ToLower().Trim();
            int headerLevel = int.Parse(tagName.Substring(1));
            return headerLevel <= Project.CutLevel;
        }

        /// <summary>
        /// Clone a node, without their children.
        /// </summary>
        /// <param name="nodo">Node to clone</param>
        /// <returns>Cloned node</returns>
        private IHTMLElement Clone(IHTMLElement nodo)
        {
            IHTMLElement e = Document.IDoc.createElement(nodo.tagName);
            IHTMLElement2 e2 = (IHTMLElement2)e;
            e2.mergeAttributes(nodo);
            return e;
        }

        /// <summary>
        /// Return the first header tag (H1,H2,etc) found on a subtree of the html document 
        /// that will split the document.
        /// TODO: Join this function and DocumentProcessor.SearchFirstCutNode
        /// </summary>
        /// <param name="root">Root of the html subtree where to search a split</param>
        /// <returns>The first split tag node. null if none was found.</returns>
        private IHTMLElement SearchFirstCutNode(IHTMLElement root)
        {
            if (IsCutHeader(root))
                return root;
            else
            {
                IHTMLElementCollection col = (IHTMLElementCollection)root.children;
                foreach (IHTMLElement e in col)
                {
                    IHTMLElement seccion = SearchFirstCutNode(e);
                    if (seccion != null)
                        return seccion;
                }
                return null;
            }
        }

        private IHTMLAnchorElement BuscarNodoA(IHTMLElement raiz)
        {
            if (raiz is IHTMLAnchorElement)
                return (IHTMLAnchorElement)raiz;
            else
            {
                IHTMLElementCollection col = (IHTMLElementCollection)raiz.children;
                foreach (IHTMLElement e in col)
                {
                    IHTMLAnchorElement seccion = BuscarNodoA(e);
                    if (seccion != null)
                        return seccion;
                }
                return null;
            }

        }

        private void GuardarParte(IHTMLElement nuevoBody)
        {
            IHTMLElement sectionHeader = SearchFirstCutNode(nuevoBody);
            ChmDocumentNode nodeToStore = null;
            if (sectionHeader == null)
            {
                if (Document.RootNode.Children.Count > 0)
                {
                    // If no section was found, its the first section of the document:
                    nodeToStore = Document.RootNode.Children[0];
                }
            }
            else
            {
                string aName = "";
                IHTMLAnchorElement a = BuscarNodoA(sectionHeader);
                if (a != null && a.name != null)
                    aName = ChmDocumentNode.ToSafeFilename(a.name);
                nodeToStore = Document.RootNode.BuscarNodo(sectionHeader, aName);
            }

            if (nodeToStore == null)
            {
                string errorMessage = "Error searching node ";
                if (sectionHeader != null)
                    errorMessage += sectionHeader.innerText;
                else
                    errorMessage += "<empty>";
                Exception error = new Exception(errorMessage);
                UI.log(error);
            }
            else
            {
                nodeToStore.SplittedPartBody = nuevoBody;
                nodeToStore.BuildListOfContainedANames();  // Store the A name's tags of the body.
            }
        }

        /// <summary>
        /// Adds a HTML node as child of other.
        /// The child node is added at the end of the list of parent children.
        /// </summary>
        /// <param name="parent">Parent witch to add the new node</param>
        /// <param name="child">The child node to add</param>
        private void InsertAfter(IHTMLElement parent, IHTMLElement child)
        {
            try
            {
                ((IHTMLDOMNode)parent).appendChild((IHTMLDOMNode)child);
            }
            catch (Exception ex)
            {
                UI.log("Warning: error adding a child (" + child.tagName + ") to his parent (" +
                     parent.tagName + "): " + ex.Message, ConsoleUserInterface.ERRORWARNING);
                UI.log(ex);
            }
        }

        /// <summary>
        /// Busca si un arbol contiene un corte de seccion.
        /// </summary>
        /// <param name="nodo">Raiz del arbol en que buscar</param>
        /// <returns>True si el arbol contiene un corte de seccion.</returns>
        private bool WillBeBroken(IHTMLElement nodo)
        {
            return SearchFirstCutNode(nodo) != null;
        }

        /// <summary>
        /// Process a HTML node of the document
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <returns>
        /// A list of subtrees of the HTML original tree broken by the cut level headers.
        /// If the node and their descendands have not cut level headers, the node will be returned as is.
        /// </returns>
        private List<IHTMLElement> ProcessNode(IHTMLElement node)
        {
            List<IHTMLElement> subtreesList = new List<IHTMLElement>();

            // Check if the node will be broken on more than one piece because it contains a cut level
            // header:
            if (WillBeBroken(node))
            {
                // It contains a cut level.
                IHTMLElementCollection children = (IHTMLElementCollection)node.children;
                IHTMLElement newNode = Clone(node);
                foreach (IHTMLElement e in children)
                {
                    if (IsCutHeader(e))
                    {
                        // A cut header was found. Cut here.
                        subtreesList.Add(newNode);
                        newNode = Clone(node);
                        InsertAfter(newNode, e);
                    }
                    else
                    {
                        List<IHTMLElement> listaHijos = ProcessNode(e);
                        foreach (IHTMLElement hijo in listaHijos)
                        {
                            InsertAfter(newNode, hijo);
                            if (listaHijos[listaHijos.Count - 1] != hijo)
                            {
                                // Si no es el ultimo, cerrar esta parte y abrir otra.
                                subtreesList.Add(newNode);
                                newNode = Clone(node);
                            }
                        }
                    }
                }
                subtreesList.Add(newNode);
            }
            else
                // The node and their children will not broken because it does not contains any
                // cut level (or upper) header title. So, add the node and their children as they are:
                subtreesList.Add(node);

            return subtreesList;
        }

        /// <summary>
        /// Splits the HTML document body and assigns each part to a node of the document tree.
        /// </summary>
        private void SplitContent()
        {
            // newBody is the <body> tag of the current splitted part 
            IHTMLElement newBody = Clone(Document.IDoc.body);
            IHTMLElementCollection col = (IHTMLElementCollection)Document.IDoc.body.children;
            // Traverse root nodes:
            foreach (IHTMLElement nodo in col)
            {
                if (IsCutHeader(nodo))
                {
                    // Found start of a new part: Store the current body part.
                    GuardarParte(newBody);
                    newBody = Clone(Document.IDoc.body);
                    InsertAfter(newBody, nodo);
                }
                else
                {
                    List<IHTMLElement> lista = ProcessNode(nodo);
                    foreach (IHTMLElement hijo in lista)
                    {
                        InsertAfter(newBody, hijo);

                        if (lista[lista.Count - 1] != hijo)
                        {
                            // Si no es el ultimo, cerrar esta parte y abrir otra.
                            GuardarParte(newBody);
                            newBody = Clone(Document.IDoc.body);
                        }
                    }
                }

                if (UI.CancellRequested())
                    return;
            }
            GuardarParte(newBody);
        }

        /// <summary>
        /// Makes a recursive search on the document to join empty nodes (node with a title and without 
        /// any other content) with other nodes with content
        /// <param name="nodo">Current node on the recursive search to check</param>
        /// </summary>
        private void JoinEmptyNodes(ChmDocumentNode nodo)
        {
            try
            {
                if (UI.CancellRequested())
                    return;

                if (nodo.HeaderTag != null && nodo.HeaderTag.innerText != null && nodo.SplittedPartBody != null)
                {
                    // Nodo con cuerpo:

                    if (nodo.HeaderTag.innerText.Trim().Equals(nodo.SplittedPartBody.innerText.Trim()) &&
                        nodo.Children.Count > 0)
                    {
                        // Nodo vacio y con hijos 
                        ChmDocumentNode hijo = (ChmDocumentNode)nodo.Children[0];
                        if (hijo.SplittedPartBody != null)
                        {
                            // El hijo tiene cuerpo: Unificarlos.
                            nodo.SplittedPartBody.insertAdjacentHTML("beforeEnd", hijo.SplittedPartBody.innerHTML);
                            hijo.SplittedPartBody = null;
                            hijo.ReplaceFile(nodo.DestinationFileName);
                        }
                    }
                }

                foreach (ChmDocumentNode hijo in nodo.Children)
                    JoinEmptyNodes(hijo);
            }
            catch (Exception ex)
            {
                UI.log(new Exception("There was some problem when we tried to join the empty section " +
                    nodo.Title + " with their children", ex));
            }
        }

        /// <summary>
        /// Joins empty nodes (node with a title and without any other content) with other 
        /// nodes with content
        /// </summary>
        private void JoinEmptyNodes()
        {
            // Intentar unificar nodos que quedarian vacios, con solo el titulo de la seccion:
            foreach (ChmDocumentNode nodo in Document.RootNode.Children)
            {
                JoinEmptyNodes(nodo);

                if (UI.CancellRequested())
                    return;
            }
        }

        private void CreateDocumentIndex(ChmDocumentNode nodo, int nivel)
        {
            if (Project.MaxHeaderIndex != 0 && nivel > Project.MaxHeaderIndex)
                return;

            Document.Index.Add(nodo);
            foreach (ChmDocumentNode hijo in nodo.Children)
                CreateDocumentIndex(hijo, nivel + 1);
        }

        private void CreateDocumentIndex()
        {
            Document.Index = new ChmDocumentIndex();
            foreach (ChmDocumentNode hijo in Document.RootNode.Children)
                CreateDocumentIndex(hijo, 1);
        }

        /// <summary>
        /// Repair or remove an internal link.
        /// Given a broken internal link, it searches a section title of the document with the
        /// same text of the broken link. If its found, the destination link is modified to point to
        /// that section. If a matching section is not found, the link will be removed and its content
        /// will be keept.
        /// </summary>
        /// <param name="link">The broken link</param>
        private void ReplaceBrokenLink(IHTMLAnchorElement link)
        {
            try
            {
                // Get the text of the link
                string linkText = ((IHTMLElement)link).innerText.Trim();
                // Seach a title with the same text of the link:
                ChmDocumentNode destinationTitle = Document.SearchBySectionTitle(linkText);
                if (destinationTitle != null)
                    // Replace the original internal broken link with this:
                    link.href = destinationTitle.Href;
                else
                {
                    // No candidate title was found. Remove the link and keep its content
                    IHTMLElementCollection linkChildren = (IHTMLElementCollection)((IHTMLElement)link).children;
                    IHTMLDOMNode domLink = (IHTMLDOMNode)link;
                    IHTMLDOMNode domParent = (IHTMLDOMNode) ((IHTMLElement)link).parentElement;
                    foreach (IHTMLElement child in linkChildren)
                        domParent.insertBefore((IHTMLDOMNode)child, domLink);
                    domLink.removeNode(false);
                }
            }
            catch (Exception ex)
            {
                UI.log("Error reparining a broken link", ConsoleUserInterface.ERRORWARNING);
                UI.log(ex);
            }
        }

        /// <summary>
        /// Makes a recursive search on the document tree to change internal document links to point to the 
        /// splitted files. Optionally repair broken links.
        /// </summary>
        /// <param name="node">Current node on the recursive search</param>
        private void ChangeInternalLinks(IHTMLElement node)
        {
            try
            {
                if (node is IHTMLAnchorElement)
                {
                    IHTMLAnchorElement link = (IHTMLAnchorElement)node;
                    string href = link.href;
                    if (href != null)
                    {
                        // An hyperlink node

                        // Remove the about:blank
                        // TODO: Check if this is really needed.
                        href = href.Replace("about:blank", "").Replace("about:", "");

                        if (href.StartsWith("#"))
                        {
                            // A internal link.
                            // Replace it to point to the right splitted file.
                            string safeRef = ChmDocumentNode.ToSafeFilename(href.Substring(1));
                            ChmDocumentNode nodoArbol = Document.RootNode.BuscarEnlace(safeRef);
                            if (nodoArbol != null)
                                link.href = nodoArbol.DestinationFileName + "#" + safeRef;
                            else
                            {
                                // Broken link.
                                UI.log("WARNING: Broken link with text: '" + node.innerText + "'", ConsoleUserInterface.ERRORWARNING);
                                //if (parent != null)
                                if( node.parentElement != null )
                                {
                                    String inText = node.parentElement.innerText;
                                    if (inText != null)
                                    {
                                        if (inText.Length > 200)
                                            inText = inText.Substring(0, 200) + "...";
                                        UI.log(" near of text: '" + inText + "'", ConsoleUserInterface.ERRORWARNING);
                                    }
                                }
                                if (ReplaceBrokenLinks)
                                    ReplaceBrokenLink(link);
                            }

                        }
                    }
                    else if (link.name != null)
                    {
                        // A HTML "boomark", the destination of a link.
                        string safeName = ChmDocumentNode.ToSafeFilename(link.name);
                        if (!link.name.Equals(safeName))
                        {
                            // Word bug? i have found names with space characters and other bad things. 
                            // They fail into the CHM:
                            //link.name = link.name.Replace(" ", ""); < NOT WORKS
                            IHTMLDOMNode domNodeParent = (IHTMLDOMNode)node.parentElement;
                            string htmlNewNode = "<a name=" + safeName + "></a>";
                            IHTMLDOMNode newDomNode = (IHTMLDOMNode)Document.IDoc.createElement(htmlNewNode);
                            domNodeParent.replaceChild(newDomNode, (IHTMLDOMNode)node);
                        }
                    }
                }

                IHTMLElementCollection col = (IHTMLElementCollection)node.children;
                foreach (IHTMLElement child in col)
                    ChangeInternalLinks(child);
            }
            catch (Exception ex)
            {
                UI.log(ex);
            }
        }

        /// <summary>
        /// Makes a recursive search on the document tree to change internal document links to point to the 
        /// splitted files. Optionally repair broken links.
        /// </summary>
        /// <param name="node">Current node on the recursive search</param>
        private void ChangeInternalLinks(ChmDocumentNode node) 
        {
            if (node.SplittedPartBody != null)
                ChangeInternalLinks(node.SplittedPartBody);

            foreach (ChmDocumentNode child in node.Children)
                ChangeInternalLinks(child);
        }

        /// <summary>
        /// Extracts the style tag of the document and save it into the document
        /// </summary>
        private void CheckForStyleTags()
        {
            IHTMLDocument3 iDoc3 = (IHTMLDocument3)Document.IDoc;
            IHTMLDOMChildrenCollection col = (IHTMLDOMChildrenCollection)iDoc3.childNodes;
            IHTMLHeadElement head = null;
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

            if (style != null && style.innerHTML != null)
            {
                // Remove comments and save the CSS contents:
                Document.EmbeddedStylesTagContent = style.innerHTML.Replace("<!--", "").Replace("-->", "");
                // Replace the node by other including the CSS file.
                IHTMLDOMNode newDomNode = (IHTMLDOMNode)Document.IDoc.createElement("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + ChmDocument.EMBEDDEDCSSFILENAME + "\" >");
                ((IHTMLDOMNode)style).replaceNode(newDomNode);
            }

        }

    }
}
