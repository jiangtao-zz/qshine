using System;
#if NETCORE
#else 
using System.Runtime.Remoting.Messaging;
#endif

namespace qshine
{
	/// <summary>
	/// Wrap .NET call context Logic data
	/// This context shared data cross threads, AppDomains, processes within logic execution path. 
	/// </summary>
	public class CallContextStore:IContextStore
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
            //Note:: CallContext.SetData do not pass parent context into child thread.
            //use LogicalSetData instead.
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
		public ContextStoreType ContextType
		{
			get
			{
				return ContextStoreType.CallContext;
			}
		}
	}
}
