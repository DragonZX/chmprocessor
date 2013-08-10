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
        /// File name where to store the content of the initial part of the document that comes without any
        /// section
        /// </summary>
        private const string INITIALSECTIONFILENAME = "start.htm";

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
        /// Makes a parse of the headers structure of the document
        /// </summary>
        /// <param name="iDoc">The HTML document to parse</param>
        /// <param name="ui">Log generation object</param>
        /// <param name="project">Information about how to split the document</param>
        public void ParseDocument(IHTMLDocument2 iDoc, UserInterface ui, ChmProject project)
        {

            // Create the empty document:
            Document = new ChmDocument(iDoc);
            UI = ui;
            Project = project;

            // Build a node for the content without an initial title
            ChmDocumentNode noSectionStart = new ChmDocumentNode(Document.RootNode, null, ui);
            Document.RootNode.Children.Add(noSectionStart);

            // Parse recursivelly the document headers structure by headers sections
            ParseHeaderStructure(iDoc.body);

            // By default, all document goes to the section without any title
            Document.RootNode.StoredAt(INITIALSECTIONFILENAME);

            // Now assign filenames where will be stored each section.
            int cnt = 2;
            foreach (ChmDocumentNode hijo in Document.RootNode.Children)
                SplitFilesStructure(hijo, ref cnt);
        }

        /// <summary>
        /// Make a recursive seach of all HTML header nodes into the document.
        /// </summary>
        /// <param name="currentNode">Current HTML node on the recursive search</param>
        private void ParseHeaderStructure(IHTMLElement currentNode)
        {
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

    }
}
