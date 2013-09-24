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
using ChmProcessorLib.DocumentStructure;
using WebIndexLib;
using System.IO;
using System.Reflection;
using System.Web;

namespace ChmProcessorLib.Generators
{
    /// <summary>
    /// Tool to create a web help site from a document.
    /// </summary>
    public class WebHelpGenerator : ContentDirectoryGenerator
    {


        public WebHelpGenerator(ChmDocument document, UserInterface ui, ChmProject project, 
            HtmlPageDecorator decorator)
            : base(document, ui, project, decorator)
        {
        }

        public void Generate(List<string> additionalFiles)
        {

            UI.Log("Generating web site", ConsoleUserInterface.INFO);

            // Create directory, and additional files
            CreateDestinationDirectory(Project.WebDirectory, additionalFiles);
            
            try
            {
                if (Project.FullTextSearch)
                {
                    // Prepare the search index
                    Indexer = new WebIndex();
                    string dbFile = Path.Combine(Project.WebDirectory, "fullsearchdb.db3");
                    string dirTextFiles = Path.Combine(Project.WebDirectory, "textFiles");
                    Indexer.Connect(dbFile);
                    Indexer.CreateDatabase(System.Windows.Forms.Application.StartupPath + Path.DirectorySeparatorChar + "searchdb.sql", dirTextFiles);
                    Indexer.StoreConfiguration(Project.WebLanguage);
                }

                // Create content help files, and index them if it was needed
                CreateHelpContentFiles(Project.WebDirectory);

            }
            finally
            {
                if (Indexer != null)
                    Indexer.Disconnect();
            }

            // Create text replacements
            Replacements replacements = CreateTextReplacements();

            // Copy web files replacing text
            string baseDir = Path.Combine( System.Windows.Forms.Application.StartupPath , "webFiles");
            replacements.CopyDirectoryReplaced(baseDir, Project.WebDirectory, MSWord.HTMLEXTENSIONS, AppSettings.UseTidyOverOutput, UI, Decorator.OutputEncoding);

            // Copy full text search files replacing text:
            if (Project.FullTextSearch)
            {
                // Copy full text serch files:
                string dirSearchFiles = System.Windows.Forms.Application.StartupPath + Path.DirectorySeparatorChar + "searchFiles";
                replacements.CopyDirectoryReplaced(dirSearchFiles, Project.WebDirectory, MSWord.ASPXEXTENSIONS, false, UI, Decorator.OutputEncoding);
            }

            if (Project.GenerateSitemap)
                // Generate site map for web indexers (google).
                GeneateSitemap();

        }

        /// <summary>
        /// Generate a HTML select tag, with the topics index of the help.
        /// </summary>
        /// <returns>The select tag with the topics.</returns>
        private string GenerateWebIndex()
        {
            Document.Index.Sort();
            string index = "<select id=\"topicsList\" size=\"30\">\n";
            foreach (ChmDocumentNode node in Document.Index)
            {
                string href = node.Href;
                if (!href.Equals(""))
                    index += "<option value=\"" + node.Href + "\">" + node.HtmlEncodedTitle + "</option>\n";
            }
            index += "</select>\n";
            return index;
        }

        /// <summary>
        /// Creates the object to make copy of the web files template replacing texts
        /// </summary>
        /// <returns>The text replacements tool</returns>
        private Replacements CreateTextReplacements()
        {

            // Generate search form HTML code:
            // TODO: Remove html resources
            //string textSearch = Project.FullTextSearch ? Resources.SearchFormFullText : Resources.SearchFormSimple;

            // Create standard replacements:
            Replacements replacements = new Replacements();
            replacements.Add("%BOOLFULLSEARCH%", Project.FullTextSearch ? "true" : "false" );
            replacements.Add("%TITLE%" , HttpUtility.HtmlEncode(Project.HelpTitle) );
            replacements.Add("%TREE%" , CreateHtmlTree( "contentsTree", "contentTree") );
            replacements.Add("%TOPICS%", GenerateWebIndex() );
            replacements.Add("%FIRSTPAGEURL%", Document.FirstNodeWithContent.Href);
            replacements.Add("%FIRSTPAGECONTENT%", Document.FirstSplittedContent );
            replacements.Add("%WEBDESCRIPTION%", Decorator.MetaDescriptionTag);
            replacements.Add("%KEYWORDS%", Decorator.MetaKeywordsTag);
            replacements.Add("%HEADER%", Decorator.HeaderHtmlCode);
            replacements.Add("%FOOTER%", Decorator.FooterHtmlCode);
            replacements.Add("%HEADINCLUDE%", Decorator.HeadIncludeHtmlCode);

            // Load translation files.
            string translationFile = Path.Combine( System.Windows.Forms.Application.StartupPath , "webTranslations" ); 
            translationFile = Path.Combine( translationFile , Project.WebLanguage + ".txt");
            try
            {
                replacements.AddReplacementsFromFile(translationFile);
            }
            catch (Exception ex)
            {
                UI.Log("Error opening web translations file" + translationFile + ": " + ex.Message, ConsoleUserInterface.ERRORWARNING);
                UI.Log(ex);
            }

            return replacements;
        }

        private void GeneateSitemap()
        {
            try
            {
                string sitemap = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                                 "<urlset xmlns=\"http://www.google.com/schemas/sitemap/0.84\">\n";
                string webBase = this.Project.WebBase;
                if (!webBase.EndsWith("/"))
                    webBase += "/";
                if (!webBase.StartsWith("http://"))
                    webBase += "http://";

                string[] htmlFiles = Directory.GetFiles(Project.WebDirectory);
                foreach (string file in htmlFiles)
                {
                    string lowerFile = file.ToLower();
                    if (lowerFile.EndsWith(".htm") || lowerFile.EndsWith(".html"))
                    {
                        // Add to the sitemap
                        sitemap += "<url>\n<loc>" + webBase + Path.GetFileName(file) + "</loc>\n<lastmod>";
                        DateTime lastmod = File.GetLastWriteTime(file);
                        sitemap += lastmod.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszzz") + "</lastmod>\n";
                        sitemap += "<changefreq>" + this.Project.ChangeFrequency + "</changefreq>\n";
                        sitemap += "</url>\n";
                    }
                }
                sitemap += "</urlset>";

                // Store
                string sitemapFile = Path.Combine( Project.WebDirectory, "sitemap.xml" );
                StreamWriter writer = new StreamWriter(sitemapFile, false, Encoding.UTF8);
                writer.Write(sitemap);
                writer.Close();
                string sitemapZiped = Path.Combine( Project.WebDirectory, "sitemap.xml.gz" );
                Zip.CompressFile(sitemapFile, sitemapZiped);
                File.Delete(sitemapFile);
            }
            catch (Exception ex)
            {
                UI.Log("Error generating the sitemap: " + ex.Message, ConsoleUserInterface.ERRORWARNING);
                UI.Log(ex);
            }
        }

        private string CreateHtmlTree(ChmDocumentNode nodo, int nivel)
        {
            if (Project.MaxHeaderContentTree != 0 && nivel > Project.MaxHeaderContentTree)
                return "";

            string texto = "";
            if (!nodo.Href.Equals(""))
            {
                texto = "<li><a href=\"" + nodo.Href;
                texto += "\">" + nodo.HtmlEncodedTitle + "</a>";
            }

            if (nodo.Children.Count > 0)
            {
                if (Project.MaxHeaderContentTree == 0 || nivel < Project.MaxHeaderContentTree)
                {
                    texto += "\n<ul>\n";
                    foreach (ChmDocumentNode hijo in nodo.Children)
                        texto += CreateHtmlTree(hijo, nivel + 1) + "\n";
                    texto += "</ul>";
                }
            }
            if (!texto.Equals(""))
                texto += "</li>";
            return texto;
        }

        private string CreateHtmlTree(string id, string classId)
        {
            string texto = "<ul";
            if (id != null)
                texto += " id=\"" + id + "\"";
            if (classId != null)
                texto += " class=\"" + classId + "\"";
            texto += ">\n";

            foreach (ChmDocumentNode hijo in Document.RootNode.Children)
                texto += CreateHtmlTree(hijo, 1) + "\n";
            texto += "</ul>\n";
            return texto;
        }

    }
}
