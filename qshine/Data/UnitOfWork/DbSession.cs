using System;
using System.Data;
using System.Data.Common;
namespace qshine
{
    /// <summary>
    /// Database session
    /// </summary>
    public class DbSession : IDisposable
    {
        /// <summary>
        /// Create a database session
        /// </summary>
        /// <param name="database">database instance</param>
        /// <param name="isTransaction">indicates a transaction session.</param>
        public DbSession(Database database, DbUnitOfWork uow)
        {
            Database = database;
            UnitOfWork = uow;
            Connection = Database.CreateConnection();
            Connection.Open();
            if (uow != null)
            {
                Transaction = Connection.BeginTransaction(uow.RequireNew? IsolationLevel.ReadCommitted: IsolationLevel.Unspecified);
            }
        }

        /// <summary>
        /// Database instance
        /// </summary>
        public Database Database { get; private set; }
        /// <summary>
        /// Database connection
        /// </summary>
        private IDbConnection Connection { get; set; }
        /// <summary>
        /// Database transaction
        /// </summary>
        public IDbTransaction Transaction { get; private set; }

        public DbUnitOfWork UnitOfWork { get; private set; }

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

        public static DbSession GetCurrentSession(Database database, bool ignoreUoW)
        {
            DbSession session;

            var uow = DbUnitOfWork.CurrentUnitOfWork;
            if (uow == null)
            {
                return new DbSession(database, null);
            }
            uow = uow.ActiveUnitOfWork;
            session = uow.GetTransactionSession(database);
            //if (session == null)
            //{
            //    session = new DbSession(database, uow);
            //    uow.SetTransactionSession(database, session);
            //}
            return session;
        }

    }
}
