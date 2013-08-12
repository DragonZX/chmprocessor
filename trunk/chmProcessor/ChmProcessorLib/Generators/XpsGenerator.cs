using System;
using System.Collections.Generic;
using System.Text;

namespace ChmProcessorLib.Generators
{

    /// <summary>
    /// Tool to create a XPS file from a document
    /// </summary>
    public class XpsGenerator
    {

        private ChmProject Project;

        private UserInterface UI;

        private string MainSourceFilePath;

        public XpsGenerator(string mainSourceFilePath, UserInterface ui, ChmProject project)
        {
            this.MainSourceFilePath = mainSourceFilePath;
            this.UI = ui;
            this.Project = project;
        }

        /// <summary>
        /// Generate a XPS file for the document.
        /// </summary>
        public void Generate()
        {
            UI.log("Generating XPS file", ConsoleUserInterface.INFO);
            try
            {
                MSWord word = new MSWord();
                word.SaveWordToXps(MainSourceFilePath, Project.XpsPath);
            }
            catch (Exception ex)
            {
                UI.log("Something wrong happened with the XPS generation. Remember you must to have Microsoft Office 2007 and the " +
                        "pdf/xps generation add-in (http://www.microsoft.com/downloads/details.aspx?FamilyID=4D951911-3E7E-4AE6-B059-A2E79ED87041&displaylang=en)", ConsoleUserInterface.ERRORWARNING);
                UI.log(ex);
            }
        }
    }
}
