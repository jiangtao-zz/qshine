using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Audit
{
    /// <summary>
    /// Audit action type
    /// </summary>
    public enum AuditActionType :int
    {
        /// <summary>
        /// Unknow action
        /// </summary>
        Unknow,
        /// <summary>
        /// Indicates audit object created
        /// </summary>
        Create,
        /// <summary>
        /// Indicates audit object updated
        /// </summary>
        Update,
        /// <summary>
        /// Indicates audit object deleted
        /// </summary>
        Delete
    }

}
