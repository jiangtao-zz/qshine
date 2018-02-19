using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace qshine.IoC
{
	/// <summary>
	/// Interface of IoC registration module
	/// </summary>
    public interface IIoCModule
    {
		/// <summary>
		/// Load IoC types registration from IoC module
		/// </summary>
		/// <param name="container">IoC Container.</param>
        void Load(IIoCContainer container);
    }
}
