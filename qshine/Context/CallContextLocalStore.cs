using System;
using System.Runtime.Remoting.Messaging;

namespace qshine
{
	/// <summary>
	/// Wrap .NET call context local data
	/// This context shared data within thread execution path.
	/// It is better than ThreadStatic to store data within same thread. 
	/// </summary>
	public class CallContextLocalStore:IContextStore
	{
		/// <summary>
		/// Gets the context data.
		/// </summary>
		/// <returns>The data.</returns>
		/// <param name="name">Name.</param>
		public object GetData(string name)
		{
			return CallContext.GetData(name);
		}

		/// <summary>
		/// Sets the data.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="data">Data.</param>
		public void SetData(string name, object data)
		{
			CallContext.SetData(name, data);
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
				return "callLocal";
			}
		}
	}
}
