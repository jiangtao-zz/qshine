using System;
namespace qshine
{
	/// <summary>
	/// Command bus interface:
	/// Command bus is used to send a command request to command handler. The command message has only a single endpoint.
	/// 1. commandBus.Send()
	/// 	Send command message synchronously and wait command process complete.
	/// 	Synchronous messaging is a bi-direction communication. The sender will receive the result immediately. 
	/// 	An exception may throw when command failed to process.
	/// 
	/// 2. commandBus.SendAsync()
	/// 	Send command asynchronously and return immedidately without waiting command handler completely.
	/// 	It is a one way communication.
	/// 
	/// </summary>
	public interface ICommandBus:IBus
	{
		/// <summary>
		/// Send a specified command to the queue and waiting for message process completely.
		/// </summary>
		/// <returns>The send.</returns>
		/// <param name="command">Command.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		void Send<T>(T command) where T : ICommandMessage;

		/// <summary>
		/// Send a specified command to the queue and do not waiting message process.
		/// </summary>
		/// <param name="command">Command.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		void SendAsync<T>(T command) where T : ICommandMessage;
	}
}
