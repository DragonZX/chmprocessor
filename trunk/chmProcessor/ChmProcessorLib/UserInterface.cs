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

namespace ChmProcessorLib
{
    /// <summary>
    /// Interface to control the process of the CHM generation.
    /// </summary>
    public interface UserInterface
    {

        /// <summary>
        /// Notifies to the generation process that the user wants to cancel the generation process.
        /// </summary>
        /// <returns>True if the user requested to cancel the process</returns>
        bool CancellRequested();

        /// <summary>
        /// Called by the generation process to add a text to the log.
        /// </summary>
        /// <param name="text">Text to log</param>
        /// <param name="level">Level of the message. 1 for errors / warnings, 2 for information, 3
        /// for debug messages</param>
        void log(string text, int level);

        /// <summary>
        /// Called by the generation process to add an exception to the log.
        /// </summary>
        /// <param name="text">Exception to log</param>
        void log(Exception exception);

    }
}
