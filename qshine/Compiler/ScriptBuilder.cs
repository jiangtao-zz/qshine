using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Compiler
{
    /// <summary>
    /// Not implemented
    /// </summary>
	public class ScriptBuilder
	{
		StringBuilder _builder;

        /// <summary>
        /// 
        /// </summary>
		public ScriptBuilder()
		{
			_builder = new StringBuilder();
		}

		private string ScriptHeader
		{
			get
			{
				return @"using System;
namespace _internal.Script
{
	public class Program
	{
		string _blankLine="""";
		public static void Main()
		{
";
			}
		}

		private string ScriptEnd
		{
			get
			{
				return @"
        }//method
    }//class
}//namespace"
				;
			}
		}

		string ScriptBlankLine
		{
			get
			{
				return "Print(_blankLine);";
			}
		}

		string ScriptTextVariableLine(int index)
		{
			return string.Format("Print(_blockV[{0}]);", index);
		}

		string ScriptBlockTextArray
		{
			get
			{
				return "";
			}
		}

		void BuildTextBlock(ScriptBlock block, int index)
		{
			if (string.IsNullOrEmpty(block.Text))
			{
				_builder.Append(ScriptBlankLine);
			}
			else
			{
				_builder.AppendLine(ScriptTextVariableLine(index));
			}
		}

		void BuildCodeBlock(ScriptBlock block)
		{
			if (!string.IsNullOrEmpty(block.Text))
			{
				_builder.Append(block.Text);
			}
		}

		void BuildVariableBlock(ScriptBlock block)
		{
			if (!string.IsNullOrEmpty(block.Text))
			{
				_builder.AppendLine(string.Format("Print({0});", block.Text));
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public string Render()
		{
			_builder.Append(ScriptHeader);
			_builder.Append(ScriptBlockTextArray);
			for (int i = 0; i < _scriptBlocks.Count; i++)
			{
				var block = _scriptBlocks[i];
				switch (block.BlockType)
				{
					case ScriptBlockType.Text:
						BuildTextBlock(block, i);
						break;
					case ScriptBlockType.Code:
                        BuildCodeBlock(block);
						break;
					case ScriptBlockType.Variable:
                        BuildVariableBlock(block);
						break;
				}
			}
			_builder.Append(ScriptEnd);
			return _builder.ToString();
		}

		int _sequence = 0;
		List<ScriptBlock> _scriptBlocks = new List<ScriptBlock>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputText"></param>
		public void AddOutputText(string outputText)
		{
			if (!string.IsNullOrEmpty(outputText))
			{
				_scriptBlocks.Add(new ScriptBlock
				{
					Sequence = ++_sequence,
					Text = outputText,
					BlockType = ScriptBlockType.Text
				});
			}
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
		public void AddCodeBlock(string code)
		{
			if (!string.IsNullOrEmpty(code))
			{
				_scriptBlocks.Add(new ScriptBlock
				{
					Sequence = ++_sequence,
					Text = code,
					BlockType = ScriptBlockType.Code
				});
			}
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="variable"></param>
		public void AddVariable(string variable)
		{
			if (!string.IsNullOrEmpty(variable))
			{
				_scriptBlocks.Add(new ScriptBlock
				{
					Sequence = ++_sequence,
					Text = variable,
					BlockType = ScriptBlockType.Variable
				});
			}
		}
	}
}
