using qshine.Audit;
using System;

namespace qshine.Domain
{
    /// <summary>
    /// Base auditable entity class.
    /// The auditable entity capture audit information of the entity.
    /// </summary>
    public abstract class AuditableEntity: Entity, IAuditable
    {
        /// <summary>
        /// When the entity created
        /// </summary>
        public DateTime CreatedOn { get; set; }
        /// <summary>
        /// Who created entity
        /// </summary>
        public string CreatedBy { get; set; }
        /// <summary>
        /// When the entity updated
        /// </summary>
        public DateTime UpdatedOn { get; set; }
        /// <summary>
        /// Who updated the entity
        /// </summary>
        public string UpdatedBy { get; set; }
    }
}
