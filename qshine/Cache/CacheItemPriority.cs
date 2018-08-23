using System;
namespace qshine
{
	/// <summary>
	/// Cache item priority enumlation
	/// </summary>
	public enum CacheItemPriority
	{
		/// <summary>
		/// cache item has the lowest level Priority
		/// </summary>
		Low,
		/// <summary>
		/// cache item has the BelowNormal level Priority
		/// </summary>
		BelowNormal,
		/// <summary>
		/// cache item has the Normal level Priority
		/// </summary>
		Normal,
		/// <summary>
		/// cache item has the AboveNormal level Priority
		/// </summary>
		AboveNormal,
		/// <summary>
		/// cache item has the High level Priority
		/// </summary>
		High,
		/// <summary>
		/// cache item has the NotRemovable Priority
		/// </summary>
		NotRemovable,
		/// <summary>
		/// cache item has the Default Priority
		/// </summary>
		Default
	}
}
