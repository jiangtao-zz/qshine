using System;
using System.Collections.Generic;
using qshine.Configuration;

namespace qshine
{
	/// <summary>
	/// It provides cache service for the application.
	/// 1. Get plugable cache service
	///  
	/// </summary>
	public class Cache
	{
		static ICacheProvider _cacheProvider;
		static ICacheProvider _defaultCacheProvider = new MemoryCacheProvider();
		static object lockProvider = new object();

		/// <summary>
		/// Gets or sets the cache provider.
		/// </summary>
		/// <value>The cache provider instance</value>
		public static ICacheProvider Provider
		{
			get
			{
				if (_cacheProvider == null)
				{
					lock(lockProvider)
					{
						if (_cacheProvider == null)
						{
							_cacheProvider = ApplicationEnvironment.GetProvider<ICacheProvider>();
							if (_cacheProvider == null)
							{
								_cacheProvider = _defaultCacheProvider;
							}
						}
					}
				}
				return _cacheProvider;
			}
			set
			{
				_cacheProvider = value;
			}
		}

		static ICache _current;
		static object lockObject = new object();
		/// <summary>
		/// Gets or sets the current cache store instance.
		/// </summary>
		/// <value>The current cache store instance.</value>
		public static ICache Current
		{
			get
			{
				if (_current == null)
				{
					lock(lockObject)
					{
						if (_current == null)
						{
							_current = Provider.Create();
						}
					}
				}
				return _current;
			}

			set
			{
				_current = value;
			}
		}

		#region Get
		/// <summary>
		/// Get cached item by given key.
		/// </summary>
		/// <returns>Retrieved cached value. If no value found it returns null.</returns>
		/// <param name="key">The cache key.</param>
		public static object Get(string key)
		{
			Check.HaveValue(key, "Cache.Get=>key");
			return Current.Get(key);
		}
		#endregion

		#region Add

		/// <summary>
		/// Adds or replaces the specified entry to the cache by a key
		/// </summary>
		/// <param name="key">The cache key used to reference the item</param>
		/// <param name="value">The entry value</param>
		/// <param name="absoluteExpiration">The time at which the inserted object expires and is removed from the cache. 
		/// To avoid possible issues with local time such as changes from standard time to daylight saving time, 
		/// use UtcNow instead of Now for this parameter value. If you are using absolute expiration, the slidingExpiration
		/// parameter must be set to TimeSpan.Zero.</param>
		/// <param name="slidingExpiration">The interval between the time that the cached object was last accessed and the time
		/// at which that object expires. If this value is the equivalent of 20 minutes, the object will expire and be removed 
		/// from the cache 20 minutes after it was last accessed. If you are using sliding expiration, the absoluteExpiration 
		/// parameter must be set to DateTime.MaxValue.</param>
		/// <param name="dependentSets">The list of cache keys that represent dependent items. 
		/// If any dependent item changed the cached item will be automatically removed</param>
		/// <param name="priority">The cost of the object relative to other items stored in the cache, as expressed by the
		/// CacheItemPriority enumeration. This value is used by the cache when it evicts objects; objects with a lower cost 
		/// are removed from the cache before objects with a higher cost.</param>
		public static void Add(string key, object value, DateTime absoluteExpiration, TimeSpan slidingExpiration, IEnumerable<string> dependentSets = null, CacheItemPriority priority = CacheItemPriority.Normal)
		{
			Check.HaveValue(key, "Cache.Add=>key");
			Current.Add(key, value, absoluteExpiration, slidingExpiration, dependentSets, priority);
		}

		public static void Add(string key, object value)
		{
			Add(key, value, DateTime.MaxValue, TimeSpan.Zero);
		}

		public static void Add(string key, object value, DateTime absoluteExpiration, IEnumerable<string> dependentSets = null, CacheItemPriority priority = CacheItemPriority.Normal)
		{
			Add(key, value, absoluteExpiration, TimeSpan.Zero, dependentSets, priority);
		}

		public static void Add(string key, object value, DateTime absoluteExpiration, CacheItemPriority priority)
		{
			Add(key, value, absoluteExpiration, TimeSpan.Zero, null, priority);
		}

		public static void Add(string key, object value, IEnumerable<string> dependentSets, CacheItemPriority priority = CacheItemPriority.Normal)
		{
			Add(key, value, DateTime.MaxValue, TimeSpan.Zero, dependentSets, priority);
		}

		public static void Add(string key, object value, TimeSpan slidingExpiration, IEnumerable<string> dependentSets = null, CacheItemPriority priority = CacheItemPriority.Normal)
		{
			Add(key, value, DateTime.MaxValue, slidingExpiration, dependentSets, priority);
		}

		public static void Add(string key, object value, TimeSpan slidingExpiration, CacheItemPriority priority = CacheItemPriority.Normal)
		{
			Add(key, value, DateTime.MaxValue, slidingExpiration, null, priority);
		}
		#endregion

		#region Remove
		/// <summary>
		/// Removes the specified item from the cache
		/// </summary>
		/// <param name="key">The identifier for the cache item to remove</param>
		public static void Remove(string key)
		{
			Current.Remove(key);
		}
		#endregion

		#region Clear
		public static void Clear()
		{
			Current.Clear();
		}
		#endregion

		#region Contains
		/// <summary>
		/// Returns true if key refers to item current stored in cache
		/// </summary>
		/// <param name="key">Key of item to check for</param>
		/// <returns>True if item referenced by the key is in the cache</returns>
		public static bool Contains(string key)
		{
			Check.HaveValue(key, "Cache.Contains=>key");
			return Current.Contains(key);
		}
		#endregion
	}
}
