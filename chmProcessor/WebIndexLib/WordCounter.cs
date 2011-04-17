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
using System.Data.Common;

namespace WebIndexLib
{
    public class WordCounter
    {
        private Hashtable table = new Hashtable();
        private long LenghtOfDocument;

        private class Counter
        {
            public int Count;
            public long Positions;
        }

        public WordCounter(long lenghtOfDocument)
        {
            LenghtOfDocument = lenghtOfDocument;
        }

        /// <summary>
        /// Add a word to the index of the page.
        /// </summary>
        /// <param name="word">Word to add to the counter.</param>
        /// <param name="position">Index of the first character of the word at the page text.</param>
        /// <param name="titleText">True if the word is at the title section of the web page.
        /// False if it's at the web page body.</param>
        public void Add(string word, long position, bool titleText)
        {
            long bit;
            int countIncrement;

            if (titleText)
            {
                bit = 1;
                countIncrement = 15;
            }
            else
            {
                // Calculate the position of the word:
                double factorPosition = ((double)position) / ((double)LenghtOfDocument);
                int bitPosition = (int)Math.Floor(factorPosition * 63.0);
                long bitOne = 1;
                bit = bitOne << bitPosition;
                countIncrement = 1;
            }

            Counter counter = (Counter)table[word];
            if (counter == null)
            {
                counter = new Counter();
                counter.Count = countIncrement;
                counter.Positions = bit;
                table[word] = counter;
            }
            else
            {
                counter.Count += countIncrement;
                counter.Positions |= bit;
            }
        }

        public void DumpToDatabase(DbConnection connection , Document doc)
        {
            IDictionaryEnumerator e = table.GetEnumerator();
            while (e.MoveNext())
            {
                // Get the DB word:
                string wordText = (string)e.Key;
                Word word = Word.Load(connection, wordText);
                if (word == null)
                {
                    word = new Word(wordText);
                    word.Insert(connection);
                }

                // Store the counter.
                Counter cnt = (Counter)e.Value;
                WordInstance inst = new WordInstance(word.Code, doc.Code, cnt.Count, cnt.Positions);
                inst.Insert(connection);
            }
        }
    }
}
