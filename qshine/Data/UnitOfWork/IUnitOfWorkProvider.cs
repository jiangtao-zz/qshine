using System;
namespace qshine
{
	public interface IUnitOfWorkProvider:IProvider
	{
		/// <summary>
		/// Create a given type scope of unit of work.
		/// </summary>
		/// <returns>Create a new unit of work instance</returns>
		/// <param name="requireNew">If set to <c>true</c> the unit of work will no merge with an existing active transaction. 
		/// It will create a new transaction which is independent from others </param>
		IUnitOfWork Create(bool requireNew);
	}
}
