using qshine.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace qshine.Configuration
{
    /// <summary>
    /// The Mapped Components is a service component collection.
    /// User can get service component by
    /// 1. By service interface type.
    /// 2. By service interface type and provider name defined in configure setting.
    /// 3. By service interface type and provider mapping name in configure setting.
    /// 
    /// Using mapping name to find a provider is a most flexible way to work with pluggable component.
    /// The mapping name could be a business object related name or tag.
    /// </summary>
    public class MappedComponents
    {
        EnvironmentConfigure _configure;
        /// <summary>
        /// Create a mapped components instance
        /// </summary>
        /// <param name="configure"></param>
        public MappedComponents(EnvironmentConfigure configure)
        {
            _configure = configure;
        }

        #region GetProvider/GetMappedProvider

        /// <summary>
        /// Get a specific interfaced type provider. 
        /// The provider must be inherited from IProvider.
        /// If plugged more than one provider, a default provider will be return.
        /// </summary>
        /// <typeparam name="T">Specifies provider interface type or base type.</typeparam>
        /// <returns></returns>
        public T GetProvider<T>()
            where T : IProvider
        {
            return (T)GetProvider(typeof(T));
        }

        /// <summary>
        /// Get a named service provider
        /// </summary>
        /// <typeparam name="T">Specifies provider interface type or base type.</typeparam>
        /// <param name="name">Component name.</param>
        /// <returns></returns>
        public T GetProvider<T>(string name)
            where T : IProvider
        {
            return (T)GetProvider(typeof(T), name);
        }


        /// <summary>
        /// Get a specific interface type provider. The provider must be inherited from IProvider.
        /// If the provider name is specified it create an instance from named provider or mapped name provider.
        /// Get default or first interface provider if the name is not specified or doesn't match to the provider name or map key.
        /// Provider configure setting map sample:
        /// <![CDATA[
        ///     <maps name="providerTypeName" default="providerName1">
        ///         <add key="mapKey" value="providerName" />
        ///     </maps>
        ///     <components>
        ///         <component name="providerName1" interface="Ixxx" type="xxx" />
        ///         <component name="providerName" interface="Ixxx" type="xxx2" />
        ///     </components>
        /// ]]>        /// </summary>
        /// <param name="providerInterface">Specifies provider interface type or base type.</param>
        /// <param name="name">Provider name or provider mapping name.</param>
        /// <returns>Returns a interface type implementation instance.</returns>
        public IProvider GetProvider(Type providerInterface, string name = "")
        {
            var component = GetComponent(providerInterface, name);

            return (component != null) ? component as IProvider : null;
        }

        /// <summary>
        /// Get provider by provider name.
        /// Returns null if the specified provider name is not found.
        /// </summary>
        /// <typeparam name="T">Type of provider</typeparam>
        /// <param name="providerName">provider name</param>
        /// <returns>Returns named provider.</returns>
        public T GetProviderByName<T>(string providerName)
            where T : IProvider
        {
            return (T)GetComponentByName(typeof(T), providerName);
        }

        /// <summary>
        /// Get component by component name
        /// </summary>
        /// <param name="interfaceType">type of component interface</param>
        /// <param name="componentName">Component name</param>
        /// <returns>Returns named component or null if name is not found.</returns>
        public object GetComponentByName(Type interfaceType, string componentName)
        {
            PluggableComponent component = GetPluggableComponent(interfaceType, componentName);

            object instance = null;
            if (component != null && component.Name == componentName)
            {
                instance = component.CreateInstance();
            }

            return instance;
        }

        /// <summary>
        /// Get all given type of providers from configured components
        /// </summary>
        /// <typeparam name="T">type of provider interface</typeparam>
        /// <returns></returns>
        public IList<T> GetProviders<T>()
            where T : IProvider
        {
            return _configure.Components.Where(
                x => x.InterfaceType != null &&
                x.InterfaceType == typeof(T) &&
                x.ClassType != null)
                .Select(
                    y => (T)y.CreateInstance()
                    )
                .ToList();
        }
        #endregion

        /// <summary>
        /// Get a specific interface type component instance.
        /// If the component name is specified it create instance from named component.
        /// Get default or first typed component if the name is not specified.
        /// </summary>
        /// <typeparam name="T">The interface type of the component.</typeparam>
        /// <param name="name">The component name defined in component setting configure. If name is blank, it will try to get default or first component listed in configure setting.</param>
        /// <returns>returns typed component instance</returns>
        public T GetComponent<T>(string name = "")
            where T : class
        {
            var component = GetComponent(typeof(T), name);

            return (component != null) ? component as T : null;
        }

        /// <summary>
        /// Create a specific interface type component instance.
        /// If the component name is specified it create instance from named component.
        /// Get default or first typed component if the name is not specified.
        /// </summary>
        /// <param name="interfaceType">The interface type of the component.</param>
        /// <param name="name">The component name defined in component setting configure. If name is blank, it will try to get default or first component listed in configure setting.</param>
        /// <returns>returns typed component instance object</returns>
        public object GetComponent(Type interfaceType, string name = "")
        {
            PluggableComponent component = GetPluggableComponent(interfaceType, name);

            object instance = null;

            if (component != null)
            {
                instance = component.CreateInstance();
            }

            return instance;
        }

        /// <summary>
        /// Get a named interface type plugin component.
        /// If the name is not specified, it returns default or first typed component.
        /// The name could be a component provider name or component mapping key name or matched mapping item.
        /// </summary>
        /// <param name="interfaceType">Specifies component interface type</param>
        /// <param name="name">Specifies the component name. If the name is blank, it returns default typed component or the first valid component from the component list.
        /// if plugin associated to a map collection, the name parameter can be a map key.
        /// </param>
        /// <returns>Returns a named or default pluggable component.</returns>
        PluggableComponent GetPluggableComponent(Type interfaceType, string name = "")
        {
            PluggableComponent pluginComponent = null;

            if (!string.IsNullOrEmpty(name))
            {
                //find provider by name and type
                pluginComponent = _configure.Components.FirstOrDefault(
                    x => x.InterfaceType != null && x.InterfaceType.Name == interfaceType.Name && x.Name == name
                    );
                if (pluginComponent != null)
                {
                    //Returns a named component.
                    return pluginComponent;
                }
            }


            bool defaultOrFirst = true;
            //if name is blank, try to find default provider name from component map.
            //  <maps name="interafceType" default="defaultComponentname">
            //    <add key="mapKey" value="providerName" />
            //  </maps>
            //
            var map = GetComponentMap(interfaceType);
            //If component map found, get the mapped name
            if (map != null && !string.IsNullOrEmpty(name))
            {
                //get mapped key
                name = map.MatchKeyValue(name);
            }

            foreach (var component in _configure.Components)
            {
                if (component.InterfaceType != null && component.InterfaceType.Name == interfaceType.Name)
                {
                    if (component.Name == name)
                    {
                        //found named component
                        pluginComponent = component;
                        break;
                    }

                    if (defaultOrFirst)
                    {
                        //reserve first component and continue keep looking for named component. 
                        pluginComponent = component;
                        defaultOrFirst = false;
                    }
                }
            }

            //if (pluginComponent != null)
            //{
            //    pluginComponent.Instantiate(_context);
            //}

            return pluginComponent;
        }

        /// <summary>
        /// Get the component map by component interface type
        /// </summary>
        /// <param name="componentType">interface type of the component</param>
        /// <returns>Returns type specific component map or null if the mapping is not found.</returns>
        Map GetComponentMap(Type componentType)
        {
            return GetEnvironmentMap(Map.GetMapName(componentType));
        }

        /// <summary>
        /// Get map by name from environment configure
        /// </summary>
        /// <param name="mapName">map name</param>
        /// <returns>return a environment map or null if mapping is not found</returns>
        Map GetEnvironmentMap(string mapName)
        {
            var maps = _configure.Maps;
            if (maps != null && maps.ContainsKey(mapName))
            {
                //get particular provider map and find mapped provider name
                return maps[mapName];
            }
            return null;
        }
    }
}
