﻿using System.Configuration;

namespace qshine.Configuration
{
	public class NamedTypeElement : NamedConfigurationElement
	{
		const string TypeAttributeName = "type";

		/// <summary>
		/// Gets or sets the type.
		/// </summary>
		/// <value>The type.</value>
		[ConfigurationProperty(TypeAttributeName)]
		public string Type
		{
			get { return (string)this[TypeAttributeName]; }
			set { this[TypeAttributeName] = value; }
		}
	}
}
