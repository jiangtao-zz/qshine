using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace qshine.Utility
{
    /// <summary>
    /// String extension
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Check source text match to given pattern using regular expression
        /// </summary>
        /// <param name="source">Text to be matching</param>
        /// <param name="pattern">Regular expression</param>
        /// <returns></returns>
        public static bool Match(this string source, string pattern)
        {
            if (string.IsNullOrEmpty(source))
            {
                if (string.IsNullOrEmpty(pattern))
                {
                    //both empty
                    return true;
                }
                else
                {
                    //source empty
                    return false;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(pattern))
                {
                    //pattern empty
                    return false;
                }
            }
            if (pattern == "*") return true;
            var match = Regex.Match(source,pattern, RegexOptions.IgnoreCase);
            return match.Success;
        }

    }
}
