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
using System.Diagnostics;
using ChmProcessorLib.Log;

namespace ChmProcessorLib
{
    /// <summary>
    /// Tool to make a command line process execution
    /// </summary>
    public class CommandLineExecution
    {

        /// <summary>
        /// Number of miliseconds to wait for process execution
        /// </summary>
        private const int MAXIMUMWAIT = 1000;

        /// <summary>
        /// Process execution information
        /// </summary>
        public ProcessStartInfo Info;

        /// <summary>
        /// Information about the executed process
        /// </summary>
        public Process ExecutionProcess;

        /// <summary>
        /// If true, all the standar error stream will be written on UI.
        /// </summary>
        public bool LogStandardError = true;

        /// <summary>
        /// Log generation
        /// </summary>
        private UserInterface UI;

        public CommandLineExecution(string exePath, string parameters, UserInterface ui)
        {
            this.UI = ui;

            Info = new ProcessStartInfo(exePath, parameters);
            Info.UseShellExecute = false;
            Info.RedirectStandardOutput = true;
            Info.RedirectStandardError = true;
            Info.CreateNoWindow = true;
        }

        public int Execute()
        {
            ExecutionProcess = Process.Start(Info);
            while (!ExecutionProcess.WaitForExit(MAXIMUMWAIT))
                UI.LogStream(ExecutionProcess.StandardOutput, ChmLogLevel.DEBUG);
            UI.LogStream(ExecutionProcess.StandardOutput, ChmLogLevel.DEBUG);

            if( LogStandardError )
                UI.LogStream(ExecutionProcess.StandardError, ChmLogLevel.ERROR);

            return ExecutionProcess.ExitCode;
        }

        public override string ToString()
        {
            if (Info == null || string.IsNullOrEmpty( Info.FileName ) )
                return string.Empty;
            string txt = Info.FileName;
            if (!string.IsNullOrEmpty(Info.Arguments))
                txt += " " + Info.Arguments;
            return txt;
        }
    }
}
