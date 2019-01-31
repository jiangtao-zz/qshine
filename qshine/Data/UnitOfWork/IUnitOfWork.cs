using System;
using System.Transactions;

namespace qshine
{
	/// <summary>
	/// Interface of Unit of work.
	/// Unit of work maintains a list of objects affected by a business transaction.
	/// The unit of work may contains many other unit of works, all child unit of work will be merged into one transaction.
	/// Any uncompleted unit of work will cause whole transaction rollback before persistent the data. 
	/// </summary>
	public interface IUnitOfWork:IDisposable
	{
		/// <summary>
		/// Complete the unit of work.
		/// Completed unit of work must be done before end of the scope.
		/// </summary>
		void Complete();

	}
}
