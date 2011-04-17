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
    public class Document
    {
        static public int LastCode;

        public long Code;
        public string Path;
        public string Description;
        public long NumberOfCharacters;

        public Document(string path, string description, long lenght)
        {
            Path = path;
            Description = description;
            NumberOfCharacters = lenght;
        }

        public void Insert(DbConnection con)
        {
            DbCommand cmd = con.CreateCommand();
            cmd.CommandText = "INSERT INTO Document ( DocCod , DocPat , DocDes , DocLen ) VALUES( ? , ? , ? , ? )";
            cmd.Connection = con;

            Code = ++LastCode;
            DbParameter parm = cmd.CreateParameter();
            parm.DbType = System.Data.DbType.Int64;
            parm.Value = Code;
            cmd.Parameters.Add(parm);

            parm = cmd.CreateParameter();
            parm.DbType = System.Data.DbType.String;
            parm.Value = Path;
            cmd.Parameters.Add(parm);

            parm = cmd.CreateParameter();
            parm.DbType = System.Data.DbType.String;
            parm.Value = Description;
            cmd.Parameters.Add(parm);

            parm = cmd.CreateParameter();
            parm.DbType = System.Data.DbType.Int64;
            parm.Value = NumberOfCharacters;
            cmd.Parameters.Add(parm);

            if (cmd.ExecuteNonQuery() != 1)
                throw new Exception("Error inserting document");
        }

    }
}
