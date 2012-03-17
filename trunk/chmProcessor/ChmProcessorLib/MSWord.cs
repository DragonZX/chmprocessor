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
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Word;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace ChmProcessorLib
{
	/// <summary>
	/// Class to handle MS Word files.
    /// Thanks to Alexander Kojevnikov for http://www.codeproject.com/csharp/winwordloader.asp 
	/// </summary>
	public class MSWord
	{
        /// <summary>
        /// File extensions for MS Word files.
        /// </summary>
        public static string[] WORDEXTENSIONS = { "doc", "docx" };

        /// <summary>
        /// File extensions for HTML files.
        /// </summary>
        public static string[] HTMLEXTENSIONS = { "htm", "html" };

        /// <summary>
        /// File extensions for ASP.NET files
        /// </summary>
        public static string[] ASPXEXTENSIONS = { "aspx" };

        /// <summary>
        /// Reference to word application
        /// </summary>
        private Application wordApp = null;

        /// <summary>
        /// Have we started a new instance of word, or we are using an existing instance?
        /// </summary>
        private bool isNewApp = false;

        private bool disposed = false;
        
        /// <summary>
        /// Check if the the file it's a MS Word document
        /// </summary>
        /// <param name="file">Path of file to check</param>
        /// <returns>True if the file its a word document</returns>
        static public bool ItIsWordDocument(string file)
        {
            string extension = Path.GetExtension(file).ToLower();
            foreach (string ext in WORDEXTENSIONS)
                if (extension.Equals("." + ext))
                    return true;
            return false;
        }

        /// <summary>
        /// Check if the files is a HTML document.
        /// </summary>
        /// <param name="file">Path of file to check</param>
        /// <returns>True if the file its a HTML document</returns>
        static public Boolean IsHtmlDocument(string file)
        {
            string extension = Path.GetExtension(file).ToLower();
            foreach (string ext in HTMLEXTENSIONS)
                if (extension.Equals("." + ext))
                    return true;
            return false;
        }

        /// <summary>
        /// Constructor
        /// Initializes a MSWord instance
        /// </summary>
        public MSWord() 
        {

            // Check if Word is registered in the ROT.
            try
            {
                wordApp = (Application)Marshal.
                    GetActiveObject("Word.Application");
            }
            catch
            {
                wordApp = null;
            }
            // Load Word if it's not.
            if(wordApp == null)
            {
                try
                {
                    wordApp = new ApplicationClass();
                    isNewApp = true;
                }
                catch(Exception ex) 
                {
                    wordApp = null;
                    throw new Exception("MS Word cannot be openened. Is it installed?", ex);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!this.disposed)
            {
                if(disposing)
                {
                    // Dispose managed resources.
                }
                
                // Dispose unmanaged resources.
                if(wordApp != null)
                {
                    try
                    {
                        if(isNewApp && wordApp.Documents.Count == 0)
                        {
                            object arg1 = WdSaveOptions.wdDoNotSaveChanges;
                            object arg2 = null;
                            object arg3 = null;
                            ((_Application)wordApp).Quit(ref arg1, ref arg2, ref arg3);

                            // Wait until Word shuts down.
                            for(;;)
                            {
                                Thread.Sleep(100);
                                try
                                {
                                    // When word shuts down this call 
                                    // throws an exception.
                                    string dummy = wordApp.Version;
                                }
                                catch
                                {
                                    break;
                                }
                            }
                        }
                    }
                    catch {}

                    wordApp = null;
                }
            }
            disposed = true;
        }

        ~MSWord()
        {
            Dispose(false);
        }

        public Application WordApplication
        {
            get
            {
                return wordApp;
            }
        }

        public bool IsOpen( string path ) 
        {
            foreach( Document doc in wordApp.Documents ) 
            {
                if( path.ToLower().Equals( doc.FullName.ToLower() ) )
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Save a word document as a XPS file.
        /// Requires Microsoft Office 2007 Add-in
        /// </summary>
        /// <param name="wordFileSrc">Absolute path of the source word document</param>
        /// <param name="htmlFileDst">Absolute path to the xps destination document</param>
        public void SaveWordToXps(string wordFileSrc, string pdfFileDst)
        {
            object format = WdSaveFormat.wdFormatXPS;
            SaveWord(wordFileSrc, pdfFileDst, format);
        }

        /// <summary>
        /// Save a word document as a PDF file.
        /// Requires Microsoft Office 2007 Add-in
        /// </summary>
        /// <param name="wordFileSrc">Absolute path of the source word document</param>
        /// <param name="htmlFileDst">Absolute path to the pdf destination document</param>
        public void SaveWordToPdf(string wordFileSrc, string pdfFileDst)
        {
            object format = WdSaveFormat.wdFormatPDF;
            SaveWord(wordFileSrc, pdfFileDst, format);
        }

        /// <summary>
        /// Save a word document as a html file.
        /// </summary>
        /// <param name="wordFileSrc">Absolute path of the source word document</param>
        /// <param name="htmlFileDst">Absolute path to the pdf destination document</param>
        public void SaveWordToHtml( string wordFileSrc , string htmlFileDst ) 
        {
            object format = WdSaveFormat.wdFormatFilteredHTML;
            SaveWord(wordFileSrc, htmlFileDst, format);
        }

        /// <summary>
        /// Unlinks external image files on the document 
        /// Thanks to Paolo Moretti for the patch.
        /// <param name="aDoc">Document where to unlink images</param>
        /// </summary>
        static public void UnlinkFields(Document aDoc)
        {
            try
            {
                aDoc.Fields.Update();
                foreach (Field field in aDoc.Fields)
                {
                    switch (field.Type)
                    {
                        case WdFieldType.wdFieldIncludePicture:
                            field.Unlink();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Converts a Word document to other format.
        /// </summary>
        /// <param name="wordFileSrc">Path to the Word document to convert</param>
        /// <param name="htmlFileDst">Path to the converted format</param>
        /// <param name="format">Format witch to convert. One of WdSaveFormat values.</param>
        public void SaveWord(string wordFileSrc, string htmlFileDst, object format)
        {
            Document aDoc = null;
            object missing = System.Reflection.Missing.Value;
            object saveChanges = false;

            try
            {
                object fileName = wordFileSrc;
                object readOnly = true;
                object isVisible = false;


                // Open the document that was chosen by the dialog
                aDoc = wordApp.Documents.Open(ref fileName, ref missing, ref readOnly, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing);

                // Unlink images:
                UnlinkFields(aDoc);

                // Save document as filtered html:
                object dst = htmlFileDst;
                aDoc.SaveAs(ref dst, ref format, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing);
            }
            finally
            {
                if (aDoc != null)
                {
                    // Close the document:
                    ((_Document)aDoc).Close(ref saveChanges, ref missing, ref missing);

                    // Be sure the document is closes (paranoic check to avoid bug with large files)
                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(100);
                    while (IsOpen(wordFileSrc))
                    {
                        System.Windows.Forms.Application.DoEvents();
                        Thread.Sleep(100);
                    }

                }
            }
        }

        /// <summary>
        /// Close all opened documents.
        /// </summary>
        public void CloseAllDocuments()
        {
            object saveChanges = false;
            object missing = System.Reflection.Missing.Value;
            wordApp.Documents.Close(ref saveChanges, ref missing, ref missing);
        }

        /// <summary>
        /// Creates a new document result of the join of a list of documents 
        /// Thanks to Jennifer Lewis for http://mwalimuscorner.blogspot.com/2008/11/c-tutorial-combine-multiple-word.html
        /// </summary>
        /// <param name="sourceDocuments">List of paths of documentos to join</param>
        /// <param name="destinationDocument">Path to the destination document. If it exists, it will
        /// be overwrited</param>
        public void JoinDocuments(string[] sourceDocuments, String destinationDocument)
        {
            object missing = System.Reflection.Missing.Value;

            // If we create a new document, all styles on source documents are lost:
            /*
            // Create the destination document:
            object template = missing, newTemplate = missing, documentType = missing;
            object visible = false;
            Document newDoc = wordApp.Documents.Add(ref template, ref newTemplate, ref documentType, ref visible);
            */
            // So, make a copy of the first document, open it and append there the new files
            // This will keep the styles of the first document and they will be applied to the other
            // docs.
            File.Copy((string)sourceDocuments[0], destinationDocument);
            // Open the document that was chosen by the dialog
            object copyOfFirstDocument = destinationDocument;
            Document newDoc = wordApp.Documents.Open(ref copyOfFirstDocument, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing);

            // The current selection on the destination document. Its the end of the file
            //newDoc.Select();
            //Selection selection = wordApp.Selection;

            // Move the "cursor" to the end of the document:
            //newDoc.Select();
            object what = WdGoToItem.wdGoToLine; 
            object which = WdGoToDirection.wdGoToLast;
            newDoc.GoTo(ref what, ref which, ref missing, ref missing).Select();
            Selection selection = wordApp.Selection;

            // Break page to add after each document:
            object sectionBreak = WdBreakType.wdSectionBreakNextPage;

            object Filename;

            // Insert other files on the destination document:
            for(int i=1; i< sourceDocuments.Length; i++) {
                // Add a page break:
                selection.InsertBreak(ref sectionBreak);
                // Add the document at the end of the file:
                object range = missing, confirmConversions = false, link = false, attachment = false;
                Filename = sourceDocuments[i];
                selection.InsertFile( (string)sourceDocuments[i] , ref range, ref confirmConversions, ref link, ref attachment);
            }

            // Unlink images:
            UnlinkFields(newDoc);

            // Save the document:
            object FileFormat = missing, LockComments = missing, Password = missing, AddToRecentFiles = missing, WritePassword = missing, ReadOnlyRecommended = missing, EmbedTrueTypeFonts = missing, SaveNativePictureFormat = missing, SaveFormsData = missing, SaveAsAOCELetter = missing, Encoding = missing, InsertLineBreaks = missing, AllowSubstitutions = missing, LineEnding = missing, AddBiDiMarks = missing;
            Filename = destinationDocument;
            //newDoc.SaveAs(ref Filename, ref FileFormat, ref LockComments, ref Password, ref AddToRecentFiles, ref WritePassword, ref ReadOnlyRecommended, ref EmbedTrueTypeFonts, ref SaveNativePictureFormat, ref SaveFormsData, ref SaveAsAOCELetter, ref Encoding, ref InsertLineBreaks, ref AllowSubstitutions, ref LineEnding, ref AddBiDiMarks);
            newDoc.Save();
            object SaveChanges = false, OriginalFormat = missing, RouteDocument = missing;
            ((_Document)newDoc).Close(ref SaveChanges, ref OriginalFormat, ref RouteDocument);
        }
	}
}
