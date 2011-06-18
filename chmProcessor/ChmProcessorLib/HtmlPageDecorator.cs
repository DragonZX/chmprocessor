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
        /// The encoding of the pattern HTML page
        /// </summary>
        private Encoding encoding;

        /// <summary>
        /// HTML code to put before the "body" tag.
        /// </summary>
        private string textBeforeBody = "";

        /// <summary>
        /// HTML code to put after the "body" tag.
        /// </summary>
        private string textAfterBody = "";

        /// <summary>
        /// HTML code to add as header to the content of the body of the HTML page.
        /// </summary>
        public string HeaderHtmlCode = "";

        /// <summary>
        /// Source file for the HTML code to include as body content header.
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
        /// Source file for the HTML code to include as body content footer.
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
            IHTMLElement clonedBody = (IHTMLElement) ((IHTMLDOMNode)body).cloneNode(true);

            // Add content headers and footers:
            if (HeaderHtmlCode != "")
                clonedBody.insertAdjacentHTML("afterBegin", HeaderHtmlCode);
            if (FooterHtmlCode != "")
                clonedBody.insertAdjacentHTML("beforeEnd", FooterHtmlCode);

            return clonedBody;
        }

        /// <summary>
        /// Writes an HTML file, adding the footer, header, etc if needed to the body.
        /// </summary>
        /// <param name="body">"body" tag to write into the html file</param>
        /// <param name="filePath">Path where to write the HTML file</param>
        /// <param name="UI">User interface of the application</param>
        public void ProcessAndSavePage(IHTMLElement body, string filePath, UserInterface UI)
        {
            // Make a copy of the body and add the header and footer:
            IHTMLElement clonedBody = AddFooterAndHeader(body);

            StreamWriter writer = new StreamWriter(filePath, false, encoding);
            writer.WriteLine(textBeforeBody);
            string bodyText = clonedBody.outerHTML;

            // Seems to be a bug that puts "about:blank" on links. Remove them:
            // TODO: Check if this still true....
            bodyText = bodyText.Replace("about:blank", "").Replace("about:", "");
            writer.WriteLine(bodyText);
            writer.WriteLine(textAfterBody);
            writer.Close();

            // Clean the files using Tidy
            if (AppSettings.UseTidyOverOutput)
                new TidyParser(UI).Parse(filePath);
        }

        /// <summary>
        /// Extracts all the HTML code before and after the "body" tag of the pattern HTML page
        /// and saves if on textBeforeBody and textAfterBody members.
        /// </summary>
        /// <param name="originalSourcePage">The HTML pattern page to extract the code</param>
        private void GetHtmlPattern(IHTMLDocument3 originalSourcePage)
        {
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
                        else if (beforeBody)
                            textBeforeBody += child.outerHTML + "\n";
                        else
                            textAfterBody += child.outerHTML + "\n";
                    }

                    // Close the HTML tag:
                    textBeforeBody += "</html>\n";
                }

            }

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="originalSourcePage">The source HTML page pattern.
        /// It will used as the pattern to build the processed pages. All its content,
        /// except the "body" tag will be put into generate the processed pages.
        /// </param>
        public HtmlPageDecorator(IHTMLDocument3 originalSourcePage)
        {
            // Get the encoding of the pattern page:
            encoding = Encoding.GetEncoding( ((IHTMLDocument2)originalSourcePage).charset);

            GetHtmlPattern(originalSourcePage);
        }
    }
}
