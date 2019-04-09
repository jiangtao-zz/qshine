using System;
namespace qshine
{
    /// <summary>
    /// Invalid provider exception
    /// </summary>
	public class InvalidProviderException:Exception
	{
        /// <summary>
        /// Ctro::
        /// </summary>
        /// <param name="error"></param>
		public InvalidProviderException(string error)
			:base(error)
		{
		}
	}
}
