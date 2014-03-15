using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;
using System.Web;
using ChmProcessorLib.Log;

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
        /// Reserved node for the initial content of the document without any title.
        /// </summary>
        private ChmDocumentNode InitialNode;

        /// <summary>
        /// True if the parse process must clear broken links
        /// </summary>
        private bool ReplaceBrokenLinks;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="htmlDoc">The HTML document to parse</param>
        /// <param name="ui">Log generation object</param>
        /// <param name="project">Information about how to split the document</param>
        public ChmDocumentParser(HtmlDocument htmlDoc, UserInterface ui, ChmProject project)
        {
            // Create the empty document:
            Document = new ChmDocument(htmlDoc);
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
            UI.Log("Searching sections", ChmLogLevel.INFO);

            // Build a node for the content without an initial title
            InitialNode = new ChmDocumentNode(Document, Document.RootNode, null, UI);
            Document.RootNode.Children.Add(InitialNode);

            if (Document.Body == null)
                throw new Exception("The document does not have a body tag. It's not valid HTML");

            // Parse recursivelly the document headers structure by headers sections
            ParseHeaderStructure(Document.Body);

            if (UI.CancellRequested())
                return null;

            // By default, all document goes to the section without any title
            foreach (ChmDocumentNode child in Document.RootNode.Children)
                child.StoredAt(ChmDocument.INITIALSECTIONFILENAME);

            // Now assign filenames where will be stored each section.
            int cnt = 1;
            foreach (ChmDocumentNode child in Document.RootNode.Children)
                SplitFilesStructure(child, ref cnt);

            if (UI.CancellRequested())
                return null;

            // Split the document content:
            UI.Log("Splitting file", ChmLogLevel.INFO);
            // TODO: This method content and all descendants are pure crap: Make a rewrite
            SplitContent();

            if (UI.CancellRequested())
                return null;

            // Join empty nodes:
            UI.Log("Joining empty document sections", ChmLogLevel.INFO);
            JoinEmptyNodes();

            if (UI.CancellRequested())
                return null;

            // Change internal document links to point to the splitted files. Optionally repair broken links.
            UI.Log("Changing internal links", ChmLogLevel.INFO);
            ChangeInternalLinks(Document.RootNode);

            if (UI.CancellRequested())
                return null;

            // Extract the embedded CSS styles of the document:
            UI.Log("Extracting CSS STYLE header tags", ChmLogLevel.INFO);
            CheckForStyleTags();

            // If the initial node for content without title is empty, remove it:
            // There is two cases:
            if (Project.CutLevel == 0)
            {
                // A single page will be created: All the content is into the InitialNode.
                // If there is some title, move the content to the first one and remove the initial
                if (Document.RootNode.Children.Count >= 2)
                {
                    ChmDocumentNode firstTitle = Document.RootNode.Children[1];
                    Document.RootNode.Children.Remove(InitialNode);
                    firstTitle.SplittedPartBody = InitialNode.SplittedPartBody;
                }
            }
            else {
                // More than one page will be created. If the initial node has no content, remove it.
                if (InitialNode.EmptyTextContent)
                    Document.RootNode.Children.Remove(InitialNode);
            }

            if (UI.CancellRequested())
                return null;

            // Create the document and pages index
            UI.Log("Creating document index", ChmLogLevel.INFO);
            CreateDocumentIndex();
            CreatePagesIndex();

            return Document;
        }

        /// <summary>
        /// Make a recursive seach of all HTML header nodes into the document.
        /// </summary>
        /// <param name="currentNode">Current HTML node on the recursive search</param>
        private void ParseHeaderStructure(HtmlNode currentNode)
        {
            if (UI.CancellRequested())
                return;

            if ( IsHeader(currentNode) )
                AddHeaderNode(currentNode);

            foreach( HtmlNode child in currentNode.ChildNodes )
                ParseHeaderStructure(child);
        }

        /// <summary>
        /// Adds a section to the section tree.
        /// The section will be added as child of the last section inserted with a level
        /// higher than then section
        /// </summary>
        /// <param name="nodo">HTML header tag with the title of the section</param>
        /// <param name="ui">Application log. It can be null</param>
        private void AddHeaderNode(HtmlNode node)
        {
            // Ignore empty headers (line breaks, etc)
            if (!IsNonEmptyHeader(node))
                return;

            int headerLevel = ChmDocumentNode.HeaderTagLevel(node);
            if (LastedNodeInserted == null || headerLevel == 1)
            {
                // Add a document main section 
                LastedNodeInserted = new ChmDocumentNode(Document, null, node, UI);
                Document.RootNode.AddChild(LastedNodeInserted);
            }
            else
            {
                // And a subsection
                ChmDocumentNode newNode = new ChmDocumentNode(Document, LastedNodeInserted, node, UI);
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
            if (node.HeaderTag != null && IsCutHeader(node.HeaderTag))
                node.StoredAt(node.NombreArchivo(Cnt++));

            foreach (ChmDocumentNode hijo in node.Children)
                SplitFilesStructure(hijo, ref Cnt);
        }

        /// <summary>
        /// Returns the inner text of the node unescaping HTML codes (&amp;nbsp; as example)
        /// </summary>
        /// <param name="node">The node to get the text</param>
        /// <returns>The unescaped text of the node</returns>
        static public string UnescapedInnerText(HtmlNode node)
        {
            if (node == null)
                return null;

            string innerTextUnescaped = node.InnerText;
            if (innerTextUnescaped == null)
                return null;
            return HttpUtility.HtmlDecode(innerTextUnescaped);
        }

        /// <summary>
        /// Returns true if the node is HTML header (h1, h2, etc)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        static private bool IsHeader(HtmlNode node)
        {
            if (!node.Name.ToLower().StartsWith("h"))
                return false;
            if (node.Name.Length <= 1)
                return false;
            return Char.IsDigit(node.Name[1]);
        }

        static public string GetAnchorName(HtmlNode node)
        {
            if (node.Name.ToLower() != "a")
                return null;

            // Check for HTML4 name attribute:
            string name = GetAttributeValue(node, "name");
            if (name != null && name.Trim() != string.Empty)
                return name;

            // Check for XHTML id attribute:
            name = GetAttributeValue(node, "id");
            if (name != null && name.Trim() != string.Empty)
            {
                // This can be a link, not an anchor: Check it:
                string href = GetAttributeValue(node, "href");
                if (href != null && href.Trim() != string.Empty)
                    return null;
                return name;
            }

            return null;
        }

        /// <summary>
        /// Returns true if the tag is a non empty (=containts some text) header (h1, h2, etc)
        /// </summary>
        /// <param name="tag">Tag to check</param>
        /// <returns>True if its a non empty header tag</returns>
        static private bool IsNonEmptyHeader(HtmlNode tag)
        {
            if( !IsHeader(tag) )
                return false;
            // HAP inner text is HTML encoded (with &nbsp; and so)
            string innerText = UnescapedInnerText(tag);
            if( innerText.Trim() == string.Empty )
                return false;
            return true;
        }

        /// <summary>
        /// Checks if a node is a HTML header tag (H1, H2, etc) upper or equal to the cut level
        /// defined by the project settings
        /// Also checks if it contains some text.
        /// </summary>
        /// <param name="node">HTML node to check</param>
        /// <returns>true if the tag is a non empty cut header</returns>
        private bool IsCutHeader(HtmlNode node)
        {
            // If its a Hx node and x <= MaximumLevel, and it contains text, its a cut node:
            if (!IsNonEmptyHeader(node))
                return false;

            int headerLevel = ChmDocumentNode.HeaderTagLevel(node);
            return headerLevel <= Project.CutLevel;
        }

        /// <summary>
        /// Clone a node, without their children.
        /// </summary>
        /// <param name="nodo">Node to clone</param>
        /// <returns>Cloned node</returns>
        private HtmlNode Clone(HtmlNode nodo)
        {
            return nodo.CloneNode(false);
        }

        /// <summary>
        /// Return the first header tag (H1,H2,etc) found on a subtree of the html document 
        /// that will split the document.
        /// TODO: Join this function and DocumentProcessor.SearchFirstCutNode
        /// </summary>
        /// <param name="root">Root of the html subtree where to search a split</param>
        /// <returns>The first split tag node. null if none was found.</returns>
        private HtmlNode SearchFirstCutNode(HtmlNode root)
        {
            // TODO: Check if there is some XPATH expression for this.
            if (IsCutHeader(root))
                return root;
            else
            {
                foreach (HtmlNode e in root.ChildNodes)
                {
                    HtmlNode seccion = SearchFirstCutNode(e);
                    if (seccion != null)
                        return seccion;
                }
                return null;
            }
        }

        private HtmlNode BuscarNodoA(HtmlNode raiz)
        {
            return raiz.SelectSingleNode(".//a");
        }

        /// <summary>
        /// Stores a splitted document part on the node tree with the main title of the part.
        /// </summary>
        /// <param name="newBody">Body part to store</param>
        private void StoreBodyPart(HtmlNode newBody)
        {
            // Get the main header of this content part:
            HtmlNode sectionHeader = SearchFirstCutNode(newBody);
            ChmDocumentNode nodeToStore = null;
            if (sectionHeader == null)
            {
                //// If no section was found, it can be the first section of the document or it can be
                //// because there no is cut headers:
                //if (Project.CutLevel == 0 && Document.RootNode.Children.Count >= 2)
                //{
                //    // There is no cut headers, and there is some title into the document: 
                //    // If the part contains any title, this content should go to the first title of the 
                //    // document (Document.RootNode.Children[1]). I
                //}

                //if( nodeToStore == null )
                    nodeToStore = InitialNode;
            }
            else
            {
                string aName = "";
                HtmlNode a = BuscarNodoA(sectionHeader);
                if (a != null && GetAttributeValue(a, "name") != null)
                    aName = GetAttributeValue(a, "name");
                nodeToStore = Document.RootNode.BuscarNodo(sectionHeader, aName);
            }

            if (nodeToStore == null)
            {
                string errorMessage = "Error searching node ";
                if (sectionHeader != null)
                    errorMessage += UnescapedInnerText(sectionHeader);
                else
                    errorMessage += "<empty>";
                Exception error = new Exception(errorMessage);
                UI.Log(error);
            }
            else
            {
                nodeToStore.SplittedPartBody = newBody;
                nodeToStore.BuildListOfContainedANames();  // Store the A name's tags of the body.
            }
        }

        /// <summary>
        /// Adds a HTML node as child of other.
        /// The child node is added at the end of the list of parent children.
        /// </summary>
        /// <param name="parent">Parent witch to add the new node</param>
        /// <param name="child">The child node to add</param>
        private void InsertAfter(HtmlNode parent, HtmlNode child)
        {
            try
            {
                parent.AppendChild(child);
            }
            catch (Exception ex)
            {
                UI.Log("Warning: error adding a child (" + child.Name + ") to his parent (" +
                     parent.Name + "): " + ex.Message, ChmLogLevel.WARNING);
                UI.Log(ex);
            }
        }

        /// <summary>
        /// Busca si un arbol contiene un corte de seccion.
        /// </summary>
        /// <param name="nodo">Raiz del arbol en que buscar</param>
        /// <returns>True si el arbol contiene un corte de seccion.</returns>
        private bool WillBeBroken(HtmlNode nodo)
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
        private List<HtmlNode> ProcessNode(HtmlNode node)
        {
            List<HtmlNode> subtreesList = new List<HtmlNode>();

            // Check if the node will be broken on more than one piece because it contains a cut level
            // header:
            if (WillBeBroken(node))
            {
                // It contains a cut level.
                HtmlNode newNode = Clone(node);
                foreach (HtmlNode e in node.ChildNodes)
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
                        List<HtmlNode> listaHijos = ProcessNode(e);
                        foreach (HtmlNode hijo in listaHijos)
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
            HtmlNode newBody = Clone(Document.Body);
            // Traverse root nodes:
            foreach (HtmlNode nodo in Document.Body.ChildNodes)
            {
                if (IsCutHeader(nodo))
                {
                    // Found start of a new part: Store the current body part.
                    StoreBodyPart(newBody);
                    newBody = Clone(Document.Body);
                    InsertAfter(newBody, nodo);
                }
                else
                {
                    List<HtmlNode> lista = ProcessNode(nodo);
                    foreach (HtmlNode hijo in lista)
                    {
                        InsertAfter(newBody, hijo);

                        if (lista[lista.Count - 1] != hijo)
                        {
                            // Si no es el ultimo, cerrar esta parte y abrir otra.
                            StoreBodyPart(newBody);
                            newBody = Clone(Document.Body);
                        }
                    }
                }

                if (UI.CancellRequested())
                    return;
            }
            StoreBodyPart(newBody);
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

                if (nodo.HeaderTag != null && nodo.HeaderTag.InnerText != null && nodo.SplittedPartBody != null)
                {
                    // Nodo con cuerpo:

                    if (UnescapedInnerText(nodo.HeaderTag).Trim() == UnescapedInnerText(nodo.SplittedPartBody).Trim() &&
                        nodo.Children.Count > 0)
                    {
                        // Nodo vacio y con hijos 
                        ChmDocumentNode hijo = (ChmDocumentNode)nodo.Children[0];
                        if (hijo.SplittedPartBody != null)
                        {
                            // El hijo tiene cuerpo: Unificarlos.
                            nodo.SplittedPartBody.AppendChildren(hijo.SplittedPartBody.ChildNodes);
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
                UI.Log(new Exception("There was some problem when we tried to join the empty section " +
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

        /// <summary>
        /// Creates the plain index of the document: The list of topics on the document,
        /// sorted by the title
        /// </summary>
        /// <param name="node">Current document node on the recursive search</param>
        /// <param name="nodeLevel">The node depth on the document tree</param>
        private void CreateDocumentIndex(ChmDocumentNode node, int nodeLevel)
        {
            if (Project.MaxHeaderIndex != 0 && nodeLevel > Project.MaxHeaderIndex)
                return;

            // Add to the content index
            Document.Index.Add(node);

            foreach (ChmDocumentNode child in node.Children)
                CreateDocumentIndex(child, nodeLevel + 1);
        }

        /// <summary>
        /// Creates the plain index of the document: The list of topics on the document,
        /// sorted by the title
        /// </summary>
        private void CreateDocumentIndex()
        {
            Document.Index = new List<ChmDocumentNode>();
            foreach (ChmDocumentNode hijo in Document.RootNode.Children)
                CreateDocumentIndex(hijo, 1);

            Document.Index.Sort();
        }

        /// <summary>
        /// Creates the plain html pages index for the document
        /// <param name="node">Current document tree node</param>
        /// </summary>
        private void CreatePagesIndex(ChmDocumentNode node)
        {
            string lastPage = Document.PagesIndex.Count > 0 ? Document.PagesIndex[Document.PagesIndex.Count - 1] : null;
            if (lastPage != node.DestinationFileName)
                Document.PagesIndex.Add(node.DestinationFileName);

            foreach (ChmDocumentNode child in node.Children)
                CreatePagesIndex(child);
        }

        /// <summary>
        /// Creates the plain html pages index for the document
        /// </summary>
        private void CreatePagesIndex()
        {
            Document.PagesIndex = new List<string>();
            foreach (ChmDocumentNode child in Document.RootNode.Children)
                CreatePagesIndex(child);
        }

        static public string GetAttributeValue(HtmlNode node, string attribute) {
            HtmlAttribute atr = node.Attributes[attribute];
            return atr == null ? null : atr.Value;
        }

        static private void SetAttributeValue(HtmlNode node, string attribute, string value)
        {
            node.SetAttributeValue(attribute, value);
        }

        /// <summary>
        /// Repair or remove an internal link.
        /// Given a broken internal link, it searches a section title of the document with the
        /// same text of the broken link. If its found, the destination link is modified to point to
        /// that section. If a matching section is not found, the link will be removed and its content
        /// will be keept.
        /// </summary>
        /// <param name="link">The broken link</param>
        private void ReplaceBrokenLink(HtmlNode link)
        {
            try
            {
                // Get the text of the link
                string linkText = UnescapedInnerText(link).Trim();
                // Seach a title with the same text of the link:
                ChmDocumentNode destinationTitle = Document.SearchBySectionTitle(linkText);
                if (destinationTitle != null)
                    // Replace the original internal broken link with this:
                    SetAttributeValue( link, "href", destinationTitle.Href);
                else
                {
                    // No candidate title was found. Remove the link and keep its content
                    foreach (HtmlNode child in link.ChildNodes)
                        link.ParentNode.InsertBefore(child, link);
                    link.ParentNode.RemoveChild(link);
                }
            }
            catch (Exception ex)
            {
                UI.Log("Error reparining a broken link", ChmLogLevel.ERROR);
                UI.Log(ex);
            }
        }

        /// <summary>
        /// Makes a recursive search on the document tree to change internal document links to point to the 
        /// splitted files. Optionally repair broken links.
        /// </summary>
        /// <param name="node">Current node on the recursive search</param>
        private void ChangeInternalLinks(HtmlNode node)
        {
            try
            {
                if (node.Name.ToLower() == "a")
                {
                    HtmlNode link = node;
                    string href = GetAttributeValue( link , "href" );
                    if (href != null)
                    {
                        // An hyperlink node
                        if (href.StartsWith("#"))
                        {
                            // A internal link.
                            // Replace it to point to the right splitted file.
                            string safeRef = href.Substring(1);
                            ChmDocumentNode nodoArbol = Document.RootNode.BuscarEnlace(safeRef);
                            if (nodoArbol != null)
                                SetAttributeValue( link , "href" , nodoArbol.DestinationFileName + "#" + safeRef );
                            else
                            {
                                // Broken link.
                                UI.Log("WARNING: Broken link with text: '" + node.InnerText + "'", ChmLogLevel.WARNING);
                                if( node.ParentNode != null )
                                {
                                    String inText = UnescapedInnerText(node.ParentNode);
                                    if (inText != null)
                                    {
                                        if (inText.Length > 200)
                                            inText = inText.Substring(0, 200) + "...";
                                        UI.Log(" near of text: '" + inText + "'", ChmLogLevel.WARNING);
                                    }
                                }
                                if (ReplaceBrokenLinks)
                                    ReplaceBrokenLink(link);
                            }

                        }
                    }
                    else if ( GetAttributeValue(link , "name") != null)
                    {
                        // A HTML "boomark", the destination of a link.
                        string anchor = GetAttributeValue(link, "name");
                        if (!anchor.Equals(anchor))
                        {
                            string htmlNewNode = "<a name=\"" + anchor + "\"></a>";
                            HtmlNode newDomNode = HtmlNode.CreateNode(htmlNewNode);
                            node.ParentNode.ReplaceChild(newDomNode, node);
                        }
                    }
                }

                // DO NOT USE AN ENUMERATOR HERE: Childs can be modified by ChangeInternalLinks
                /*foreach(HtmlNode child in node.ChildNodes)
                    ChangeInternalLinks(child);*/
                for( int i=0; i < node.ChildNodes.Count; i++ )
                    ChangeInternalLinks(node.ChildNodes[i]);

            }
            catch (Exception ex)
            {
                UI.Log(ex);
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
            HtmlNode style = Document.HtmlDoc.DocumentNode.SelectSingleNode("/html/head/style");
            if (style == null)
                return;

            // Remove comments and save the CSS contents:
            Document.EmbeddedStylesTagContent = style.InnerHtml.Replace("<!--", "").Replace("-->", "");

            // Replace the node by other including the CSS file.
            HtmlNode head = Document.HtmlDoc.DocumentNode.SelectSingleNode("/html/head");
            HtmlNode link = Document.HtmlDoc.CreateElement("link");
            link.SetAttributeValue("rel", "stylesheet");
            link.SetAttributeValue("type", "text/css");
            link.SetAttributeValue("href", ChmDocument.EMBEDDEDCSSFILENAME);
            head.ReplaceChild(link, style);
            //head.InsertAfter(link, style);
            //head.RemoveChild(style);
        }

    }
}
