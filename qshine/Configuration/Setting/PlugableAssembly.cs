using System.Reflection;
namespace qshine
{
	public class PlugableAssembly
	{
		/// <summary>
		/// Gets or sets the path of assembly.
		/// </summary>
		/// <value>The path.</value>
		public string Path { get; set; }

		/// <summary>
		/// Gets or sets the assembly.
		/// </summary>
		/// <value>The assembly.</value>
		public Assembly Assembly { get; set; }
	}
}
