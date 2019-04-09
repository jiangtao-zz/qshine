using System;
using System.Collections.Generic;
using qshine.Compiler;

namespace qshine.Compiler
{
    /// <summary>
    /// Not implemented
    /// </summary>
	public class ScriptCompiler
	{
        readonly string _leadingSymbol = "#@";
		ScriptBuilder  _scriptBuilder = new ScriptBuilder();

		/// <summary>
		/// Parse the specified input.
		/// </summary>
		/// <returns>The parse.</returns>
		/// <param name="input">Input.</param>
		public string Parse(string input)
		{
			var blocks = input.Split(new String[] { _leadingSymbol }, StringSplitOptions.None);

			//No script code found.
			if (blocks.Length <= 1) return input;

			_scriptBuilder.AddOutputText(blocks[0]);

			for (int i = 1; i < blocks.Length; i++)
			{
				//block #@/* ... #@*/
				if(blocks[i].StartsWith("/*", StringComparison.InvariantCulture)
					&& i<blocks.Length - 1 && blocks[i + 1].StartsWith("*/", StringComparison.InvariantCulture))
				{
					_scriptBuilder.AddOutputText(blocks[i+1].Substring(2));
					i++;
				}
				//c# block
				else
				{
					var dataCode = new BlockSection(blocks[i]);
					//block #@{ ... #@}
					if (!dataCode.HasVariable
					    && blocks[i].StartsWith("{", StringComparison.InvariantCulture)
						&& i < blocks.Length - 1 && blocks[i + 1].StartsWith("}", StringComparison.InvariantCulture)
						)
					{
						_scriptBuilder.AddCodeBlock(blocks[i].Substring(1));
						_scriptBuilder.AddOutputText(blocks[i + 1].Substring(1));
						i++;
					}
					else
					{
						_scriptBuilder.AddCodeBlock(dataCode.Code);
						if (dataCode.HasVariable)
						{
							_scriptBuilder.AddVariable(dataCode.Variable);
						}
						if (dataCode.HasText)
						{
							_scriptBuilder.AddOutputText(dataCode.Text);
						}
					}
				}
			}

			var code = _scriptBuilder.Render();


			return code;
		}

	}
	
    /// <summary>
    /// 
    /// </summary>
	public class BlockSection
	{
        readonly long _length;
		char _currentChar;
		int _currentPosition;
		string _input;
        /// <summary>
        /// 
        /// </summary>
		public BlockSection() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
		public BlockSection(string input)
		{
			_input = input;
			if (!string.IsNullOrEmpty(input))
			{
				_length = input.Length;
			}
			_currentPosition = 0;
			_currentChar = _input[_currentPosition];
			int begin = _currentPosition;
            SkipWhiteSpace();
			if (_currentChar == '{')
			{
				Read();
				SkipWhiteSpace();
				int beginVar = _currentPosition;
				SkipVariable();
				int endVar = _currentPosition;
				SkipWhiteSpace();
				if (_currentChar == '}')
				{
					Read();
					var value = GetSegment(beginVar, endVar-1);
					Variable = value;
					SkipNewLine();
					Text = GetSegment(_currentPosition + 1);
				}
				else
				{
					Code = GetSegment(begin);
				}
			}
			else if (_currentChar == '}')
			{
				ReadLine();
				Code = GetSegment(begin, _currentPosition-1);
				Text = GetSegment(_currentPosition);
			}
			else
			{
                ReadLine();
				Code = GetSegment(begin, _currentPosition-1);
				Text = GetSegment(_currentPosition);
			}
		}

		void Read()
		{
			_currentPosition++;
			if (_currentPosition >= _length)
			{
				_currentChar = '\0';
			}
			else
			{
				_currentChar = _input[_currentPosition];
			}
		}

		void ReadLine()
		{
			while (_currentPosition<_length && !IsNewline())
			{
				Read();
			}
			if (IsNewline()) { Read(); }
			if (IsNewline()) { Read(); }
		}
		string GetSegment(int begin, int end=-1)
		{
			if (begin >= _length) return null;
			if (end >= _length || end ==-1)
			{
				return _input.Substring(begin);
			}
			return _input.Substring(begin, end - begin + 1);
		}


		/// <summary>
		/// Skips the white space, but not new line.
		/// </summary>
		void SkipWhiteSpace()
		{
			while(_currentPosition<_length)
			{
				if (!IsWhiteSpace()) return;
				Read();
			}
		}

		void SkipVariable()
		{
			if (!IsVariableLeadChar()) return;
			while(_currentPosition<_length)
			{
				if (!IsVariableChar()) return;

				Read();
			}
		}

		void SkipNewLine()
		{
			
			while(_currentPosition<_length)
			{
				if (!IsNewline()) return;

				Read();
			}
		}

		bool IsVariableChar()
		{
			if (Char.IsLetterOrDigit(_currentChar) 
			    || _currentChar == '_' 
			   ) return true;

			return false;
		}

		bool IsVariableLeadChar()
		{
			if (Char.IsLetter(_currentChar)
				|| _currentChar == '_'
			   ) return true;

			return false;
		}

		bool IsWhiteSpace()
		{
			return Char.IsWhiteSpace(_currentChar)
					   && !IsNewline();
		}

		bool IsNewline()
		{
			return (_currentChar == '\r'
					|| _currentChar == '\n'
					|| _currentChar == '\u0085'
					|| _currentChar == '\u2028'
					|| _currentChar == '\u2029');
		}

        /// <summary>
        /// 
        /// </summary>
		public string Code { get; set; }

        /// <summary>
        /// 
        /// </summary>
		public string Variable { get; set; }

        /// <summary>
        /// 
        /// </summary>
		public string Text { get; set; }

        /// <summary>
        /// 
        /// </summary>
		public bool HasText
		{
			get
			{
				return !string.IsNullOrEmpty(Text);
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public bool HasVariable
		{
			get
			{
				return !string.IsNullOrEmpty(Variable);
			}
		}

	}
}
