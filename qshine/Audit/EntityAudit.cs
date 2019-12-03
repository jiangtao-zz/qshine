using System;
using System.Collections.Generic;
using System.Text;
using qshine.Domain;
using qshine.Utility;
using qshine.Messaging;

namespace qshine.Audit
{
    /// <summary>
    /// Entity audit trail service.
    /// Usuage:
    /// 
    ///  var people = peopleRepository.Load("john");
    ///  var auditAddition = ContextManager.Current.GetData("auditParameters");
    ///  var auditTrail = new EntityAudit(people, auditAddition);
    ///  
    ///  people.Some = "changed";
    ///  peopleRepository.Save(people);
    ///  
    ///  auditTrail.AuditUpdate(people);
    ///  
    /// </summary>
    public class EntityAudit<T>
        where T: IAuditable
    {
        static string MaperName = CommonMapperName.AuditEntityNameMapper.ToString();
        static EntityAudit()
        {
            //register default common name mapper
            NamedMapper.Register(MaperName
                , (t) =>
                {
                    Type type = t as Type;
                    if (t == null) return "unknowAuditEntityName";
                    return type.FullName;
                });
        }
        #region Ctor.
        /// <summary>
        /// Create entity audit service for particular entity object.
        /// </summary>
        /// <param name="entityName">A unique name to identify an entity object.
        /// The audit object name can utilize for business categorization and usage.
        /// Use AuditEntityName mapper to customize the audit entity name.
        /// </param>
        /// <param name="entity">original entity object</param>
        /// <param name="additionalInfo">additional audit information to be added into audit trail.
        /// It is a class object. All object proeprty/value pair will be audited. 
        /// The additional information could be the request source (agent), url, machine/ip, application environment parameters and any others. 
        /// </param>
        public EntityAudit(string entityName, T entity, object additionalInfo = null)
        {
            _entityName = entityName;
            _originalEntity = entity.Serialize();
            _additionalInfo = additionalInfo;
            _id = entity.Id;
        }
        /// <summary>
        /// Create entity audit service for particular entity object.
        /// </summary>
        /// <param name="entity">original entity object</param>
        /// <param name="additionalInfo">additional information push to audit object for logging</param>
        /// <remarks>The audit object name is a object type specific name. It can utilize for business categorization and usage.
        /// Use AuditEntityName mapper to customize the audit entity name.
        /// </remarks>
        public EntityAudit(T entity, object additionalInfo=null)
            : this(
                  NamedMapper.MapTo(MaperName, typeof(T)).ToString()
                  , entity, additionalInfo
                  )
        {
        }
        #endregion

        /// <summary>
        /// Request to capture a new object audit information in audit trail
        /// </summary>
        /// <param name="entity">new auditable object</param>
        public void AuditEntityCreate(T entity)
        {
            LogAuditTrail(AuditActionType.Create, _originalEntity, entity);
        }

        /// <summary>
        /// Request to capture updated object information in audit trail
        /// </summary>
        /// <param name="entity">auditable object updated</param>
        public void AuditEntityUpdate(T entity)
        {
            LogAuditTrail(AuditActionType.Update, _originalEntity, entity);
        }

        /// <summary>
        /// Request to capture audit information for object deletion
        /// </summary>
        /// <param name="entity">deleted auditable object</param>
        public void AuditEntityDelete(T entity)
        {
            LogAuditTrail(AuditActionType.Delete, _originalEntity, entity);
        }

        #region protected EventBusName
        /// <summary>
        /// Get/set event bus name.
        /// The default event bus name is ebus.AuditTrail.
        /// </summary>
        protected string EventBusName
        {
            get { return _eventBusName; }
            set { _eventBusName = value; }
        }
        string _eventBusName = EventBusNames.AuditTrailBusName;

        #endregion

        #region protected LogAuditTrail
        /// <summary>
        /// Log audit entity audit trail.
        /// It will get the difference of original and updated entity values 
        /// and send the difference to audit trail logger through an event bus.
        /// </summary>
        /// <param name="action">Action of the entity.
        /// AuditActionType.Create: Create a new entity. The oldEntity could be null.
        /// AuditActionType.Update: Update an entity. The oldEntity is original one and newEntity is updated one.
        /// AuditActionType.Delete: Delete an entity. The oldEntity could be null, the newEntity is the entity to be deleted.
        /// </param>
        /// <param name="oldEntity">original entity. it is null when action is Create.</param>
        /// <param name="newEntity">new updated entity.</param>
        /// <remarks>
        /// The application can pass the addition audit information associate to the entity.
        /// </remarks>
        protected void LogAuditTrail(AuditActionType action, string oldEntity, T newEntity)
        {
            //get difference of two entities
            var differ = new JsonDiffer(oldEntity, newEntity.Serialize());
            var dataDiff = differ.GetDiff();

            string entityId = _id.ToString();

            //Create audit trail object
            var auditTrail = new AuditTrail
            {
                EntityName = _entityName,
                Id = Guid.NewGuid(),//audit record id
                Key = entityId,
                AuditActionType = action,
                AuditActionTime = DateTime.UtcNow,
                AuditActionBy = 
                    action == AuditActionType.Create? newEntity.CreatedBy
                    :newEntity.UpdatedBy,
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
        #endregion

        #region private
        readonly string _entityName;
        readonly string _originalEntity;
        readonly object _additionalInfo;
        readonly EntityIdType _id;
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
                    _eventBus = new EventBus(EventBusName);
                }
                return _eventBus;
            }

            set {_eventBus = value;}
        }
        #endregion
    }

}
