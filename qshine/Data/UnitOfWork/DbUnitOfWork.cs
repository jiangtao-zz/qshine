using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.Common;

namespace qshine
{
	/// <summary>
	/// Manages each database independantly. The trasactions under same database can be merged into one trasaction as one unit of work.
	/// For multiple databases operations under same unit of work will be treated as a single unit of work. Any uncompleted transaction could cause whole unit of work rollback.
	/// 
	/// Unit of work does not work for cross threads transaction and DTC (distributed transaction coordinator).
	/// Use TrasactionScopeUnitOfWork() for DTC. 
	///  
	/// </summary>
	public class DbUnitOfWork:IUnitOfWork
	{
        [ThreadStatic]
        static int _threadCode = 0; //Thread specific value to differentiate two UoWs in same context 

        const string ContextName = "db.DbUnitOfWork";		
		bool _cancelled;
		bool _canComplete;
		/// <summary>
		/// Initializes a new instance of the <see cref="T:qshine.DbUnitOfWork"/> class.
		/// Create or merge unit of work 
		/// </summary>
		public DbUnitOfWork()
			:this(false)
		{
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="T:qshine.DbUnitOfWork"/> class .
		/// Create a new scope of unit of work
		/// </summary>
		/// <param name="requireNew">If set to <c>true</c> require new.</param>
		public DbUnitOfWork(bool requireNew)
		{
            if (_threadCode == 0)
            {
                //This value is used to reduce persistence UoW conflict in multi-threads environment
                _threadCode = this.GetHashCode();
            }
            ThreadCode = _threadCode;

            RequireNew = requireNew;
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
        /// Gets or sets a value indicating whether this <see cref="T:qshine.DbUnitOfWork"/> require a new transaction.
        /// </summary>
        /// <value><c>true</c> if require new; otherwise, <c>false</c>.</value>
        public bool RequireNew { get; private set; }

        /// <summary>
        /// Parent UoW
        /// </summary>
        internal DbUnitOfWork ParentUoW { get; set; }


        private Dictionary<Database, DbSession> Databases { get; set; }

        internal DbUnitOfWork ActiveUnitOfWork
        {
            get
            {
                var uow = this;
                while(!uow.RequireNew && uow.ParentUoW != null)
                {
                    uow = ParentUoW;
                }
                return uow;
            }
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
            var session = new DbSession(database, this);

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


        int ThreadCode;

        /// <summary>
        /// Get current unit of work
        /// </summary>
        public static DbUnitOfWork CurrentUnitOfWork
        {
            get
            {
                return ContextManager.GetData(ContextName) as DbUnitOfWork;
            }
        }

        public static void SetCurrentUnitOfWork(DbUnitOfWork uow)
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
                if (!_canComplete && Databases.Count == 0 && !RequireNew)
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
