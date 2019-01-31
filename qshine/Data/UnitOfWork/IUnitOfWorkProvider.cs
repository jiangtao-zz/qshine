namespace qshine
{
	public interface IUnitOfWorkProvider:IProvider
	{
        /// <summary>
        /// Create a given type scope of unit of work.
        /// </summary>
        /// <returns>Create a new unit of work instance</returns>
        /// <param name="option">A transaction scope option for the unit of work.
        /// The concept of transaction scope option is similar as TransactionScopeOption.
        /// See reference https://github.com/dotnet/docs/blob/master/docs/framework/data/transactions/implementing-an-implicit-transaction-using-transaction-scope.md
        ///     Required: Join the ambient transaction, or create a new one if one does not exist.
        ///     RequiresNew: Be a new root scope, that is, start a new unit of work transaction and have that transaction be the new ambient transaction inside its own scope.
        ///     Suppress: New connection but not take part in a transaction at all. There is no ambient transaction as a result.
        IUnitOfWork Create(UnitOfWorkOption option);
	}
}
