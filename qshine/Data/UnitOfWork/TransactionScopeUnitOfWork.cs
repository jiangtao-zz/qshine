using System;
using System.Transactions;
namespace qshine
{
	public class TransactionScopeUnitOfWork:IUnitOfWork
	{
		TransactionScope _scope;
		public TransactionScopeUnitOfWork(bool requireNew)
		{
			_scope = new TransactionScope(requireNew ? TransactionScopeOption.RequiresNew : TransactionScopeOption.Required);
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
