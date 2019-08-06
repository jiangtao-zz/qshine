using System;

namespace qshine
{
    /// <summary>
    /// .NET Enum class extension
    /// </summary>
    public static class EnumExtension
    {
        /// <summary>
        /// Convert a enum to a string value..
        /// </summary>
        /// <param name="value">A enum property</param>
        /// <param name="option">Indicates different format of string.
        ///     EnumValueType.OriginalValue: return a numberic value
        ///     EnumValueType.OriginalString: return enum defined name
        ///     EnumValueType.StringValue: return "StringValue" attribute on enum property.
        ///     
        ///     default is original enum string
        /// </param>
        /// <returns>Returns a given formatted enum property string</returns>
        public static string GetStringValue(this Enum value, EnumValueType option= EnumValueType.OriginalString)
        {
            switch (option)
            {
                case EnumValueType.StringValue:
                    return GetEnumStringValue(value);

                case EnumValueType.OriginalValue:
                    return Convert.ToInt32(value).ToString();

                default:
                    return value.ToString();
            }
        }

        /// <summary>
        /// Using String.Format() to format a given enum StringValue.
        /// </summary>
        /// <param name="value">A enum property</param>
        /// <param name="args">format arguments</param>
        /// <returns></returns>
        public static string Format(this Enum value, params object[] args)
        {
            return string.Format(value.GetStringValue(EnumValueType.StringValue), args);
        }

        /// <summary>
        /// Convert a string to a particular type of enum value.
        /// </summary>
        /// <returns>The enum value or throw an exception if it is not an expected string.</returns>
        /// <param name="value">A string value to be converted.</param>
        /// <param name="option">Indicates different format of string.
        ///     EnumValueType.OriginalValue: return a numberic value
        ///     EnumValueType.OriginalString: return enum defined name
        ///     EnumValueType.StringValue: return "StringValue" attribute on enum property.
        ///     
        ///     default is original enum string
        /// </param>
        /// <typeparam name="T">The Enum type to be return.</typeparam>
        public static T GetEnumValue<T>(this string value, EnumValueType option= EnumValueType.OriginalString)
            where T : struct
        {
            switch (option)
            {
                case EnumValueType.OriginalValue:
                    return (T)Enum.ToObject(typeof(T), Convert.ToInt32(value));

                case EnumValueType.OriginalString:
                    return (T)Enum.Parse(typeof(T), value);

                default:
                    foreach (T enumValue in Enum.GetValues(typeof(T)))
                    {
                        if (string.Compare(value, GetEnumStringValue(enumValue), StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return enumValue;
                        }
                    }
                    throw new ArgumentException(string.Format("{0}.{1}", typeof(T), value));
            }
        }

        /// <summary>
        /// Convert a string to a particular type of enum value.
        /// If the string is unidentified, it returns faultEnumValue.
        /// </summary>
        /// <returns>The enum value.</returns>
        /// <param name="value">A string value to be converted.</param>
        /// <param name="faultEnumValue">Use this value if the string is an invalid enum value.</param>
        /// <typeparam name="T">Type of enum to be converted.</typeparam>
        /// <param name="option"></param>
        public static T GetEnumValue<T>(this string value, T faultEnumValue, EnumValueType option = EnumValueType.OriginalString)
            where T : struct
        {
            switch (option)
            {
                case EnumValueType.OriginalValue:
                    Int32 v;
                    if (Int32.TryParse(value, out v))
                    {
                        return (T)Enum.ToObject(typeof(T), v);
                    }
                    else
                    {
                        return faultEnumValue;
                    }

                case EnumValueType.OriginalString:
                    T v1 = default(T);
                    if (Enum.TryParse(value, true, out v1))
                    {
                        return v1;
                    }
                    else
                        return faultEnumValue;
                default:
                    foreach (T enumValue in Enum.GetValues(typeof(T)))
                    {
                        if (string.Compare(value, GetEnumStringValue(enumValue), StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return enumValue;
                        }
                    }
                    return faultEnumValue;
            }
        }

        private static string GetEnumStringValue(object value)
        {
            var type = value.GetType();
            var fieldInfo = type.GetField(value.ToString());
            var attribs = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
            return attribs != null && attribs.Length > 0 ? attribs[0].StringValue : null;
        }
    }

    /// <summary>
    /// String value attribute for Enum
    /// Usage:
    /// 1. Define enum with a string value attribute
    /// <![CDATA[
    /// public enum EnumSample
    /// {
    ///    [StringValue("Status 1")]
    ///    Status1,
    ///    [StringValue("Status 2")]
    ///    Status2,
    ///    [StringValue("Status 3")]
    ///    Status3,
    ///    [StringValue("Status 4")]
    ///    Status4,
    ///    Fault handler for internal use only
    ///    [StringValue("Unknown value")]
    ///    Unknown = -1
    ///}
    ///]]>
    /// 2. Get string value from enum
    ///
    /// </summary>
    public class StringValueAttribute : Attribute
    {
        #region Properties

        /// <summary>
        /// Holds the stringvalue for a value in an enum.
        /// </summary>
        public string StringValue { get; protected set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor used to init a StringValue Attribute
        /// </summary>
        /// <param name="value"></param>
        public StringValueAttribute(string value)
        {
            StringValue = value;
        }

        #endregion
    }

    /// <summary>
    /// Define enum for any enum value that required by enum reader
    /// </summary>
    public enum EnumValueType
    {
        /// <summary>
        /// Return int type value of the enum, such as "0"
        /// </summary>
        OriginalValue,
        /// <summary>
        /// Return enum Name string, such as "OriginalString"
        /// </summary>
        OriginalString,
        /// <summary>
        /// Return StringValue attribute value of enum, such as "Original String".
        /// </summary>
        StringValue,
    }
}
