using System;
namespace qshine
{
	public interface IContextStore:IProvider
	{
		/// <summary>
		/// Sets the data.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="data">Data.</param>
		void SetData(string name, object data);
		/// <summary>
		/// Gets the data.
		/// </summary>
		/// <returns>The data.</returns>
		/// <param name="name">Name.</param>
		object GetData(string name);
		/// <summary>
		/// Frees the data.
		/// </summary>
		/// <param name="name">Name.</param>
		void FreeData(string name);

        /// <summary>
        /// Get context type
        /// </summary>
        ContextStoreType ContextType { get; }
	}
}
