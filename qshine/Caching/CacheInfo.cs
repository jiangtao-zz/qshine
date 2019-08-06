using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Caching
{
    /// <summary>
    /// Cache entity object information
    /// </summary>
    public class CacheInfo
    {
        /// <summary>
        /// Gets/Sets cache key. The cache key is a unique value for a cache entity object.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets an absolute expiration date for the cache entry.
        /// </summary>
        public DateTimeOffset AbsoluteExpiration { get; set; }

        /// <summary>
        /// Gets or sets cache time duration.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets/Sets a cache service for the cache entity
        /// </summary>
        public ICache CacheService { get; set; }

        /// <summary>
        /// Dependency tags is a list of tags associated to a cacheobject.
        /// The application can invalid cacheable objects by any tag.
        /// </summary>
        public IEnumerable<string> DependencyTags { get; set; }

        /// <summary>
        /// Gets or sets the priority for keeping the cache entry in the cache
        /// </summary>
        public CacheItemPriority Priority { get; set; }

        ///// <summary>
        ///// Gets/Sets cache entity data loader method
        ///// </summary>
        //public Func<object> DataLoder  { get; set; }
    }
}
