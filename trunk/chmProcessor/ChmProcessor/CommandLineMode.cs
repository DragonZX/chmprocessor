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

namespace ProcesadorHtml
{

    /// <summary>
    /// Command line handler for the generator.
    /// </summary>
    class CommandLineMode
    {

        [DllImport("kernel32.dll")]
        public static extern bool AttachConsole(int dwProcessId);

        const int ATTACH_PARENT_PROCESS = -1;

        string projectFile = null;

        enum ConsoleOperation { Run , Generate };

        private void PrintUsage() {
            Console.WriteLine("Use chmProcessor.exe [<projectfile.WHC>] [/g] [/e] [/y] [/?]");
            Console.WriteLine("Options:");
            Console.WriteLine("/g\tGenerate help sets (chm, javahelp, pdfs,…) specified by the project");
            Console.WriteLine("/e\tExit after generate");
            Console.WriteLine("/y\tDont ask for confirmations");
            Console.WriteLine("/?\tPrint this help and exit");
        }

        private void GenerateConsole(string file)
        {
            if (!File.Exists(file))
            {
                Console.WriteLine("File " + file + " does not exist");
                return;
            }

        }

        // Constructor.
        public CommandLineMode(string[] argv)
        {
            ConsoleOperation op = ConsoleOperation.Run;
            bool askConfirmations = true;
            bool exitAfterGenerate = false;

            try
            {
                AttachConsole(ATTACH_PARENT_PROCESS);
            }
            catch
            {
                // AttachConsole is not defined at windows 2000 lower than SP 2.
            }

            int i = 0;
            while( i < argv.Length ) {
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
                    {
                        PrintUsage();
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Unknown option " + argv[i]);
                        PrintUsage();
                        return;
                    }
                }
                else
                    projectFile = argv[i];
                i++;
            }

            if( op == ConsoleOperation.Generate ) {
                if( projectFile == null ) {
                    Console.WriteLine("Not project file specified");
                    return;
                }

                ChmProcessorForm frm = new ChmProcessorForm(projectFile);
                frm.ProcessProject(askConfirmations, true, exitAfterGenerate);
                if (!exitAfterGenerate)
                    Application.Run(frm);
            }
            else if ( op == ConsoleOperation.Run )
            {
                if( projectFile == null )
                    Application.Run(new ChmProcessorForm());
                else
                    Application.Run(new ChmProcessorForm(projectFile));
            }
        }

        /// <summary>
        /// Application entry point.
        /// </summary>
        [STAThread]
        /*[MTAThread]*/
        static void Main(string[] argv)
        {
            try
            {
                ExceptionMessageBox.UrlBugReport = "http://sourceforge.net/tracker/?group_id=197104&atid=960127";
                new CommandLineMode(argv);
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Unhandled exception: " + ex.Message + "\n" + ex.StackTrace);
                new ExceptionMessageBox(ex).ShowDialog();
            }
        }

    }
}
