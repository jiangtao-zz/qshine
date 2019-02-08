using System;
using System.Collections.Generic;
using System.Text;
using qshine.Mapper;
using qshine.Messaging;

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
        readonly T _originalEntity;
        readonly object _additionalInfo;
        /// <summary>
        /// Create entity audit service for particular entity object.
        /// </summary>
        /// <param name="entityName">A unique name to identify an entity object.
        /// The audit object name can utilize for business categorization and usage.
        /// Use AuditEntityName mapper to customize the audit entity name.
        /// </param>
        /// <param name="entity">original entity object</param>
        /// <param name="additionalInfo">additional audit information to be added into audit trail.
        /// It is a class object. All object proeprty/value pair will be audited</param>
        public EntityAudit(string entityName, T entity, object additionalInfo = null)
        {
            _entityName = entityName;
            _originalEntity = entity;
            _additionalInfo = additionalInfo;
        }
        /// <summary>
        /// Create entity audit service for particular entity object.
        /// </summary>
        /// <param name="entity">original entity object</param>
        /// <remarks>The audit object name is a object type specific name. It can utilize for business categorization and usage.
        /// Use AuditEntityName mapper to customize the audit entity name.
        /// </remarks>
        public EntityAudit(T entity, object additionalInfo)
        /// <param name="additionalInfo">additional audit information to be added in audit trail.
        /// It could be any class object.</param>
            : this(NameMapper.GetAuditEntityName(typeof(T)), entity, additionalInfo)
        {
        }

        /// <summary>
        /// Request to capture a new object audit information in audit trail
        /// </summary>
        /// <param name="entity">new auditable object</param>
        public void AuditCreate(T entity)
        {
            CreateAuditTrail(AuditActionType.Create, _originalEntity, entity);
        }

        /// <summary>
        /// Request to capture updated object information in audit trail
        /// </summary>
        /// <param name="entity">auditable object updated</param>
        public void AuditUpdate(T entity)
        {
            CreateAuditTrail(AuditActionType.Update, _originalEntity, entity);
        }

        /// <summary>
        /// Request to capture audit information for object deletion
        /// </summary>
        /// <param name="entity">deleted auditable object</param>
        public void AuditDelete(T entity)
        {
            CreateAuditTrail(AuditActionType.Delete, default(T), entity);
        }

        void CreateAuditTrail(AuditActionType action, T oldEntity, T newEntity)
        {
            //get difference of two entities
            var differ = new JsonDiffer(oldEntity, newEntity);
            var dataDiff = differ.GetDiff();

            //Create audit trail object
            var auditTrail = new AuditTrail
            {
                EntityName = _entityName,
                Id = Guid.NewGuid(),
                KeyFieldName = "Id",
                //Key =
                //Version = 
                AuditActionType = action,
                AuditActionTime = DateTime.UtcNow,
                AuditActionBy = 
                    action == AuditActionType.Create? newEntity.UpdatedBy
                    :action == AuditActionType.Update? newEntity.UpdatedBy
                    : action == AuditActionType.Delete ? newEntity.UpdatedBy
                    : "",
                //Source =
                Machine = EnvironmentEx.Machine,
                Data = dataDiff,
            };

            //add additional audit info
            if (_additionalInfo != null)
            {
                auditTrail.Addition = _additionalInfo.Serialize().DeserializeDictionary();
            }

            AuditEventBus.Publish(auditTrail);

        }

        EventBus _eventBus;
        /// <summary>
        /// Get audit event bus for audit trail message publish.
        /// </summary>
        EventBus AuditEventBus
        {
            get
            {
                if (_eventBus == null)
                {
                    _eventBus = new EventBus(EventBusNames.AuditTrailBusName);
                }
                return _eventBus;
            }

            set {_eventBus = value;}
        }
    }

}
