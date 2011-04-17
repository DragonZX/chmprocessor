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
using System.IO.Compression;

namespace ChmProcessorLib
{
    /// <summary>
    /// Tools to zip a file.
    /// </summary>
    public class Zip
    {
        static public void CompressFile(string sourceFile, string destinationFile)
        {
            int checkCounter;
            if (System.IO.File.Exists(sourceFile) == false)
            {
                return;
            }

            byte[] buffer;
            System.IO.FileStream sourceStream = null;
            System.IO.FileStream destinationStream = null;
            //System.IO.Compression.DeflateStream compressedStream = null;
            System.IO.Compression.GZipStream compressedStream = null;

            try
            {
                sourceStream = new System.IO.FileStream(sourceFile, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, System.IO.FileShare.Read);
                buffer = new byte[Convert.ToInt64(sourceStream.Length)];
                checkCounter = sourceStream.Read(buffer, 0, buffer.Length);

                //output (ZIP) file name
                destinationStream = new System.IO.FileStream(destinationFile, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);

                // Create a compression stream pointing to the destiantion stream
                compressedStream = new System.IO.Compression.GZipStream(destinationStream, System.IO.Compression.CompressionMode.Compress, true);
                compressedStream.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                // Make sure we allways close all streams
                if (sourceStream != null)
                {
                    sourceStream.Close();
                }
                if (compressedStream != null)
                {
                    compressedStream.Close();
                }

                if (destinationStream != null)
                {
                    destinationStream.Close();
                }
            }

        }
    }
}
