using System;
namespace qshine
{
	/// <summary>
	/// Command handler.
	/// </summary>
	public interface ICommandHandler<T> : ICommandHandler
		where T:ICommandMessage
	{
		void Handle(T commandMessage);
	}

	public interface ICommandHandler
	{
	}

}
