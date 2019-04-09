using System;
namespace qshine
{
	/// <summary>
	/// Command handler class.
	/// </summary>
	public interface ICommandHandler<T> : ICommandHandler
		where T:ICommandMessage
	{
        /// <summary>
        /// Command handler
        /// </summary>
        /// <param name="commandMessage"></param>
		void Handle(T commandMessage);
	}

    /// <summary>
    /// Command handler interface
    /// </summary>
	public interface ICommandHandler
	{
	}

}
