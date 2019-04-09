using System;
namespace qshine.Messaging
{
    /// <summary>
    /// A generic handler to handle message.
    /// It can apply to event handler, command handler, message handler.
    /// All handler must implement Handle method
    /// </summary>
    /// <typeparam name="T">Type of handle message</typeparam>
	public interface IHandler<T>
	{
		/// <summary>
		/// Handle specific event.
		/// </summary>
		/// <param name="eventMessage">Event or message.</param>
		void Handle(T eventMessage);
	}
}
