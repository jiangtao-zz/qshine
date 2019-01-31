using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Audit
{
    /// <summary>
    /// Audit value contains a proeprty value before and after.
    /// If the property value 
    /// </summary>
    public class AuditValue
    {
        ///// <summary>
        ///// Value proeprty name.
        ///// </summary>
        //public string Name { get; set; }

        /// <summary>
        /// Original value before change.
        /// </summary>
        public object OldValue { get; set; }

        /// <summary>
        /// New value after changed.
        /// </summary>
        public object NewValue { get; set; }
    }
}
