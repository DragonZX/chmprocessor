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
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace ChmProcessorLib
{
    /// <summary>
    /// Class to clean and repair HTML with Tidy (http://tidy.sourceforge.net/)
    /// </summary>
    public class TidyParser
    {

        /// <summary>
        /// Path to the tidy executable.
        /// </summary>
        private static string TidyPath {
            get
            {
                return Application.StartupPath + Path.DirectorySeparatorChar + "tidy.exe";
            }
        }

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
        public bool XmlOutput;

        /// <summary>
        /// Encoding for output files.
        /// Tidy encoding names are not equal to .NET encoding names. .NET uses IANA names and Tidy uses custom names.
        /// Tidy documentation says this encoding names as example: raw, ascii, latin0, latin1, utf8, iso2022, mac, win1252, ibm858, utf16le, utf16be, utf16, big5, shiftjis
        /// Here the Tidy name is used, not the IANA name.
        /// If null, we will use the default (tidy documentations says ascii)
        /// </summary>
        public string Encoding = null;

        /// <summary>
        /// Standard output for tidy execution
        /// </summary>
        private string StandardOutput;

        /// <summary>
        /// Standard error output for tidy execution
        /// </summary>
        private string StandardError;

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
        /// <param name="encoding">Tidy encoding name to read/write the output. See the tidy documentacion.
        /// If its null, the default encoding (ASCII?) will be used</param>
        public TidyParser(UserInterface ui, string encoding)
        {
            this.ui = ui;
            this.Encoding = encoding;
        }

        /// <summary>
        /// Configures tidy commandline to make the conversion / repair.
        /// </summary>
        /// <returns>The command line with the options configured</returns>
        protected string ConfigureParse()
        {
            /*
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
            
            return tdoc;*/

            string commandLine = "--alt-text image";
            if (Encoding != null)
                commandLine += " -" + Encoding;
            if (XmlOutput)
                commandLine += " -asxml";
            return commandLine;
        }

        /// <summary>
        /// Called when tidy writes something on the standard error
        /// </summary>
        public void ErrorReceivedEventHandler(Object sender,DataReceivedEventArgs e) 
        {
            StandardError += e.Data;
        }

        /// <summary>
        /// Called when tidy writes something on the standard output
        /// </summary>
        public void OutputReceivedEventHandler(Object sender, DataReceivedEventArgs e)
        {
            StandardOutput += e.Data;
        }

        /// <summary>
        /// Executes the command line for tidy.exe
        /// </summary>
        /// <param name="parameters">Command line parameters for the execution</param>
        /// <param name="stdInputForTidy">If its not null, the standard input to "inject" to tidy. If
        /// its null, no standard input will be injected</param>
        /// <returns>Standard output of the execution</returns>
        private string ExecuteTidy(string parameters, StreamReader stdInputForTidy)
        {
            if (!File.Exists(TidyPath))
                throw new Exception("Tidy executable " + TidyPath + " not found.");

            Process process = new Process();
            process.StartInfo.FileName = TidyPath;
            process.StartInfo.Arguments = parameters;
            process.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
            process.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
            process.StartInfo.UseShellExecute = false;
            if (stdInputForTidy != null)
                process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.ErrorDataReceived += new DataReceivedEventHandler(ErrorReceivedEventHandler);
            process.OutputDataReceived += new DataReceivedEventHandler(OutputReceivedEventHandler);
            process.Start();

            // Start asyncronous standard output / error read
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
           
            // Write the standard input content:
            if (stdInputForTidy != null)
            {
                process.StandardInput.Write(stdInputForTidy.ReadToEnd());
                process.StandardInput.Close();
            }

            // Wait for end of execution:
            /*if (!process.WaitForExit(60 * 1000))
            {
                try { process.Kill(); }
                catch { }
                throw new Exception("Tidy hang up");
            }*/
            process.WaitForExit();

            // Check the exit code:
            switch( process.ExitCode ) {
                case 0:
                    // All input files were processed successfully.
                    break;

                case 1:
                    // There were warnings:
                    // Dont write: They are html warnings and usually is too much text
                    //log("There were tidy warnings: " + StandardError, ConsoleUserInterface.ERRORWARNING);
                    break;

                case 2:
                    // There were errors:
                    log("There were tidy errors:\n" + StandardError, ConsoleUserInterface.ERRORWARNING);
                    break;
            }

            return StandardOutput;
        }

        public void Parse( string file ) {

            try
            {
                log("Parsing file " + file + "...", 2);

                /*Document tdoc = ConfigureParse();

                int status = 0;
                status = tdoc.ParseFile(file);
                CheckStatus(status);

                status = tdoc.CleanAndRepair();
                CheckStatus(status);

                status = tdoc.SaveFile(file);
                CheckStatus(status);*/

                string parameters = ConfigureParse();
                // Set the file path to repair:
                parameters += " -modify \"" + file + "\"";
                ExecuteTidy(parameters, null);
            }
            catch (Exception ex)
            {
                log(ex);
            }
        }

        // TODO: NOT TESTED. MAYBE DOES NOT WORK.
        public string ParseString(string htmlText)
        {
            log("Parsing html...", 2);

            /*Document tdoc = ConfigureParse();

            int status = 0;
            status = tdoc.ParseString(htmlText);
            CheckStatus(status);

            status = tdoc.CleanAndRepair();
            CheckStatus(status);

            string cleanHtml = tdoc.SaveString();
            CheckStatus(status);

            return cleanHtml;*/

            string parameters = ConfigureParse();
            MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(htmlText));
            return ExecuteTidy(parameters,new StreamReader(ms));
        }

        /*private void CheckStatus(int status) {
            if (status < 0)
                throw new Exception("Error runing Tidy.NET: " + status);
        }*/

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
