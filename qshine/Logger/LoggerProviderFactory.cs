using qshine.Configuration;
using qshine.Utility;

namespace qshine.Logger
{
    /// <summary>
    /// Log provider factory
    /// </summary>
    public class LoggerProviderFactory: ILoggerProviderFactory
    {
        SafeDictionary<string, ILoggerProvider> _map = new SafeDictionary<string, ILoggerProvider>();
        ILoggerProvider _defaultProvider = null;
        /// <summary>
        /// Returns a Log provider instance 
        /// </summary>
        /// <param name="category">log category</param>
        /// <returns></returns>
        public ILoggerProvider CreateProvider(string category)
        {
            if (_map.ContainsKey(category)) return _map[category];

            var provider = ApplicationEnvironment.Default.Services.GetProvider<ILoggerProvider>(category);
            if (provider == null)
            {
                if (_defaultProvider != null)
                {
                    provider = _defaultProvider;
                }
                else
                {
                    //try get the default provider
                    provider = ApplicationEnvironment.Default.Services.GetProvider<ILoggerProvider>();
                    if (provider != null)
                    {
                        _defaultProvider = provider;
                    }
                }

                if (provider != null)
                {
                    //register provider association
                    _map.Add(category, provider);
                }
                else
                {
                    //returns internal provider
                    provider = InternalDefaultProvider;
                }
            }

            return provider;
        }

        /// <summary>
        /// Register a logger framework provider and associate it to a logging category in the Logging system. 
        /// </summary>
        /// <param name="provider">logger provider instance</param>
        /// <param name="category">logger category name</param>
        public void RegisterProvider(ILoggerProvider provider, string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                //register default provider
                _defaultProvider = provider;
            }
            else
            {
                if (_map.ContainsKey(category))
                {
                    _map[category] = provider;
                }
                else
                {
                    _map.Add(category, provider);
                }
            }
        }

        ILoggerProvider _internalProvider = null;
        /// <summary>
        /// Internal Log provider instance.
        /// </summary>
        ILoggerProvider InternalDefaultProvider
        {
            get
            {
                if (_internalProvider == null)
                {
                    _internalProvider = new TraceLoggerProvider();
                }
                return _internalProvider;
            }
        }

    }
}
