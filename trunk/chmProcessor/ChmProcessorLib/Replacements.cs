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
using System.Collections;
using System.IO;

namespace ChmProcessorLib
{
    /// <summary>
    /// Used to make text replacements in files.
    /// </summary>
    class Replacements
    {

        /// <summary>
        /// A simple text replacement
        /// </summary>
        private class ReplacementPair
        {
            public string ValueToReplace;
            public string NewValue;

            public ReplacementPair(string valueToReplace, string newValue)
            {
                ValueToReplace = valueToReplace;
                NewValue = newValue;
            }
        }

        /// <summary>
        /// List of replacements to do. 
        /// <see cref="ReplacementPair"/>
        /// </summary>
        private ArrayList ReplacementsList = new ArrayList();

        /// <summary>
        /// Constructor. Creates replacementes from two arrays. Each string into valuesToReplace is
        /// replaced with the string at the same index into replacedValues.
        /// </summary>
        /// <param name="valuesToReplace">Array with values to replace into the text</param>
        /// <param name="replacedValues">Array with replaced values.</param>
        public Replacements(string[] valuesToReplace, string[] replacedValues)
        {
            for (int i = 0; i < valuesToReplace.Length; i++)
                ReplacementsList.Add(new ReplacementPair(valuesToReplace[i], replacedValues[i]));
        }

        /// <summary>
        /// Loads a set of replacements from a file. The file must be a text file with a text to replace
        /// and the replaced text on each line. Example:
        /// textotreplace1
        /// replacedtext1
        /// textotreplace2
        /// replacedtext2
        /// ...
        /// </summary>
        /// <param name="file">path to file where load the replacements</param>
        public void AddReplacementsFromFile(string file)
        {
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(file);
                while (!reader.EndOfStream)
                {
                    string valueToReplace = reader.ReadLine();
                    if ( !valueToReplace.Trim().Equals("") && !reader.EndOfStream)
                    {
                        string replacedValue = reader.ReadLine();
                        ReplacementsList.Add(new ReplacementPair("%" + valueToReplace + "%", replacedValue));
                    }
                }
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        public void CopyReplaced(string srcPath, string dstPath)
        {
            StreamReader reader = new StreamReader(srcPath);
            string text = reader.ReadToEnd();
            reader.Close();
            foreach (ReplacementPair pair in ReplacementsList)
                text = text.Replace(pair.ValueToReplace, pair.NewValue);
            StreamWriter writer = new StreamWriter(dstPath);
            writer.WriteLine(text);
            writer.Close();
        }

        public void CopyDirectoryReplaced( string srcDirectoryPath , string dstDirectoryPath , string[] extensions , bool runTidy , DocumentProcessor.UserInterface ui ) 
        {
            string[] files = Directory.GetFiles(srcDirectoryPath);
            foreach (string file in files)
            {
                string extension = Path.GetExtension(file.ToLower());
                bool goodExtension = false;
                if (extensions == null)
                    goodExtension = true;
                else
                {
                    foreach (string ext in extensions)
                        if (ext.ToLower().Equals(extension))
                        {
                            goodExtension = true;
                            break;
                        }
                }
                string dstPath = dstDirectoryPath + Path.DirectorySeparatorChar + Path.GetFileName(file);
                if (goodExtension)
                {
                    CopyReplaced(file, dstPath);
                    if (runTidy)
                        // Clean html over the destination file:
                        new TidyParser(ui).Parse(dstPath);
                }
                else
                    File.Copy(file, dstPath);
            }

            // Copy subdirectories too.
            string[] subdirectories = Directory.GetDirectories(srcDirectoryPath);
            foreach (string subdir in subdirectories)
            {
                DirectoryInfo info = new DirectoryInfo(subdir);
                string newSubdir = dstDirectoryPath + Path.DirectorySeparatorChar + info.Name;
                Directory.CreateDirectory(newSubdir);
                CopyDirectoryReplaced(subdir, newSubdir, extensions, runTidy, ui);
            }
        }
    }
}
