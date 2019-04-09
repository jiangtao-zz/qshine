using System;
namespace qshine.database
{
    /// <summary>
    /// Database tracking object type
    /// </summary>
	public enum TrackingObjectType
	{
        /// <summary>
        /// Not defined
        /// </summary>
		Unknown = 0,
        /// <summary>
        /// Table object
        /// </summary>
		Table = 1,
        /// <summary>
        /// View object
        /// </summary>
		View = 2,
	}
}
