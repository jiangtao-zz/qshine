using System;
namespace qshine
{
	/// <summary>
	/// Ioc provider.
	/// </summary>
	public interface IIocProvider:IProvider
	{
		/// <summary>
		/// Create a container
		/// </summary>
		/// <returns>The container.</returns>
		IIocContainer CreateContainer();
	}

}
