using System;
using System.Collections.Generic;
using System.Linq;

namespace qshine
{
    /// <summary>
    /// Pluggable component from plugin configure setting
    /// </summary>
	public class PlugableComponent
	{
        /// <summary>
        /// Component configure file path. 
        /// </summary>
        public string ConfigureFilePath { get; set; }
		/// <summary>
		/// Gets or sets the name of component.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the name of the interface type.
		/// It represents qualified type of component interface or base class.
		/// The assembly could be placed in any folder specied by environment config file
		/// </summary>
		/// <value>The name of the interface type.</value>
		public string InterfaceTypeName { get; set; }
		/// <summary>
		/// Gets or sets the type of the interface type.
		/// </summary>
		/// <value>The type of the interface.</value>
		public Type InterfaceType { get; set; }
		/// <summary>
		/// Gets or sets the name of the class type setting value.
		/// It represents a qualified type for interface implementation class
		/// The assembly could be placed in any folder specied by environment config file
		/// </summary>
		/// <value>The name of the class type.</value>
		public string ClassTypeName { get; set; }
		/// <summary>
		/// Gets or sets the type of the class.
		/// </summary>
		/// <value>The type of the class.</value>
		public Type ClassType { get; set; }

		Dictionary<string, string> _parameters = new Dictionary<string, string>();
		/// <summary>
		/// Gets or sets the parameters for component class constructor.
		/// </summary>
		/// <value>The parameters.</value>
		public IDictionary<string, string> Parameters { get { return _parameters; } }

        /// <summary>
        /// Defines a life scope of the component.
        /// The valid scopes are:
        ///     Singleton,
        ///     Transient
        ///     
        /// The value could be mapped to IocInstanceScope.
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// Indicates a default service for given interface.
        /// If more than one default components found, it always picks first one as default.
        /// </summary>
        public bool IsDefault { get; set; }

        object _singletonInstance { get; set; }
        bool _initialized = false;

        /// <summary>
        /// Create a component instance.
        /// If the scope of compoennt is a singleton instance, it always returns the same one.
        /// Otherwise, it creates a new instance.
        /// </summary>
        public object CreateInstance()
        {
            if (_singletonInstance != null) return _singletonInstance;

            if (!_initialized) Instantiate();

            if (!string.IsNullOrEmpty(InvalidReason))
            {
                throw new Exception(InvalidReason);
            }

            object instance = CreateInstance(ClassType, Parameters.Values.ToArray());

            //Singleton component only have single instance in scope of app life
            if (IocInstanceScope.Singleton.ToString().AreEqual(Scope))
            {
                _singletonInstance = instance;
            }

            return instance;
        }


        /// <summary>
        /// Instantiate the plugable component
        /// </summary>
        /// <returns></returns>
        public bool Instantiate()
        {
            //already initialized
            if(_initialized) return string.IsNullOrEmpty(InvalidReason);

            if (!string.IsNullOrEmpty(InterfaceTypeName))
            {
                //try to load interface type from type name
                if (InterfaceType == null)
                {
                    InterfaceType = SafeLoadType(InterfaceTypeName);
                }

                if (InterfaceType != null)
                {
                    //try to load concrete class type from type name 
                    if (ClassType == null)
                    {
                        ClassType = SafeLoadType(ClassTypeName);
                    }
                }
            }
            else
            {
                //direct load from concete class type
                if (ClassType == null)
                {
                    ClassType = SafeLoadType(ClassTypeName);
                }
            }

            _initialized = true;

            //Singleton component only have single instance in scope of app life
            if (IocInstanceScope.Singleton.ToString().AreEqual(Scope))
            {
                try
                {
                    CreateInstance();

                }catch(Exception ex)
                {
                    InvalidReason = String.Format("Failed to instatiate instance {0}.({1})",
                        ClassTypeName, ex.Message);
                }
            }

            //No error
            return string.IsNullOrEmpty(InvalidReason);
        }

        /// <summary>
        /// Creates the instance by type and given arguments.
        /// </summary>
        /// <returns>The instance.</returns>
        /// <param name="type">Type of object</param>
        /// <param name="parameters">Parameters.</param>
        public static object CreateInstance(Type type, params object[] parameters)
        {
            if (type == null) return null;
            return Activator.CreateInstance(type, parameters);
        }

        /// <summary>
        /// Load a qualified type without throw exception
        /// </summary>
        /// <returns>The type or null.</returns>
        /// <param name="typeValue">Type value.</param>
        public Type SafeLoadType(string typeValue)
        {
            Type type = null;
            try
            {
                //try to get type from loaded assembly
                //if it has not loaded yet, ask resolver to load the assembly
                type = Type.GetType(typeValue);

                if (type == null)
                {
                    //This should not happen. Just in case type cannot be resolved by previous assembly_resolver, need load type from assembly directly
                    var assemblyNameParts = typeValue.Split(',');
                    if (assemblyNameParts.Length > 1)
                    {
                        var assemblyName = assemblyNameParts[1].Trim();
                        var typeNameOnly = assemblyNameParts[0].Trim();

                        type = PluggableAssembly.GetType(assemblyName, typeNameOnly, true);
                        if (type == null)
                        {
                            InvalidReason = string.Format("SafeLoadType:: Cannot load assembly {0} type {1}", assemblyName, typeValue);
                        }
                    }
                    else
                    {
                        InvalidReason = string.Format("SafeLoadType:: Cannot get type {0}", typeValue);
                    }
                }
            }
            catch (Exception ex)
            {
                InvalidReason = string.Format("SafeLoadType:: {0} throw exception: {1}", typeValue, ex.Message);
            }

            return type;
        }

        /// <summary>
        /// Find teh reason why the component failed to Instantiate. 
        /// </summary>
        public string InvalidReason
        {
            get; private set;
        }

    }
}
