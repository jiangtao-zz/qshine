using System;
using System.Collections.Generic;

namespace qshine
{
    /// <summary>
    /// Manages each database independantly. The trasactions under same database can be merged into one trasaction as one unit of work.
    /// For multiple databases operations, a unit of work maintains all database transactions in same UoW, one transaction per database.
    /// Any uncompleted operation could cause all transactions rollback in a unit of work.
    /// Note: multi-databases operations within single UoW may not work properly if an exception throw in UoW Dispose() moment.
    /// 
    /// The Unit of Work may not work for cross processor transaction and DTC (distributed transaction coordinator).
    /// Use TrasactionScopeUnitOfWork() for DTC if it is required.
    /// 
    /// The UoW transaction is logical context based ambiente translation. It works in multi-threads environment. 
    ///  
    /// </summary>
    public class DbUnitOfWork:IUnitOfWork
	{

        const string ContextName = "db.DbUnitOfWork";		
		bool _cancelled;
		bool _canComplete;
        /// <summary>
        /// Initializes a new instance of the <see cref="T:qshine.DbUnitOfWork"/> class.
        /// Join the ambient transaction, or create a new one if one does not exist.
        /// </summary>
        public DbUnitOfWork()
			:this(UnitOfWorkOption.Required)
		{
		}
        /// <summary>
        /// Initializes a new instance of the <see cref="T:qshine.DbUnitOfWork"/> class .
        /// Create a new scope of unit of work
        /// </summary>
        /// <param name="scopeOption">Scope of the unit of work</param>
        public DbUnitOfWork(UnitOfWorkOption scopeOption)
		{
            ScopeOption = scopeOption;
            //store individual database transaction
            Databases = new Dictionary<Database, DbSession>();
            //push this unit of work into store
            ParentUoW = CurrentUnitOfWork;
            SetCurrentUnitOfWork(this);
        }

        /// <summary>
        /// Complete the unit of work and commit the transaction.
        /// </summary>
        public void Complete()
        {
            if (!_cancelled)
            {
                _canComplete = true;
            }
        }

        /// <summary>
        /// Gets unit of work transaction scope
        /// </summary>
        internal UnitOfWorkOption ScopeOption { get; private set; }

        /// <summary>
        /// Parent UoW
        /// </summary>
        internal DbUnitOfWork ParentUoW { get; set; }


        Dictionary<Database, DbSession> Databases { get; set; }

        internal DbUnitOfWork ActiveUnitOfWork
        {
            get
            {
                var uow = this;
                while(uow.ScopeOption == UnitOfWorkOption.Required && uow.ParentUoW != null)
                {//join to parent uow
                    uow = ParentUoW;
                }
                return uow;
            }
        }

        internal static DbSession GetCurrentTransactionSession(Database database)
        {
            DbSession session = null;

            var uow = CurrentUnitOfWork;
            if (uow != null)
            {
                uow = uow.ActiveUnitOfWork;
                session = uow.GetTransactionSession(database);
            }
            return session;
        }

        internal DbSession GetTransactionSession(Database database)
        {

            if (Databases.ContainsKey(database))
            {
                return Databases[database];
            }
            return CreateTransactionSession(database);
        }

        internal DbSession CreateTransactionSession(Database database)
        {
            var session = new DbSession(database);
            if (ScopeOption == UnitOfWorkOption.Required ||
                ScopeOption == UnitOfWorkOption.RequiresNew)
            {
                session.CreateTransaction();
            }

            if (Databases.ContainsKey(database))
            {
                Databases[database] = session;
            }
            else
            {
                Databases.Add(database, session);
            }
            return session;
        }


        /// <summary>
        /// Get current unit of work
        /// </summary>
        internal static DbUnitOfWork CurrentUnitOfWork
        {
            get
            {
                return ContextManager.GetData(ContextName) as DbUnitOfWork;
            }
        }

        static void SetCurrentUnitOfWork(DbUnitOfWork uow)
        {
            ContextManager.SetData(ContextName, uow);
        }


        bool CanComplete
		{
			get
			{
				return _canComplete;
			}
		}

		/// <summary>
		/// Rollback this instance.
		/// </summary>
	    void Rollback()
		{
			_cancelled = true;
			_canComplete = false;
		}

        void RemoveUnitOfWork()
        {
            if (ParentUoW != null)
            {
                SetCurrentUnitOfWork(ParentUoW);
            }
            else
            {
                ContextManager.FreeData(ContextName);
            }
        }

		#region Dispose

		bool disposed;
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}
			if (disposing)
			{
				//if current unit of work contain active session, try to commit/rollback transaction and close the connection.
				foreach(var session in Databases.Values){
					if (session.Transaction != null)
					{
						if(_canComplete){
							session.Transaction.Commit();
						}
						else
						{
							session.Transaction.Rollback();
						}
					}
                    session.Dispose();
                }

                //if a merageable unit of work is incompleted, it should rollback the up level active unit of work
                if (!_canComplete && Databases.Count == 0 && ScopeOption == UnitOfWorkOption.Required)
				{
					var activeUnitOfWork = ActiveUnitOfWork;
					if (activeUnitOfWork != null)
					{
						activeUnitOfWork.Rollback();
					}
				}

                RemoveUnitOfWork();
			}
			disposed = true;
		}

		#endregion

	}
}
