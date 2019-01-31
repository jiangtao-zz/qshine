using System;
using System.Data;
using System.Data.Common;
namespace qshine
{
    /// <summary>
    /// Database session
    /// </summary>
    public sealed class DbSession : IDisposable
    {
        /// <summary>
        /// Create a database session.
        /// A session is a database connection and uow transaction.
        /// </summary>
        /// <param name="database">database instance</param>
        public DbSession(Database database)
        {
            Database = database;
            Connection = Database.CreateConnection();
            Connection.Open();
        }

        /// <summary>
        /// Database instance
        /// </summary>
        public Database Database { get; private set; }
        /// <summary>
        /// Database connection
        /// </summary>
        public IDbConnection Connection { get; set; }
        /// <summary>
        /// Database transaction
        /// </summary>
        public IDbTransaction Transaction { get; private set; }

        //public IUnitOfWork UnitOfWork { get; private set; }

        /// <summary>
        /// Create a Sql command
        /// </summary>
        /// <returns></returns>
        public IDbCommand CreateCommand()
        {
            var cmd = ActiveConnection.CreateCommand();
            if (Transaction != null)
            {
                cmd.Transaction = Transaction;
            }
            return cmd;
        }

        /// <summary>
        /// Create a transaction
        /// </summary>
        /// <returns></returns>
        public IDbTransaction CreateTransaction()
        {
            Transaction = Connection.BeginTransaction();
            return Transaction;
        }

        #region Implementation of IDsiposable interface
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //The transaction need be managed outside of session
                    if (Transaction != null)
                    {
                        Transaction.Dispose();
                    }
                    Close();
                    Connection.Dispose();
                    _disposed = true;
                }
            }
        }

        #endregion

        /// <summary>
        /// Get current transaction session if exists
        /// </summary>
        /// <param name="database">database instance</param>
        /// <returns>return a uow transaction session</returns>
        internal static DbSession GetCurrentTransactionSession(Database database)
        {
            return DbUnitOfWork.GetCurrentTransactionSession(database);
        }


        void Close()
        {
            if (Connection.State == ConnectionState.Open)
            {
                Connection.Close();
            }
        }
        /// <summary>
        /// Gets the active connection for current database.
        /// </summary>
        /// <value>The active connection.</value>
        IDbConnection ActiveConnection
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
    }
}
