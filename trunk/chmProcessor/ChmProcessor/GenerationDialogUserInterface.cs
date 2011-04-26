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
using System.Linq;
using System.Text;
using ChmProcessorLib;

namespace ChmProcessor
{
    /// <summary>
    /// Class to handle the log and cancellation of the help generation process on windows interface.
    /// </summary>
    class GenerationDialogUserInterface : ConsoleUserInterface
    {
        /// <summary>
        /// Dialog attached to this user interface
        /// </summary>
        private GenerationDialog dialog;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dialog">Dialog to attach to the user interface</param>
        public GenerationDialogUserInterface(GenerationDialog dialog)
        {
            this.dialog = dialog;
        }

        /// <summary>
        /// Logs inconditionally a text.
        /// Writes the text to the console and to the dialog.
        /// </summary>
        /// <param name="text">Text to log</param>
        override protected void log(string text)
        {
            //base.log(text);
            dialog.Log(text);
        }

        /// <summary>
        /// Checks if the user has pressed the cancel button.
        /// </summary>
        /// <returns>True if the cancel button was pressed</returns>
        override public bool CancellRequested()
        {
            return dialog.CancellationPending;
        }

        /// <summary>
        /// Called by the generation process to add an exception to the log.
        /// Its written to the console and to the generation dialog.
        /// </summary>
        /// <param name="text">Exception to log</param>
        override public void log(Exception exception)
        {
            //base.log(exception);
            dialog.Log(exception);
        }
    }
}
