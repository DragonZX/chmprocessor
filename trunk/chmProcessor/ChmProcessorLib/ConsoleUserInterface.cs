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
using System.IO;

namespace ChmProcessorLib
{
    /// <summary>
    /// User interface of the help generation process for text console.
    /// </summary>
    public class ConsoleUserInterface : UserInterface
    {

        /// <summary>
        /// Code for error / warning messages.
        /// </summary>
        public static int ERRORWARNING = 1;

        /// <summary>
        /// Code for information messages.
        /// </summary>
        public static int INFO = 2;

        /// <summary>
        /// Code for debug messages.
        /// </summary>
        public static int DEBUG = 3;

        /// <summary>
        /// Maximum log level to write the log on the console.
        /// </summary>
        public int LogLevel = INFO;

        /// <summary>
        /// Checks if the user has requested to cancel.
        /// On console it has no sense.
        /// </summary>
        /// <returns>Always false</returns>
        public virtual bool CancellRequested()
        {
            return false;
        }

        /// <summary>
        /// Writes conditionally a log text.
        /// If the log level of the message is higher than LogLevel member, its not written.
        /// </summary>
        /// <param name="text">Text to log</param>
        /// <param name="level">Log level of the message</param>
        public void Log(string text, int level)
        {
            if (level <= this.LogLevel)
                Log(text);
        }

        /// <summary>
        /// Logs inconditionally a text.
        /// Writes the text to the console.
        /// </summary>
        /// <param name="text">Text to log</param>
        protected virtual void Log(string text)
        {
            try
            {
                Console.WriteLine(text);
            }
            catch {}
        }

        /// <summary>
        /// Called by the generation process to add an exception to the log.
        /// Its written to the console.
        /// </summary>
        /// <param name="text">Exception to log</param>
        public virtual void Log(Exception exception)
        {
            Console.WriteLine(exception.ToString());
        }

        /// <summary>
        /// Writes the content of a stream to the user interface.
        /// </summary>
        /// <param name="ui">User interface where to write the stream content</param>
        /// <param name="reader">The stream to write</param>
        /// <param name="logLevel">The log level for the log messages</param>
        static public void LogStream(UserInterface ui, StreamReader reader, int logLevel)
        {
            string linea = reader.ReadLine();
            while (linea != null)
            {
                ui.Log(linea, logLevel);
                linea = reader.ReadLine();
            }
        }

        /// <summary>
        /// Writes the content of a stream to the user interface.
        /// </summary>
        /// <param name="reader">The stream to write</param>
        /// <param name="logLevel">The log level for the log messages</param>
        public void LogStream(StreamReader reader, int logLevel)
        {
            LogStream(this, reader, logLevel);
        }
    }
}
