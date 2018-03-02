using System;
using System.Collections;
using System.Globalization;
using System.Text;

namespace qshine
{
	/// <summary>
	/// Inspect object properties value
	/// 
	/// obj.FormatObjectValues;
	/// </summary>
	public static class ObjectInspector
	{
		const char indent = ' ';//'\t'

		/// <summary>
		/// inspect and format class object property detail information
		/// </summary>
		/// <param name="obj">object to be inspected</param>
		/// <returns>Formatted detail object property information</returns>
		public static string FormatObjectValues(this object obj)
		{
			return FormattingObject(obj, 0);
		}

		#region private
		private static void FormattingArray(object obj, StringBuilder sb, int level)
		{
			var po = obj as Array;
			for (var i = 0; i < po.Length; i++)
			{
				var pobj = po.GetValue(i);
				sb.Append(indent, level + 1);
				FormatingArrayValue(pobj, i, null, sb, level);
			}
		}

		private static void FormattingDictionary(object obj, StringBuilder sb, int level)
		{
			var dictionary = obj as IDictionary;
			var count = dictionary.Count;
			var myObject = new object[count];
			var key = new object[count];
			if (count > 0)
			{
				var i = 0;
				foreach (var x in dictionary.Keys)
				{
					key[i] = x;
					i++;
				}

				i = 0;
				foreach (var x in dictionary.Values)
				{
					myObject[i] = x;
					i++;
				}

				for (i = 0; i < count; i++)
				{
					sb.Append(indent, level + 1);
					FormatingArrayValue(myObject[i], i, key[i], sb, level);
				}
			}
		}

		private static void FormattingList(object obj, StringBuilder sb, int level)
		{
			var list = obj as IList;

			//formatting IList
			var i = 0;
			var count = list.Count;
			if (count > 0)
			{
				foreach (var x in list)
				{
					sb.Append(indent, level + 1);
					FormatingArrayValue(x, i, null, sb, level);
					i++;
				}
			}
		}

		private static void FormattingSimpleObject(object obj, StringBuilder sb, int level)
		{
			sb.Append(indent, level + 1);
			sb.AppendLine(GetSimpleObjectValue(obj));
		}

		private static string FormattingObject(object obj, int level)
		{
			const int MAXIMUM_LEVEL = 10;

			var tp = obj.GetType();
			var sb = new StringBuilder();

			//output property type name
			if (level == 0)
			{
				sb.Append(tp.Name);
			}
			//Set maximum level to 10 to avoid unexpected overflow just in case.
			else if (level > MAXIMUM_LEVEL)
			{
				return "...";
			}

			sb.AppendLine(" {");

			//Exception
			var exceptionObject = obj as Exception;
			if (exceptionObject != null)
			{
				sb.Append(GetExceptionCallStack(exceptionObject));
			}
			//formatting array
			else if (tp.IsArray)
			{
				FormattingArray(obj, sb, level);
			}
			//formatting generic
			else if (tp.IsGenericType && IsSystemType(tp))
			{
				//IDictionary
				if (obj is IDictionary)
				{
					FormattingDictionary(obj, sb, level);
				}
				//IList
				else if (obj is IList)
				{
					FormattingList(obj, sb, level);
				}
				//Other unknown structure
				else
				{
					FormattingSimpleObject(obj, sb, level);
				}
			}
			//formatting class
			else if (tp.IsClass)
			{
				//formating system type
				if (IsSystemType(tp))
				{
					FormattingSimpleObject(obj, sb, level);
				}
				//formatting user class
				else
				{
					//formatting all properties
					foreach (var pinfo in tp.GetProperties())
					{
						//only formatting readable properties
						if (pinfo.CanRead)
						{
							FormatObjectValue(pinfo.GetValue(obj, null), pinfo.Name, pinfo.PropertyType, sb, level);
						}
					}
					//formatting all fields
					foreach (var finfo in tp.GetFields())
					{
						//only for public fields
						if (finfo.IsPublic)
						{
							FormatObjectValue(finfo.GetValue(obj), finfo.Name, finfo.FieldType, sb, level);
						}
					}
				}
			}
			else
			{
				FormattingSimpleObject(obj, sb, level);
			}
			sb.Append(indent, level + 1);
			sb.AppendLine("}");
			return sb.ToString();
		}

		private static void FormatingArrayValue(object obj, int i, object key, StringBuilder sb, int level)
		{
			if (obj == null)
			{
				sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "[{0}]=null", i));
			}
			else
			{
				if (key != null)
				{
					sb.Append(string.Format(CultureInfo.CurrentCulture, "{0}[{1}:{2}]={3}", obj.GetType().Name, i, key, FormattingObject(obj, level + 1)));
				}
				else
				{
					sb.Append(string.Format(CultureInfo.CurrentCulture, "{0}[{1}]={2}", obj.GetType().Name, i, FormattingObject(obj, level + 1)));
				}
			}
		}


		private static void FormatObjectValue(object obj, string name, Type tp, StringBuilder sb, int level)
		{
			sb.Append(indent, level + 1);
			sb.Append(tp.Name + " ");
			sb.Append(name + "=");
			if (tp.IsArray || tp.IsGenericType || !IsSystemType(tp))
			{
				sb.Append(FormattingObject(obj, level + 1));
			}
			else
			{
				sb.Append(GetSimpleObjectValue(obj));
				sb.AppendLine(";");
			}
		}

		private static string GetSimpleObjectValue(object obj)
		{
			if (obj == null)
			{
				return "null";
			}

			var name = obj.GetType().Name;
			switch (name)
			{
				case "String":
					return string.Format(CultureInfo.CurrentCulture, "\"{0}\"", obj);
				case "DateTime":
				case "DateSpan":
					return string.Format(CultureInfo.CurrentCulture, "#{0}#", obj);
				default:
					return obj.ToString();
			}
		}

		private static bool IsSystemType(Type t)
		{
			return t.FullName.StartsWith("System", StringComparison.Ordinal);
		}

		/// <summary>
		/// Get exception call stack information
		/// </summary>
		/// <param name="ex">Exception object</param>
		/// <returns>Call stack of the exception</returns>
		public static string GetExceptionCallStack(Exception ex)
		{
			return ex.ToString();
		}
		#endregion
	}

}
