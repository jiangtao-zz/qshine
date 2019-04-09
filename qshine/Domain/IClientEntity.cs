using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Domain
{
    /// <summary>
    /// Multi-tenancy:: Client Tenant Id 
    /// </summary>
    public interface IClientEntity
    {
        /// <summary>
        /// Client Tenant id
        /// </summary>
        EntityIdType ClientId { get; set; }
    }
}
