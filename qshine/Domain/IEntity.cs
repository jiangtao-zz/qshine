using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Domain
{
    /// <summary>
    /// Entity interface.
    /// (Usually a domain root entity with a global Id
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Get/set entity id, hide IEntity.Id.
        /// </summary>
        EntityIdType Id { get; set; }
    }

}
