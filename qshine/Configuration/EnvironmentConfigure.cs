using System;
using System.Collections.Generic;
using System.Configuration;

namespace qshine.Configuration
{
	public class EnvironmentConfigure
	{
		IDictionary<string, EnvironmentElement> _environmentConfigure = new Dictionary<string, EnvironmentElement>();
		IDictionary<string, PlugableComponent> _componentConfigure = new Dictionary<string, PlugableComponent>();
		IDictionary<string, NamedTypeElement> _moduleConfigure = new Dictionary<string, NamedTypeElement>();
		IDictionary<string, string> _keyValues = new Dictionary<string, string>();


		/// <summary>
		/// The configure folders contains all configure files to be used by the application.
		/// </summary>
		List<string> _configureFolders = new List<string>();
		/// <summary>
		/// The bin folders contain all assemblies to be loaded by configure files.
		/// </summary>
		List<string> _binFolders = new List<string>();

		public List<string> ConfigureFolders
		{
			get
			{
				return _configureFolders;
			}
		}

		public List<string> AssemblyFolders
		{
			get
			{
				return _binFolders;
			}
		}


		public IDictionary<string, EnvironmentElement> Environments
		{
			get
			{
				return _environmentConfigure;
			}
		}

		public IDictionary<string, PlugableComponent> Components
		{
			get
			{
				return _componentConfigure;
			}
		}

		public IDictionary<string, NamedTypeElement> Modules
		{
			get
			{
				return _moduleConfigure;
			}
		}

		public IDictionary<string, string> AppSettings
		{
			get
			{
				return _keyValues;
			}
		}

		public ConnectionStringSettingsCollection ConnectionStrings
		{
			get
			{
				return _connectionStrings;
			}
		}

		/// <summary>
		/// Adds the environment.
		/// </summary>
		/// <returns>The environment.</returns>
		/// <param name="environment">Environment.</param>
		/// <param name="overLoad">If set to <c>true</c> over load.</param>
		public EnvironmentElement AddEnvironment(EnvironmentElement environment, bool overLoad = false)
		{
			if (environment == null) return null;

			if (!_environmentConfigure.ContainsKey(environment.Name))
			{
				_environmentConfigure.Add(environment.Name, environment);
			}
			else
			{
				if (overLoad)
				{
					_environmentConfigure[environment.Name] = environment;
				}
			}
			return _environmentConfigure[environment.Name];
		}

		/// <summary>
		/// Adds the component.
		/// </summary>
		/// <returns>The component.</returns>
		/// <param name="component">Component.</param>
		/// <param name="overLoad">If set to <c>true</c> over load.</param>
		public PlugableComponent AddComponent(ComponentElement component, bool overLoad = false)
		{
			if (component == null) return null;
			var c = new PlugableComponent
			{
				Name = component.Name,
				InterfaceTypeName = component.InterfaceType,
				ClassTypeName = component.Type,

			};
			foreach (var p in component.Parameters)
			{
				c.Parameters.Add(p.Name, p.Value);
			}
			//c.InterfaceType = Type.GetType(c.InterfaceTypeName);
			//c.ClassType = Type.GetType(c.ClassTypeName);


			if (!_componentConfigure.ContainsKey(component.Name))
			{
				_componentConfigure.Add(component.Name, c);
			}
			else
			{
				if (overLoad)
				{
					_componentConfigure[component.Name] = c;
				}
			}
			return _componentConfigure[component.Name];
		}

		/// <summary>
		/// Adds the module.
		/// </summary>
		/// <returns>The module.</returns>
		/// <param name="module">Module.</param>
		/// <param name="overLoad">If set to <c>true</c> over load.</param>
		public NamedTypeElement AddModule(NamedTypeElement module, bool overLoad = false)
		{
			if (module == null) return null;

			if (!_moduleConfigure.ContainsKey(module.Name))
			{
				_moduleConfigure.Add(module.Name, module);
			}
			else
			{
				if (overLoad)
				{
					_moduleConfigure[module.Name] = module;
				}
			}
			return _moduleConfigure[module.Name];
		}

		ConnectionStringSettingsCollection _connectionStrings = new ConnectionStringSettingsCollection();
		public void AddConnectionString(ConnectionStringSettings c)
		{
			if (_connectionStrings[c.Name] == null)
			{
				_connectionStrings.Add(new ConnectionStringSettings(c.Name, c.ConnectionString, c.ProviderName));
			}
		}
	}
}
