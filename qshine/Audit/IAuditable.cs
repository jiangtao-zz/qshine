using System;

namespace qshine.Audit
{
    /// <summary>
    /// Auditable entity interface
    /// </summary>
    public interface IAuditable
    {
        /// <summary>
        /// When the entity created
        /// </summary>
        DateTime CreatedOn { get; set; }
        /// <summary>
        /// Who created entity
        /// </summary>
        string CreatedBy { get; set; }
        /// <summary>
        /// When the entity updated
        /// </summary>
        DateTime UpdatedOn { get; set; }
        /// <summary>
        /// Who updated the entity
        /// </summary>
        string UpdatedBy { get; set; }
    }
}