using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Globalization
{
    /// <summary>
    /// Local string resource storage
    /// </summary>
    public interface ILocalString
    {
        /// <summary>
        /// Get localized string
        /// </summary>
        /// <param name="format">string format in Invariant Culture (Neutral resource language)</param>
        /// <param name="arguments">The values to format the string with.</param>
        /// <returns></returns>
        string GetString(string format, params object[] arguments);

    }
}
