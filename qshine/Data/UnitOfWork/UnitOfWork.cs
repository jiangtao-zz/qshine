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
		static object lockObject = new object();
		readonly List<IUnitOfWork> _unitOfWorks;

		/// <summary>
		/// Gets or sets the provider.
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

		public UnitOfWork(bool requireNew = false)
		{
			_unitOfWorks = Providers.Select(x=>x.Create(requireNew)).ToList();
		}

		#region Dispose

		bool disposed = false;
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

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
		/// Gets or sets a value indicating whether this <see cref="T:qshine.UnitOfWork"/> can complete.
		/// </summary>
		/// <value><c>true</c> if can complete; otherwise, <c>false</c>.</value>
		public void Complete()
		{
            foreach (var u in _unitOfWorks)
            {
                u.Complete();
            }
		}
	}
}
