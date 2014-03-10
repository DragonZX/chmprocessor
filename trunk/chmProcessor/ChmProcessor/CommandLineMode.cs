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
    /// Command line handler for the UI generator.
    /// </summary>
    class CommandLineMode : CommandLine
    {

        
        [DllImport("kernel32.dll")]
        public static extern bool AttachConsole(int dwProcessId);
        const int ATTACH_PARENT_PROCESS = -1;

        ///// <summary>
        ///// Operation types to execute from the command line
        ///// </summary>
        //private enum ConsoleOperation { Run, Generate, ShowHelp };

        ///// <summary>
        ///// File path on the command line call to the project / word / html file to generate
        ///// </summary>
        //private string ProjectFile = null;

        ///// <summary>
        ///// Operation defined on the command line call
        ///// </summary>
        //private ConsoleOperation Op = ConsoleOperation.Run;

        ///// <summary>
        ///// Should we ask questions to the user?
        ///// </summary>
        //private bool AskConfirmations = true;

        ///// <summary>
        ///// Exit the application after generation is finished?
        ///// </summary>
        //private bool ExitAfterGenerate = false;

        //private bool OutputQuiet = false;

        //private int LogLevel = 3;

        ///// <summary>
        ///// Shows a message to the user.
        ///// If we are on quiet mode, we show it on the console. Otherwise a dialog will be showed.
        ///// </summary>
        ///// <param name="text">Message to show</param>
        //private void Message(string text)
        //{
        //    if (OutputQuiet)
        //        Console.WriteLine(text);
        //    else
        //        MessageBox.Show(text);
        //}

        ///// <summary>
        ///// Shows an error message to the user.
        ///// If we are on quiet mode, we show it on the console. Otherwise a dialog will be showed.
        ///// </summary>
        ///// <param name="text">Error message</param>
        ///// <param name="exception">The exception</param>
        //private void Message(string text, Exception exception)
        //{

        //    Console.WriteLine(text);
        //    Console.WriteLine(exception.ToString());

        //    if( !OutputQuiet )
        //    {
        //        try
        //        {
        //            // Show dialog with error details
        //            new ExceptionMessageBox(text, exception).Show();
        //        }
        //        catch (Exception ex2)
        //        {
        //            // Error showing error dialog...
        //            Console.WriteLine("Error opening exception dialog:");
        //            Console.WriteLine(ex2.ToString());
        //        }
        //    }

        //}

        ///// <summary>
        ///// Writes a help message for the user.
        ///// </summary>
        //private void PrintUsage() {
            
        //    String txt =
        //        "Use " + Path.GetFileName(Application.ExecutablePath) + 
        //        " [<projectfile.WHC>] [/g] [/e] [/y] [/?] [/q] [/l1] [/l2] [/l3]\n" +
        //        "Options:\n" +
        //        "/g\tGenerate help sets (chm, javahelp, pdfs,…) specified by the project\n" +
        //        "/e\tExit after generate\n" +
        //        "/y\tDont ask for confirmations\n" +
        //        "/?\tPrint this help and exit\n" +
        //        "/q\tPrevents a window being shown when run with the /g command line and logs messages to stdout/stderr\n" +
        //        "/l1 /l2 /l3\tLets you choose how much information is output, where /l1 is minimal and /l3 is all the information";
        //    Message(txt);
        //}

        ///// <summary>
        ///// Process the command line parameters
        ///// </summary>
        ///// <param name="argv">The command line parameters</param>
        //public void ReadCommandLine(string[] argv)
        //{
        //    int i = 0;
        //    while (i < argv.Length)
        //    {
        //        if (argv[i].StartsWith("/"))
        //        {
        //            // Option:
        //            argv[i] = argv[i].ToLower();
        //            if (argv[i].Equals("/g"))
        //                // Generate at windows:
        //                Op = ConsoleOperation.Generate;
        //            else if (argv[i].Equals("/y"))
        //                // Dont ask for confirmations
        //                AskConfirmations = false;
        //            else if (argv[i].Equals("/e"))
        //                ExitAfterGenerate = true;
        //            else if (argv[i].Equals("/?"))
        //                Op = ConsoleOperation.ShowHelp;
        //            else if (argv[i].Equals("/q"))
        //            {
        //                OutputQuiet = true;
        //            }
        //            else if (argv[i].Equals("/l1"))
        //            {
        //                LogLevel = 1;
        //            }
        //            else if (argv[1].Equals("/l2"))
        //            {
        //                LogLevel = 2;
        //            }
        //            else if (argv[1].Equals("/l3"))
        //            {
        //                LogLevel = 3;
        //            }
        //            else
        //            {
        //                Message("Unknown option " + argv[i]);
        //                Op = ConsoleOperation.ShowHelp;
        //            }
        //        }
        //        else
        //            ProjectFile = argv[i];
        //        i++;
        //    }
        //}

        ///// <summary>
        ///// Executes the generation of a help project on the console.
        ///// </summary>
        //private void GenerateOnConsole()
        //{
        //    // User interface that will log to the console:
        //    ConsoleUserInterface ui = new ConsoleUserInterface();
        //    ui.LogLevel = LogLevel;

        //    try
        //    {
        //        ChmProject project = ChmProject.OpenChmProjectOrWord(ProjectFile);
        //        DocumentProcessor processor = new DocumentProcessor(project, ui);
        //        processor.GenerateHelp();
        //        ui.Log("DONE!", 1);
        //    }
        //    catch (Exception ex)
        //    {
        //        ui.Log(ex);
        //        ui.Log("Failed", 1);
        //    }
        //}

        ///// <summary>
        ///// Run the application.
        ///// </summary>
        //public void Run()
        //{
        //    switch (Op)
        //    {
        //        case ConsoleOperation.ShowHelp:
        //            PrintUsage();
        //            break;

        //        case ConsoleOperation.Generate:
        //            // Generate right now a help project
        //            if (ProjectFile == null)
        //            {
        //                Message("No file specified");
        //                return;
        //            }

        //            if (OutputQuiet)
        //                GenerateOnConsole();
        //            else
        //            {
        //                ChmProcessorForm frm = new ChmProcessorForm(ProjectFile);
        //                frm.ProcessProject(AskConfirmations, ExitAfterGenerate, LogLevel);
        //                if (!ExitAfterGenerate)
        //                    Application.Run(frm);
        //            }
        //            break;

        //        case ConsoleOperation.Run:
        //            // Run the user interface
        //            if (ProjectFile == null)
        //                Application.Run(new ChmProcessorForm());
        //            else
        //                Application.Run(new ChmProcessorForm(ProjectFile));
        //            break;
        //    }
        //}

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
                frm.ProcessProject(AskConfirmations, ExitAfterGenerate, LogLevel);
                if (!ExitAfterGenerate)
                    Application.Run(frm);
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
