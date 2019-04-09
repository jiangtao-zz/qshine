using qshine.Domain;
using qshine.EventSourcing;
using qshine.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using EventStore.ClientAPI;
using qshine.Configuration;
using System.Text;

namespace qshine.EventStore
{
    /// <summary>
    /// Implement eventstore.org as event store provider.
    /// The provider is tareget to a specific 3rd-party component (it's eventstore.org in this implementation).
    /// The provider requires below parameters for provider instance construction:
    /// 2. connectionstring: specifies store connection. 
    /// </summary>
	public class Provider : IEventStoreProvider
    {
        readonly string _connectionStringName;
        /// <summary>
        /// Initialize provider instance from a given connectionstring name.
        /// The connectionstring can be defined in application environment connectionString collection.
        /// </summary>
        /// <param name="connectionStringName">The name of the connectionstring.</param>
        public Provider(string connectionStringName)
        {
            _connectionStringName = connectionStringName;
        }

        /// <summary>
        /// Create a event store instance for given aggregate
        /// </summary>
        /// <param name="aggregateName">aggregate name. It is usually the name of the aggregate.
        /// The aggregate name can be used to select mapped event store provider.
        /// </param>
        /// <returns>event store instance</returns>
        public IEventStore Create(string aggregateName)
        {
            return new AggregateEventStore(aggregateName, _connectionStringName);
        }
    }

    /// <summary>
    /// IStore instance
    /// </summary>
    public class AggregateEventStore : IEventStore
    {
        readonly string _connectionString;
        readonly string _aggregateName;
        /// <summary>
        /// Instanciates event store for a specific aggregate.
        /// </summary>
        /// <param name="aggregateName">aggregate name</param>
        /// <param name="connectionStringName">connection string for the event store</param>
        public AggregateEventStore(string aggregateName, string connectionStringName)
        {
            var connection = ApplicationEnvironment.Current.ConnectionStrings[connectionStringName];
            Check.Assert<ArgumentNullException>(connection != null, "Couldn't find name [{0}] EventStore connection string.", connectionStringName);

            _connectionString = connection.ConnectionString;
            Check.HaveValue(_connectionString, "connectionString");

            _aggregateName = aggregateName;
            Check.HaveValue(_aggregateName, "aggregateName");
        }


        /// <summary>
        /// Load aggregate events from store
        /// </summary>
        /// <param name="aggregateId">aggregate id</param>
        /// <param name="fromVersion">start version of the aggregate stream</param>
        /// <returns></returns>
        public IEnumerable<IDomainEvent> Load(EntityIdType aggregateId, long fromVersion)
        {
            const int EventStorePageSize = 200;
            string streamName = GetStreamName(_aggregateName, aggregateId.ToString());
            using (var connect = EventStoreConnection.Create(_connectionString))
            {
                connect.ConnectAsync();
                var events = new List<IDomainEvent>();
                var streamEvents = new List<ResolvedEvent>();
                StreamEventsSlice currentSlice;
                var nextSliceStart = fromVersion < 0 ? StreamPosition.Start : fromVersion;

                //Read the stream using pagesize which was set before.
                //We only need to read the full page ahead if expected results are larger than the page size
                int count = int.MaxValue;

                do
                {
                    int nextReadCount = count - streamEvents.Count();

                    if (nextReadCount > EventStorePageSize)
                    {
                        nextReadCount = EventStorePageSize;
                    }

                    currentSlice = connect.ReadStreamEventsForwardAsync(
                        streamName, nextSliceStart, nextReadCount, false).Result;

                    nextSliceStart = currentSlice.NextEventNumber;

                    streamEvents.AddRange(currentSlice.Events);

                } while (!currentSlice.IsEndOfStream);

                //Deserialize and add to events list
                foreach (var returnedEvent in streamEvents)
                {
                    var metaData = Encoding.UTF8.GetString(returnedEvent.Event.Metadata);
                    var type = Type.GetType(metaData);
                    var e = Encoding.UTF8.GetString(returnedEvent.Event.Data).Deserialize(type) as IDomainEvent;
                    e.Version = (int)returnedEvent.Event.EventNumber;
                    events.Add(e);
                }

                return events;
            }
        }

        /// <summary>
        /// Save aggregate events into store
        /// </summary>
        /// <param name="events"></param>
        public void Save(EntityIdType aggregateId, IEnumerable<IDomainEvent> events)
        {
            var firstEvent = events.FirstOrDefault();
            if (firstEvent == null) return;

            string streamName = GetStreamName(_aggregateName, aggregateId.ToString());

            using (var connect = EventStoreConnection.Create(_connectionString))
            {
                connect.ConnectAsync();

                var lastVersion = firstEvent.Version-1;
                List<EventData> lstEventData = new List<EventData>();

                foreach (var @event in events)
                {
                    if (@event.Id == Guid.Empty)
                    {
                        @event.Id = Guid.NewGuid();
                    }

                    lstEventData.Add(
                        new EventData(@event.Id, @event.GetType().Name, true,
                        Encoding.UTF8.GetBytes(@event.Serialize()),
                        Encoding.UTF8.GetBytes(GetClrTypeName(@event))));
                }

                connect.AppendToStreamAsync(streamName, lastVersion <= 0 ? ExpectedVersion.Any : lastVersion, lstEventData).Wait();
            }
        }

        private static string GetClrTypeName(object @event)
        {
            return @event.GetType().ToString() + "," + @event.GetType().Assembly.GetName().Name;
        }

        private string GetStreamName(string name, string aggregateId)
        {
            return string.Format("{0}-{1}",name, aggregateId);
        }
    }
}
