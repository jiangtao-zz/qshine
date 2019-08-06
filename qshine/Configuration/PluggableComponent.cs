using qshine.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace qshine
{
    /// <summary>
    /// Pluggable component from plugin configure setting
    /// </summary>
	public class PluggableComponent
	{
        #region Properties

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
        /// Indicates a default type of service for given interface.
        /// If more than one default components found, it always picks first one as default.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// The reason of failure. 
        /// </summary>
        public string InvalidReason {get; private set; }

        #endregion

        #region Fields

        object _singletonInstance { get; set; }
        bool _initialized = false;

        #endregion

        #region Methods

        /// <summary>
        /// Create a component instance.
        /// If the scope of compoennt is a singleton instance, it always returns the same one.
        /// Otherwise, it creates a new instance.
        /// </summary>
        /// <remarks>
        /// The component will hold an reference of singleton instance.
        /// </remarks>
        public object CreateInstance()
        {
            if (_singletonInstance != null) return _singletonInstance;

            if (!_initialized)
            {
                if (ClassType != null)
                {
                    _initialized = true; //omit Instantiated
                }
                else
                {
                    throw new Exception(string.Format("The pluggable component {0} need be instantiated first.", Name));
                }
            }

            if (!string.IsNullOrEmpty(InvalidReason))
            {
                throw new Exception(InvalidReason);
            }

            if (ClassType != null)
            {
                object instance = ClassType.TryCreateInstance(Parameters.Values.ToArray());

                //Singleton component only have single instance in scope of app life
                if (IocInstanceScope.Singleton.ToString().AreEqual(Scope))
                {
                    _singletonInstance = instance;
                }
                return instance;
            }

            //This never happen
            return null;
        }

        #endregion

        /// <summary>
        /// Instantiate the plugable component
        /// </summary>
        /// <returns>
        /// Returns true if the component instanciated without error.
        /// </returns>
        internal bool Instantiate(ApplicationEnvironmentContext context)
        {
            if (_initialized) return string.IsNullOrEmpty(InvalidReason);

            if (!string.IsNullOrEmpty(InterfaceTypeName))
            {
                //try to load interface type by type name
                if (InterfaceType == null)
                {
                    InterfaceType = SafeLoadType(context, InterfaceTypeName);
                }
            }

            //try to load concrete class type from type name 
            if (ClassType == null && !string.IsNullOrWhiteSpace(ClassTypeName))
            {
                ClassType = SafeLoadType(context, ClassTypeName);
            }

            if(ClassType == null)
            {
                InvalidReason = "The component class type is required.";
            }

            _initialized = true;

            //Singleton component only have single instance in scope of app life
            if (IocInstanceScope.Singleton.ToString().AreEqual(Scope))
            {
                try
                {
                    CreateInstance();

                }
                catch (Exception ex)
                {
                    InvalidReason = String.Format("Cannot instatiate pluggable component {0}.[Error: {1}]",
                        ClassTypeName, ex.Message);
                }
            }

            //No error
            return string.IsNullOrEmpty(InvalidReason);
        }

        /// <summary>
        /// Load a qualified type without throw exception
        /// </summary>
        /// <returns>The type or null.</returns>
        /// <param name="context">Application environment contex.</param>
        /// <param name="typeValue">Type value.</param>
        private Type SafeLoadType(ApplicationEnvironmentContext context, string typeValue)
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

                        type = context.PlugableAssemblies.GetType(assemblyName, typeNameOnly, true);
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
    }
}
