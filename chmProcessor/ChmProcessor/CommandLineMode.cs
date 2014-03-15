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
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;
using ChmProcessorLib;
using ChmProcessorLib.Log;

namespace ChmProcessor
{

    /// <summary>
    /// Command line handler for the UI generator.
    /// </summary>
    class CommandLineMode : CommandLine
    {

        
        [DllImport("kernel32.dll")]
        public static extern bool AttachConsole(int dwProcessId);
        const int ATTACH_PARENT_PROCESS = -1;

        /// <summary>
        /// Constructor
        /// </summary>
        public CommandLineMode()
        {
            try
            {
                // Write output on console:
                AttachConsole(ATTACH_PARENT_PROCESS);
            }
            catch
            {
                // AttachConsole is not defined at windows 2000 lower than SP 2.
            }
        }

        /// <summary>
        /// Shows a message to the user.
        /// </summary>
        /// <param name="text">Message to show</param>
        protected override void Message(string text)
        {
            if (OutputQuiet)
                Console.WriteLine(text);
            else
                MessageBox.Show(text);
        }

        /// <summary>
        /// Shows an error message to the user.
        /// If we are on quiet mode, we show it on the console. Otherwise a dialog will be showed.
        /// </summary>
        /// <param name="text">Error message</param>
        /// <param name="exception">The exception</param>
        protected override void Message(string text, Exception exception)
        {
            base.Message(text, exception);

            if (!OutputQuiet)
            {
                try
                {
                    // Show dialog with error details
                    new ExceptionMessageBox(text, exception).Show();
                }
                catch (Exception ex2)
                {
                    // Error showing error dialog...
                    Console.WriteLine("Error opening exception dialog:");
                    Console.WriteLine(ex2.ToString());
                }
            }
        }

        /// <summary>
        /// Generates the project file
        /// </summary>
        protected override void GenerateProject()
        {
            if (OutputQuiet)
                GenerateOnConsole();
            else
            {
                ChmProcessorForm frm = new ChmProcessorForm(ProjectFile);
                ChmLogLevel minimumLogLevel = 
                    frm.ProcessProject(AskConfirmations, ExitAfterGenerate, LogLevel);
                if (!ExitAfterGenerate)
                    Application.Run(frm);
                else
                    SetAplicationExitCode(minimumLogLevel);
            }
        }

        /// <summary>
        /// Launches the application UI 
        /// </summary>
        protected override void RunApplication()
        {
            // Run the user interface
            if (ProjectFile == null)
                Application.Run(new ChmProcessorForm());
            else
                Application.Run(new ChmProcessorForm(ProjectFile));
        }

        /// <summary>
        /// Application entry point.
        /// </summary>
        [STAThread]
        //[MTAThread]
        static void Main(string[] argv)
        {
            // TODO: Return value to check if the generation was right.
            CommandLineMode commandLineMode = new CommandLineMode();
            try
            {
                ExceptionMessageBox.UrlBugReport = "http://sourceforge.net/tracker/?group_id=197104&atid=960127";
                commandLineMode.ReadCommandLine(argv);
                commandLineMode.Run();
            }
            catch (Exception ex)
            {
                commandLineMode.Message("Unhandled exception", ex);
            }
        }

    }
}
