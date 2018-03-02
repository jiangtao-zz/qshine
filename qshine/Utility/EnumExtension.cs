using System;

namespace qshine
{
    /// <summary>
    /// .NET Enum class extension
    /// </summary>
    public static class EnumExtension
    {
        /// <summary>
        /// Will get the string value for a given enums value, this will
        /// only work if you assign the StringValue attribute to the items in your enum.
        /// </summary>
        /// <param name="value">A enum property</param>
        /// <returns>Returns a text format of a enum property</returns>
        public static string GetStringValue(this Enum value)
        {
            return GetEnumStringValue(value);
        }

		/// <summary>
		/// Gets the enum value. possible throw exception when value is not defined in string attribute.
		/// </summary>
		/// <returns>The enum value.</returns>
		/// <param name="value">Value.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T GetEnumValue<T>(this string value)
            where T : struct
        {
            foreach (T enumValue in Enum.GetValues(typeof(T)))
            {
                if (string.Compare(value, GetEnumStringValue(enumValue), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return enumValue;
                }
            }
            throw new ArgumentOutOfRangeException(string.Format("{0}.{1}", typeof(T), value));
        }

		/// <summary>
		/// Gets the enum value.
		/// </summary>
		/// <returns>The enum value.</returns>
		/// <param name="value">Value.</param>
		/// <param name="faultEnumValue">Fault enum value.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T GetEnumValue<T>(this string value, T faultEnumValue)
            where T : struct
        {
            foreach (T enumValue in Enum.GetValues(typeof(T)))
            {
                if (string.Compare(value, GetEnumStringValue(enumValue), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return enumValue;
                }
            }
            return faultEnumValue;
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
    /// public enum ActionType
    /// {
    ///    [StringValue("Status Changed")]
    ///    StatusChanged,
    ///    [StringValue("Voice Note")]
    ///    VoiceNotes,
    ///    [StringValue("Added Progress Note")]
    ///    AddProgressNotes,
    ///    [StringValue("E-Mailed")]
    ///    Emailed,
    ///    //Fault handler for internal use only
    ///    [StringValue("Inner-Unknown")]
    ///    Unknown = -1000
    ///}
    ///]]>
    /// 2. Get string value from enum
    ///
    /// string actionType = ActionType.StatusChanged.GetStringValue();
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
        OrigunalString,
        /// <summary>
        /// Return StringValue attribute value of enum.
        /// </summary>
        StringValue,
    }
}
