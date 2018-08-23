using System;
namespace qshine
{
	/// <summary>
	/// Define a module loader which can load/init a module.
	/// The module can be loaded automatically if it implemented this interface.
	/// </summary>
	public interface IModuleLoader
	{
		/// <summary>
		/// Initialize this instance.
		/// </summary>
		void Initialize();

		///The destructor can be added to release the resource in end of the process.
		///~MyModule()
		///{
		///	// One time only destructor   }
		///}
	}
}
