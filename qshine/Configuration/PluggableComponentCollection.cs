using qshine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace qshine.Configuration
{
    /// <summary>
    /// Pluggable component collection
    /// </summary>
    public class PluggableComponentCollection:
        IEnumerable<PluggableComponent>
    {

        /// <summary>
        /// pluggable assemblies
        /// </summary>
        private SafeDictionary<string, PluggableComponent> _pluggableComponents = new SafeDictionary<string, PluggableComponent>();

        /// <summary>
        /// Count the number of components.
        /// </summary>
        public int Count
        {
            get
            {
                return _pluggableComponents.Count;
            }
        }

        /// <summary>
        /// Add a pluggable assembly into the collection
        /// </summary>
        /// <param name="name">simple assembly name</param>
        /// <param name="assembly">Pluggable assembly compoenent (may not loaded yet)</param>
        /// <returns>returns true if assembly added into the collection</returns>
        public bool Add(string name, PluggableComponent assembly)
        {
            if (!_pluggableComponents.ContainsKey(name))
            {
                _pluggableComponents.Add(name, assembly);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the pluggable assembly in the collection.
        /// </summary>
        /// <param name="name">The pluggable assembly name.</param>
        /// <returns>return true if the assembly exists in the collection.</returns>
        public bool Contains(string name)
        {
            return _pluggableComponents.ContainsKey(name);
        }

        /// <summary>
        /// Clear all pluggable components from the collection
        /// </summary>
        public void Clear()
        {
            _pluggableComponents.Clear();
        }

        /// <summary>
        /// Get pluggable assembly by name
        /// </summary>
        /// <param name="name">The pluggable assembly name.</param>
        /// <returns>Returns a pluggable assembly by name. Or, returns null if the assembly not found.</returns>
        public PluggableComponent this[string name]
        {
            get
            {
                if (_pluggableComponents.ContainsKey(name))
                {
                    return _pluggableComponents[name];
                }
                return null;
            }
            set
            {
                if (_pluggableComponents.ContainsKey(name))
                {
                    _pluggableComponents[name] = value;
                }
            }
        }

        /// <summary>
        /// Register a given interface type with its implementation class.
        /// </summary>
        /// <typeparam name="IT">Interface type</typeparam>
        /// <typeparam name="T">implementation class</typeparam>
        /// <param name="name">service name</param>
        /// <returns>It returns true when the component registered sucessfully.</returns>
        public bool AddTransien<IT,T>(string name)
        {
            return AddTransien(name, typeof(IT), typeof(T));
        }

        /// <summary>
        /// Register a transient service
        /// </summary>
        /// <typeparam name="IT">Service interface type</typeparam>
        /// <typeparam name="T">Service class type</typeparam>
        /// <returns></returns>
        public bool AddTransien<IT, T>()
        {
            return AddTransien(GetAutoName(typeof(T).Name), typeof(IT), typeof(T));
        }

        /// <summary>
        /// Register a transient service
        /// </summary>
        /// <param name="name">Service name</param>
        /// <param name="interfaceType">Service interface type</param>
        /// <param name="implementationType">Service implementation class type</param>
        /// <returns></returns>
        public bool AddTransien(string name, Type interfaceType, Type implementationType)
        {
            return AddService(name, interfaceType, implementationType);
        }

        /// <summary>
        /// Register a signleton service
        /// </summary>
        /// <typeparam name="IT">Interface type</typeparam>
        /// <typeparam name="T">implementation class</typeparam>
        /// <param name="name">service name</param>
        /// <returns>It returns true when the component registered sucessfully.</returns>
        public bool AddSingleton<IT, T>(string name)
        {
            return AddSingleton(name, typeof(IT), typeof(T));
        }

        /// <summary>
        /// Register a signleton service
        /// </summary>
        /// <typeparam name="IT">Service interface type</typeparam>
        /// <typeparam name="T">Service class type</typeparam>
        /// <returns></returns>
        public bool AddSingleton<IT, T>()
        {
            return AddSingleton(GetAutoName(typeof(T).Name), typeof(IT), typeof(T));
        }

        /// <summary>
        /// Register a signleton service
        /// </summary>
        /// <param name="name">Service name</param>
        /// <param name="interfaceType">Service interface type</param>
        /// <param name="implementationType">Service implementation class type</param>
        /// <returns></returns>
        public bool AddSingleton(string name, Type interfaceType, Type implementationType)
        {
            return AddService(name, interfaceType, implementationType, IocInstanceScope.Singleton);
        }


        /// <summary>
        /// Register a service component
        /// </summary>
        /// <typeparam name="IT">Service interface type</typeparam>
        /// <typeparam name="T">Service class type</typeparam>
        /// <param name="name">name of the component</param>
        /// <param name="scope">component lifetime scope</param>
        /// <param name="args">service component constructor arguments</param>
        /// <returns>It returns true when the component registered sucessfully.</returns>
        public bool AddService<IT, T>(string name, IocInstanceScope scope = IocInstanceScope.Transient, IDictionary<string, string> args = null)
        {
            return AddService(name, typeof(IT), typeof(T), scope, args);
        }
        /// <summary>
        /// Register a service component
        /// </summary>
        /// <param name="interfaceType">Service interface type</param>
        /// <param name="classType">Service component class type</param>
        /// <param name="name">name of the component</param>
        /// <param name="scope">component lifetime scope</param>
        /// <param name="args">service component constructor arguments</param>
        /// <returns>It returns true when the component registered sucessfully.</returns>
        public bool AddService(string name, Type interfaceType, Type classType, IocInstanceScope scope = IocInstanceScope.Transient, IDictionary<string, string> args = null)
        {
            var component = new PluggableComponent
            {
                ConfigureFilePath = ".",
                Name = name,
                InterfaceType = interfaceType,
                InterfaceTypeName = interfaceType.FullName,
                ClassType = classType,
                ClassTypeName = classType.FullName,
                Scope = scope.ToString(),
                IsDefault = false
            };

            if (args != null)
            {
                foreach (var key in args.Keys)
                {
                    component.Parameters.Add(key, args[key]);
                }
            }
            return Add(name, component);
        }



        /// <summary>
        /// Get all valid services from pluggable component collection
        /// </summary>
        /// <typeparam name="T">Type of component interface</typeparam>
        /// <returns></returns>
        public IList<T> GetServices<T>()
            where T:class
        {
            return _pluggableComponents.Where( 
                x=>
                x.Value.InterfaceType != null &&
                x.Value.InterfaceType == typeof(T) &&
                x.Value.ClassType != null)
                .Select(
                    y => (T)y.Value.CreateInstance()
                    )
                .ToList();
        }

        /// <summary>
        /// Exposes the enumerator, which supports a simple iteration over a collection of a specified type.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<PluggableComponent> GetEnumerator()
        {
            return _pluggableComponents.Values.GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return _pluggableComponents.Values.GetEnumerator();
        }

        /// <summary>
        /// Generate a key name automatimatically.
        /// </summary>
        /// <param name="baseName"></param>
        /// <returns></returns>
        string GetAutoName(string baseName)
        {
            int index = 1;
            while (_pluggableComponents.ContainsKey(baseName + index))
            {
                index++;
            }
            return baseName + index;
        }
    }
}
