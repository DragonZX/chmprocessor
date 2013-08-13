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
using System.Data.Common;

namespace WebIndexLib
{

    /// <summary>
    /// TODO: Try to replace all this stuff with Lucene or other external tool
    /// </summary>
    class Word
    {
        static int LastCode;

        public long Code;
        public string Text;

        public Word(string text)
        {
            Text = text;
        }

        public Word(long code, string text)
        {
            Code = code;
            Text = text;
        }

        static public Word Load(DbConnection con, string word)
        {
            DbCommand cmd = con.CreateCommand();
            cmd.CommandText = "SELECT WrdCod FROM Word WHERE WrdTxt = ?";
            cmd.Connection = con;

            DbParameter parm = cmd.CreateParameter();
            parm.DbType = System.Data.DbType.String;
            parm.Value = word;
            cmd.Parameters.Add(parm);

            object obj = cmd.ExecuteScalar();
            if (obj == null)
                return null;
            else
                return new Word((long)obj, word);
        }

        public void Insert(DbConnection con)
        {
            DbCommand cmd = con.CreateCommand();
            cmd.CommandText = "INSERT INTO Word ( WrdCod , WrdTxt ) VALUES( ? , ? )";
            cmd.Connection = con;

            Code = ++LastCode;
            DbParameter parm = cmd.CreateParameter();
            parm.DbType = System.Data.DbType.Int64;
            parm.Value = Code;
            cmd.Parameters.Add(parm);

            parm = cmd.CreateParameter();
            parm.DbType = System.Data.DbType.String;
            parm.Value = Text;
            cmd.Parameters.Add(parm);

            if( cmd.ExecuteNonQuery() != 1 )
                throw new Exception("Error inserting word");
        }

    }
}
