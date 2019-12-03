using qshine.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Messaging
{
    /// <summary>
    /// Event bus service
    /// Publish or subscrible events.
    /// 
    /// Event bus could be configured in application environment based on bus name (route name).
    /// The bus could be produced from different bus factory by bus name.
    /// 
    /// The common usuage::
    ///     Publish event: 
    ///         var bus = new EventBus("busName");
    ///         bus.Publish(myEvent);
    ///     
    ///     Subscrible events:
    ///         var bus = new EventBus("busName");
    ///         bus.Subscribe("endpointname", eventHandler);
    ///     or
    ///         bus.Subscribe(eventhandler);
    ///     
    /// 
    /// If no any plugable bus factory available, a default bus factory will be selected for event bus.
    /// The default one is a memory event bus for test purpose.
    /// 
    /// </summary>
    public class EventBus
    {
        static Interceptor _interceptor = Interceptor.Get<EventBus>();

        IEventBus _bus;
        IEventBusFactory _busFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:qshine.EventBus"/> class using default bus parameter.
        /// </summary>
        public EventBus()
            :this(EventBusNames.DefaultEventBusName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:qshine.EventBus"/> class for specific named bus.
        /// </summary>
        /// <param name="busName">Bus name</param>
        public EventBus(string busName)
            :this(busName, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:qshine.EventBus"/> class by a given bus name and bus factory.
        /// The bus name could be used to specify a type of bus factory and a separated bus route.
        /// Application may utilize different type of bus factory for bus route.
        /// </summary>
        /// <param name="busName">Bus name.</param>
        /// <param name="factory">event bus factory</param>
        public EventBus(string busName, IEventBusFactory factory)
        {
            if (string.IsNullOrEmpty(busName)) busName = EventBusNames.DefaultEventBusName;

            BusName = busName;

            _busFactory = factory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:qshine.EventBus"/> for given bus instance.
        /// </summary>
        /// <param name="bus">bus instance for particular route.</param>
        public EventBus(IEventBus bus)
        {
            _bus = bus;
        }

        /// <summary>
        /// Publish a event message through given event bus
        /// </summary>
        /// <typeparam name="T">Event message type</typeparam>
        /// <param name="eventMessage">event message to be published to all bus end points</param>
        public void Publish<T> (T eventMessage)
            where T : IEventMessage
        {
            int method()
            {
                var bus = Bus;
                if (bus != null)
                {
                    bus.Publish(eventMessage);
                }
                return 0;
            }

            _interceptor.JoinPoint(method, this, "Publish", Bus, eventMessage);
        }

        /// <summary>
        /// Subscribe the specified event message for one message handler.
        /// </summary>
        /// <typeparam name="T">Type of message event</typeparam>
        /// <param name="handler">Message event handler</param>
        public void Subscribe<T>(IHandler<T> handler) where T : IEventMessage
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
        public void Subscribe<T>(string endpoint, IHandler<T> handler) where T : IEventMessage
        {
            var bus = Bus;

            if (bus != null)
                bus.Subscribe(endpoint, handler);
        }

        string BusName { get; set; }

        IEventBus Bus
        {
            get
            {
                if (_bus == null)
                {
                    if(_busFactory ==null)
                    {
                        _busFactory = GetBusFactory(BusName);
                    }
                    if (_busFactory != null)
                    {
                        _bus = _busFactory.Create(BusName);
                    }
                }
                return _bus;
            }
        }

        /// <summary>
        /// Buffer all bus factories.
        /// </summary>
        static Dictionary<string, IEventBusFactory> _factories = new Dictionary<string, IEventBusFactory>();
        static readonly object lockobj = new object();
        static readonly Interceptor _intercepter = Interceptor.Get<EventBus>();

        /// <summary>
        /// Bus factory resolver.
        /// Find a bus factory based on bus name.
        /// You can configure different bus factory for certain bus name.
        /// </summary>
        IEventBusFactory GetBusFactory(string name)
        {
            Check.HaveValue(name, "GetBusFactory(name)");

            if(_busFactory==null)
            {
                if (!_factories.ContainsKey(name))
                {
                    var _busFactory = ApplicationEnvironment.Default.Services.GetProvider<IEventBusFactory>(name);
                    if (_busFactory != null)
                    {
                        lock (lockobj)
                        {
                            if (!_factories.ContainsKey(name))
                            {
                                _factories[name] = _busFactory;
                            }
                        }
                    }
                    else
                    {
                        //No bus factory found
                    }
                }
                else
                {
                    _busFactory = _factories[name];
                }
            }
            return _busFactory;
        }
    }
}
