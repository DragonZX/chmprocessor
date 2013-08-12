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
                UI.log("Generating PDF file", ConsoleUserInterface.INFO);
                if (Project.PdfGeneration == ChmProject.PdfGenerationWay.OfficeAddin)
                {
                    MSWord word = new MSWord();
                    word.SaveWordToPdf(MainSourceFilePath, Project.PdfPath);
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
                    UI.log("Something wrong happened with the PDF generation. Remember you must to have Microsoft Office 2007 and the" +
                        "pdf/xps generation add-in (http://www.microsoft.com/downloads/details.aspx?FamilyID=4D951911-3E7E-4AE6-B059-A2E79ED87041&displaylang=en)", ConsoleUserInterface.ERRORWARNING);
                else
                    UI.log("Something wrong happened with the PDF generation. Remember you must to have PdfCreator (VERSION " + PdfPrinter.SUPPORTEDVERSION +
                        " AND ONLY THIS VERSION) installed into your computer to " +
                        "generate a PDF file. You can download it from http://www.pdfforge.org/products/pdfcreator/download", ConsoleUserInterface.ERRORWARNING);
                UI.log(ex);
            }
        }
    }
}
