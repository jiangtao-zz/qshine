using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Audit
{
    public enum AuditActionType
    {
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
