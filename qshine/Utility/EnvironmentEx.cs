using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace qshine
{
    public static partial class EnvironmentEx
    {
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

        /// <summary>
        /// Return current application running platform x86 or x64.
        /// Note: qshine builder target on Any CPU. It can load component from x86 
        /// </summary>
		public static string CpuArchitecture
		{
			get
			{
                if (Environment.Is64BitProcess)
                {
                    return "x64";
                }
                else
                {
                    return "x86";
                    //return Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
                }
			}
		}
	}
}
