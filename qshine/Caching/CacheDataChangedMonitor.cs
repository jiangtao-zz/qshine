using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace qshine.Caching
{
    /// <summary>
    /// A base class of cache object source data change montor.
    /// The change monitor raise event when cacheable object source data changed.
    /// The derived data change monitor class need pass cache key(s) or cache tag(s) values into data change event to reset the corresponding cache objects.
    /// 
    /// </summary>
    public abstract class CacheDataChangedMonitor
    {
        /// <summary>
        /// Data changed event handler
        /// The cache object can add handler to the monitor
        /// </summary>
        public event EventHandler<CacheDataChangedEventArgs> DataChangedHandler;

        /// <summary>
        /// Raise data changed event
        /// </summary>
        /// <param name="args"></param>
        void OnDataChanged(CacheDataChangedEventArgs args)
        {
            DataChangedHandler?.Invoke(this, args);
        }

        /// <summary>
        /// The derived class need call this method to invalid cacheable objects by the cache key when the source data changed.
        /// </summary>
        /// <param name="cacheKeys">A list of cache keys</param>
        public void InvalidCacheKeys(params string[] cacheKeys)
        {
            if (cacheKeys.Length > 0)
            {
                OnDataChanged(new CacheDataChangedEventArgs
                {
                    CacheKeys = cacheKeys
                });
            }
        }

        /// <summary>
        /// The derived class need call this method to invalid cacheable objects by the cache dependency tag when the source data changed.
        /// </summary>
        /// <param name="dependencyTags">A list of cache tags</param>
        public void InvalidCacheTags(params string[] dependencyTags)
        {
            if (dependencyTags.Length > 0)
            {
                OnDataChanged(new CacheDataChangedEventArgs
                {
                    DependencyTags = dependencyTags
                });
            }
        }
    }

    /// <summary>
    /// Cache source data changed event arguments
    /// </summary>
    public class CacheDataChangedEventArgs:EventArgs
    {
        /// <summary>
        /// A list of cache keys affected by source data change.
        /// When the source data changed, the cached data by listed cache keys need be reset.
        /// </summary>
        public IEnumerable<string> CacheKeys;

        /// <summary>
        /// A list of cache key tags affected by source data change.
        /// When teh source data changed, the cached data associated with cache tags need be reset.
        /// </summary>
        public IEnumerable<string> DependencyTags;
    }
}
