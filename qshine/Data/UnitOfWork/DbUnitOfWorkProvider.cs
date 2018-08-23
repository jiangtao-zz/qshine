using System;
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
		/// <param name="requireNew">If set to <c>true</c> the unit of work will no merge with an existing active transaction. 
		/// It will create a new transaction which is independent from others </param>		
		public IUnitOfWork Create(bool requireNew)
		{
			return new DbUnitOfWork(requireNew);
		}
	}
}
