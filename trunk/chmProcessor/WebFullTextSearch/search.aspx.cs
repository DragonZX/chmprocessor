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
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using WebIndexLib;
using System.IO;
using System.Collections;

public partial class _Default : System.Web.UI.Page 
{
    protected const int NRESULTSBYPAGE = 10;

    protected string HRef(string q, int s)
    {
        //return "search.aspx?q=" + q + "&amp;s=" + s;
        return "search.aspx?q=" + q + "&s=" + s;
    }

    protected string Link( string q , int s , string text ) {
        return "<a href=\"search.aspx?q=" + q + "&amp;s=" + s + "\">" + text + "</a>";
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        DateTime start = DateTime.Now;

        txtResult.Text = "";
        string queryText = Request["q"];
        int startPage = 0;
        if( Request["s"] != null )
            try { startPage = Int32.Parse(Request["s"]); }
            catch { };

        if (queryText != null)
        {
            char[] tokens = { ' ' };
            string[] words = queryText.Split(tokens);

            string webDirectory = Path.GetDirectoryName(Request.PhysicalPath);
            string pathDB = webDirectory + Path.DirectorySeparatorChar + "fullsearchdb.db3";
            WebIndex idx = null;
            ArrayList results = null;
            SearchWords searchWords = null;
            try
            {
                idx = new WebIndex();
                idx.Connect(pathDB);
                //results = idx.Search(words);
                searchWords = idx.PrepareRealWords(words);
                if (searchWords == null)
                    // Some word not found:
                    results = new ArrayList(0);
                else
                    results = idx.Search(searchWords);
            }
            finally
            {
                if (idx != null)
                    idx.Disconnect();
            }

            //ArrayList normalizedWords = WebIndex.NormalizeWordsSet(words);

            string textFilesDir = webDirectory + Path.DirectorySeparatorChar + "textFiles";

            int startResult = startPage * NRESULTSBYPAGE;
            int lastResult = (startPage + 1) * NRESULTSBYPAGE;
            if (lastResult > results.Count)
                lastResult = results.Count;

            txtSearchText.Text = "";
            foreach (string word in words)
                txtSearchText.Text += word + " ";

            txtShowResults.Text = (startResult + 1) + " - " + lastResult;
            txtTotalResults.Text = results.Count.ToString();

            txtResult.Text += "<p>";
            for (int i = startResult; i < lastResult; i++)
            {
                Result result = (Result)results[i];
                txtResult.Text += result.GoogleTextFormat(textFilesDir, searchWords) + "\n";
            }
            txtResult.Text += "</p>";

            if (results.Count > NRESULTSBYPAGE)
            {

                int nPages = results.Count / NRESULTSBYPAGE;
                if ((results.Count % NRESULTSBYPAGE) > 0)
                    nPages++;

                if (startPage != 0)
                    lnkPrevious.NavigateUrl = HRef(queryText, startPage - 1);
                else
                    lnkPrevious.Visible = false;

                for (int i = 0; i < nPages; i++)
                {
                    if (startPage != i)
                    {
                        int number = i + 1;
                        txtResultLinks.Text += " " + Link(queryText, i, number.ToString());
                    }
                    else
                        txtResultLinks.Text += " <font color=\"red\">" + (i + 1) + "</font>";
                }
                if (startPage != (nPages - 1))
                    lnkNext.NavigateUrl = HRef(queryText, startPage + 1);
                else
                    lnkNext.Visible = false;
            }
            else
            {
                txtMoreResults.Visible = false;
                lnkPrevious.Visible = false;
                lnkNext.Visible = false;
            }

        }
        else
        {
            txtSearchText.Text = "";
            txtShowResults.Text = "0 - 0";
            txtTotalResults.Text = "0";
            txtMoreResults.Visible = false;
            lnkPrevious.Visible = false;
            lnkNext.Visible = false;
        }

        DateTime end = DateTime.Now;
        TimeSpan span = end.Subtract(start);
        double roundMs = Math.Round(span.TotalMilliseconds, 2);
        txtMiliseconds.Text = roundMs.ToString();
    }
}
