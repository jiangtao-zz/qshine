using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Domain
{
    /// <summary>
    /// Base entity class
    /// </summary>
    public abstract class Entity : IEntity
    {
        /// <summary>
        /// Global identity of the entity
        /// </summary>
        public EntityIdType Id
        {
            get; set;
        }
    }
}
