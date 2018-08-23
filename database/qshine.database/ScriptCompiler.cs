using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace qshine.database
{
	public enum TokenType
	{
		If,
		Else,
		ElseIf,
		EndIf,
		And,
		Or,
		Eq,
		NotEq,
		Ge,
		Gt,
		Lt,
		Le,
		LeftParen,
		RightParen,
		Text,
		Number,
		Variable,
		Escape,
		AtEnd,
	}
	public class ScriptToken
	{
		public ScriptToken(TokenType type, string value)
		{
			TokenType = type;
			TokenValue = value;
		}
		public TokenType TokenType { get; private set; }

		public string TokenValue { get; private set; }

		public int Index { get; set; }
	}

	public class ScriptInterpreter
	{
		int _position;
		int _length;
		string _text;

		const char ESCAPE = '\\';

		static readonly Dictionary<string, TokenType> _operators = new Dictionary<string, TokenType> {
			{"if",TokenType.If},
			{"else",TokenType.Else},
			{"elseif",TokenType.ElseIf},
			{"endif",TokenType.EndIf},
			{"&&",TokenType.And},	//and
			{"and",TokenType.And},	//and
			{"||",TokenType.Or},	//or
			{"or",TokenType.Or},	//or
			{"==",TokenType.Eq},	//Eq
			{"<>",TokenType.NotEq},	//NotEq
			{"!=",TokenType.NotEq},	//NotEq
			{">=",TokenType.Ge},	//Ge
			{">",TokenType.Gt},		//Gt
			{"<",TokenType.Lt},		//Lt
			{"<=",TokenType.Le},	//Le
			{"(",TokenType.LeftParen},	//LeftParen
			{")",TokenType.RightParen},	//RightParen
			{"\"",TokenType.Text},		//String start or end
			{"'",TokenType.Text}		//String start or end
		};

		static readonly char[] _opChars = _operators.SelectMany(x => x.Key.ToCharArray()).Distinct().ToArray();
		static readonly int _maxKeywordLength = _operators.Keys.Select(x => x.Length).Max();

		public List<ScriptToken> Tokenize(string input)
		{
			ScriptToken token;
			var tokenList = new List<ScriptToken>();

			if (string.IsNullOrEmpty(input))
			{
				return tokenList;
			}

			_text = input;
			_length = _text.Length;


			for(int _position=0;_position<_length;_position++)
			{
				if (char.IsWhiteSpace(_text[_position]))
				{
					continue; // just skip whitespace
				}

				if (IsOperatorChar(_text[_position]))
				{
					// we have back-buffer; could be a>b, but could be >=
					// need to check if there is a combined operator candidate
					if (TryGetNextToken(out token))
					{
						tokenList.Add(token);
					}
				}
				else
				{
					tokenList.Add(GetStringToken());
				}
			}
			return tokenList;
		}

		bool IsOperatorChar(char newChar)
		{
			return Array.IndexOf(_opChars, newChar) >= 0;
		}

		bool TryGetNextToken(out ScriptToken token)
		{
			token = null;
			var size = Math.Min(_maxKeywordLength, _length - _position);
			var text = _text.Substring(_position, size);
			foreach (var op in _operators)
			{
				if (text.StartsWith(op.Key, StringComparison.InvariantCultureIgnoreCase))
				{
					if (op.Value == TokenType.Text)
					{
						token = GetStringToken(_text[_position]);
					}
					else
					{
						token = new ScriptToken(op.Value, op.Key);
						_position += op.Key.Length;
					}
					return true;
				}
			}
			return false;
		}

		ScriptToken GetStringToken(char separator='\0')
		{
			StringBuilder buffer = new StringBuilder();
			bool hasQuote = separator!='\0';
			if (hasQuote)
			{
				_position++;
				for (; _position < _length; _position++)
				{
					var c = _text[_position];
					if (c != separator)
					{
						if (c == ESCAPE)
						{
							_position++;
							if (_position >= _length)
							{
								break;
							}
						}
						buffer.Append(_text[_position]);
					}
					else //end String text
					{
						_position++;
						break;
					}
				}
			}
			else
			{
				//find a variable
				for (; _position < _length; _position++)
				{
					var c = _text[_position];
					if (char.IsLetterOrDigit(c))
					{
						buffer.Append(c);
						continue;
					}
					break;
				}
			}
			return new ScriptToken(TokenType.Text, buffer.ToString());
		}
	}

}
