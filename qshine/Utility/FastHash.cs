using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qshine.Utility
{
    /// <summary>
    /// Caculate a hash code
    /// </summary>
    public class FastHash
    {
        /// <summary>
        /// Calculate hash code for given properties.
        /// Note: The object hash code may not identical from each run. Only use Primitive type and string property for identical hashcode
        /// calculation.
        /// </summary>
        /// <param name="items">Contains a list of primitive values for hash code calculation.</param>
        /// <returns>An identical hash code.</returns>
        public static int GetHashCode(params object[] items)
        {
            int hashCode = 0;
            int valueHashCode = 0;

            foreach (object item in items)
            {
                if (item == null)
                {
                    valueHashCode = 0;
                }
                else if(item is string)
                {
                    //dotnet code string.GetHashCode is not identical for each run.
                    valueHashCode = GetIdenticalHashCode(item as string);
                }
                else
                {
                    valueHashCode = item.GetHashCode();
                }
                hashCode = Combine(hashCode, valueHashCode);
            }

            return hashCode;
        }

        static int Combine(int x, int y)
        {
            unchecked
            {
                return (x << 5) + 3 + x ^ y;
            }
        }

        static int GetIdenticalHashCode(string value)
        {
            int hascode = 0;
            foreach(int x in value)
            {
                hascode = Combine(hascode, x);
            }
            return hascode;
        }
    }
}
