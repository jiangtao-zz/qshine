using System;
namespace qshine.Compiler
{
	public enum ScriptBlockType
	{
		Text,
		Code,
		Variable
	}
	public class ScriptBlock
	{
		public int Sequence { get; set; }

		public string Text { get; set; }

		public ScriptBlockType BlockType { get; set; }
	}
}
