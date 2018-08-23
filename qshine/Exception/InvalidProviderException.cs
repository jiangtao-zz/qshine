using System;
namespace qshine
{
	public class InvalidProviderException:Exception
	{
		public InvalidProviderException(string error)
			:base(error)
		{
		}
	}
}
