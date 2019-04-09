using System.Reflection;
using System.Runtime.CompilerServices;
namespace qshine
{
	public static partial class EnvironmentEx
	{
        /// <summary>
        /// Get target framewrok of build
        /// </summary>
		public static string TargetFramework {
			get{
#if netcoreapp2_2
	return "netcoreapp2.2";
#elif netcoreapp2_1
	return "netcoreapp2.1";
#elif net461
	return "net461";
#else
	return "";
#endif
			}
		}
	}
}
