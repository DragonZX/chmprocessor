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
    class WordInstance
    {
        public long WordCode;
        public long DocumentCode;
        public long Count;
        public long Positions;

        public WordInstance(long wordCode, long documentCode, long count, long positions)
        {
            WordCode = wordCode;
            DocumentCode = documentCode;
            Count = count;
            Positions = positions;
        }

        public void Insert(DbConnection con)
        {
            DbCommand cmd = con.CreateCommand();
            cmd.CommandText = "INSERT INTO WordInstance ( InsWrdCod , InsDocCod , InsCount , InsPos ) VALUES( ? , ? , ? , ? )";
            cmd.Connection = con;

            DbParameter parm = cmd.CreateParameter();
            parm.DbType = System.Data.DbType.Int64;
            parm.Value = WordCode;
            cmd.Parameters.Add(parm);

            parm = cmd.CreateParameter();
            parm.DbType = System.Data.DbType.Int64;
            parm.Value = DocumentCode;
            cmd.Parameters.Add(parm);

            parm = cmd.CreateParameter();
            parm.DbType = System.Data.DbType.Int64;
            parm.Value = Count;
            cmd.Parameters.Add(parm);

            parm = cmd.CreateParameter();
            parm.DbType = System.Data.DbType.Int64;
            parm.Value = Positions;
            cmd.Parameters.Add(parm);

            if (cmd.ExecuteNonQuery() != 1)
                throw new Exception("Error inserting WordInstance");
        }

    }
}
