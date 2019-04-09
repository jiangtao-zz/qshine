using System;
using System.Collections.Generic;

namespace qshine.Caching
{
    /// <summary>
    /// Cacheable object. 
    /// A cacheable object is a given type of object loaded from data source and cached in cache store for a period of time.
    /// If the cacheable object expired or got reset the data will be loaded from source again.
    /// The reset can be triggered through a cache object change monitor which can track the source data change.
    /// </summary>
    public class CacheObject<T>
        where T:class
    {

        /// <summary>
        /// Ctro.
        /// </summary>
        public CacheObject()
        {
            //set cache key
            CacheKey = "co_" + typeof(T).FullName;
            ExpirationTimeSpan = TimeSpan.FromMinutes(5);
            Priority = CacheItemPriority.Normal;
        }

        /// <summary>
        /// Get/Set cache time expiration time interval.
        /// </summary>
        public TimeSpan ExpirationTimeSpan { get; set; }

        /// <summary>
        /// Get/set cache key
        /// </summary>
        public string CacheKey { get; private set; }

        /// <summary>
        /// Get/set cache object priority
        /// </summary>
        public CacheItemPriority Priority { get; set; }

        /// <summary>
        /// Load source data
        /// </summary>
        public Func<T> LoadSourceData { get; set; }

        /// <summary>
        /// Get cached value
        /// </summary>
        public T Value {
            get
            {
                var obj = Cache.Get(CacheKey);
                if (obj == null)
                {
                    //reload the cache object when cache object not found
                    if (LoadSourceData != null)
                    {
                        obj = LoadSourceData();
                        Cache.Set(CacheKey, obj, ExpirationTimeSpan, DependencyTags, Priority);
                    }
                }
                if (obj == null) return default(T);
                return obj as T;
            }
        }

        /// <summary>
        /// A comma separate tags associate to group of cacheable objects.
        /// The cache monitor can reset a group of cacheable objects by the cache tag.
        /// For example, 
        /// A cacheobject's CacheTags = "Person,Location" means Reset tag Person or Location could cause this cacheobject got reset.
        /// </summary>
        public IEnumerable<string> DependencyTags { get; set; }


    }
}
