using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.Common;

namespace qshine
{
	/// <summary>
	/// A simple database transaction unit of work.
	/// It manages each database independantly. The trasactions under same database can be merged into one trasaction as one unit of work.
	/// For multiple databases operations under same unit of work will be treated as a single unit of work. Any uncompleted transaction could cause whole unit of work rollback.
	/// 
	/// Unit of work does not work for cross threads transaction and DTC (distributed transaction coordinator).
	/// Use TrasactionScopeUnitOfWork() for DTC. 
	///  
	/// </summary>
	public class DbUnitOfWork:IUnitOfWork
	{
		const string ContextName = "db.UnitOfWork";		
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
			RequireNew = requireNew;
			//store individual database transaction
			//Databases = new Dictionary<Database, DbSession>();
			//push this unit of work into store
			UnitOfWorks.Push(this);
		}
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:qshine.DbContext"/> under unit of work.
		/// </summary>
		/// <value><c>true</c> if under unit of work; otherwise, <c>false</c>.</value>
		internal static Stack<DbUnitOfWork> UnitOfWorks
		{
			get
			{
				var unitOfWorkChains = ContextManager.GetData(ContextName) as Stack<DbUnitOfWork>;
				if (unitOfWorkChains == null)
				{
					unitOfWorkChains = new Stack<DbUnitOfWork>();
					ContextManager.SetData(ContextName, unitOfWorkChains);
				}
				return unitOfWorkChains;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:qshine.DbUnitOfWork"/> require a new transaction.
		/// </summary>
		/// <value><c>true</c> if require new; otherwise, <c>false</c>.</value>
		public bool RequireNew { get; set; }

		internal bool CanComplete
		{
			get
			{
				return _canComplete;
			}
		}

		public Dictionary<Database, DbSession> Databases { get; private set; }

		/// <summary>
		/// Tru to complete unit of work.
		/// </summary>
		public void Complete()
		{
			if (!_cancelled)
			{
				_canComplete = true;
			}
		}

		/// <summary>
		/// Rollback this instance.
		/// </summary>
		internal void Rollback()
		{
			_cancelled = true;
			_canComplete = false;
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
				//if has transaction, close connection and transactions
				foreach(var session in Databases.Values){
					if (session.Connection != null)
					{
						if(_canComplete){
							session.Transaction.Commit();
						}
						else
						{
							session.Transaction.Rollback();
						}
						session.Transaction.Dispose();
						//Try to close connection
						if (session.Connection.State == ConnectionState.Open)
						{
							session.Connection.Close();
						}
						session.Connection.Dispose();
						session.Connection = null;
					}
				}
				//if no transaction, and unit of work is not complete and not requirNew, cancel available transaction
				if (!_canComplete && Databases.Count == 0 && !RequireNew)
				{
					var activeUnitOfWork = UnitOfWorks.LastOrDefault(x => x.Databases.Count > 0);
					if (activeUnitOfWork != null)
					{
						activeUnitOfWork.Rollback();
					}
				}
				if (UnitOfWorks.Count >0)
				{
					var unitOfWork = UnitOfWorks.Pop();
					if (unitOfWork != this)
					{
						throw new InvalidOperationException("Cannot complete the unit of work with an active child unit of work.");
					}
				}
			}
			disposed = true;
		}

		#endregion

	}
}
