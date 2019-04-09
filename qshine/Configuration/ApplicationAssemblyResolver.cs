#if NETCORE
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using qshine.Utility;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;

namespace qshine
{

    internal sealed class ApplicationAssemblyResolver : IDisposable
    {
        #region static
        static SafeDictionary<string, ApplicationAssemblyResolver> _dotnetResolvers = new SafeDictionary<string, ApplicationAssemblyResolver>();
        static SafeDictionary<string, Assembly> _resolvedAssemblies = new SafeDictionary<string, Assembly>();

        /// <summary>
        /// Resolve dotnet core dependence from given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static public Assembly Resolve(string path)
        {
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            if (assembly != null)
            {
                if (!_dotnetResolvers.ContainsKey(path))
                {
                    _dotnetResolvers.Add(path, new ApplicationAssemblyResolver(assembly));
                }
            }
            return assembly;
        }
        #endregion

        private readonly ICompilationAssemblyResolver _assemblyResolver;
        private readonly DependencyContext _dependencyContext;
        private readonly AssemblyLoadContext _loadContext;
        private readonly string _loadPath;

        public ApplicationAssemblyResolver(Assembly assembly)
        {
            _loadPath = assembly.Location;
            _dependencyContext = DependencyContext.Load(assembly);
            if (_dependencyContext != null)
            {
                _assemblyResolver = new CompositeCompilationAssemblyResolver(new ICompilationAssemblyResolver[]
                {
                    new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(_loadPath)),
                    new ReferenceAssemblyPathResolver(),
                    new PackageCompilationAssemblyResolver(),
                });

                _loadContext = AssemblyLoadContext.GetLoadContext(assembly);
                _loadContext.Resolving += OnResolving;
            }
        }

        public void Dispose()
        {
            this._loadContext.Resolving -= OnResolving;
        }

        private Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
        {
            if (_resolvedAssemblies.ContainsKey(name.Name))
            {
                return _resolvedAssemblies[name.Name];
            }
            else
            {
                var assembly = ResolveReferenceAsselbly(
                    _dependencyContext,
                    _assemblyResolver, context, name);
                if (assembly != null)
                {
                    _resolvedAssemblies.Add(name.Name, assembly);
                }
                return assembly;
            }
        }

        private enum AssemblyType
        {
            Assembly,
            Native,
            Stream
        }

        private class ReferenceAssembly
        {
            public AssemblyType AssemblyType { get; set; }
            public CompilationLibrary Library { get; set; }
        }

        private Assembly ResolveReferenceAsselbly(DependencyContext dependencyContext, 
            ICompilationAssemblyResolver assemblyResolver,
            AssemblyLoadContext loadContext,
            AssemblyName name)
        {

            var referenceAssembly = LoadReferenceLibrary(dependencyContext, assemblyResolver, loadContext, name.Name);

            if (referenceAssembly == null) return null;

            var assemblies = new List<string>();
            assemblyResolver.TryResolveAssemblyPaths(referenceAssembly.Library, assemblies);


            if (assemblies.Count > 0)
            {
                ////AddDllDirectory
                //var selectedAssembly = assemblies[0];
                //if (referenceAssembly.Library.Dependencies != null)
                //{
                //    foreach (var d in referenceAssembly.Library.Dependencies)
                //    {
                //        var referenceLibrary = LoadReferenceLibrary(dependencyContext,
                //            assemblyResolver, loadContext, d.Name);
                //        if (referenceLibrary != null)
                //        {
                //            var refAssemblies = new List<string>();
                //            assemblyResolver.TryResolveAssemblyPaths(referenceLibrary.Library, refAssemblies);

                //            if (refAssemblies.Count > 0)
                //            {
                //                if (referenceLibrary.AssemblyType == AssemblyType.Native)
                //                {
                //                    var refAssembly = loadContext.LoadFromNativeImagePath(refAssemblies[0], null);
                //                }
                //                else
                //                {
                //                    var refAssembly = loadContext.LoadFromAssemblyPath(refAssemblies[0]);
                //                }
                //            }
                //        }
                //    }
                //}
            
                if (referenceAssembly.AssemblyType == AssemblyType.Assembly)
                {
                    return loadContext.LoadFromAssemblyPath(assemblies[0]);
                }
                else
                {
                    return loadContext.LoadFromNativeImagePath(null, assemblies[0]);
                }
            }
            return null;
        }

        private ReferenceAssembly LoadReferenceLibrary(DependencyContext dependencyContext,
            ICompilationAssemblyResolver assemblyResolver,
            AssemblyLoadContext loadContext,
            string assemblyName)
        {
            bool NamesMatch(RuntimeLibrary runtime)
            {
                return string.Equals(runtime.Name, assemblyName, StringComparison.OrdinalIgnoreCase);
            }

            //Try find library by assembly name
            RuntimeLibrary library =
                dependencyContext.RuntimeLibraries.FirstOrDefault(NamesMatch);

            //try find library by assembly file name
            if (library == null)
            {
                //try to find the assembly by dll name instead of name self
                var dllName = assemblyName + ".dll";
                library = dependencyContext.RuntimeLibraries.FirstOrDefault(
                    x => x.RuntimeAssemblyGroups.Any(
                        y => y.RuntimeFiles.Any(
                            z => z.Path.Contains(dllName, StringComparison.OrdinalIgnoreCase)))
                    );
            }

            if (library != null)
            {
                var ridGraph = dependencyContext.RuntimeGraph.Any()
                               ? dependencyContext.RuntimeGraph
                               : DependencyContext.Default.RuntimeGraph;

                var rid = Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment.GetRuntimeIdentifier();
                var fallbackRid = EnvironmentEx.OSPlatform + "-" + EnvironmentEx.CpuArchitecture;
                var fallbackGraph = ridGraph.FirstOrDefault(g => g.Runtime == rid)
                    ?? ridGraph.FirstOrDefault(g => g.Runtime == fallbackRid)
                    ?? new RuntimeFallbacks("any");

                //if the library only contains dependency package, load from dependency
                if (library.RuntimeAssemblyGroups.Count == 0 &&
                                    library.NativeLibraryGroups.Count == 0 &&
                                   string.Equals(library.Type, "package", StringComparison.OrdinalIgnoreCase) &&
                                    library.Dependencies.Count > 0)
                {
                    var dependencies = library.Dependencies;
                    foreach (var d in dependencies)
                    {
                        library =
                            dependencyContext.RuntimeLibraries.FirstOrDefault(
                                x => string.Equals(x.Name, d.Name, StringComparison.OrdinalIgnoreCase));
                        if(library.NativeLibraryGroups.Any(x => fallbackGraph.Fallbacks.Contains(x.Runtime))){
                            break;
                        }
                    }
                    //var dependencyName = library.Dependencies[0].Name;
                }

                var runtimeLibrarys = library.RuntimeAssemblyGroups.Where(x => fallbackGraph.Fallbacks.Contains(x.Runtime));
                if (runtimeLibrarys == null || runtimeLibrarys.Count() == 0)
                {
                    //if couldn't find fallback runtime asselbly, get default assembly
                    runtimeLibrarys = library.RuntimeAssemblyGroups;
                }
                AssemblyType assemblyType;
                var depAssemblies = runtimeLibrarys.SelectMany(g => g.AssetPaths).ToList();
                if (depAssemblies != null && library.NativeLibraryGroups.Count > 0)
                {
                    assemblyType = AssemblyType.Native;

                    depAssemblies.AddRange(library.NativeLibraryGroups.SelectMany(x => x.AssetPaths));
                }
                else
                {
                    assemblyType = AssemblyType.Assembly;
                }

                var wrapper = new CompilationLibrary(
                    library.Type,
                    library.Name,
                    library.Version,
                    library.Hash,
                    depAssemblies,//library.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths),
                    library.Dependencies,
                    library.Serviceable,
                    library.Path,
                    library.HashPath);

                return new ReferenceAssembly
                {
                    AssemblyType = assemblyType,
                    Library = wrapper
                };
            }
            return null;
        }

    }
}
#endif