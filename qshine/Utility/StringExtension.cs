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

            if(!pattern.StartsWith("^"))
            {
                pattern = "^" + pattern;
            }

            if (!pattern.EndsWith("$"))
            {
                pattern = pattern+"$";
            }

            var match = Regex.Match(source,pattern, RegexOptions.IgnoreCase);
            return match.Success;
        }

        /// <summary>
        /// Convert to Base64 string
        /// </summary>
        /// <param name="source">Source string</param>
        /// <param name="encoding">Encoding method. The default is ASCII encoding</param>
        /// <returns>Return encoded bas64 string</returns>
        public static string ToBase64(this string source, Encoding encoding = null)
        {
            Encoding textEncoding = encoding ?? System.Text.ASCIIEncoding.ASCII;
            byte[] data = textEncoding.GetBytes(source);
            return Convert.ToBase64String(data);
        }

        /// <summary>
        /// Convert to base64 string url 
        /// </summary>
        /// <param name="source">source string</param>
        /// <param name="encoding">Encoding method. The default is UTF8 encoding</param>
        /// <returns></returns>
        public static string ToBase64Url(this string source, Encoding encoding = null)
        {
            Encoding textEncoding = encoding ?? System.Text.ASCIIEncoding.UTF8;
            byte[] data = textEncoding.GetBytes(source);

            return Convert.ToBase64String(data)
                      .Replace('+', '-')
                      .Replace('/', '_')
                      .Replace("=", "");
        }
    }
}
