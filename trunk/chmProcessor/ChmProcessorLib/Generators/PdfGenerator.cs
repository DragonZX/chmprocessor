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

namespace ChmProcessorLib.Generators
{
    /// <summary>
    /// Tool to create a PDF file from a document
    /// </summary>
    public class PdfGenerator
    {

        private ChmProject Project;

        private UserInterface UI;

        private string MainSourceFilePath;

        public PdfGenerator(string mainSourceFilePath, UserInterface ui, ChmProject project)
        {
            this.MainSourceFilePath = mainSourceFilePath;
            this.UI = ui;
            this.Project = project;
        }

        /// <summary>
        /// Generate a PDF file for the document.
        /// </summary>
        public void Generate()
        {
            try
            {
                UI.Log("Generating PDF file", ConsoleUserInterface.INFO);
                if (Project.PdfGeneration == ChmProject.PdfGenerationWay.OfficeAddin)
                {
                    MSWord word = new MSWord();
                    if( !word.SaveWordToPdf(MainSourceFilePath, Project.PdfPath) )
                        UI.Log("Warning: There was a time out waiting to close the word document", 1);
                }
                else
                {
                    PdfPrinter pdfPrinter = new PdfPrinter();
                    pdfPrinter.ConvertToPdf(MainSourceFilePath, Project.PdfPath);
                }
            }
            catch (Exception ex)
            {
                if (Project.PdfGeneration == ChmProject.PdfGenerationWay.OfficeAddin)
                    UI.Log("Something wrong happened with the PDF generation. Remember you must to have Microsoft Office 2007 and the" +
                        "pdf/xps generation add-in (http://www.microsoft.com/downloads/details.aspx?FamilyID=4D951911-3E7E-4AE6-B059-A2E79ED87041&displaylang=en)", ConsoleUserInterface.ERRORWARNING);
                else
                    UI.Log("Something wrong happened with the PDF generation. Remember you must to have PdfCreator (VERSION " + PdfPrinter.SUPPORTEDVERSION +
                        " AND ONLY THIS VERSION) installed into your computer to " +
                        "generate a PDF file. You can download it from http://www.pdfforge.org/products/pdfcreator/download", ConsoleUserInterface.ERRORWARNING);
                UI.Log(ex);
            }
        }
    }
}
