using System;
using System.Collections.Generic;
namespace qshine
{
	public class PlugableComponent
	{
		/// <summary>
		/// Gets or sets the name.
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
		/// Indicates whether the component has been registered and ready
		/// </summary>
		/// <value><c>true</c> if is registered; otherwise, <c>false</c>.</value>
		public bool hasRegistered { get; set; }

	}
}
