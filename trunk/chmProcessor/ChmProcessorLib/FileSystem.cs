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
using System.IO;

namespace ChmProcessorLib
{
    class FileSystem
    {
        // Thanks to Richard Lopes. http://www.codeproject.com/cs/files/copydirectoriesrecursive.asp
        public static void CopyDirectory(string Src,string Dst){
            String[] Files;

            if(Dst[Dst.Length-1]!=Path.DirectorySeparatorChar) 
                Dst+=Path.DirectorySeparatorChar;
            if(!Directory.Exists(Dst)) Directory.CreateDirectory(Dst);
            Files=Directory.GetFileSystemEntries(Src);
            foreach(string Element in Files){
                // Sub directories
                if(Directory.Exists(Element))
                    CopyDirectory(Element, Dst + Path.GetFileName(Element));
                // Files in directory
                else 
                    File.Copy(Element,Dst+Path.GetFileName(Element),true);
                
            }

        }
    }
}
