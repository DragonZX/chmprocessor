using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChmProcessorLib
{
    /// <summary>
    /// A list of encoding
    /// </summary>
    public class EncodingsList : List<EncodingItem>
    {

        /// <summary>
        /// Searches an encoding on the list by its codepage.
        /// </summary>
        /// <param name="codePage">Code page to search</param>
        /// <returns>The encoding with the code page. null if the code page was not found.</returns>
        public EncodingItem SearchByCodePage(int codePage)
        {
            foreach (EncodingItem e in this)
                if (e.EncodingInfo.CodePage == codePage)
                    return e;
            return null;
        }

        /// <summary>
        /// Returns the preferred encoding to use from the list.
        /// </summary>
        public EncodingItem DefaultEncoding
        {
            get {
                // Try with the default:
                EncodingItem e = SearchByCodePage(Encoding.Default.CodePage);
                if (e != null)
                    return e;

                // Try with UTF-8:
                e = SearchByCodePage(Encoding.UTF8.CodePage);
                if (e != null)
                    return e;

                // Try with 1250 (ANSI Central European; Central European (Windows)): My personal default.
                e = SearchByCodePage(1250);
                if (e != null)
                    return e;

                // Fuck. Get the first one:
                if (this.Count > 0)
                    return this[0];

                // Good bye:
                return null;
            }
        }
    }
}
