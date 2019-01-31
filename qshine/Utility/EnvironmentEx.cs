using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
#if NETCORE
using System.Runtime.InteropServices;
#endif

namespace qshine
{
    public static partial class EnvironmentEx
    {
		static string _ip;

        /// <summary>
        /// Get computer machine name and IP
        /// </summary>
        public static string Machine
        {
            get
            {
                return Environment.MachineName +":" +MachineIp;
            }
        }

        /// <summary>
        /// Get computer machine IP
        /// </summary>
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
        /// Return current application running platform x86, x64, arm or arm64.
        /// Note: qshine builder target on Any CPU. It could load component from x86 or x64 
        /// </summary>
		public static string CpuArchitecture
		{
			get
			{
#if NETCORE
                //return x86,x64,arm or arm64
                return RuntimeInformation.OSArchitecture.ToString().ToLower();
#else
                if (Environment.Is64BitProcess)
                {
                    return "x64";
                }
                else
                {
                    return "x86";
                    //return Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
                }
#endif
            }
		}

        /// <summary>
        /// Get current operation system code
        /// </summary>
        public static string OSPlatform
        {
            get
            {
#if NETCORE
                if (RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    return "win";
                }
                if (RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                {
                    return "linux";
                }
                if (RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                {
                    return "osx";
                }
                return "any";
#else
                return "win";
#endif
            }
        }


    }
}
