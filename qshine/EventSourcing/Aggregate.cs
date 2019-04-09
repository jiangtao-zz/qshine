using System;
using System.Collections.Generic;
using System.Linq;
using qshine.Domain;
using qshine.Messaging;

namespace qshine.EventSourcing
{
    /// <summary>
    /// Implement abstract class of ES aggregate base class
    /// </summary>
    public abstract class Aggregate : Entity, IAggregate
    {
        private readonly List<IDomainEvent> _eventqueue = new List<IDomainEvent>();
        private int _version = -1;

        /// <summary>
        /// Get Version
        /// </summary>
        public int Version => _version;

        /// <summary>
        /// Uncommitted events
        /// </summary>
        public IEnumerable<IDomainEvent> EventQueue => _eventqueue.AsReadOnly();

        /// <summary>
        /// Returns uncommitted events in the queue and clean queued events.
        /// </summary>
        public IEnumerable<IDomainEvent> FlushEventQueue()
        {
            lock (_eventqueue)
            {
                var events = _eventqueue.ToArray();


                Check.Assert<ArgumentException>(events.Any() && !Id.Equals(default(EntityIdType)),
                    "Aggregate Id cannot be blank.");

                foreach (var @event in events)
                {
                    //if (@event.Id.Equals(default(EntityIdType)))
                    //{
                    //    @event.AggregateId = AggregateId;
                    //}
                    @event.TimeStamp = DateTimeOffset.UtcNow;
                }
                _eventqueue.Clear();
                return events;
            }
        }

        /// <summary>
        /// Raise a domain event
        /// </summary>
        /// <typeparam name="TEvent">type of domain event</typeparam>
        /// <param name="domainEvent"></param>
        public void RaiseEvent<TEvent>(TEvent domainEvent) 
            where TEvent : IDomainEvent
        {
            Apply(domainEvent);

            //Add domain event in the queue
            _eventqueue.Add(domainEvent);
        }

        /// <summary>
        /// Restore the aggregate object from history events
        /// </summary>
        /// <param name="events">events from ES.</param>
        public void RestoreFromHistoryEvents<TEvent>(IEnumerable<TEvent> events)
            where TEvent : IDomainEvent
        {
            if (events!=null && events.Any())
            {
                _version = events.Min(x => x.Version)-1;

                foreach (var e in events.ToArray())
                {
                    Check.Assert<IndexOutOfRangeException>(e.Version == Version + 1,
                        "The event is out of order. \n{0}", e.FormatObjectValues());

                    //AggregateId = e.AggregateId;
                    Apply(e);
                }
            }
        }

        private void Apply<TEvent>(TEvent @event) 
            where TEvent : IDomainEvent
        {
            Check.Assert<ArgumentNullException>(@event != null, "@event");

            //increase version number
            @event.Version = ++_version;

            //apply the domain event to the domain aggregate
            this.TryCall("Handle", @event);
            /*
            var handler = this as IHandler<TEvent>;
            Check.Assert<NotImplementedException>(handler != null, "Domain event [{0}] handler is not found.", typeof(TEvent));

            //Handler may throw business validation exception.
            handler.Handle(@event);
            */
            //((dynamic)this).Handle(@event);
        }
    }
}
