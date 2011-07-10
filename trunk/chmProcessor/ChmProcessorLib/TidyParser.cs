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
using Tidy;

namespace ChmProcessorLib
{
    /// <summary>
    /// Class to clean and repair HTML with Tidy (http://tidy.sourceforge.net/)
    /// </summary>
    public class TidyParser
    {
        /// <summary>
        /// Tidy encoding name for UTF-8
        /// </summary>
        public static string UTF8 = "utf8";

        /// <summary>
        /// User interface where to write the messages. If null, no messages will be written.
        /// </summary>
        private UserInterface ui;

        /// <summary>
        /// True if tidy should write the output as XHTML. If false, HTML will be written.
        /// </summary>
        private bool XmlOutput;

        /// <summary>
        /// Encoding for input files.
        /// Tidy encoding names are not equal to .NET encoding names. .NET uses IANA names and Tidy uses custom names.
        /// Tidy documentation says this encoding names as example: raw, ascii, latin0, latin1, utf8, iso2022, mac, win1252, ibm858, utf16le, utf16be, utf16, big5, shiftjis
        /// Here the Tidy name is used, not the IANA name.
        /// If null, we will use the default (tidy documentation say latin1)
        /// </summary>
        public string InputEncoding = null;

        /// <summary>
        /// Encoding for output files.
        /// Tidy encoding names are not equal to .NET encoding names. .NET uses IANA names and Tidy uses custom names.
        /// Tidy documentation says this encoding names as example: raw, ascii, latin0, latin1, utf8, iso2022, mac, win1252, ibm858, utf16le, utf16be, utf16, big5, shiftjis
        /// Here the Tidy name is used, not the IANA name.
        /// If null, we will use the default (tidy documentations says ascii)
        /// </summary>
        public string OutputEncoding = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ui">Object to output the tidy messages. It can be null, and no messages will be written.</param>
        public TidyParser(UserInterface ui)
        {
            this.ui = ui;
            this.XmlOutput = false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ui">Object to output the tidy messages. It can be null, and no messages will be written.</param>
        /// <param name="xmlOutput">True if the output should be written with XHTML format</param>
        public TidyParser(UserInterface ui, bool xmlOutput)
        {
            this.ui = ui;
            this.XmlOutput = xmlOutput;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ui">Object to output the tidy messages. It can be null, and no messages will be written.</param>
        /// <param name="outputEncoding">Tidy encoding name to write the output. If its null, the default 
        /// encoding (ASCII) will be used</param>
        public TidyParser(UserInterface ui, string outputEncoding)
        {
            this.ui = ui;
            this.OutputEncoding = outputEncoding;
        }

        /// <summary>
        /// Configures tidy to make the conversion / repair.
        /// </summary>
        /// <returns>The document to make the conversion</returns>
        protected Document ConfigureParse()
        {
            Document tdoc = new Document();
            int status = 0;
            // Set alternative text for IMG tags:
            status = tdoc.SetOptValue(TidyOptionId.TidyAltText, "image");
            CheckStatus(status);

            if (XmlOutput)
                status = tdoc.SetOptBool(TidyOptionId.TidyXhtmlOut, 1);
            CheckStatus(status);

            
            if(InputEncoding != null)
                status = tdoc.SetOptValue(TidyOptionId.TidyInCharEncoding, InputEncoding);
            CheckStatus(status);

            if (OutputEncoding != null)
                status = tdoc.SetOptValue(TidyOptionId.TidyOutCharEncoding, OutputEncoding);
            CheckStatus(status);
            
            return tdoc;
        }

        public void Parse( string file ) {

            try
            {
                log("Parsing file " + file + "...", 2);

                Document tdoc = ConfigureParse();

                int status = 0;
                status = tdoc.ParseFile(file);
                CheckStatus(status);

                status = tdoc.CleanAndRepair();
                CheckStatus(status);

                status = tdoc.SaveFile(file);
                CheckStatus(status);
            }
            catch (Exception ex)
            {
                log(ex);
            }
        }

        public string ParseString(string htmlText)
        {
            log("Parsing html...", 2);

            Document tdoc = ConfigureParse();

            int status = 0;
            status = tdoc.ParseString(htmlText);
            CheckStatus(status);

            status = tdoc.CleanAndRepair();
            CheckStatus(status);

            string cleanHtml = tdoc.SaveString();
            CheckStatus(status);

            return cleanHtml;
        }

        private void CheckStatus(int status) {
            if (status < 0)
                throw new Exception("Error runing Tidy.NET: " + status);
        }

        private void log(string text, int level)
        {
            if (ui != null)
                ui.log("Tidy: " + text, level);
        }

        private void log(Exception ex)
        {
            if (ui != null)
                ui.log(ex);
        }

    }
}
