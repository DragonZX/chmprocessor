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

namespace ChmProcessorLib.Generators
{
    /// <summary>
    /// Tool to create a web help site from a document.
    /// TODO: The project directory creation, the topic pages generation and decoration, and the copy
    /// TODO: of additional files should be done here.
    /// </summary>
    public class WebHelpGenerator
    {

        /// <summary>
        /// Structured document to compile to CHM
        /// </summary>
        private ChmDocument Document;

        /// <summary>
        /// Log generator
        /// </summary>
        private UserInterface UI;

        /// <summary>
        /// Generation settings
        /// </summary>
        private ChmProject Project;

        public WebHelpGenerator(ChmDocument document, UserInterface ui, ChmProject project)
        {
            this.Document = document;
            this.UI = ui;
            this.Project = project;
        }

        /*
        public void Generate()
        {

            if (Project.FullTextSearch)
            {
                indexer = new WebIndex();
                string dbFile = dirWeb + Path.DirectorySeparatorChar + "fullsearchdb.db3";
                string dirTextFiles = dirWeb + Path.DirectorySeparatorChar + "textFiles";
                indexer.Connect(dbFile);
                indexer.CreateDatabase(System.Windows.Forms.Application.StartupPath + Path.DirectorySeparatorChar + "searchdb.sql", dirTextFiles);
                indexer.StoreConfiguration(Project.WebLanguage);
            }

        }

        private void GenerateSearchIndex()
        {
            WebIndex indexer = null;
            try
            {
                if (Project.FullTextSearch)
                {
                    indexer = new WebIndex();
                    string dbFile = Path.Combine( Project.WebDirectory , "fullsearchdb.db3" );
                    string dirTextFiles = dirWeb + Path.DirectorySeparatorChar + "textFiles";
                    indexer.Connect(dbFile);
                    indexer.CreateDatabase(System.Windows.Forms.Application.StartupPath + Path.DirectorySeparatorChar + "searchdb.sql", dirTextFiles);
                    indexer.StoreConfiguration(Project.WebLanguage);
                }

                // Create new files for the web help:
                GuardarDocumentos(dirWeb, webDecorator, indexer);
            }
            finally
            {
                if (indexer != null)
                    indexer.Disconnect();
            }
        }
        */
    }
}
