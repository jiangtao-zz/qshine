﻿using System;
namespace qshine
{
	/// <summary>
	/// Event bus interface. (publish/subscribe pattern)
	/// Event bus is used to deliever a message to zero or many event subscribers. Each endpoint can process the message in different way without depend each other.
	/// 1. eventBus.Publish()
	/// 	A event message published through the event bus.
	/// 
	/// 2. eventBus.Subscriber(new MyEventHandler());
	/// 	Subscriber a specified event.
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
		/// Subscribe the specified event message for one message handler.
		/// </summary>
		/// <returns>The subscriber of one type of event message.</returns>
		/// <param name="handler">Handler.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>

		void Subscribe<T>(IEventMessageHandler<T> handler) where T : IEventMessage;

		/// <summary>
		/// Unsubscribe the specified handler.
		/// </summary>
		/// <returns>The unsubscribe.</returns>
		/// <param name="handler">Handler.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		void Unsubscribe<T>(IEventMessageHandler<T> handler) where T : IEventMessage;

	}
}