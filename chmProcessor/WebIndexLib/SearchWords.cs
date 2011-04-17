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

namespace WebIndexLib
{
    /// <summary>
    /// Class that stores the normalized search words and their synonymous:
    /// </summary>
    public class SearchWords
    {
        public ArrayList WordSets = new ArrayList();

        public bool Contains(string word)
        {
            foreach (ArrayList set in WordSets)
            {
                foreach (Word wrd in set)
                {
                    if (wrd.Text == word)
                        return true;
                }
            }
            return false;
        }
    }
}
