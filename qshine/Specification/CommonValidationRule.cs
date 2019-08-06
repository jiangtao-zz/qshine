using System;
using System.Linq;

namespace qshine.Specification
{
    /// <summary>
    /// 
    /// </summary>
    public class CommonCheck
    {
        /// <summary>
        /// Precision and Scale validation:
        ///     Precision is the total number of digits in a Numeric value, both to the right and left of the decimal point.
        ///     Scale refers to the total number of digits after the decimal point.
        ///     So, for 123.45 or -123.45 the Precision is 5 and Scale is 2. 
        ///         for 123 the Precision is 3 and Scale is 0.
        /// </summary>
        /// <param name="value">The value to be evaluated</param>
        /// <param name="precision">Expected precision.</param>
        /// <param name="scale">Expected scale</param>
        /// <param name="ignoreNull">Treat null is true</param>
        /// <returns>Returns True if the value satisfied the requirement.</returns>
        public static bool PrecisionScale(object value, int precision, int scale, bool ignoreNull)
        {
            if (value == null) return ignoreNull;

            bool satisfied;

            decimal decimalValue;
            if (!decimal.TryParse(value.ToString(), out decimalValue)) return false;

            string decimalString = decimalValue.ToString();
            var count = decimalString.Count(c => c >= '0' && c <= '9');
            satisfied = count <= precision;

            int i = decimalString.IndexOf('.');

            if (i < 0) return satisfied;

            return satisfied && (decimalString.Length - i) < scale;
        }

        /// <summary>
        /// Range validation:
        ///     Compare date, number and any IComparable object
        /// 
        /// </summary>
        /// <param name="value">The value to be evaluated</param>
        /// <param name="minValue">minimal value to be compare</param>
        /// <param name="maxValue">maximum value to be compare</param>
        /// <param name="ignoreNull">Treat null as true.</param>
        /// <returns>Returns True if the value satisfied the requirement.</returns>
        public static bool Range(object value, IComparable minValue, IComparable maxValue, bool ignoreNull)
        {
            var cv = value as IComparable;

            if (value == null) return ignoreNull;

            double nv = 0, minV = 0, maxV = 0;

            if (double.TryParse(cv.ToString(), out nv))
            {
                // For numeric values, the type should not matter, so we convert them to double
                return (minValue == null || (double.TryParse(minValue.ToString(), out minV) && minV <= nv)) &&
                    (maxValue == null || (double.TryParse(maxValue.ToString(), out maxV) && nv <= maxV));
            }
            // For non numeric values, use IComparable interface
            return (minValue == null || (cv.GetType() == minValue.GetType() && cv.CompareTo(minValue) >= 0)) &&
                   (maxValue == null || (cv.GetType() == maxValue.GetType() && cv.CompareTo(maxValue) <= 0));

        }


    }

    //public class CommonPropertyRules
    //{
    //    public static BusinessValidationRule<T>
    //        PrecisionScaleRule<T>(System.Linq.Expressions.Expression<Func<T, string>> property, int precision, int scale)
    //        where T:class
    //    {
    //        var rule = new BusinessValidationRule<T>("a", x => CommonCheck.PrecisionScale(property.Parameters[0], precision, scale, true), "aa");

    //        return null;
    //    }
    //}
}
