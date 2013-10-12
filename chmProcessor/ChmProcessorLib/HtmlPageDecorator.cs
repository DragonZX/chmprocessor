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
using System.Text;
using HtmlAgilityPack;
using System.IO;
using ChmProcessorLib.DocumentStructure;

namespace ChmProcessorLib
{
    /// <summary>
    /// Class to manipulate a single page to add headers, footers or other stuff.
    /// </summary>
    public class HtmlPageDecorator
    {

        /// <summary>
        /// Text pattern to replace on the "textBeforeBody" field by the title tag of the page.
        /// </summary>
        static private String TITLETAG = "<%TITLE%>";

        /// <summary>
        /// User interface to log messages. Can be null
        /// </summary>
        public UserInterface ui;

        /// <summary>
        /// The encoding of the pattern HTML page. It can be null.
        /// </summary>
        private Encoding inputEncoding;

        /// <summary>
        /// HTML code to put before the "body" tag.
        /// </summary>
        private string textBeforeBody = "";

        /// <summary>
        /// HTML code to put after the "body" tag.
        /// </summary>
        private string textAfterBody = "";

        /// <summary>
        /// Text to add as a meta keywords tag. If its empty, no tag will be added
        /// </summary>
        public string MetaKeywordsValue = "";

        /// <summary>
        /// Encoding that will be used to write the pages. It can be null.
        /// If it is, the input encoding will be used to write the pages.
        /// </summary>
        public Encoding OutputEncoding;

        /// <summary>
        /// Should we execute tidy over the written files?
        /// Only will be executed if this is true, AppSettings.UseTidyOverOutput is true 
        /// AND the efective output encoding is UTF-8.
        /// </summary>
        public bool UseTidy;

        /// <summary>
        /// HTML keywords meta tag, if MetaKeywordsValue is not empty. "" otherwise.
        /// </summary>
        public string MetaKeywordsTag
        {
            get
            {
                if (MetaKeywordsValue != "")
                    return "<meta name=\"keywords\" content=\"" + MetaKeywordsValue + "\" >";
                else
                    return "";
            }
        }

        /// <summary>
        /// Text to add as a meta description tag. If its empty, no tag will be added
        /// </summary>
        public string MetaDescriptionValue = "";

        /// <summary>
        /// HTML keywords meta tag, if MetaDescriptionValue is not empty. "" otherwise.
        /// </summary>
        public string MetaDescriptionTag
        {
            get
            {
                if (MetaKeywordsValue != "")
                    return "<meta name=\"description\" content=\"" + MetaDescriptionValue + "\" >";
                else
                    return "";
            }
        }

        /// <summary>
        /// HTML code to add as header to the content of the body of the HTML page.
        /// </summary>
        public string HeaderHtmlCode = "";

        /// <summary>
        /// Path to file for the HTML code to include as body content header.
        /// </summary>
        public string HeaderHtmlFile
        {
            set
            {
                StreamReader reader = new StreamReader(value);
                HeaderHtmlCode = reader.ReadToEnd();
                reader.Close();
            }
        }

        /// <summary>
        /// HTML code to add as footer to the content of the body of the HTML page.
        /// </summary>
        public string FooterHtmlCode = "";

        /// <summary>
        /// Path to file for the HTML code to include as body content footer.
        /// </summary>
        public string FooterHtmlFile
        {
            set
            {
                StreamReader reader = new StreamReader(value);
                FooterHtmlCode = reader.ReadToEnd();
                reader.Close();
            }
        }

        /// <summary>
        /// HTML code to include into the "head" tag of the page.
        /// </summary>
        public string HeadIncludeHtmlCode = "";

        /// <summary>
        /// Path to file for the HTML code to include into the "head" tag.
        /// </summary>
        public string HeadIncludeFile
        {
            set
            {
                // Read the HTML code to include into the <head> tag.
                StreamReader reader = new StreamReader(value);
                HeadIncludeHtmlCode = reader.ReadToEnd();
                reader.Close();
            }
        }

        /// <summary>
        /// Returns the header/footer page texts with navigation links replaced
        /// </summary>
        /// <param name="node">Current page to calculate the navigation links</param>
        /// <param name="document">Owner of the page</param>
        /// <param name="headerHtmlCode">The header with links replaced</param>
        /// <param name="footerHtmlCode">The footer with links replaced</param>
        private void MakeLinkReplacements(ChmDocumentNode node, ChmDocument document, 
            out string headerHtmlCode, out string footerHtmlCode) 
        {
            string previousLink, nextLink, homeLink;
            previousLink = nextLink = homeLink = "#";

            // Home:
            if( document.PagesIndex.Count > 0 )
                homeLink = document.PagesIndex[0];

            int currentPageIdx = document.PagesIndex.IndexOf(node.DestinationFileName);
            if (currentPageIdx >= 0)
            {
                // Previous
                if (currentPageIdx > 0)
                    previousLink = document.PagesIndex[currentPageIdx - 1];
                // Next
                if (currentPageIdx < (document.PagesIndex.Count - 1))
                    nextLink = document.PagesIndex[currentPageIdx + 1];
            }

            // Make replacements:
            headerHtmlCode = HeaderHtmlCode.Replace("%PREVLINK%", previousLink)
                .Replace("%NEXTLINK%", nextLink)
                .Replace("%HOMELINK%", homeLink);
            footerHtmlCode = FooterHtmlCode.Replace("%PREVLINK%", previousLink)
                .Replace("%NEXTLINK%", nextLink)
                .Replace("%HOMELINK%", homeLink);
        }

        /// <summary>
        /// Adds footer and / or header, if its needed.
        /// </summary>
        /// <param name="node">Node with the body to write</param>
        /// <param name="document">Owner of the node</param>
        /// <returns>If a footer or a header was specified, return a copy
        /// of the original body with the footer and / or header added. If none
        /// was specified, return the original body itself.</returns>
        private HtmlNode AddFooterAndHeader(ChmDocumentNode node, ChmDocument document)
        {

            HtmlNode body = node.SplittedPartBody;

            if (HeaderHtmlCode == "" && FooterHtmlCode == "")
                return body;

            // Clone the body:
            HtmlNode clonedBody = body.CloneNode(true);

            try
            {
                // Make previous, next and home link replacements:
                string header, footer;
                MakeLinkReplacements(node, document, out header, out footer);

                // Add content headers and footers:
                if (HeaderHtmlCode != "")
                    clonedBody.PrependChild(HtmlNode.CreateNode("<div>" + header + "</div>"));
                if (FooterHtmlCode != "")
                    clonedBody.AppendChild(HtmlNode.CreateNode("<div>" + footer + "</div>"));
            }
            catch (Exception ex)
            {
                log(new Exception("Error adding headers / footers to node " + node, ex));
            }

            return clonedBody;
            
        }

        /// <summary>
        /// Writes an HTML file, adding the footer, header, etc if needed to the body.
        /// </summary>
        /// <param name="body">"body" tag to write into the html file</param>
        /// <param name="filePath">Path where to write the HTML file</param>
        /// <param name="UI">User interface of the application</param>
        public void ProcessAndSavePage(ChmDocumentNode node, ChmDocument document, string filePath)
        {

            HtmlNode body = node.SplittedPartBody;
            if (body == null)
                throw new Exception("The node " + node + " has no body");

            // Make a copy of the body and add the header and footer:
            HtmlNode clonedBody = AddFooterAndHeader(node, document);

            StreamWriter writer;

            // Determine the encoding to write the page:
            Encoding writeEncoding = OutputEncoding;
            if (writeEncoding == null)
                writeEncoding = inputEncoding;

            if (writeEncoding != null)
                writer = new StreamWriter(filePath, false, writeEncoding);
            else
                // Use the default encoding.
                writer = new StreamWriter(filePath, false);

            writer.WriteLine( textBeforeBody.Replace(TITLETAG, "<title>" + node.Title + "</title>") );
            string bodyText = clonedBody.OuterHtml;

            // Seems to be a bug that puts "about:blank" on links. Remove them:
            bodyText = bodyText.Replace("about:blank", "").Replace("about:", "");
            writer.WriteLine(bodyText);
            writer.WriteLine(textAfterBody);
            writer.Close();

            // Clean the files using Tidy, only if it was written with UTF-8
            if (AppSettings.UseTidyOverOutput && writeEncoding == Encoding.UTF8 && UseTidy )
            {
                TidyParser tidy = new TidyParser(ui, TidyParser.UTF8);
                tidy.DocType = TidyParser.LOOSE;
                tidy.Parse(filePath);
            }

        }

        /// <summary>
        /// Verify if an HTML node is a meta with the content type.
        /// </summary>
        /// <param name="e">Element to check</param>
        /// <returns>True if its the meta with the content-type</returns>
        static private bool IsContentTypeTag(HtmlNode e)
        {
            // Check if its the "<META content="text/html; charset=XXX" http-equiv=Content-Type>" tag
            bool isContentType = false;
            //if (e is IHTMLMetaElement && ((IHTMLMetaElement)e).httpEquiv != null)
            if( e.Name.ToLower() == "meta" && e.Attributes.Contains("http-equiv") )
            {
                string httpEquiv = e.Attributes["http-equiv"].Value.Trim().ToLower();
                if (httpEquiv == "content-type")
                    isContentType = true;
            }
            return isContentType;
        }

        /// <summary>
        /// Tag meta with the conten type to override the original content type. If OutputEncoding
        /// is null, this is an empty string
        /// </summary>
        private string ContentTypeMetaTag
        {
            get
            {
                if (OutputEncoding == null)
                    return string.Empty;

                return "<meta content=\"text/html; charset=" + OutputEncoding.WebName +
                        "\" http-equiv=\"content-type\">\n";
            }

        }

        /// <summary>
        /// Get a new head tag with the desired include code.
        /// </summary>
        /// <param name="head">The original head tag</param>
        /// <returns>HTML code wit the new head tag processed.</returns>
        private string ProcessHeadTag(HtmlNode head)
        {

            // Copy the original <head> node:
            string newHeadText = "<head>\n";
            bool contentTypeFound = false;
            foreach (HtmlNode e in head.ChildNodes)
            {
                if (OutputEncoding != null && IsContentTypeTag(e))
                {
                    // Replace the encoding:
                    newHeadText += ContentTypeMetaTag;
                    contentTypeFound = true;
                }
                else if (e.Name.ToLower() == "title")
                    // Is the title. We will replace it after.
                    newHeadText += TITLETAG + "\n";
                else
                    newHeadText += e.OuterHtml + "\n";
            }

            if (!contentTypeFound && OutputEncoding != null)
                // We are going to replace the original encondig. So, make sure to declare it:
                newHeadText += ContentTypeMetaTag;
            

            // Add head includes
            if (HeadIncludeHtmlCode != "")
                newHeadText += HeadIncludeHtmlCode + "\n";

            // Add some spam:
            newHeadText += "<meta name=\"generator\" content=\"chmProcessor\" >\n";

            // Add other metas
            newHeadText += MetaKeywordsTag + "\n" + MetaDescriptionTag + "\n";

            newHeadText += "</head>\n";
            return newHeadText;
        }

        /// <summary>
        /// Extracts all the HTML code before and after the "body" tag of the pattern HTML page
        /// and saves if on textBeforeBody and textAfterBody members.
        /// This function MUST to be called before start to make calls to ProcessHeadTag.
        /// </summary>
        /// <param name="originalSourcePage">The HTML pattern page to extract the code</param>
        public void PrepareHtmlPattern(HtmlDocument originalSourcePage)
        {

            // Get the encoding of the pattern page:
            try
            {
                inputEncoding = originalSourcePage.DeclaredEncoding;
            }
            catch
            {
                inputEncoding = null;
            }

            bool beforeBody = true; // Are we currently before or after the "body" node?
            
            // Traverse the root nodes of the HTML page:
            foreach (HtmlNode e in originalSourcePage.DocumentNode.ChildNodes)
            {
                if (e is HtmlCommentNode)
                {
                    // head tag and other stuff.
                    HtmlCommentNode com = (HtmlCommentNode)e;
                    if (beforeBody)
                        textBeforeBody += com.OuterHtml + "\n";
                    else
                        textAfterBody += com.OuterHtml + "\n";
                }
                else if (e.Name.ToLower() == "html")
                {
                    // Copy the <html> tag (TODO: check if clone() can be used here to make the copy)
                    textBeforeBody += "<html ";
                    // Get the attributes of the html tag:
                    foreach (HtmlAttribute atr in e.Attributes)
                    {
                        //if (atr.specified) ?
                        textBeforeBody += atr.Name + "=\"" + atr.Value + "\"";
                    }
                    textBeforeBody += " >\n";

                    // Traverse the <html> children:
                    foreach (HtmlNode child in e.ChildNodes)
                    {
                        if (child.Name.ToLower() == "body")
                            beforeBody = false;
                        else if (child.Name.ToLower() == "head" )
                            textBeforeBody += ProcessHeadTag(child);
                        else if (beforeBody)
                            textBeforeBody += child.OuterHtml + "\n";
                        else
                            textAfterBody += child.OuterHtml + "\n";
                    }

                    // Close the HTML tag:
                    textAfterBody += "</html>\n";

                }

                // TODO: Other tags should not be added too?
            }

        }

        /// <summary>
        /// Log an exception to the user interface
        /// </summary>
        /// <param name="ex">Exception to log</param>
        private void log(Exception ex)
        {
            if (ui != null)
                ui.Log(ex);
        }
    }
}
