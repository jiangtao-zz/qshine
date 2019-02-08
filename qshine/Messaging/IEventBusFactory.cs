using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Messaging
{
    public interface IEventBusFactory
    {
        /// <summary>
        /// Create an event bus
        /// </summary>
        /// <param name="busName">event bus name</param>
        /// <returns>return a named event bus.
        /// </returns>
        IEventBus Create(string busName);
    }
}
