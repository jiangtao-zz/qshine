using System;
using System.Collections.Generic;

namespace qshine.Caching
{
	/// <summary>
	/// Cache provider interface.
	/// </summary>
	public interface ICacheProvider:IProvider
	{
		/// <summary>
		/// Create a cache instance.
		/// </summary>
		/// <returns>The cache instance.</returns>
		ICache Create(string name, int limitMegabytes, int physicalMemoryLimitPercentage, int pollingInterval);

		/// <summary>
		/// Create a default cache instance.
		/// </summary>
		/// <returns>The default cache instance</returns>
		ICache Create();
	}
}
