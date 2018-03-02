﻿using System;
using System.Runtime.Remoting.Messaging;

namespace qshine
{
	/// <summary>
	/// Wrap .NET call context Logic data
	/// This context shared data cross threads, AppDomains, processes within logic execution path. 
	/// </summary>
	public class CallContextLogicStore:IContextStore
	{
		/// <summary>
		/// Gets the context data.
		/// </summary>
		/// <returns>The data.</returns>
		/// <param name="name">Name.</param>
		public object GetData(string name)
		{
			return CallContext.LogicalGetData(name);
		}

		/// <summary>
		/// Sets the data.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="data">Data.</param>
		public void SetData(string name, object data)
		{
			CallContext.LogicalSetData(name, data);
		}

		/// <summary>
		/// Frees the data.
		/// </summary>
		/// <param name="name">Name.</param>
		public void FreeData(string name)
		{
			CallContext.FreeNamedDataSlot(name);
		}

		/// <summary>
		/// Gets the name of the context type.
		/// </summary>
		/// <value>The name of the context type.</value>
		public string ContextTypeName
		{
			get
			{
				return "callLogic";
			}
		}
	}
}
