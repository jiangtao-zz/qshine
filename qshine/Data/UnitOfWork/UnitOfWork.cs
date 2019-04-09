using System;
using System.Collections.Generic;
using System.Linq;
using qshine.Configuration;

namespace qshine
{
    /// <summary>
    /// Manages Unit of Work for all UnitOfWork providers.
    /// Each unit of work provider can create UnitOfWork instance for transaction management.
    /// </summary>
	public class UnitOfWork:IDisposable
	{
		static IList<IUnitOfWorkProvider> _providers = null;
		static readonly object lockObject = new object();

		readonly List<IUnitOfWork> _unitOfWorks;

		/// <summary>
		/// Gets or sets a list of UoW providers.
        /// Single UoW may contain many UoW transaction implementations for differnt database, transaction process.
        /// Each UoW transaction implementation different UoW provider.
        /// The UoW can be injected by DI or configuration.
		/// </summary>
		/// <value>The provider.</value>
		public static IList<IUnitOfWorkProvider> Providers {
			get{
				if(_providers == null){
                    lock (lockObject)
                    {
                        _providers = ApplicationEnvironment.Current.GetProviders<IUnitOfWorkProvider>();
                        if (_providers == null)
                        {
                            _providers = new List<IUnitOfWorkProvider>();
                        }
                        _providers.Add(new DbUnitOfWorkProvider());
                    }
				}
				return _providers;
			}
			set{
				lock(lockObject)
				{
					_providers = value;
				}
			}
		}

        /// <summary>
        /// Create a UoW instance
        /// </summary>
        /// <param name="option"></param>
		public UnitOfWork(UnitOfWorkOption option = UnitOfWorkOption.Required)
		{
			_unitOfWorks = Providers.Select(x=>x.Create(option)).ToList();
		}

        /// <summary>
        /// Dispose UoW
        /// </summary>
		#region Dispose

		bool disposed = false;
        /// <summary>
        /// Dispose
        /// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}
			if (disposing)
			{
				foreach(var u in _unitOfWorks)
                {
                    u.Dispose();
                }
			}
			disposed = true;
		}

		#endregion

		/// <summary>
		/// Indicates all operations within scope are completed sucessfully.
        /// Note: The UoW will be abort if the Complete() havn't been called in end of UoW.
		/// </summary>
		public void Complete()
		{
            foreach (var u in _unitOfWorks)
            {
                u.Complete();
            }
		}
	}
}
