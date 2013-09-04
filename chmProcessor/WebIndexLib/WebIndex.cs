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
using System.Windows.Forms;
using System.IO;
using HtmlAgilityPack;
using System.Collections;
using System.Web;

namespace WebIndexLib
{

    /// <summary>
    /// TODO: Try to replace all this stuff with Lucene or other external tool
    /// </summary>
    public class WebIndex
    {

        private string textFilesDirectory;

        DbProviderFactory fact;
        DbConnection cnn;

        public WebIndex() {
        }

        public void Connect(string dataBaseFile)
        {
            Disconnect();
            fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            cnn = fact.CreateConnection();
            cnn.ConnectionString = "Data Source=" + dataBaseFile;
            cnn.Open();
        }

        public void Disconnect()
        {
            if (cnn != null)
            {
                cnn.Close();
                cnn = null;
                fact = null;
            }
        }

        public void StoreConfiguration(string language)
        {
            IndexCfg cfg = new IndexCfg(language);
            cfg.Insert(cnn);
        }

        public void CreateDatabase(string sqlCreationPath , string textFilesDirectory )
        {
            //string scriptPath = Application.StartupPath + Path.DirectorySeparatorChar + "searchdb.sql";
            StreamReader reader = null;
            try
            {
                if (!Directory.Exists(textFilesDirectory))
                    Directory.CreateDirectory(textFilesDirectory);
                this.textFilesDirectory = textFilesDirectory;

                reader = new StreamReader(sqlCreationPath);
                char[] splitters = { ';' };

                string[] commands = reader.ReadToEnd().Split( splitters );
                foreach (string command in commands)
                {
                    DbCommand cmd = cnn.CreateCommand();
                    cmd.CommandText = command;
                    cmd.Connection = cnn;
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        static public bool GoodChar(char c)
        {
            //return (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9');
            return char.IsLetterOrDigit(c);
        }

        /// <summary>
        /// Add a text of a web page to the index
        /// </summary>
        /// <param name="text">Text of the web page to add.</param>
        /// <param name="count">Number of characters of the text.</param>
        /// <param name="counter">Counter used to indexate the web page.</param>
        /// <param name="titleText">True if the text is the title section of the web page.
        /// False if it's the web page body.</param>
        private void AddText( string text, int count, WordCounter counter , bool titleText )
        {

            // Get a list of words:
            bool atWord = false;
            string currentWord = "";
            int wordStart = 0;

            for (int i = 0; i < count; i++)
            {
                char c = Normalize(text[i]);
                if (GoodChar(c))
                {
                    if (!atWord)
                    {
                        currentWord = "";
                        atWord = true;
                        wordStart = i;
                    }
                    currentWord += c;
                }
                else
                {
                    if (atWord)
                    {
                        atWord = false;
                        counter.Add(currentWord, wordStart, titleText );
                    }
                }
            }
            if (atWord)
                counter.Add(currentWord, wordStart, titleText );
        }

        public void AddPage(string url , string title , HtmlNode body)
        {
            DbTransaction trn = null;

            try
            {
                trn = cnn.BeginTransaction();

                string originalPageText = HttpUtility.HtmlDecode(body.InnerText);
                string lowerPageText = originalPageText.ToLower();
                int count = lowerPageText.Length;

                Document doc = new Document(url, title, count);
                doc.Insert(cnn);

                // Save a text copy of the HTML file.
                string textFileName = Path.GetFileNameWithoutExtension( url ) + ".txt";
                StreamWriter writer = new StreamWriter(textFilesDirectory + Path.DirectorySeparatorChar + textFileName);
                writer.WriteLine(originalPageText);
                writer.Close();

                WordCounter counter = new WordCounter(count);
                // Get the text of the main body:
                AddText(lowerPageText, count, counter, false);
                // Add the text of the title:
                AddText(title.ToLower(), title.Length, counter , true);

                counter.DumpToDatabase(cnn, doc);

                trn.Commit();
            }
            catch( Exception ex ) 
            {
                if (trn != null)
                    trn.Rollback();
                throw ex;
            }
        }

        static public char Normalize(char c)
        {
            switch (c)
            {
                case 'á':
                case 'à':
                case 'â':
                case 'ä':
                    return 'a';

                case 'é':
                case 'è':
                case 'ê':
                case 'ë':
                    return 'e';

                case 'í':
                case 'ì':
                case 'ï':
                case 'î':
                    return 'i';

                case 'ó':
                case 'ò':
                case 'ô':
                case 'ö':
                    return 'o';

                case 'ú':
                case 'ù':
                case 'û':
                case 'ü':
                    return 'u';

                default:
                    return c;
            }
        }

        static private string Normalize(string word)
        {
            string aux = word.Trim().ToLower();
            string normalizedWord = "";
            for (int i = 0; i < aux.Length; i++)
            {
                char c = Normalize(aux[i]);
                if( GoodChar(c) )
                    normalizedWord += c;
            }

            return normalizedWord;
        }

        private static int getNumberOneBits(long bits, int nLowBits)
        {
            int count = 0;
            for (int i = 0; i < nLowBits; i++)
            {
                if ((bits & 1) != 0)
                    count++;
                bits = bits >> 1;
            }
            return count;
        }

        static public ArrayList NormalizeWordsSet(string[] words )
        {
            ArrayList normalizedWord = new ArrayList();

            foreach (string word in words)
            {
                string n = WebIndex.Normalize(word);
                if (!n.Equals(""))
                    normalizedWord.Add(n);
            }
            return normalizedWord;
        }

        public ArrayList SearchSynonyms(string word, string language )
        {
            ArrayList synonyms = new ArrayList();

            if (language.Equals("spanish"))
            {
                if (word.EndsWith("es"))
                {
                    // plural?
                    string maybeSynonymous = word.Substring(0, word.Length - 2);
                    Word wrd = Word.Load(cnn, maybeSynonymous);
                    if (wrd != null)
                        synonyms.Add(wrd);
                }
                if (word.EndsWith("s"))
                {
                    // plural?
                    string maybeSynonymous = word.Substring(0, word.Length - 1);
                    Word wrd = Word.Load(cnn, maybeSynonymous);
                    if (wrd != null)
                        synonyms.Add(wrd);
                }
                else
                {
                    // not plural
                    string maybeSynonymous = word + "es";
                    Word wrd = Word.Load(cnn, maybeSynonymous);
                    if (wrd != null)
                        synonyms.Add(wrd);
                    else
                    {
                        maybeSynonymous = word + "s";
                        wrd = Word.Load(cnn, maybeSynonymous);
                        if (wrd != null)
                            synonyms.Add(wrd);
                    }
                }
            }
            if (language.Equals("english"))
            {
                if (word.EndsWith("s"))
                {
                    // plural?
                    string maybeSynonymous = word.Substring(0, word.Length - 1);
                    Word wrd = Word.Load(cnn, maybeSynonymous);
                    if (wrd != null)
                        synonyms.Add(wrd);
                }
                else
                {
                    // not plural
                    string maybeSynonymous = word + "s";
                    Word wrd = Word.Load(cnn, maybeSynonymous);
                    if (wrd != null)
                        synonyms.Add(wrd);
                }
            }
            return synonyms;
        }

        private DbCommand CreateCommandSinonymous(int numberOfWords, ArrayList wordSets )
        {
            DbCommand cmd = cnn.CreateCommand();
            cmd.Connection = cnn;

            string cmdText = "SELECT d.DocPat , d.DocDes , d.DocLen , ins0.InsPos ";
            // Get the positions of each word:
            for (int i = 1; i < numberOfWords; i++)
                cmdText += ", ins" + i + ".InsPos";

            // Get the count of instances:
            string sumatory = "ins0.InsCount";
            for (int i = 1; i < numberOfWords; i++)
                sumatory += "+ ins" + i + ".InsCount";
            cmdText += ", " + sumatory;

            // Retrieve the document code too:
            cmdText += ",ins0.InsDocCod";
            
            cmdText += " FROM WordInstance ins0 LEFT JOIN Document d ON d.DocCod = ins0.InsDocCod ";
            for (int i = 1; i < numberOfWords; i++)
                cmdText += ", WordInstance ins" + i;

            cmdText += " WHERE ";
            
            // Add words search:
            int parametersCount = 0;
            for( int i=0; i< wordSets.Count; i++ )
            {
                ArrayList set = (ArrayList)wordSets[i];

                if (i > 0)
                    cmdText += " AND ";

                cmdText += " ( ";
                for (int j = 0; j < set.Count; j++)
                {
                    if (j > 0)
                        cmdText += " OR ";
                    cmdText += " ins" + i + ".InsWrdCod = ? ";

                    DbParameter parm = cmd.CreateParameter();
                    parm.DbType = System.Data.DbType.Int64;
                    cmd.Parameters.Add(parm);

                    cmd.Parameters[parametersCount++].Value = ((Word)set[j]).Code;
                }
                cmdText += " ) ";
            }

            // Add all-words-in-same-document condition:
            for (int i = 1; i < numberOfWords; i++)
                cmdText += " AND ins" + i + ".InsDocCod = ins0.InsDocCod";

            cmdText += " ORDER BY ins0.InsDocCod, (" + sumatory + ") DESC";

            cmd.CommandText = cmdText;
            return cmd;
        }

        private DbCommand CreateCommand(int numberOfWords)
        {
            string cmdText = "SELECT d.DocPat , d.DocDes , d.DocLen , ins0.InsPos ";
            // Get the positions of each word:
            for (int i = 1; i < numberOfWords; i++)
                cmdText += ", ins" + i + ".InsPos";
            // Get the count of instances:
            cmdText += ", ins0.InsCount";
            for (int i = 1; i < numberOfWords; i++)
                cmdText += "+ ins" + i + ".InsCount";

            cmdText += " FROM WordInstance ins0 LEFT JOIN Document d ON d.DocCod = ins0.InsDocCod ";
            for (int i = 1; i < numberOfWords; i++)
                cmdText += ", WordInstance ins" + i;

            cmdText += " WHERE ins0.InsWrdCod = ? ";

            for (int i = 1; i < numberOfWords; i++)
                cmdText += " AND ins" + i + ".InsWrdCod = ? AND ins" + i + ".InsDocCod = ins0.InsDocCod";

            DbCommand cmd = cnn.CreateCommand();
            cmd.CommandText = cmdText;
            cmd.Connection = cnn;
            for (int i = 0; i < numberOfWords; i++)
            {
                DbParameter parm = cmd.CreateParameter();
                parm.DbType = System.Data.DbType.Int64;
                cmd.Parameters.Add(parm);
            }

            return cmd;
        }

        private ArrayList SearchWords(ArrayList normalized, DbCommand cmd )
        {
            ArrayList results = new ArrayList();

            string lastDocId = "";

            DbDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string path = reader.GetString(0);
                string description = reader.GetString(1);
                long docLenght = reader.GetInt64(2);

                // Get positions of words:
                long[] positions = new long[normalized.Count];
                for (int i = 3; i < (3 + normalized.Count); i++)
                    positions[i - 3] = reader.GetInt64(i);

                long numberOfInstances = reader.GetInt64(3 + normalized.Count);
                string docId = reader.GetString(4 + normalized.Count);

                // Do not reapeat documents. We will get the first, wich contains the biggest number of instances.
                if (docId != lastDocId)
                {
                    int startPosition = 0;
                    int lenghtTexToToShow;
                    if (docLenght > Result.LENGHTDISPLAYTEXT)
                    {

                        // Calculate the lenght in bits of the text window:
                        double charsPerBit = ((double)docLenght) / 63.0;
                        if (charsPerBit <= 0.0)
                            charsPerBit = 1.0;
                        int nBits = (int)(((double)(Result.LENGHTDISPLAYTEXT)) / charsPerBit);

                        if (nBits <= 0)
                            nBits = 1;
                        else if (nBits > 63)
                            nBits = 63;

                        // Get the bits window with more words inside:
                        int runLenght = 64 - nBits;
                        int numWindowBitsCurrent, maxNumBits = 0, indexMaxNumBits = -1;
                        for (int i = 0; i < runLenght; i++)
                        {
                            numWindowBitsCurrent = 0;
                            for (int j = 0; j < normalized.Count; j++)
                            {
                                numWindowBitsCurrent += getNumberOneBits(positions[j], nBits);
                                positions[j] = positions[j] >> 1;
                            }
                            if (numWindowBitsCurrent > maxNumBits)
                            {
                                maxNumBits = numWindowBitsCurrent;
                                indexMaxNumBits = i;
                            }
                        }

                        startPosition = (int)(((double)indexMaxNumBits) * charsPerBit);
                        lenghtTexToToShow = (int)Math.Ceiling(((double)nBits) * charsPerBit);
                        if (lenghtTexToToShow < Result.LENGHTDISPLAYTEXT)
                            lenghtTexToToShow = Result.LENGHTDISPLAYTEXT;
                    }
                    else
                        lenghtTexToToShow = (int)docLenght;

                    Result result = new Result(path, description, startPosition, docLenght, lenghtTexToToShow, numberOfInstances);
                    results.Add(result);
                }

                lastDocId = docId;
            }
            reader.Close();
            return results;
        }

        public SearchWords PrepareRealWords(string[] words)
        {
            // Load index language:
            string language = "";
            IndexCfg cfg = IndexCfg.Load(cnn);
            if (cfg != null)
                language = cfg.Language.ToLower();

            // Normalize words, see if they are at the index and search synonymous:
            SearchWords synonimous = new SearchWords();
            foreach (string word in words)
            {
                string n = Normalize(word);
                if (!n.Equals(""))
                {
                    ArrayList currentSet = new ArrayList(3);
                    Word wrd = Word.Load(cnn, n);
                    if (wrd == null)
                    {
                        currentSet = SearchSynonyms(n, language);
                        if (currentSet.Count == 0)
                            // Word not found: Exit without results.
                            return null;
                    }
                    else
                    {
                        currentSet.Add(wrd);
                        currentSet.AddRange(SearchSynonyms(n, language));
                    }
                    synonimous.WordSets.Add(currentSet);
                }
            }

            if (synonimous.WordSets.Count == 0)
                // Any good search word: Exit without results.
                return null;

            return synonimous;
        }

        public ArrayList Search( SearchWords searchWords )
        {
            // Search documents contaning all words:
            DbCommand cmd = CreateCommandSinonymous(searchWords.WordSets.Count, searchWords.WordSets );
            ArrayList results = SearchWords( searchWords.WordSets, cmd);
            results.Sort();

            return results;

        }
    }
}
