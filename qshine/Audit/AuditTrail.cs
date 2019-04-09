using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Audit
{
    /// <summary>
    /// Audit trail contains entity object auditing information.
    /// </summary>
    public class AuditTrail: IEventMessage
    {
        /// <summary>
        /// Auditing entity name.
        /// It is used to classify a particular entity class.
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Audit trail unique ID.
        /// It is audit trail storage record Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Entity object key (Id) proeprty value.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Audit action type enum value.
        ///     Create - Create a new entity object
        ///     Update - Update an entity object
        ///     Delete - Delete an entity object
        ///     
        /// It indicates the action of data change.
        /// </summary>
        public AuditActionType AuditActionType { get; set; }

        /// <summary>
        /// When perform the action.
        /// It should always be UCT time
        /// </summary>
        public DateTime AuditActionTime { get; set; }

        /// <summary>
        /// Who perform the action.
        /// </summary>
        public string AuditActionBy { get; set; }

        /// <summary>
        /// Name of the computer on which to action.
        /// </summary>
        public string Machine { get; set; }

        /// <summary>
        /// Data values in JSON format.
        /// It contains new/old value pair for all value modified properties and values.
        /// The modified daa could be new proeprty, modified property or deleted proeprty.
        /// </summary>
        public Dictionary<string,AuditValue> Data { get; set; }

        /// <summary>
        /// A collect of proeprty/value pair as addition audit information.
        /// It could be null
        /// </summary>
        public Dictionary<string, object> Addition { get; set; }

    }
}
