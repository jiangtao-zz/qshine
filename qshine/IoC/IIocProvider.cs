using System;
namespace qshine.IoC
{
	/// <summary>
	/// Ioc provider.
	/// </summary>
	public interface IIoCProvider:IProvider
	{
		/// <summary>
		/// Create a container
		/// </summary>
		/// <returns>The container.</returns>
		IIoCContainer CreateContainer();
	}

}
