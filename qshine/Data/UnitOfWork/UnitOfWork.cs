using System;
using qshine.Configuration;

namespace qshine
{
	public class UnitOfWork:IDisposable
	{
		static IUnitOfWorkProvider _provider;
		static object lockObject = new object();
		readonly IUnitOfWork _unitOfWork;

		/// <summary>
		/// Gets or sets the provider.
		/// </summary>
		/// <value>The provider.</value>
		public static IUnitOfWorkProvider Provider {
			get{
				if(_provider==null){
					_provider = EnvironmentManager.GetProvider<IUnitOfWorkProvider>();
					if (_provider == null)
					{
						lock(lockObject)
						{
							_provider = new DbUnitOfWorkProvider();
						}
					}
				}
				return _provider;
			}
			set{
				lock(lockObject)
				{
					_provider = value;
				}
			}
		}

		public UnitOfWork(bool requireNew = false)
		{
			_unitOfWork = Provider.Create(requireNew);
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
				_unitOfWork.Dispose();
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
			_unitOfWork.Complete();
		}
	}
}
