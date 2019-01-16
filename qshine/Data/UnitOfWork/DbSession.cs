using System;
using System.Data;
using System.Data.Common;
namespace qshine
{
    /// <summary>
    /// Database session
    /// </summary>
    public class DbSession:IDisposable
    {
        /// <summary>
        /// Create a database session
        /// </summary>
        /// <param name="database">database instance</param>
        /// <param name="isTransaction">indicates a transaction session.</param>
        public DbSession(Database database, bool isTransaction)
        {
            Database = database;
            Connection = Database.CreateConnection();
            Connection.Open();
            if (isTransaction)
            {
                Transaction = Connection.BeginTransaction();
            }
        }

        /// <summary>
        /// Database instance
        /// </summary>
        public Database Database { get; private set; }
        /// <summary>
        /// Database connection
        /// </summary>
        private IDbConnection Connection { get;  set; }
        /// <summary>
        /// Database transaction
        /// </summary>
        public IDbTransaction Transaction { get; private set; }

        /// <summary>
        /// Create Sql command from current session
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

        private void Close()
        {
            if (Connection.State == ConnectionState.Open)
            {
                Connection.Close();
            }
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
        private void Dispose(bool disposing)
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


        public static DbSession GetCurrentSession(Database database)
        {
            DbSession session;

            var uow = DbUnitOfWork.CurrentUnitOfWork;
            if (uow == null)
            {
                return null;
            }

            session = uow.GetTransactionSession(database);
            if (session == null)
            {
                session = new DbSession(database,true);
                uow.SetTransactionSession(database, session);
            }
            return session;
        }

        /// <summary>
        /// Gets the active connection for current database.
        /// </summary>
        /// <value>The active connection.</value>
        private IDbConnection ActiveConnection
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
