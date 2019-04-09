using System;
using System.Collections.Generic;
using qshine.Domain;
using qshine.Messaging;

namespace qshine.EventSourcing
{
    /// <summary>
    /// Event sourcing aggregate interface
    /// </summary>
    public interface IAggregate:IEntity
    {
        /// <summary>
        /// Get the aggregate version.
        /// The aggregate version is the last version number taken from the ES or the last taken position (sequence number) from the event stream.
        /// The version number used to resolve conflicts for ES writing (concurrency)
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Keep a sequence of events raised after the aggregate is created (initialized or restored).
        /// </summary>
        /// <returns>list of queued events</returns>
        IEnumerable<IDomainEvent> EventQueue { get; }

        /// <summary>
        /// Returns uncommitted events in the queue and clean queued events.
        /// </summary>
        IEnumerable<IDomainEvent> FlushEventQueue();

        /// <summary>
        /// Raise a domain event and add the event to the queue.
        /// When a domai nevent raised, the event will be applied (through handler) to the aggregate.
        /// </summary>
        /// <param name="domainEvent">domain event</param>
        void RaiseEvent<TEvent>(TEvent domainEvent)
            where TEvent: IDomainEvent;

        /// <summary>
        /// Load history events from event store and restore the aggregate.
        /// </summary>
        /// <param name="events">History events from event store</param>
        void RestoreFromHistoryEvents<TEvent>(IEnumerable<TEvent> events)
            where TEvent : IDomainEvent;

    }
}
