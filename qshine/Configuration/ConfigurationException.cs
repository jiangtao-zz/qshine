using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Configuration
{
    /// <summary>
    /// application Environment Configuration Exception
    /// </summary>
    public class ConfigurationException:Exception
    {
        List<string> _errorMessages = new List<string>();

        /// <summary>
        /// Gete a list of Exception
        /// </summary>
        public List<string> InnerErrorMessages { get { return _errorMessages; } }
    }
}
