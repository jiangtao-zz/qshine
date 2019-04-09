using System;
namespace qshine.Compiler
{
    /// <summary>
    /// ...
    /// </summary>
	public enum ScriptBlockType
	{
        /// <summary>
        /// 
        /// </summary>
		Text,
        /// <summary>
        /// 
        /// </summary>
		Code,
        /// <summary>
        /// 
        /// </summary>
		Variable
	}
    /// <summary>
    /// 
    /// </summary>
	public class ScriptBlock
	{
        /// <summary>
        /// 
        /// </summary>
		public int Sequence { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string Text { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public ScriptBlockType BlockType { get; set; }
	}
}
