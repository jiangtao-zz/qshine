using System;
namespace qshine
{
    /// <summary>
    /// Default command bus factory
    /// </summary>
	public class DefaultCommandBusFactory:ICommandBusFactory
	{
        /// <summary>
        /// Create a command bus
        /// </summary>
        /// <returns></returns>
		public ICommandBus Create()
		{
			return new NetCommandBus();
		}
	}
}
