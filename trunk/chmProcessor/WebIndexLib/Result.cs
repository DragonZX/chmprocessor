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
using System.Web;
using System.Collections;

namespace WebIndexLib
{
    public class Result : IComparable
    {
        public const int LENGHTDISPLAYTEXT = 100;

        public string UrlPath;
        public string Description;
        public int TextStartPosition;
        public long DocLenght;
        public int LenghTextToShow;
        public long NumberOfInstances;

        public Result(string path, string description, int textStartPosition, long docLenght, int lenghTextToShow, long numberOfInstances)
        {
            UrlPath = path;
            Description = description;
            TextStartPosition = textStartPosition;
            DocLenght = docLenght;
            LenghTextToShow = lenghTextToShow;
            NumberOfInstances = numberOfInstances;
        }

        public string DisplayText( string textFilesDirectory ) {
            FileStream s = null;
            TextReader reader = null;
            try
            {
                string fileName = textFilesDirectory + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(UrlPath) + ".txt";
                s = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                s.Seek(TextStartPosition, SeekOrigin.Begin);
                reader = new StreamReader(s);
                char[] characters = new char[LenghTextToShow * 2];
                int count = reader.Read(characters, 0, LenghTextToShow * 2);
                string showText = new string(characters, 0, count);
                if (TextStartPosition != 0)
                    showText = "..." + showText;
                if (count == (LenghTextToShow * 2))
                    showText += "...";
                return showText;
            }
            catch /*(Exception ex)*/
            {
                return "";
            }
            finally
            {
                if (s != null)
                    s.Close();
                if (reader != null)
                    reader.Close();
            }
        }

        static private string FormatSearchWord(string currentWord, SearchWords searchWords)
        {
            string currentWordLower = currentWord.ToLower();

            // Check if the word is a search word:
            /*bool searchWord = false;
            foreach (string word in normalizedWords)
            {
                if (word.Equals(currentWordLower))
                {
                    searchWord = true;
                    break;
                }
            }*/
            if ( !searchWords.Contains(currentWordLower) )
                return HttpUtility.HtmlEncode(currentWord);
            else
                return "<b><u>" + HttpUtility.HtmlEncode(currentWord) + "</u></b>";

        }

        static private string BoldSearchedWords(string showText, SearchWords searchWords)
        {
            string result = "";

            bool atWord = false;
            string currentWord = "";
            string currentOutOfWord = "";
            int count = showText.Length;

            for (int i = 0; i < count; i++)
            {
                char c = WebIndex.Normalize( showText[i] );
                if (WebIndex.GoodChar(c))
                {
                    if (!atWord)
                    {
                        currentWord = "";
                        atWord = true;
                        result += HttpUtility.HtmlEncode(currentOutOfWord);
                    }
                    currentWord += c;
                }
                else
                {
                    if (atWord)
                    {
                        currentOutOfWord = "";
                        atWord = false;
                        result += FormatSearchWord(currentWord, searchWords);
                    }
                    currentOutOfWord += c;
                }
            }
            if (atWord)
                result += FormatSearchWord(currentWord, searchWords);
            else
                result += HttpUtility.HtmlEncode(currentOutOfWord);

            return result;
        }

        public string GoogleTextFormat(string textFilesDirectory, SearchWords searchWords)
        {
            string showText = DisplayText(textFilesDirectory);
            showText = BoldSearchedWords(showText, searchWords);

            long kb = (long)(Math.Ceiling(((double)DocLenght) / 1024.0)) + 1;
            string text = "<a href=\"" + UrlPath + "\">" + BoldSearchedWords(Description, searchWords) + 
                "</a><br/>" +
                showText + "<br/><font color=\"green\">" + UrlPath + "</font>";
/*#if DEBUG
            text += " - " + NumberOfInstances + " instances";
#endif*/
            text += "<br/><br/>";
            return text;
        }

        public int CompareTo(Object obj)
        {
            if (obj is Result)
            {
                int cmp = this.NumberOfInstances.CompareTo( ((Result)obj).NumberOfInstances );
                if (cmp < 0)
                    return 1;
                else if (cmp > 0)
                    return -1;
                else
                    return 0;
            }
            else
                return -1;
        }


    }
}
