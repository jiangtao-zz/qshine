using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using qshine.Configuration;

namespace qshine
{
	/// <summary>
	/// database context
	/// </summary>
	public class DbContext
	{
		const string ContextName = "db.current";

		/// <summary>
		/// Initializes a new instance of the <see cref="T:qshine.DbContext"/> class by a given database.
		/// </summary>
		/// <param name="database">Database instance.</param>
		public DbContext(Database database)
		{
			Database = database;
			Id = new Guid();
            Current = this;
        }

		/// <summary>
		/// Gets the identifier of DbContext.
		/// </summary>
		/// <value>The identifier.</value>
		public Guid Id { get; private set; }

		/// <summary>
		/// Gets the database.
		/// </summary>
		/// <value>The database.</value>
		public Database Database { get; private set; }


		/// <summary>
		/// Gets/Sets current database context
		/// Database current context is per database and per context store
		/// </summary>
		/// <value>The current.</value>
		public static DbContext Current
		{
			get
			{
				var current = ContextManager.GetData(ContextName) as DbContext;
				if (current != null)
				{
					return current;
				}

				var context = new DbContext(new Database());
				Current = context;
				return context;
			}

			set
			{
				ContextManager.SetData(ContextName, value);
			}
		}

		IDbConnection _connection = null;
		/// <summary>
		/// Gets or sets database connection.
		/// </summary>
		/// <value>The connection.</value>
		public IDbConnection Connection
		{
			get
			{
				var unitOfWorks = DbUnitOfWork.UnitOfWorks;
				//If no unit of work, return transaction directly
				if (unitOfWorks.Count == 0)
				{
					if (_connection == null)
					{
						_connection = Database.CreateConnection();
					}
					return _connection;
				}

				//If found unit of work with require new
				var requiredUnitOfWork = unitOfWorks.LastOrDefault(x => x.RequireNew);
				if (requiredUnitOfWork != null)
				{
					if (requiredUnitOfWork.Databases.Keys.Contains(Database))
					{
						return requiredUnitOfWork.Databases[Database].Connection;
					}
					var connection = Database.CreateConnection();
					connection.Open();
					var transaction = connection.BeginTransaction();
					requiredUnitOfWork.Databases.Add(Database, new DbSession
					{
						Connection = connection,
						Transaction = transaction
					});
					return connection;
				}

				//If no any requireNew, then try to find first unit of work
				var unitOfWork = unitOfWorks.FirstOrDefault(x => !x.RequireNew);
				if (unitOfWork != null)
				{
					if (unitOfWork.Databases.Keys.Contains(Database))
					{
						return unitOfWork.Databases[Database].Connection;
					}
					var connection = Database.CreateConnection();
					connection.Open();
					var transaction = connection.BeginTransaction();
					unitOfWork.Databases.Add(Database, new DbSession
					{
						Connection = connection,
						Transaction = transaction
					});
					return connection;
				}
				//this should never happen
				throw new NotImplementedException("Couldn't find UnitOfWork transaction.");
			}
			set
			{
				_connection = value;

			}
		}
		/// <summary>
		/// Gets the active connection for current database.
		/// </summary>
		/// <value>The active connection.</value>
		public IDbConnection ActiveConnection
		{
			get
			{
				if (Connection.State != ConnectionState.Open)
				{
					Connection.Open();
				}
				return Connection;
			}
		}

		public void CloseConnection()
		{
			var unitOfWorks = DbUnitOfWork.UnitOfWorks;
			//If no unit of work close the connection directly
			if (unitOfWorks.Count == 0)
			{
				if (_connection != null)
				{
					//Try to close connection
					if (_connection.State == ConnectionState.Open)
					{
						_connection.Close();
					}
					_connection.Dispose();
					_connection = null;
				}
			}
		}
	}
}
