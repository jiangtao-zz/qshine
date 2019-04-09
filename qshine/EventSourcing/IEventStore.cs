using qshine.Domain;
using qshine.Messaging;
using System.Collections.Generic;

namespace qshine.EventSourcing
{
    /// <summary>
    /// Event store service interface
    /// </summary>
    public interface IEventStore
    {
        /// <summary>
        /// Save events to event store
        /// </summary>
        /// <param name="aggregateId">Specifies an aggregate Id associates to the events</param>
        /// <param name="events">aggregate events</param>
        void Save(EntityIdType aggregateId, IEnumerable<IDomainEvent> events);

        /// <summary>
        /// Load events for an aggregate from event store
        /// </summary>
        /// <param name="aggregateId">Specifies an aggregate Id associates to the events</param>
        /// <param name="fromVersion">All events after this should be returns.
        /// The -1 value indicates retrieve all events for the aggregate.</param>
        /// <returns>Aggregate events</returns>
        IEnumerable<IDomainEvent> Load(EntityIdType aggregateId, long fromVersion);
    }
}
