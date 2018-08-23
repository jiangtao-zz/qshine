using System;
namespace qshine
{
	/// <summary>
	/// Interface of ommand bus factory.
	/// </summary>
	public interface ICommandBusFactory:IProvider
	{
		/// <summary>
		/// Create a command bus instance.
		/// </summary>
		/// <returns>The create.</returns>
		ICommandBus Create();
	}
}
