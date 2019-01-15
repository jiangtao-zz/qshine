using System;
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

        /// <summary>
        /// Rollback the unit of work when UoW is out of scope without Complete()
		/// Uncompleted unit of work will not be committed when it out of the scope.
        /// </summary>
        //void Rollback();
	}
}
