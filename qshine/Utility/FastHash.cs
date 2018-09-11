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
        /// Calculate hash code for given properties
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static int GetHashCode(params object[] items)
        {
            int hashCode = 0;

            foreach (object item in items)
            {
                hashCode = Combine(hashCode, item != null ? item.GetHashCode() : 0);
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
    }
}
