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
    }
}
