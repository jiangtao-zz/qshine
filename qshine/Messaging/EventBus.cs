using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Messaging
{
    /// <summary>
    /// Event bus service
    /// Publish or subscrible event 
    /// </summary>
    public class EventBus
    {
        IEventBus _bus;
        /// <summary>
        /// Create a named EventBus
        /// </summary>
        /// <param name="busName"></param>
        public EventBus(string busName)
        {
            BusName = busName;
        }

        /// <summary>
        /// Event bus name
        /// </summary>
        public string BusName { get; private set; }

        /// <summary>
        /// Publish a event message through given event bus
        /// </summary>
        /// <typeparam name="T">Event message type</typeparam>
        /// <param name="eventMessage">event message to be published to all bus end points</param>
        public void Publish<T> (T eventMessage)
            where T : IEventMessage
        {
            Bus.Publish(eventMessage);
        }

        /// <summary>
        /// Subscribe the specified event message for one message handler.
        /// </summary>
        /// <typeparam name="T">Type of message event</typeparam>
        /// <param name="handler">Message event handler</param>
        public void Subscribe<T>(IEventMessageHandler<T> handler) where T : IEventMessage
        {
            //endpoint name is the handler name.
            var endpoint = handler.GetType().FullName;

            Subscribe(endpoint, handler);
        }

        /// <summary>
        /// Subscribe the specified event message for one message handler from a given endpoint.
        /// </summary>
        /// <typeparam name="T">Type of message event</typeparam>
        /// <param name="endpoint">event bus endpoint</param>
        /// <param name="handler">Message event handler</param>
        public void Subscribe<T>(string endpoint, IEventMessageHandler<T> handler) where T : IEventMessage
        {
            Bus.Subscribe(endpoint, handler);
        }

        IEventBus Bus
        {
            get
            {
                if (_bus == null)
                {
                    _bus = BusFactory.Create(BusName);
                }
                return _bus;
            }
        }

        /// <summary>
        /// Bus factory.
        /// The bus factory resolve the bus provider based on bus name.
        /// </summary>
        public IEventBusFactory BusFactory
        {
            get;set;
        }
    }
}
