using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Messaging
{
    /// <summary>
    /// Common event bus name
    /// </summary>
    public class EventBusNames
    {
        /// <summary>
        /// Default event bus name
        /// </summary>
        public const string DefaultEventBusName = "ebus.Default";
        /// <summary>
        /// Audit trail event bus
        /// </summary>
        public const string AuditTrailBusName = "ebus.AuditTrail";

        /// <summary>
        /// Event sourcing event bus name
        /// </summary>
        public const string EventSourcingBusName = "ebus.EventSourcing";
    }
}
