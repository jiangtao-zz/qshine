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

        /// <summary>
        /// Indicates the assembly has been initialized
        /// </summary>
        public ulong Initialized { get; set; }
	}

    public enum InitializationType:ulong
    {
        Undefined = 0,
        IStartupInitializerType =1,
    }
}
