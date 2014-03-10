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

        protected bool OutputQuiet = false;

        protected int LogLevel = 3;

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
        }

        /// <summary>
        /// Writes a help message for the user.
        /// </summary>
        protected void PrintUsage() {
            
            String txt =
                "Use " + Path.GetFileName(Application.ExecutablePath) + 
                " [<projectfile.WHC>] [/g] [/e] [/y] [/?] [/q] [/l1] [/l2] [/l3]\n" +
                "Options:\n" +
                "/g\tGenerate help sets (chm, javahelp, pdfs,…) specified by the project\n" +
                "/e\tExit after generate\n" +
                "/y\tDont ask for confirmations\n" +
                "/?\tPrint this help and exit\n" +
                "/q\tPrevents a window being shown when run with the /g command line and logs messages to stdout/stderr\n" +
                "/l1 /l2 /l3\tLets you choose how much information is output, where /l1 is minimal and /l3 is all the information";
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
                    {
                        OutputQuiet = true;
                    }
                    else if (argv[i].Equals("/l1"))
                    {
                        LogLevel = 1;
                    }
                    else if (argv[1].Equals("/l2"))
                    {
                        LogLevel = 2;
                    }
                    else if (argv[1].Equals("/l3"))
                    {
                        LogLevel = 3;
                    }
                    else
                    {
                        Message("Unknown option " + argv[i]);
                        Op = ConsoleOperation.ShowHelp;
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
                ChmProject project = ChmProject.OpenChmProjectOrWord(ProjectFile);
                DocumentProcessor processor = new DocumentProcessor(project, ui);
                processor.GenerateHelp();
                ui.Log("DONE!", 1);
            }
            catch (Exception ex)
            {
                ui.Log(ex);
                ui.Log("Failed", 1);
            }
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

    }
}
