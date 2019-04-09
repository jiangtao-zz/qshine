using qshine.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Messaging
{
    /// <summary>
    /// Domain event:: An event affects the domain state changed.
    /// It also used to notify external components (BCs) when a domain state changed.
    /// 
    /// In eventsourcing system, the domain events need be stored in Event Store.
    /// In non-eventsourceing system, it stored in event log or event system.
    /// </summary>
    public interface IDomainEvent : IEventMessage
    {
        /// <summary>
        /// Domain event version number. This number is only valid when use eventsouring 
        /// The new event version always be -1. 
        /// The actual version number is a sequence number generated when the event saved in ES.
        /// 
        /// Ignore this property without ES.
        /// </summary>
        int Version { get; set; }

        /// <summary>
        /// Event timestamp in UTC
        /// </summary>
        DateTimeOffset TimeStamp { get; set; }
    }

}
