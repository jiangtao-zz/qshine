using System;
namespace qshine
{
    /// <summary>
    /// Event bus interface. (publish/subscribe pattern)
    /// Event bus is used to deliever a message to zero or many event subscribers. Each endpoint can process the message in different way without depend each other.
    /// 1. eventBus = new EventBus(busName)
    ///     Create a named event bus. each named event bus could have different implementation, but the end result is same.  
    ///     
    /// 2. eventBus.Publish(event)
    /// 	A event message published through the event bus. The event message will be delivered to all endpoints.
    /// 	Exception will be thrown if the message delivery is failed.
    /// 
    /// 2. eventBus.PublishAsync(event, callback)
    /// 	A event message published through the event bus in asynchronous way. 
    /// 	Call the callback method when event delivered.
    /// 
    /// 3. eventBus.Subscriber(string endpoint);
    /// 	Subscriber a specified event from one endpoint.
    /// 	
    /// </summary>
    public interface IEventBus
	{
		/// <summary>
		/// Publish the specified event message to event queue and deliever the event message to each event subscriber.
		/// </summary>
		/// <returns>The publish.</returns>
		/// <param name="eventMessage">Event message.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		void Publish<T>(T eventMessage) where T : IEventMessage;

        /// <summary>
        /// Subscribe the specified event message for one message handler in a given endpoint.
        /// </summary>
        /// <param name="endpoint">Event bus endpoint. The event message will be publish to all endpoints</param>
        /// <param name="handler">event message handler.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>

        void Subscribe<T>(string endpoint, IEventMessageHandler<T> handler) where T : IEventMessage;

		/// <summary>
		/// Unsubscribe the specified handler.
		/// </summary>
		/// <returns>The unsubscribe.</returns>
		/// <param name="handler">Handler.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		void Unsubscribe(string endpoint);

	}
}
