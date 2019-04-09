using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.database
{
	enum PreCondition
	{
		Null,
		ConditionTrue,
		ConditionFalse,
	}

	enum LogicCondition
	{
		Null,
		And,
		Or,
	}
	/// <summary>
	/// Parse script for particular database language
	/// 
	/// #@ if Oracle
	/// 	#@ var p1 = "XYZ"
	/// #@ elseif MsSql
	/// 	#@ var p1 = ABC
	/// #@ endif
	/// 
	/// Create table #@{p1} {
	/// 
	/// }
	/// 
	/// 
	/// 
	/// 
	/// </summary>
	public class ScriptParser
	{
		readonly string _document;
		readonly string[] _preDefinedVariables;
		readonly List<string> _variables = new List<string>();
        //readonly bool _withinTrueBlock;
        //readonly bool _hasInit;
		readonly Stack<PreCondition> _conditionStack = new Stack<PreCondition>();

		public ScriptParser(string document, params string[] preDefinedVariables)
		{
			_document = document;
			_preDefinedVariables = preDefinedVariables;
		}

        //pre-defined boolean variables
        readonly string[] _predefinedBooleans = new [] {
			"ORACLE",
			"MSSQL",
			"MYSQL",
			"SQLITE",
			"POSTGRE",
			"MSACCESS",
		};

		const string KeyIf = "if";
		const string KeyElse = "else";
		const string KeyElseIf = "elseif";
		const string KeyEndIf = "endif";
		const string KeyVar = "var";
		const string KeyComments = "//";

        /// <summary>
        /// The keywords.
        /// </summary>
        readonly string[] _keywords = new[] {
			KeyIf, 		//if condition
			KeyElse, 	//else condition
			KeyElseIf, 	//else if
			KeyEndIf,	//end if
			KeyVar,		//define variable
			KeyComments	//comments
		};
        readonly string[] _logicOperators = new string[] {
			"or",
			"and"
		};

		private readonly string _leadingDirective = "#@";
		//string _variablePlaceholder = "#@{{0}}";

		/// <summary>
		/// Transfer a document based on directive script syntax.
		/// The document contains "#@" leading directive 
		/// Type of directive:
		/// 	Conditional directive: evaluate pre-defined variable. If the variable defined, return true.
		/// 		#@ if preDefinedVariable1 or predefinedVariable2
		/// 		#@ elseif preDefinedVariable3 and preDefinedVariable4
		/// 		#@ endif
		/// 		The predefined variable is pass from external
		/// 	Variable directive:	define variable to replace variable placeholder
		/// 		#@ var v1 = abc
		/// 		#@ var v2 = "ABC DEF"
		/// 	Placeholder directive: a variable placeholder
		/// 		free text #@{v1} other free text
		/// 
		/// It outputs true condition text block and replace the placeholder variables.
		/// 
		/// </summary>
		/// <returns></returns>
		public string Transfer()
		{
			if (string.IsNullOrEmpty(_document)) return "";

			var blocks = _document.Split(new String[] { _leadingDirective }, StringSplitOptions.None);
			var builder = new StringBuilder();
			if (blocks.Length > 0)
			{
				builder.Append(blocks[0]);
				for (int i = 1; i < blocks.Length; i++)
				{
					builder.Append(ParseBlock(blocks[i]));
				}
			}
			return builder.ToString();
		}

		string ParseBlock(string block)
		{
            var directiveLine = ReadLine(block, out int index);

            PreCondition condition = PreCondition.Null;

            if (_conditionStack.Count > 0)
            {
                condition = _conditionStack.Peek();
            }

			if (directiveLine.Length > 0)
			{
				if (directiveLine.StartsWith(KeyComments, StringComparison.Ordinal)) //comments
                {
					if (condition == PreCondition.ConditionTrue && index > 0)
					{

						return block.Substring(index);
					}
					return "";
                }

				if (directiveLine.StartsWith(KeyVar, StringComparison.Ordinal)) //variable
                {
					if (condition == PreCondition.ConditionTrue && index > 0)
					{

						return block.Substring(index);
					}
					return "";
                }

                if (directiveLine.StartsWith(KeyIf, StringComparison.Ordinal)) //if condition
                {
                    bool result = EvaluateCondition(directiveLine);
					if (condition == PreCondition.Null)
					{
						_conditionStack.Push(result ? PreCondition.ConditionTrue : PreCondition.ConditionFalse);
					}
					else
					{
						throw new InvalidOperationException("Nest if condition is not available.");
					}

					if (!result || index == -1) return ""; //do not output anything if condition is false.

                    return block.Substring(index);
                }
                
				if (directiveLine.StartsWith(KeyElseIf, StringComparison.Ordinal)) //elseif condition
                {
					if (condition == PreCondition.ConditionFalse)
					{
						bool result = EvaluateCondition(directiveLine);
						if (result)
						{
							_conditionStack.Pop();
							_conditionStack.Push(result ? PreCondition.ConditionTrue : PreCondition.ConditionFalse);

							if (result && index > 0)
							{
								return block.Substring(index);
							}
						}
					}
					return ""; //do not output anything if condition is false.
                }

				if (directiveLine.StartsWith(KeyElse, StringComparison.Ordinal)) //else condition
                {
					if (condition == PreCondition.ConditionTrue || index == -1) return ""; //condition false or end of result
                    return block.Substring(index);
                }

                if (directiveLine.StartsWith(KeyElse, StringComparison.Ordinal)) //endif condition
                {
                    _conditionStack.Pop();
                    if (index == -1) return ""; //condition false or end of result
                    return block.Substring(index);
                }

			}
			return block;
		}



		/// <summary>
		/// Read one line data
		/// </summary>
		/// <param name="text"></param>
		/// <param name="indexNextPosition"></param>
		/// <returns></returns>
		string ReadLine(string text, out int indexNextPosition)
		{
			indexNextPosition = -1;

			if (!string.IsNullOrEmpty(text))
			{
				int index = text.IndexOf("\n", StringComparison.Ordinal);//for non-Windows
				if (index >= 0)
				{
					indexNextPosition = index + 1;
					if (index > 0 && text[index - 1] == '\r') //for Windows
					{
						index--;
					}
				}
				if (indexNextPosition > 0)
				{
					return text.Substring(0, index);
				}
				return text;
			}
			return "";
		}

		string[] GetTokens(string text, out int index)
		{
			var keys = ReadLine(text, out index);
			if (!string.IsNullOrEmpty(keys))
			{
				return keys.Trim().Split(null);//splitted by white space
			}
			return new[] { "" };
		}

		//if Oracle or MySql
		//elseif Oracle and OtherPreDefine
		private bool EvaluateCondition(string lineText)
		{
			var words = lineText.Trim().Split(null);//splitted by white space

			int count = words.Length;
			bool result = true;
			LogicCondition logicOp = LogicCondition.Null;//initial using and logic
			bool currentResult = true;

			for (int i = 1; i < count; i++)
			{
				var token = RemoveQuote(words[i]);
				if (string.Compare(LogicCondition.And.ToString(), token, StringComparison.OrdinalIgnoreCase) == 0)
				{
					logicOp = LogicCondition.And;
				}
				else if (string.Compare(LogicCondition.Or.ToString(), token, StringComparison.OrdinalIgnoreCase) == 0)
				{
					logicOp = LogicCondition.Or;
				}
				else
				{
					var index = Array.IndexOf(_preDefinedVariables, token);
					currentResult = index != -1;
					if (logicOp == LogicCondition.Null)
					{
						result = currentResult;
					}
					else if (logicOp == LogicCondition.Or)
					{
						result = currentResult ||result;
					}
					else if (logicOp == LogicCondition.And)
					{
						result = currentResult && result;
					}
				}
			}
			return result;
		}

		private string RemoveQuote(string text)
		{
			var lineText = text;

			if ((text.StartsWith("\'", StringComparison.Ordinal) && text.EndsWith("\'", StringComparison.Ordinal)) || 
			    (text.StartsWith("\"", StringComparison.Ordinal) && text.EndsWith("\"", StringComparison.Ordinal)))
			{
				return text.Substring(1, text.Length - 2);
			}
			return text;
		}
	}
}
