using System.Reflection;
using System.Runtime.CompilerServices;
namespace qshine
{
	public static partial class EnvironmentEx
	{
		public static string TargetFramework {
			get{
#if netcoreapp2_1
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
