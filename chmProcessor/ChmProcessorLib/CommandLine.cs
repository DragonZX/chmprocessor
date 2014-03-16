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

namespace ChmProcessorLib
{
    /// <summary>
    /// Base for command line handlers.
    /// Used by the ChmProcessor.exe and ChmProcessorCmd.exe to handle the command line
    /// </summary>
    public abstract class CommandLine
    {

        /// <summary>
        /// Operation types to execute from the command line
        /// </summary>
        protected enum ConsoleOperation { Run, Generate, ShowHelp };

        /// <summary>
        /// File path on the command line call to the project / word / html file to generate
        /// </summary>
        protected string ProjectFile = null;

        /// <summary>
        /// Operation defined on the command line call
        /// </summary>
        protected ConsoleOperation Op = ConsoleOperation.Run;

        /// <summary>
        /// Should we ask questions to the user?
        /// </summary>
        protected bool AskConfirmations = true;

        /// <summary>
        /// Exit the application after generation is finished?
        /// </summary>
        protected bool ExitAfterGenerate = false;

        /// <summary>
        /// Should we show the user interface when generating? 
        /// </summary>
        protected bool OutputQuiet = false;

        /// <summary>
        /// Log level selected
        /// </summary>
        protected ChmLogLevel LogLevel = ChmLogLevel.DEBUG;

        /// <summary>
        /// Shows a message to the user.
        /// </summary>
        /// <param name="text">Message to show</param>
        protected abstract void Message(string text);

        /// <summary>
        /// Shows an error message to the user.
        /// If we are on quiet mode, we show it on the console. Otherwise a dialog will be showed.
        /// </summary>
        /// <param name="text">Error message</param>
        /// <param name="exception">The exception</param>
        protected virtual void Message(string text, Exception exception)
        {
            Console.WriteLine(text);
            Console.WriteLine(exception.ToString());
            SetAplicationExitCode(ChmLogLevel.ERROR);
        }

        /// <summary>
        /// Writes a help message for the user.
        /// </summary>
        protected void PrintUsage() {
            
            String txt =
                "Use " + Path.GetFileName(Application.ExecutablePath) +
                " [<projectfile.WHC>] [/g] [/e] [/y] [/?] [/q] [/l1] [/l2] [/l3] [/l4]\n" +
                "Options:\n" +
                "/g\tGenerate help sets (chm, javahelp, pdfs,…) specified by the project\n" +
                "/e\tExit after generate\n" +
                "/y\tDont ask for confirmations\n" +
                "/?\tPrint this help and exit\n" +
                "/q\tPrevents a window being shown when run with the /g command line and logs " + 
                "messages to stdout/stderr\n" +
                "/l1 /l2 /l3 /4\tLets you choose how much information is output, where /l1 are errors, " +
                "/l2 warnings, /l3 application status information and /l4 are debug messages";
            Message(txt);
        }

        /// <summary>
        /// Process the command line parameters
        /// </summary>
        /// <param name="argv">The command line parameters</param>
        protected void ReadCommandLine(string[] argv)
        {
            int i = 0;
            while (i < argv.Length)
            {
                if (argv[i].StartsWith("/"))
                {
                    // Option:
                    argv[i] = argv[i].ToLower();
                    if (argv[i].Equals("/g"))
                        // Generate at windows:
                        Op = ConsoleOperation.Generate;
                    else if (argv[i].Equals("/y"))
                        // Dont ask for confirmations
                        AskConfirmations = false;
                    else if (argv[i].Equals("/e"))
                        ExitAfterGenerate = true;
                    else if (argv[i].Equals("/?"))
                        Op = ConsoleOperation.ShowHelp;
                    else if (argv[i].Equals("/q"))
                        OutputQuiet = true;
                    else if (argv[i].Equals("/l1"))
                        LogLevel = ChmLogLevel.ERROR;
                    else if (argv[i].Equals("/l2"))
                        LogLevel = ChmLogLevel.WARNING;
                    else if (argv[i].Equals("/l3"))
                        LogLevel = ChmLogLevel.INFO;
                    else if (argv[i].Equals("/l4"))
                        LogLevel = ChmLogLevel.DEBUG;
                    else
                    {
                        Message("Unknown option " + argv[i]);
                        Op = ConsoleOperation.ShowHelp;
                        SetAplicationExitCode(ChmLogLevel.ERROR);
                    }
                }
                else
                    ProjectFile = argv[i];
                i++;
            }
        }

        /// <summary>
        /// Executes the generation of a help project on the console.
        /// </summary>
        protected void GenerateOnConsole()
        {
            // User interface that will log to the console:
            ConsoleUserInterface ui = new ConsoleUserInterface();
            ui.LogLevel = LogLevel;

            try
            {
                // Read or create the default project
                ChmProject project = ChmProject.OpenChmProjectOrWord(ProjectFile);

                // Check the project
                ChmProjectVerifier verifier = new ChmProjectVerifier(project, false, false);
                if (!verifier.Verifiy())
                {
                    SetAplicationExitCode(ChmLogLevel.ERROR);
                    return;
                }

                // Generate the products
                DocumentProcessor processor = new DocumentProcessor(project, ui);
                processor.GenerateHelp();
                ui.Log("DONE!", ChmLogLevel.INFO);
            }
            catch (Exception ex)
            {
                ui.Log(ex);
                ui.Log("Failed", ChmLogLevel.ERROR);
            }

            // Set the application exit code
            SetAplicationExitCode(ui.MinimumChmLogLevel);

        }

        /// <summary>
        /// Generates the project file
        /// </summary>
        protected abstract void GenerateProject();

        /// <summary>
        /// Launches the application UI 
        /// </summary>
        protected abstract void RunApplication();

        /// <summary>
        /// Run the application.
        /// </summary>
        public void Run()
        {
            switch (Op)
            {
                case ConsoleOperation.ShowHelp:
                    PrintUsage();
                    break;

                case ConsoleOperation.Generate:
                    // Generate right now a help project
                    if (ProjectFile == null)
                    {
                        Message("No file specified");
                        SetAplicationExitCode(ChmLogLevel.ERROR);
                        return;
                    }

                    GenerateProject();
                    break;

                case ConsoleOperation.Run:
                    // Run the user interface
                    RunApplication();
                    break;
            }
        }

        /// <summary>
        /// Set the application exit code from a level log message value.
        /// </summary>
        /// <remarks>
        /// Will set 0 all was ok, 1 if there was errors and 2 if there was warnings.
        /// </remarks>
        /// <param name="logLevel">The log level. ChmLogLevel.ERROR, .WARNING, etc.</param>
        static public void SetAplicationExitCode(ChmLogLevel logLevel)
        {
            switch (logLevel)
            {
                case ChmLogLevel.DEBUG:
                case ChmLogLevel.INFO:
                    Environment.ExitCode = 0;
                    break;

                case ChmLogLevel.WARNING:
                    Environment.ExitCode = 1;
                    break;

                case ChmLogLevel.ERROR:
                    Environment.ExitCode = 2;
                    break;
            }
        }
    }
}
