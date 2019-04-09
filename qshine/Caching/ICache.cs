using System;
using System.Collections.Generic;

namespace qshine.Caching
{
    /// <summary>
    /// Cache interface
    /// </summary>
	public interface ICache:IDisposable
	{
		/// <summary>
		/// Get cached item by given key.
		/// </summary>
		/// <returns>Retrieved cached value. If no value found it returns null.</returns>
		/// <param name="key">The cache key.</param>
		object Get(string key);

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
		/// <param name="priority">The cost of the object relative to other items stored in the cache, as expressed by the
		/// CacheItemPriority enumeration. This value is used by the cache when it evicts objects; objects with a lower cost 
		/// are removed from the cache before objects with a higher cost.</param>
		void Set(string key, object value, DateTime absoluteExpiration, TimeSpan slidingExpiration,  CacheItemPriority priority);

		/// <summary>
		/// Removes all items from the cache.
		/// </summary>
		void Clear();

		/// <summary>
		/// Removes the specified item from the cache
		/// </summary>
		/// <param name="key">The identifier for the cache item to remove</param>
		void Remove(string key);

		/// <summary>
		/// Returns true if key refers to item current stored in cache
		/// </summary>
		/// <param name="key">Key of item to check for</param>
		/// <returns>True if item referenced by the key is in the cache</returns>
		bool Contains(string key);

	}
}
