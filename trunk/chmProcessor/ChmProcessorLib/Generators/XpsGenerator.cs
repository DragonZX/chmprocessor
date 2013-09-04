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
            UI.Log("Generating XPS file", ConsoleUserInterface.INFO);
            try
            {
                MSWord word = new MSWord();
                word.SaveWordToXps(MainSourceFilePath, Project.XpsPath);
            }
            catch (Exception ex)
            {
                UI.Log("Something wrong happened with the XPS generation. Remember you must to have Microsoft Office 2007 and the " +
                        "pdf/xps generation add-in (http://www.microsoft.com/downloads/details.aspx?FamilyID=4D951911-3E7E-4AE6-B059-A2E79ED87041&displaylang=en)", ConsoleUserInterface.ERRORWARNING);
                UI.Log(ex);
            }
        }
    }
}
