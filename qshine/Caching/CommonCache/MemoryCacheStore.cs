using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace qshine.Caching
{
	/// <summary>
	/// In-memory cache implementation
	/// </summary>
	public class MemoryCacheProvider : ICache, IProvider
    {
		MemoryCache _cache;

        /// <summary>
        /// Memory cache provider
        /// </summary>
		public MemoryCacheProvider()
		{
			_cache = MemoryCache.Default;
		}

		/// <summary>
		/// Wrap .NET MemoryCache instance.
		/// </summary>
		/// <returns>Create .NET MemoryCache wrap instance.</returns>
		/// <param name="name">Name of the cache store.</param>
		/// <param name="limitMegabytes">Limit megabytes. If the value is 0, the system will manage
		/// their own memory based on the amount of memory that is installed on the computer.</param>
		/// <param name="physicalMemoryLimitPercentage">The percentage of physical memory that the cache can use, expressed as an integer value from 1 to 100. 
		/// The default is zero, which indicates that MemoryCache instances manage their own memory based on the amount of memory that is installed on the computer.</param>
		/// <param name="pollingInterval">Polling interval in minutes. The cached item will be removed after polling interval time. The default value is 0 indicates 2 minutes</param>
		public MemoryCacheProvider(string name, int limitMegabytes, int physicalMemoryLimitPercentage, int pollingInterval)
		{
			TimeSpan timeSpan;
			System.Collections.Specialized.NameValueCollection cacheParameters;
			//For default setting, the parameters can be loaded from application config file.
			//See
			//	<system.runtime.caching >  
			//		<memoryCache>
			//			<namedCaches>
			//				<add name = "qcache" cacheMemoryLimitMegabytes="200" />
			//			</namedCaches>
			// 		< memoryCache />
			//    </system.runtime.caching >

			if (limitMegabytes == 0 && physicalMemoryLimitPercentage == 0 && pollingInterval == 0)
			{
				cacheParameters = null;
			}
			else
			{
				timeSpan = TimeSpan.FromMinutes(pollingInterval == 0 ? 2 : pollingInterval);
				cacheParameters= new System.Collections.Specialized.NameValueCollection
				{
					{"CacheMemoryLimitMegabytes",limitMegabytes.ToString()},
					{"PhysicalMemoryLimitPercentage",physicalMemoryLimitPercentage.ToString()},
					{"PollingInterval",timeSpan.ToString("hh:mm:ss")}
				};				
			}
			_cache = new MemoryCache(name, cacheParameters);
		}

		/// <summary>
		/// Get cached item by given key.
		/// </summary>
		/// <returns>Retrieved cached value. If no value found it returns null.</returns>
		/// <param name="key">The cache key.</param>
		public object Get(string key)
		{
			return _cache.Get(key);
		}

		/// <summary>
		/// Adds or replaces the specified entry to the cache by a key
		/// </summary>
		/// <param name="key">The cache key used to reference the item</param>
		/// <param name="value">The entry value</param>
		/// <param name="absoluteExpiration">The time at which the inserted object expires and is removed from the cache. 
		/// To avoid possible issues with local time such as changes from standard time to daylight saving time, 
		/// use UtcNow instead of Now for this parameter value. If you are using absolute expiration, the slidingExpiration
		/// parameter must be set to null.</param>
		/// <param name="slidingExpiration">The interval between the time that the cached object was last accessed and the time
		/// at which that object expires. If this value is the equivalent of 20 minutes, the object will expire and be removed 
		/// from the cache 20 minutes after it was last accessed. If you are using sliding expiration, the absoluteExpiration 
		/// parameter must be set to DateTime.MaxValue.</param>
		/// <param name="priority">The cost of the object relative to other items stored in the cache, as expressed by the
		/// CacheItemPriority enumeration. This value is used by the cache when it evicts objects; objects with a lower cost 
		/// are removed from the cache before objects with a higher cost.</param>
		public void Set(string key, object value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration, CacheItemPriority priority)
		{
			var policy = new CacheItemPolicy();

			policy.Priority = Convert2CacheItemPriority(priority);

			if(absoluteExpiration!=null)
			{
				policy.AbsoluteExpiration = absoluteExpiration.Value;
			}
			else if (slidingExpiration != null)
			{
				policy.SlidingExpiration = slidingExpiration.Value;
			}

			_cache.Remove(key);
			_cache.Add(key, value, policy);
		}

		/// <summary>
		/// Removes the specified item from the cache
		/// </summary>
		/// <param name="key">The identifier for the cache item to remove</param>
		public void Remove(string key)
		{
			_cache.Remove(key);
		}

		/// <summary>
		/// Clear cache store
		/// </summary>
		public void Clear()
		{
			var cacheKeys = _cache.Select(x => x.Key).ToList();
			foreach(var key in cacheKeys)
			{
				_cache.Remove(key);
			}
		}

		/// <summary>
		/// Returns true if key refers to item current stored in cache
		/// </summary>
		/// <param name="key">Key of item to check for</param>
		/// <returns>True if item referenced by the key is in the cache</returns>
		public bool Contains(string key)
		{
            return _cache.Contains(key);
		}

		/// <summary>
		/// Releases all resource used by the <see cref="T:qshine.MemoryCacheStore"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="T:qshine.MemoryCacheStore"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="T:qshine.MemoryCacheStore"/> in an unusable state. After
		/// calling <see cref="Dispose"/>, you must release all references to the <see cref="T:qshine.MemoryCacheStore"/> so
		/// the garbage collector can reclaim the memory that the <see cref="T:qshine.MemoryCacheStore"/> was occupying.</remarks>
		public void Dispose()
		{
            if (_cache != null)
            {
                _cache.Dispose();
            }
		}

		System.Runtime.Caching.CacheItemPriority Convert2CacheItemPriority(CacheItemPriority priority)
		{
			switch (priority)
			{
				case CacheItemPriority.AboveNormal:
				case CacheItemPriority.BelowNormal:
				case CacheItemPriority.Default:
				case CacheItemPriority.High:
				case CacheItemPriority.Low:
				case CacheItemPriority.Normal:
					return System.Runtime.Caching.CacheItemPriority.Default;
				case CacheItemPriority.NotRemovable:
					return System.Runtime.Caching.CacheItemPriority.NotRemovable;
				default:
					throw new NotImplementedException();
			}
		}
	}

	///// <summary>
	///// In memory cache provider.
	///// </summary>
	//public class MemoryCacheProvider : ICacheProvider
	//{
	//	/// <summary>
	//	/// Wrap .NET MemoryCache instance in this provider.
	//	/// </summary>
	//	/// <returns>Create .NET MemoryCache wrap instance.</returns>
	//	/// <param name="name">Name of the cache store.</param>
	//	/// <param name="limitMegabytes">Limit megabytes.</param>
	//	/// <param name="physicalMemoryLimitPercentage">Physical memory limit percentage.</param>
	//	/// <param name="pollingInterval">Polling interval in minutes. The cached item will be removed after polling interval time.</param>
	//	public ICache Create(string name, int limitMegabytes, int physicalMemoryLimitPercentage, int pollingInterval)
	//	{
	//		return new MemoryCacheStore(name,limitMegabytes,physicalMemoryLimitPercentage,pollingInterval);
	//	}

	//	/// <summary>
	//	/// Create a default cache instance.
	//	/// </summary>
	//	/// <returns>The create.</returns>
	//	public ICache Create()
	//	{
	//		return new MemoryCacheStore();
	//	}
	//}

}
