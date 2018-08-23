using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;

namespace qshine
{
	public static class EnvironmentEx
	{
		public static bool IsWebApplication
		{
			get
			{
				return (HttpRuntime.AppDomainAppId != null);
			}
		}

		static string _ip;
		public static string MachineIp
		{
			get
			{
                return _ip?? (_ip= Dns.GetHostEntry(Dns.GetHostName()).AddressList
				              .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString()
				             );
			}
		}

		public static string CpuArchitecture
		{
			get
			{
				return Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
			}
		}
	}
}
