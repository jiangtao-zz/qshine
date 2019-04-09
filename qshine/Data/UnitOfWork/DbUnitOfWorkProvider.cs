namespace qshine
{
	/// <summary>
	/// provides a simple database transaction unit of work.
	/// </summary>
	public class DbUnitOfWorkProvider:IUnitOfWorkProvider
	{
        /// <summary>
        /// Create a given type scope of unit of work.
        /// </summary>
        /// <returns>Create a new unit of work instance</returns>
        /// <param name="scope">scope of the uow. 
        /// It will create a new transaction which is independent from others </param>		
        public IUnitOfWork Create(UnitOfWorkOption scope)
		{
			return new DbUnitOfWork(scope);
		}
	}
}
