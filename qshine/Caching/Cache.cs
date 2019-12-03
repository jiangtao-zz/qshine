using System;
using System.Linq;
using System.Collections.Generic;
using qshine.Configuration;
using qshine.Utility;

namespace qshine.Caching
{
    /// <summary>
    /// It provides cache service for the application caching.
    /// Each cacheable object could be temporarily stored in one of the cache store by the cache key. 
    /// The cache key is used to lookup up a cache provider through environment provider name pattern matching.
    /// The cache provider instance can be plugged through environment configure setting or by code directly.
    /// 
    /// Using code inject a cache provider instance for the key cacheable object
    /// <![CDATA[
    ///     
    ///     ICache cache = new MemoryCacheStore();
    ///     ApplicationEnvironment.Default.MapProvider<ICache>("unitTestCache*", cache);
    ///     
    ///     var testObject1 = Cache.GetOrSet("unitTestCache.key1", ()=>{return MyUnitTetstLoadData();}, TimeSpan.FromMinutes(5));
    ///     
    /// 
    ///     
    /// ]]>
    /// </summary>
    public class Cache
	{
        //ICache _provider;

        ///// <summary>
        ///// Create a cache service from a named cache provider.
        ///// </summary>
        ///// <param name="cacheProviderName">Cache provider name or a mapped cache provider name.
        ///// See application environment provider mapping section to find how to set mapped provider.</param>
        //public Cache(string cacheProviderName)
        //{
        //    _provider = ApplicationEnvironment.Default.CreateProvider<ICache>(cacheProviderName);
        //}

        ///// <summary>
        ///// Create a cache service for particular cache object.
        ///// </summary>
        ///// <param name="cacheProvider">A Pluggable cache provider instance.</param>
        //public Cache(ICache cacheProvider, string cacheKey)
        //{
        //    _provider = cacheProvider;
        //}

        /// <summary>
        /// Get cache service
        /// </summary>
        /// <param name="key">cache key</param>
        /// <returns>return ICache instance</returns>
        private static ICache MappedCacheProvider(string key)
        {
            return ApplicationEnvironment.Default.Services.GetProvider<ICache>(key);
        }

        #region TryGet
        /// <summary>
        /// Try to get cacheable value by cache key.
        /// </summary>
        /// <param name="key">cache key</param>
        /// <param name="value">return cacheable value</param>
        /// <returns>Return true if the cacheable value found</returns>
        public static bool TryGet(string key, out object value)
        {
            Check.HaveValue(key, "Cache.Get=>key");

            if (_cacheEntitys.ContainsKey(key))
            {
                value = _cacheEntitys[key].CacheService.Get(key);

                return value!=null;
            }

            value = null;
            return false;
        }


        #endregion

        #region Get
        /// <summary>
        /// Get cached item by given key.
        /// </summary>
        /// <returns>Retrieved cached value. If no value found it returns null.</returns>
        /// <param name="key">The cache key.</param>
        public static object Get(string key)
		{
			Check.HaveValue(key, "Cache.Get=>key");

            if (_cacheEntitys.ContainsKey(key))
            {
                return _cacheEntitys[key].CacheService.Get(key);
            }

            return null;
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
        /// parameter must be set to null.</param>
        /// <param name="slidingExpiration">The interval between the time that the cached object was last accessed and the time
        /// at which that object expires. If this value is the equivalent of 20 minutes, the object will expire and be removed 
        /// from the cache 20 minutes after it was last accessed. If you are using sliding expiration, the absoluteExpiration 
        /// parameter must be set to null.</param>
        /// <param name="dependencyTags">A list of tags associate to the cache object. It is a dependent tag.
        /// When the cache object source data changed, the source data change monitor will raise change event with dependencyTags to reset the cache object.</param>
        /// <param name="priority">The cost of the object relative to other items stored in the cache, as expressed by the
        /// CacheItemPriority enumeration. This value is used by the cache when it evicts objects; objects with a lower cost 
        /// are removed from the cache before objects with a higher cost.</param>
        public static void Set(string key, object value, Nullable<DateTimeOffset> absoluteExpiration, Nullable<TimeSpan> slidingExpiration, 
            IEnumerable<string> dependencyTags = null, CacheItemPriority priority = CacheItemPriority.Normal)
		{
			Check.HaveValue(key, "Cache.Set=>key");
            Check.HaveValue(absoluteExpiration!=null || slidingExpiration!=null, "Cache.Set=>absoluteExpiration and slidingExpiration");


            CacheInfo cacheInfo;

            if(_cacheEntitys.ContainsKey(key))
            {
                cacheInfo = _cacheEntitys[key];
            }
            else
            {
                cacheInfo = new CacheInfo
                {
                    Key = key,
                    CacheService = MappedCacheProvider(key),
                };
                _cacheEntitys.Add(key, cacheInfo);
            }

            if (absoluteExpiration != null) {
                cacheInfo.AbsoluteExpiration = absoluteExpiration.Value;
                cacheInfo.Duration = absoluteExpiration.Value - DateTimeOffset.UtcNow;
            }else
            {
                cacheInfo.Duration = slidingExpiration.Value;
                cacheInfo.AbsoluteExpiration = DateTimeOffset.UtcNow+ cacheInfo.Duration;
            }

            cacheInfo.Priority = priority;
            cacheInfo.DependencyTags = dependencyTags;

            cacheInfo.CacheService.Set(key, value, absoluteExpiration, slidingExpiration, priority);
		}

        /// <summary>
        /// Cache keyed value
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Cached value</param>
		public static void Set(string key, object value)
		{
            Set(key, value, DateTimeOffset.MaxValue, null);
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
            Set(key, value, null, slidingExpiration, dependencyTags, priority);
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
		public static void Set(string key, object value, DateTimeOffset absoluteExpiration, IEnumerable<string> dependencyTags = null, CacheItemPriority priority = CacheItemPriority.Normal)
        {
            Set(key, value, absoluteExpiration, null, dependencyTags, priority);
        }
        #endregion

        #region GetOrSet

        /// <summary>
        /// Get cached object by given key.
        /// If the cacheable object is not cached, it will load and cache data.
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="dataLoader">cacheable data loader</param>
        /// <param name="absoluteExpiration">The time at which the inserted object expires and is removed from the cache. 
        /// To avoid possible issues with local time such as changes from standard time to daylight saving time, 
        /// use UtcNow instead of Now for this parameter value. If you are using absolute expiration, the slidingExpiration
        /// parameter must be set to null.</param>
        /// <param name="slidingExpiration">The interval between the time that the cached object was last accessed and the time
        /// at which that object expires. If this value is the equivalent of 20 minutes, the object will expire and be removed 
        /// from the cache 20 minutes after it was last accessed. If you are using sliding expiration, the absoluteExpiration 
        /// parameter must be set to null.</param>
        /// <param name="dependencyTags">A list of tags associate to the cache object. It is a dependent tag.
        /// When the cache object source data changed, the source data change monitor will raise change event with dependencyTags to reset the cache object.</param>
        /// <param name="priority">The cost of the object relative to other items stored in the cache, as expressed by the
        /// CacheItemPriority enumeration. This value is used by the cache when it evicts objects; objects with a lower cost 
        /// are removed from the cache before objects with a higher cost.</param>
        /// <returns>Returns cached data.</returns>
        public static object GetOrSet(string key, Func<object> dataLoader, Nullable<DateTimeOffset> absoluteExpiration, Nullable<TimeSpan> slidingExpiration,
            IEnumerable<string> dependencyTags = null, CacheItemPriority priority = CacheItemPriority.Normal)
        {
            Check.HaveValue(key, "Cache.GetOrSet=>key");
            object value;

            if (_cacheEntitys.ContainsKey(key))
            {
                value = _cacheEntitys[key].CacheService.Get(key);
                if (value != null) return value;
            }

            value = dataLoader();

            Set(key, value, absoluteExpiration, slidingExpiration, dependencyTags, priority);
            return value;

        }

        /// <summary>
        /// Get cached object by given key.
        /// If the cacheable object is not cached, it will load and cache data.
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="dataLoader">cacheable data loader</param>
        /// <param name="absoluteExpiration">The time at which the inserted object expires and is removed from the cache. 
        /// To avoid possible issues with local time such as changes from standard time to daylight saving time, 
        /// use UtcNow instead of Now for this parameter value. If you are using absolute expiration, the slidingExpiration
        /// parameter must be set to null.</param>
        /// <param name="dependencyTags">A list of tags associate to the cache object. It is a dependent tag.
        /// When the cache object source data changed, the source data change monitor will raise change event with dependencyTags to reset the cache object.</param>
        /// <param name="priority">The cost of the object relative to other items stored in the cache, as expressed by the
        /// CacheItemPriority enumeration. This value is used by the cache when it evicts objects; objects with a lower cost 
        /// are removed from the cache before objects with a higher cost.</param>
        /// <returns>Returns cached data.</returns>
        public static object GetOrSet(string key, Func<object> dataLoader, Nullable<DateTimeOffset> absoluteExpiration,
            IEnumerable<string> dependencyTags = null, CacheItemPriority priority = CacheItemPriority.Normal)
        {
            return GetOrSet(key, dataLoader, absoluteExpiration, null, dependencyTags, priority);
        }

        /// <summary>
        /// Get cached object by given key.
        /// If the cacheable object is not cached, it will load and cache data.
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="dataLoader">cacheable data loader</param>
        /// <param name="slidingExpiration">The interval between the time that the cached object was last accessed and the time
        /// at which that object expires. If this value is the equivalent of 20 minutes, the object will expire and be removed 
        /// from the cache 20 minutes after it was last accessed. If you are using sliding expiration, the absoluteExpiration 
        /// parameter must be set to null.</param>
        /// <param name="dependencyTags">A list of tags associate to the cache object. It is a dependent tag.
        /// When the cache object source data changed, the source data change monitor will raise change event with dependencyTags to reset the cache object.</param>
        /// <param name="priority">The cost of the object relative to other items stored in the cache, as expressed by the
        /// CacheItemPriority enumeration. This value is used by the cache when it evicts objects; objects with a lower cost 
        /// are removed from the cache before objects with a higher cost.</param>
        /// <returns>Returns cached data.</returns>
        public static object GetOrSet(string key, Func<object> dataLoader, Nullable<TimeSpan> slidingExpiration,
            IEnumerable<string> dependencyTags = null, CacheItemPriority priority = CacheItemPriority.Normal)
        {
            return GetOrSet(key, dataLoader, null, slidingExpiration, dependencyTags, priority);
        }


        #endregion

        #region Remove
        /// <summary>
        /// Removes the specified item from the cache
        /// </summary>
        /// <param name="key">The identifier for the cache item to remove</param>
        public static void Remove(string key)
		{
            Check.HaveValue(key, "Cache.Remove=>key");

            if (_cacheEntitys.ContainsKey(key))
            {
                _cacheEntitys[key].CacheService.Remove(key);
                _cacheEntitys.Remove(key);
            }
        }
		#endregion

		#region Clear
        /// <summary>
        /// Clear current cache store.
        /// </summary>
		public static void Clear()
		{
            var cacheServices = ApplicationEnvironment.Default.Services.GetProviders<ICache>();
            foreach(var service in cacheServices)
            {
                service.Clear();
            }

            _cacheEntitys.Clear();

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
            if (_cacheEntitys.ContainsKey(key))
            {
                return _cacheEntitys[key].CacheService.Contains(key);
            }
            return false;
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
        static SafeDictionary<string, CacheInfo> _cacheEntitys = new SafeDictionary<string, CacheInfo>();

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
                        if (_cacheEntitys.ContainsKey(key))
                        {
                            _cacheEntitys[key].CacheService.Remove(key);
                        }
                    }
                }

                //if contains cache tags, clean cache objects by cache tags
                if (args.DependencyTags != null && args.DependencyTags.Any())
                {
                    foreach (var tag in args.DependencyTags)
                    {
                        var caches = _cacheEntitys.Values.Where(x => x.DependencyTags != null && x.DependencyTags.Contains(tag)).ToList();
                        if (caches.Count() > 0)
                        {
                            caches.ForEach(p => p.CacheService.Remove(p.Key));
                        }
                    }
                }
            }

        }
        #endregion

    }
}
