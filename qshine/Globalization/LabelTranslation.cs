using System;
using System.Collections.Generic;
using System.Text;

namespace qshine
{
    /// <summary>
    /// Lable translation object.
    /// Transalte an English phase to other environment language 
    /// </summary>
    public static class LabelTranslation
    {
        /// <summary>
        /// Translate to native language
        /// </summary>
        /// <param name="format">English phase with format</param>
        /// <param name="args">arguments.</param>
        /// <returns></returns>
        public static string _G(this string format, params object[] args)
        {
            //TODO:: Need translate to other language
            return string.Format(format, args);
        }
    }
}
