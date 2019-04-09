using System;
using System.Collections.Generic;
using System.Linq;
using qshine.Messaging;
using qshine.Configuration;
using qshine.Domain;

namespace qshine.EventSourcing
{
    /// <summary>
    /// Event store repository
    /// </summary>
    public class EventStoreRepository
    {
        IEventStore _eventStore;

        #region Ctor.
        /// <summary>
        /// Construct an event store repository by aggregate name and event store.
        /// Pick event store from application environment setting if the event store instance is not present. 
        /// </summary>
        /// <param name="aggregateName">aggregate name. It is usually the aggregate type name.</param>
        /// <param name="eventStore">event store</param>
        public EventStoreRepository(string aggregateName, IEventStore eventStore)
        {
            if (eventStore == null)
            {
                var provider = ApplicationEnvironment.GetProvider(typeof(IEventStoreProvider), aggregateName) as IEventStoreProvider;
                if (provider != null)
                {
                    eventStore = provider.Create(aggregateName);
                }
            }
            _eventStore = eventStore;

            Check.Assert<NotImplementedException>(_eventStore != null,
                "Couldn't find any event store component.");
        }

        /// <summary>
        ///  Construct an event store repository by aggregate name
        /// </summary>
        /// <param name="aggregateName">aggregate name. It is usually the aggregate type name</param>
        public EventStoreRepository(string aggregateName)
            : this(aggregateName, null)
        {
        }

        /// <summary>
        /// Construct a default event store repository through application environment setting.
        /// </summary>
        public EventStoreRepository()
            : this(string.Empty, null)
        {
        }

        #endregion

        #region Get

        /// <summary>
        /// Get an aggregate from event store by aggregate id.
        /// </summary>
        /// <param name="id">Aggregate id</param>
        /// <returns>new aggregate object or aggregate from repository.
        /// The new aggregate always have Version -1</returns>
        public virtual T Get<T>(EntityIdType id)
            where T : IAggregate, new()
        {
            var aggregate = new T();
            return (T)LoadData(id, aggregate);
        }

        #endregion

        #region Save
        /// <summary>
        /// Save an aggregate to event store and publish the event
        /// </summary>
        /// <param name="aggregate">Aggregate instance</param>
        public void Save(IAggregate aggregate)
        {
            var events = aggregate.FlushEventQueue();

            if (events != null && events.Any())
            {
                //save es to store
                _eventStore.Save(aggregate.Id, events);

                //publish events
                var eventBus = new EventBus(EventBusName);
                foreach (var e in events) {
                    eventBus.Publish(e);
                }
            }
        }

        #endregion

        #region EventBusName
        /// <summary>
        /// Get/set event bus name.
        /// The default event bus name is ebus.EventSourcing.
        /// </summary>
        protected string EventBusName
        {
            get { return _eventBusName; }
            set { _eventBusName = value; }
        }
        #endregion

        #region private
        string _eventBusName = EventBusNames.EventSourcingBusName;
        IAggregate LoadData(EntityIdType id, IAggregate aggregate)
        {
            aggregate.Id = id;

            var events = _eventStore.Load(id, -1);
            if (events.Any())
            {
                aggregate.RestoreFromHistoryEvents(events);
            }
            return aggregate;
        }
        #endregion
    }

    /// <summary>
    /// Defines an aggregate type entity store repository
    /// </summary>
    /// <typeparam name="T">The aggregate type.</typeparam>
    public class EventStoreRepository<T> : EventStoreRepository
        where T : IAggregate, new()
    {
        #region Ctor.
        /// <summary>
        /// construct default entity store repository from application environemnt setting.
        /// </summary>
        public EventStoreRepository()
            :base(typeof(T).FullName.ToLowerInvariant())
        {
        }

        /// <summary>
        /// construct an entity store repository by given event store.
        /// </summary>
        /// <param name="eventStore"></param>
        public EventStoreRepository(IEventStore eventStore)
            : base(typeof(T).FullName.ToLowerInvariant(),eventStore)
        {
        }
        #endregion

        #region Get
        /// <summary>
        /// Get an aggregate from event store by aggregate id.
        /// </summary>
        /// <param name="id">Aggregate id</param>
        /// <returns>new aggregate object or aggregate from repository.
        /// The new aggregate always have Version -1</returns>
        public T Get(EntityIdType id)
        {
            return base.Get<T>(id);
        }
        #endregion
    }

}
