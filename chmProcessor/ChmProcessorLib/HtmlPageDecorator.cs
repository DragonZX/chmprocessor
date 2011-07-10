using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mshtml;
using System.IO;

namespace ChmProcessorLib
{
    /// <summary>
    /// Class to manipulate a single page to add headers, footers or other stuff.
    /// </summary>
    class HtmlPageDecorator
    {

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
        /// Adds footer and / or header, if its needed.
        /// </summary>
        /// <param name="body">Original "body" tag of the page to write</param>
        /// <returns>If a footer or a header was specified, return a copy
        /// of the original body with the footer and / or header added. If none
        /// was specified, return the original body itself.</returns>
        private IHTMLElement AddFooterAndHeader(IHTMLElement body)
        {
            
            if (HeaderHtmlCode == "" && FooterHtmlCode == "")
                return body;

            // Clone the body:
            IHTMLElement clonedBody = (IHTMLElement)((IHTMLDOMNode)body).cloneNode(true);

            try
            {
                // Add content headers and footers:
                if (HeaderHtmlCode != "")
                    clonedBody.insertAdjacentHTML("afterBegin", HeaderHtmlCode);
                if (FooterHtmlCode != "")
                    clonedBody.insertAdjacentHTML("beforeEnd", FooterHtmlCode);
            }
            catch (Exception ex)
            {
                log(new Exception("There is something wrong with your HTML header or footer. Internet " +
                    "Explorer said NO when we tried to add them to the body", ex));
            }

            return clonedBody;
            
        }

        /// <summary>
        /// Writes an HTML file, adding the footer, header, etc if needed to the body.
        /// </summary>
        /// <param name="body">"body" tag to write into the html file</param>
        /// <param name="filePath">Path where to write the HTML file</param>
        /// <param name="UI">User interface of the application</param>
        public void ProcessAndSavePage(IHTMLElement body, string filePath)
        {
            // Make a copy of the body and add the header and footer:
            IHTMLElement clonedBody = AddFooterAndHeader(body);

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

            writer.WriteLine(textBeforeBody);
            string bodyText = clonedBody.outerHTML;

            // Seems to be a bug that puts "about:blank" on links. Remove them:
            // TODO: Check if this still true....
            bodyText = bodyText.Replace("about:blank", "").Replace("about:", "");
            writer.WriteLine(bodyText);
            writer.WriteLine(textAfterBody);
            writer.Close();

            // Clean the files using Tidy, only if it was written with UTF-8
            if (AppSettings.UseTidyOverOutput && writeEncoding == Encoding.UTF8 && UseTidy )
            {
                TidyParser tidy = new TidyParser(ui, TidyParser.UTF8);
                tidy.Parse(filePath);
            }

        }

        /// <summary>
        /// Verify if an HTML node is a meta with the content type.
        /// </summary>
        /// <param name="e">Element to check</param>
        /// <returns>True if its the meta with the content-type</returns>
        static private bool IsContentTypeTag(IHTMLElement e)
        {
            // Check if its the "<META content="text/html; charset=XXX" http-equiv=Content-Type>" tag
            bool isContentType = false;
            if (e is IHTMLMetaElement && ((IHTMLMetaElement)e).httpEquiv != null)
            {
                string httpEquiv = ((IHTMLMetaElement)e).httpEquiv.Trim().ToLower();
                if (httpEquiv == "content-type")
                    isContentType = true;
            }
            return isContentType;
        }

        /// <summary>
        /// Get a new head tag with the desired include code.
        /// </summary>
        /// <param name="head">The original head tag</param>
        /// <returns>HTML code wit the new head tag processed.</returns>
        private string ProcessHeadTag(IHTMLElement head)
        {

            // Copy the original <head> node:
            string newHeadText = "<head>\n";
            IHTMLElementCollection headChidren = (IHTMLElementCollection)head.children;
            foreach (IHTMLElement e in headChidren)
            {
                if (OutputEncoding != null && IsContentTypeTag(e) )
                    // Replace the encoding:
                    newHeadText += "<META content=\"text/html; charset=" + OutputEncoding.WebName + 
                        "\" http-equiv=Content-Type>\n";
                else
                    newHeadText += e.outerHTML + "\n";
            }

            // Add includes
            if (HeadIncludeHtmlCode != "")
                newHeadText += HeadIncludeHtmlCode + "\n";

            // Add some spam:
            newHeadText += "<meta name=\"GENERATOR\" content=\"chmProcessor\" >\n";

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
        public void PrepareHtmlPattern(IHTMLDocument3 originalSourcePage)
        {

            // Get the encoding of the pattern page:
            try
            {
                inputEncoding = Encoding.GetEncoding(((IHTMLDocument2)originalSourcePage).charset);
            }
            catch
            {
                inputEncoding = null;
            }

            bool beforeBody = true; // Are we currently before or after the "body" node?
            
            // Traverse the root nodes of the HTML page:
            IHTMLDOMChildrenCollection col = (IHTMLDOMChildrenCollection)originalSourcePage.childNodes;
            foreach (IHTMLElement e in col)
            {
                if (e is IHTMLCommentElement)
                {
                    // head tag and other stuff.
                    IHTMLCommentElement com = (IHTMLCommentElement)e;
                    if (beforeBody)
                        textBeforeBody += com.text + "\n";
                    else
                        textAfterBody += com.text + "\n";
                }
                else if (e is IHTMLHtmlElement)
                {
                    // Copy the <html> tag (TODO: check if clone() can be used here to make the copy)
                    textBeforeBody += "<html ";
                    IHTMLAttributeCollection atrCol = (IHTMLAttributeCollection)((IHTMLDOMNode)e).attributes;
                    // Get the attributes of the html tag:
                    foreach (IHTMLDOMAttribute atr in atrCol)
                    {
                        if (atr.specified)
                            textBeforeBody += atr.nodeName + "=\"" + atr.nodeValue + "\"";
                    }
                    textBeforeBody += " >\n";

                    // Traverse the <html> children:
                    IHTMLElementCollection htmlChidren = (IHTMLElementCollection)e.children;
                    foreach (IHTMLElement child in htmlChidren)
                    {
                        if (child is IHTMLBodyElement)
                            beforeBody = false;
                        else if (child is IHTMLHeadElement)
                            textBeforeBody += ProcessHeadTag(child);
                        else if (beforeBody)
                            textBeforeBody += child.outerHTML + "\n";
                        else
                            textAfterBody += child.outerHTML + "\n";
                    }

                    // Close the HTML tag:
                    textBeforeBody += "</html>\n";
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
                ui.log(ex);
        }
    }
}
