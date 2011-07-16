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

namespace ChmProcessor
{

    /// <summary>
    /// Command line handler for the generator.
    /// </summary>
    class CommandLineMode
    {

        
        [DllImport("kernel32.dll")]
        public static extern bool AttachConsole(int dwProcessId);
        const int ATTACH_PARENT_PROCESS = -1;
        
        private string projectFile = null;

        private enum ConsoleOperation { Run, Generate , ShowHelp };

        private ConsoleOperation op = ConsoleOperation.Run;
        private bool askConfirmations = true;
        private bool exitAfterGenerate = false;
        private bool outputQuiet = false;
        private int logLevel = 3;

        /// <summary>
        /// Shows a message to the user.
        /// If we are on quiet mode, we show it on the console. Otherwise a dialog will be showed.
        /// </summary>
        /// <param name="text">Message to show</param>
        private void Message(string text)
        {
            if (outputQuiet)
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
        private void Message(string text, Exception exception)
        {

            Console.WriteLine(text);
            Console.WriteLine(exception.ToString());

            /*if (outputQuiet)
            {
                // Write error to console
                Console.WriteLine(text);
                Console.WriteLine(exception.ToString());
            }
            else*/

            if( !outputQuiet )
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
        /// Writes a help message for the user.
        /// </summary>
        private void PrintUsage() {
            
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
        public void ReadCommandLine(string[] argv)
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
                        op = ConsoleOperation.Generate;
                    else if (argv[i].Equals("/y"))
                        // Dont ask for confirmations
                        askConfirmations = false;
                    else if (argv[i].Equals("/e"))
                        exitAfterGenerate = true;
                    else if (argv[i].Equals("/?"))
                        op = ConsoleOperation.ShowHelp;
                    else if (argv[i].Equals("/q"))
                    {
                        outputQuiet = true;
                    }
                    else if (argv[i].Equals("/l1"))
                    {
                        logLevel = 1;
                    }
                    else if (argv[1].Equals("/l2"))
                    {
                        logLevel = 2;
                    }
                    else if (argv[1].Equals("/l3"))
                    {
                        logLevel = 3;
                    }
                    else
                    {
                        Message("Unknown option " + argv[i]);
                        op = ConsoleOperation.ShowHelp;
                    }
                }
                else
                    projectFile = argv[i];
                i++;
            }
        }

        /// <summary>
        /// Executes the generation of a help project on the console.
        /// </summary>
        private void GenerateOnConsole()
        {
            // User interface that will log to the console:
            ConsoleUserInterface ui = new ConsoleUserInterface();
            ui.LogLevel = logLevel;

            try
            {
                ChmProject project = ChmProject.Open(projectFile);
                DocumentProcessor processor = new DocumentProcessor(project);
                processor.UI = ui;
                processor.GenerateHelp();
                ui.log("DONE!", 1);
            }
            catch (Exception ex)
            {
                ui.log(ex);
                ui.log("Failed", 1);
            }
        }

        /// <summary>
        /// Run the application.
        /// </summary>
        public void Run()
        {
            switch (op)
            {
                case ConsoleOperation.ShowHelp:
                    PrintUsage();
                    break;

                case ConsoleOperation.Generate:
                    // Generate right now a help project
                    if (projectFile == null)
                    {
                        Message("Not project file specified");
                        return;
                    }

                    if (outputQuiet)
                        GenerateOnConsole();
                    else
                    {
                        ChmProcessorForm frm = new ChmProcessorForm(projectFile);
                        frm.ProcessProject(askConfirmations, exitAfterGenerate, logLevel);
                        if (!exitAfterGenerate)
                            Application.Run(frm);
                    }
                    break;

                case ConsoleOperation.Run:
                    // Run the user interface
                    if (projectFile == null)
                        Application.Run(new ChmProcessorForm());
                    else
                        Application.Run(new ChmProcessorForm(projectFile));
                    break;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public CommandLineMode()
        {
            try
            {
                // Write output on console:
                AttachConsole(ATTACH_PARENT_PROCESS);
                //FunnyMicrosoftConsole.AttachConsoleToProcess();
            }
            catch
            {
                // AttachConsole is not defined at windows 2000 lower than SP 2.
            }
        }

        /// <summary>
        /// Application entry point.
        /// </summary>
        [STAThread]
        //[MTAThread]
        static void Main(string[] argv)
        {
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
