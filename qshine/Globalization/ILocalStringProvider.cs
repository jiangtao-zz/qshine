using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Globalization
{
    /// <summary>
    /// Provide string localization service.
    /// </summary>
    public interface ILocalStringProvider: IProvider
    {
        /// <summary>
        /// Create a named local string store.
        /// </summary>
        /// <param name="name">The name of local resource store.
        /// The name is used to categorize the local string resource storage.
        /// </param>
        /// <returns>Returns an instance of local string resource store.</returns>
        ILocalString Create(string name);
    }
}
