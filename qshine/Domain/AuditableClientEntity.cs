using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Domain
{
    /// <summary>
    /// Common auditable multi-tenancy entity base class
    /// </summary>
    public abstract class AuditableClientEntity:AuditableEntity,IClientEntity
    {
        /// <summary>
        /// Client Tenant Id
        /// </summary>
        public EntityIdType ClientId { get; set; }
    }
}
