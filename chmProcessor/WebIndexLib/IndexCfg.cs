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
    /// Class to load and store the search index configuration.
    /// </summary>
    class IndexCfg
    {
        public string Language = "";

        public IndexCfg(string language)
        {
            Language = language;
        }

        static public IndexCfg Load(DbConnection con)
        {
            DbCommand cmd = con.CreateCommand();
            cmd.CommandText = "SELECT CfgLanguage FROM IndexCfg WHERE CfgCod = 0";
            cmd.Connection = con;

            object obj = cmd.ExecuteScalar();
            if (obj == null)
                return null;
            else
                return new IndexCfg((string)obj);
        }

        public void Insert(DbConnection con)
        {
            DbCommand cmd = con.CreateCommand();
            cmd.CommandText = "INSERT INTO IndexCfg ( CfgCod , CfgLanguage ) VALUES( 0 , ? )";
            cmd.Connection = con;

            DbParameter parm = cmd.CreateParameter();
            parm.DbType = System.Data.DbType.String;
            parm.Value = Language;
            cmd.Parameters.Add(parm);

            if (cmd.ExecuteNonQuery() != 1)
                throw new Exception("Error inserting configuration");
        }

    }
}
