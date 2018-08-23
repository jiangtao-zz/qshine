using System;
namespace qshine
{
	public interface IEventMessageHandler<T>:IEventMessageHandler
		where T:IEventMessage
	{
		/// <summary>
		/// Handler the specified event message.
		/// </summary>
		/// <returns>The handler.</returns>
		/// <param name="eventMessage">Event message.</param>
		T Handler(T eventMessage);
	}

	public interface IEventMessageHandler
	{
	}
}
