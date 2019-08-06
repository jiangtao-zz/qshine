using System;
using System.Collections.Generic;
using System.Text;

namespace Archive
{
    public class Output
    {
        public static void WriteLine(string format)
        {
            Console.WriteLine(format);
        }

        public static void Write(string format)
        {
            Console.Write(format);
        }
    }
}
