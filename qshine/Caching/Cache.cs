using System;
using System.Linq;
using System.Collections.Generic;
using qshine.Configuration;
using qshine.Utility;

namespace qshine.Caching
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

        //public static SafeDictionary<string, DateTime> CacheKeys = new SafeDictionary<string, DateTime>();

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

        #region Set

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
        /// <param name="dependencyTags">A list of tags associate to the cache object. It is a dependent tag.
        /// When the cache object source data changed, the source data change monitor will raise change event with dependencyTags to reset the cache object.</param>
        /// <param name="priority">The cost of the object relative to other items stored in the cache, as expressed by the
        /// CacheItemPriority enumeration. This value is used by the cache when it evicts objects; objects with a lower cost 
        /// are removed from the cache before objects with a higher cost.</param>
        public static void Set(string key, object value, DateTime absoluteExpiration, TimeSpan slidingExpiration, IEnumerable<string> dependencyTags = null, CacheItemPriority priority = CacheItemPriority.Normal)
		{
			Check.HaveValue(key, "Cache.Add=>key");
            MapCacheKeyTags(key, dependencyTags);
            Current.Set(key, value, absoluteExpiration, slidingExpiration, priority);
		}

        /// <summary>
        /// Cache keyed value
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Cached value</param>
		public static void Set(string key, object value)
		{
			Set(key, value, DateTime.MaxValue, TimeSpan.Zero);
		}

        /// <summary>
        /// Cache keyed value for a given time
        /// </summary>
        /// <param name="key">cache key</param>
        /// <param name="value">value to be cached</param>
        /// <param name="slidingExpiration">Timespan </param>
        /// <param name="dependencyTags">A list of tags associate to the cache object. It is a dependent tag.
        /// When the cache object source data changed, the source data change monitor will raise change event with dependencyTags to reset the cache object.</param>
        /// <param name="priority"></param>
		public static void Set(string key, object value, TimeSpan slidingExpiration, IEnumerable<string> dependencyTags = null, 
            CacheItemPriority priority = CacheItemPriority.Normal)
		{
            Set(key, value, DateTime.MaxValue, slidingExpiration, dependencyTags, priority);
		}

        /// <summary>
        /// Cache keyed value with more parameters
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Cached value</param>
        /// <param name="absoluteExpiration">Cache expiration time</param>
        /// <param name="dependencyTags">A list of tags associate to the cache object. It is a dependent tag.
        /// When the cache object source data changed, the source data change monitor will raise change event with dependencyTags to reset the cache object.</param>
        /// <param name="priority"></param>
		public static void Set(string key, object value, DateTime absoluteExpiration, IEnumerable<string> dependencyTags = null, CacheItemPriority priority = CacheItemPriority.Normal)
        {
            Set(key, value, absoluteExpiration, TimeSpan.Zero, dependencyTags, priority);
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
        /// <summary>
        /// Clear current cache store.
        /// </summary>
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

        /// <summary>
        /// Register a cache object source data change monitor.
        /// A Data Change Monitor can monitor source data change.
        /// If the source data changed an data change event will be raised.
        /// The registered event handlers will get notified to invalid the cache object.
        /// </summary>
        public static void RegisterDataChangeMonitor(CacheDataChangedMonitor cacheMonitor)
        {
            cacheMonitor.DataChangedHandler += CacheMonitorHandler;
            _cacheMonitors.Add(cacheMonitor);
        }


        #region private static methods and proeprties

        /// <summary>
        /// Store all cache data change monitor
        /// </summary>
        static List<CacheDataChangedMonitor> _cacheMonitors = new List<CacheDataChangedMonitor>();

        /// <summary>
        /// Map cache key to tags
        /// </summary>
        static SafeDictionary<string, IEnumerable<string>> _cacheKeyTagMaps = new SafeDictionary<string, IEnumerable<string>>();

        /// <summary>
        /// Associate cache key to cache tags.
        /// Any cache tag associated cache object source data updated, the associated cache key object must be updated
        /// </summary>
        /// <param name="cacheKey">cache key</param>
        /// <param name="dependencyTags">cache key dependency tags</param>
        static void MapCacheKeyTags(string cacheKey, IEnumerable<string> dependencyTags)
        {
            if (dependencyTags != null)
            {

                if (_cacheKeyTagMaps.ContainsKey(cacheKey))
                {
                    _cacheKeyTagMaps[cacheKey] = dependencyTags;
                }
                else
                {
                    _cacheKeyTagMaps.Add(cacheKey, dependencyTags);
                }
            }
        }

        /// <summary>
        /// Generic cache monitor handler.
        /// The monitor handler will clean cache by cache keys or cache tags.
        /// </summary>
        /// <param name="monitor">cache monitor instance</param>
        /// <param name="args">cache monitor argument.</param>
        static void CacheMonitorHandler(object monitor, CacheDataChangedEventArgs args)
        {
            if (args != null)
            {
                //if CacheKeys contain value, clean cache objects by keys
                if (args.CacheKeys != null && args.CacheKeys.Any())
                {
                    foreach (var key in args.CacheKeys)
                    {
                        if (_cacheKeyTagMaps.ContainsKey(key))
                        {
                            Cache.Remove(key);
                        }
                    }
                }

                //if contains cache tags, clean cache objects by cache tags
                if (args.DependencyTags != null && args.DependencyTags.Any())
                {
                    foreach (var tag in args.DependencyTags)
                    {
                        //find all cache tag associated cache key
                        foreach (var keyTagMap in _cacheKeyTagMaps)
                        {
                            if (keyTagMap.Value != null && keyTagMap.Value.Contains(tag))
                            {
                                Cache.Remove(keyTagMap.Key);
                            }
                        }
                    }
                }
            }

        }
        #endregion

    }
}
