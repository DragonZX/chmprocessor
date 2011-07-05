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
using System.Globalization;

namespace ChmProcessorLib
{
    /// <summary>
    /// Description of an encoding available on the system.
    /// Useful to use it on a combo box.
    /// </summary>
    public class EncodingItem
    {
        /// <summary>
        /// The encoding described.
        /// </summary>
        public EncodingInfo EncodingInfo;

        /// <summary>
        /// Description of the encoding.
        /// </summary>
        /// <returns></returns>
        override public string ToString()
        {
            return EncodingInfo.DisplayName + " ( name \"" + EncodingInfo.Name + "\" / codepage " + EncodingInfo.CodePage + " )";
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="encodingInfo">Encoding to describe.</param>
        public EncodingItem(EncodingInfo encodingInfo)
        {
            this.EncodingInfo = encodingInfo;
        }

        /// <summary>
        /// Checks if an enconding can be used with CHM project files
        /// </summary>
        /// <param name="encoding">Encoding to check</param>
        /// <returns>True if the encoding can be used to encode CHM files</returns>
        static public bool IsEncodingValidForChm(EncodingInfo encoding)
        {
            if (encoding.Name.ToLower().StartsWith("utf"))
                // Discard unicode
                return false;

            Encoding enc = Encoding.GetEncoding(encoding.CodePage);
            //if (!e.Name.ToLower().StartsWith("windows-"))
            //    return false;

            return true;
        }

        /// <summary>
        /// Return a list of available encodings valid for CHM characters encoding.
        /// </summary>
        static public EncodingsList AvailableEncodingsForChm
        {
            get
            {
                EncodingsList encodings = new EncodingsList();
                foreach (EncodingInfo e in Encoding.GetEncodings())
                {
                    // TODO: What encodings will work with CHM compiler??? Unicode for sure not.
                    if( IsEncodingValidForChm(e) )
                        encodings.Add(new EncodingItem(e));
                }

                return encodings;
            }
        }

        /// <summary>
        /// Return the default encoding to use with CHM files.
        /// </summary>
        static public EncodingItem DefaultEncodingForChm
        {
            get
            {
                return AvailableEncodingsForChm.DefaultEncoding;
            }
        }
    }
}
