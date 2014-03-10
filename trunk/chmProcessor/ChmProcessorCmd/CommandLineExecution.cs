using System;
using System.Collections.Generic;
using System.Text;
using ChmProcessorLib;

namespace ChmProcessorCmd
{

    /// <summary>
    /// Command line handler for the Console generator.
    /// </summary>
    class CommandLineExecution : CommandLine
    {

        /// <summary>
        /// Shows a message to the user.
        /// </summary>
        /// <param name="text">Message to show</param>
        protected override void Message(string text)
        {
            Console.WriteLine(text);
        }

        /// <summary>
        /// Launches the application UI. Not supported by this exe.
        /// </summary>
        protected override void RunApplication()
        {
            Console.WriteLine("This exe is for command line help generation only. Use ChmProcessor.exe instead to open the Windows user interface.");
            PrintUsage();
        }

        /// <summary>
        /// Generates the project file
        /// </summary>
        protected override void GenerateProject()
        {
            GenerateOnConsole();
        }

        /// <summary>
        /// Application entry point.
        /// </summary>
        [STAThread]
        static void Main(string[] argv)
        {
            // TODO: Return value to check if the generation was right.
            CommandLineExecution commandLineMode = new CommandLineExecution();
            try
            {
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
