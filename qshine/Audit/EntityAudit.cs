using System;
using System.Collections.Generic;
using System.Text;
using qshine.Mapper;

namespace qshine.Audit
{
    /// <summary>
    /// Entity audit trail service.
    /// Usuage:
    /// 
    ///  var people = peopleRepository.Load("john");
    ///  var auditTrail = new EntityAudit(people);
    ///  people.Some = "changed";
    ///  peopleRepository.Save(people);
    ///  auditTrail.AuditUpdate(people);
    ///  
    /// </summary>
    public class EntityAudit<T>
        where T: IAuditable
    {
        readonly string _entityName;
        readonly string _originalEntity;
        string _updatedEntity;
        AuditActionType _auditAction;
        AuditTrail _auditTrail;
        /// <summary>
        /// Create entity audit service for particular entity object.
        /// </summary>
        /// <param name="entityName">A unique name to identify an entity object.
        /// The audit object name can utilize for business categorization and usage.
        /// Use AuditEntityName mapper to customize the audit entity name.
        /// </param>
        /// <param name="entity">original entity object</param>
        public EntityAudit(string entityName, T entity)
        {
            _entityName = entityName;
            if (entity != null)
            {
                _originalEntity = entity.Serialize();
            }
            _auditAction = AuditActionType.Unknow;
        }
        /// <summary>
        /// Create entity audit service for particular entity object.
        /// </summary>
        /// <param name="entity">original entity object</param>
        /// <remarks>The audit object name is a object type specific name. It can utilize for business categorization and usage.
        /// Use AuditEntityName mapper to customize the audit entity name.
        /// </remarks>
        public EntityAudit(T entity)
            :this(NameMapper.GetAuditEntityName(typeof(T)), entity)
        {
        }

        /// <summary>
        /// Request to capture a new object audit information in audit trail
        /// </summary>
        /// <param name="entity">new auditable object</param>
        public void AuditCreate(T entity)
        {
            _auditAction = AuditActionType.Create;
            _updatedEntity = entity.Serialize();

        }

        /// <summary>
        /// Request to capture updated object information in audit trail
        /// </summary>
        /// <param name="entity">auditable object updated</param>
        public void AuditUpdate(T entity)
        {
            var differ = new JsonDiffer(_originalEntity, entity);

            _auditTrail = new AuditTrail
            {
                EntityName = _entityName,
                Id = Guid.NewGuid(),
                KeyFieldName = "Id",
                //Key =
                //Version = 
                AuditActionType = AuditActionType.Update,
                AuditActionTime = DateTime.UtcNow,
                AuditActionBy = entity.UpdatedBy,
                //Source =
                Machine = EnvironmentEx.Machine,
                Data = differ.GetDiff(),
            };
        }

        /// <summary>
        /// Request to capture audit information for object deletion
        /// </summary>
        /// <param name="entity">deleted auditable object</param>
        public void AuditDelete(T entity)
        {
            _auditTrail = new AuditTrail
            {
                EntityName = _entityName,
                Id = Guid.NewGuid(),
                KeyFieldName = "Id",
                //Key =
                //Version = 
                AuditActionType = AuditActionType.Delete,
                AuditActionTime = DateTime.UtcNow,
                AuditActionBy = entity.UpdatedBy,
                //Source =
                Machine = EnvironmentEx.Machine
            };
        }
    }

}
