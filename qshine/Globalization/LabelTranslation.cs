using System;
using System.Collections.Generic;
using System.Text;

namespace qshine
{
    public static class LabelTranslation
    {
        public static string _G(this string format, params object[] args)
        {
            //TODO:: Need translate to other language
            return string.Format(format, args);
        }
    }
}
