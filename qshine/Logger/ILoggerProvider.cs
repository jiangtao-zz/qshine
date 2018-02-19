using System;
namespace qshine
{
	/// <summary>
	/// Logger provider interface
	/// </summary>
	public interface ILoggerProvider : IProvider
	{
		/// <summary>
		/// Get a specific logger by a named category.
		/// </summary>
		/// <param name="category">logger category 
		/// <returns>Logger</returns>
		ILogger GetLogger(string category);
	}
}
