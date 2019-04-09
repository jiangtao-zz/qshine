using System.Transactions;
namespace qshine
{
    /// <summary>
    /// Transaction scope UoW provider
    /// </summary>
    public class TransactionScopeUnitOfWorkProvider : IUnitOfWorkProvider
    {
        /// <summary>
        /// Create unit of work instance
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public IUnitOfWork Create(UnitOfWorkOption option)
        {
            return new TransactionScopeUnitOfWork(option);
        }
    }

    /// <summary>
    /// Transaction scope Unit of Work
    /// </summary>
    public class TransactionScopeUnitOfWork:IUnitOfWork
	{
		TransactionScope _scope;

        /// <summary>
        /// Ctor::
        /// </summary>
        /// <param name="option"></param>
		public TransactionScopeUnitOfWork(UnitOfWorkOption option)
		{
            _scope = new TransactionScope(
                option== UnitOfWorkOption.Required? TransactionScopeOption.Required
                :option== UnitOfWorkOption.RequiresNew? TransactionScopeOption.RequiresNew
                : option == UnitOfWorkOption.Suppress ? TransactionScopeOption.Suppress
                :throw new System.NotSupportedException(),
                TransactionScopeAsyncFlowOption.Enabled);
		}
        /// <summary>
        /// Complete the transaction
        /// </summary>
		public void Complete()
		{
			_scope.Complete();
		}

        /// <summary>
        /// Dispose the resource.
        /// If scope hasn't completed,  rollback the unit of work.
        /// </summary>
		public void Dispose()
		{
			_scope.Dispose();
		}

	}
}
