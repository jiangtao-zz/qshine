using System.Transactions;
namespace qshine
{
    public class TransactionScopeUnitOfWorkProvider : IUnitOfWorkProvider
    {
        public IUnitOfWork Create(UnitOfWorkOption option)
        {
            return new TransactionScopeUnitOfWork(option);
        }
    }

    public class TransactionScopeUnitOfWork:IUnitOfWork
	{
		TransactionScope _scope;
		public TransactionScopeUnitOfWork(UnitOfWorkOption option)
		{
            _scope = new TransactionScope(
                option== UnitOfWorkOption.Required? TransactionScopeOption.Required
                :option== UnitOfWorkOption.RequiresNew? TransactionScopeOption.RequiresNew
                : option == UnitOfWorkOption.Suppress ? TransactionScopeOption.Suppress
                :throw new System.NotSupportedException(),
                TransactionScopeAsyncFlowOption.Enabled);
		}

		public void Complete()
		{
			_scope.Complete();
		}

		public void Dispose()
		{
			_scope.Dispose();
		}

	}
}
