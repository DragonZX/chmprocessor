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
using System.Runtime.InteropServices;
using System.Text;

namespace ChmProcessorLib
{
    /// <summary>
    /// Utilities to work with files and file paths.
    /// The author of functions to make relative paths is Jozsef Bekes.
    /// </summary>
    class FileSystem
    {

        /// <summary>
        /// Constant for directories.
        /// </summary>
        private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;

        /// <summary>
        /// Constant for files.
        /// </summary>
        private const int FILE_ATTRIBUTE_NORMAL = 0x80;

        /// <summary>
        /// Win32 function to get a path relative to other
        /// </summary>
        /// <returns></returns>
        [DllImport("shlwapi.dll", SetLastError = true)]
        private static extern int PathRelativePathToW(StringBuilder pszPath,
            string pszFrom, int dwAttrFrom, string pszTo, int dwAttrTo);

        /// <summary>
        /// Kind of file system objects
        /// </summary>
        public enum FSObjType
        {
            /// <summary>
            /// Autodetect if the object is a directory or a file.
            /// </summary>
            eAuto,

            /// <summary>
            /// Object is a directory
            /// </summary>
            eDir,

            /// <summary>
            /// Object is a file.
            /// </summary>
            eFile
        }


        /// <summary>
        /// Get the relative path from one file/directory to other.
        /// Thanks to Jozsef Bekes for the patch.
        /// </summary>
        /// <param name="fromType">Kind of file system "from" object (directory or file?)</param>
        /// <param name="fromPath">Absolute path of the file system "from" object</param>
        /// <param name="toType">Kind of file system "to" object (directory or file?)</param>
        /// <param name="toPath">Absolute path of the file system "to" object</param>
        /// <returns>The path of the "to" object relative to the "from" object. 
        /// As example if, "from" is c:\foo\file.txt and "to" is c:\foo\bar\otherfile.txt, it will
        /// return ".\bar\otherfile.txt"</returns>
        /// <exception cref="ArgumentException">If "from" and "to" dont have a common root. 
        /// As example "c:\foo.txt" and "d:\bar.txt" dont have it.</exception>
        public static string GetRelativePath(FSObjType fromType, string fromPath, FSObjType toType, string toPath)
        {

            int fromAttr = GetPathAttribute(fromPath, fromType);
            int toAttr = GetPathAttribute(toPath, toType);

            StringBuilder path = new StringBuilder(260); // MAX_PATH
            if (PathRelativePathToW(
                path,
                fromPath,
                fromAttr,
                toPath,
                toAttr) == 0)
            {
                throw new ArgumentException("Paths must have a common prefix");
            }
            return path.ToString();
        }

        /// <summary>
        /// Get win32 attribute value for file system object.
        /// Thanks to Jozsef Bekes  for the patch.
        /// </summary>
        /// <param name="path">Path to the file system object</param>
        /// <param name="fsObjType">Kind of the file system object</param>
        /// <returns>FILE_ATTRIBUTE_DIRECTORY if the object is a directory. FILE_ATTRIBUTE_NORMAL
        /// if its a file</returns>
        /// <exception cref="FileNotFoundException">If fsObjType is eAuto and the object path does 
        /// not exist.</exception>
        private static int GetPathAttribute(string path, FSObjType fsObjType)
        {
            switch (fsObjType)
            {
                case FSObjType.eDir:
                    return FILE_ATTRIBUTE_DIRECTORY;

                case FSObjType.eFile:
                    return FILE_ATTRIBUTE_NORMAL;

                default:
                    DirectoryInfo di = new DirectoryInfo(path);
                    if (di.Exists)
                    {
                        return FILE_ATTRIBUTE_DIRECTORY;
                    }

                    FileInfo fi = new FileInfo(path);
                    if (fi.Exists)
                    {
                        return FILE_ATTRIBUTE_NORMAL;
                    }

                    throw new FileNotFoundException();
            }
        }

        /// <summary>
        /// Copy a directory recursivelly
        /// Author is Richard Lopes: http://www.codeproject.com/cs/files/copydirectoriesrecursive.asp
        /// </summary>
        /// <param name="Src">Path of directory to copy</param>
        /// <param name="Dst">Path of the destination copy</param>
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
