using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using qshine.Utility;
#if NETCORE
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;

namespace qshine
{
    internal sealed class ApplicationAssemblyResolver : IDisposable
    {
        private readonly ICompilationAssemblyResolver _assemblyResolver;
        private readonly DependencyContext _dependencyContext;
        private readonly AssemblyLoadContext _loadContext;
        private readonly string _loadPath;

        public ApplicationAssemblyResolver(string path)
        {
            _loadPath = path;
            this.Assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            this._dependencyContext = DependencyContext.Load(this.Assembly);
            if (this._dependencyContext != null)
            {
                //this._dependencyContext = this._dependencyContext.Merge(DependencyContext.Default);
                //DependencyContext.Default.CompileLibraries.Union(this._dependencyContext.CompileLibraries);
                //DependencyContext.Default.RuntimeLibraries.Union(this._dependencyContext.RuntimeLibraries);
                //DependencyContext.Default.RuntimeGraph.Union(this._dependencyContext.RuntimeGraph);

                this._assemblyResolver = new CompositeCompilationAssemblyResolver(new ICompilationAssemblyResolver[]
                {
                    new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(path)),
                    new ReferenceAssemblyPathResolver(),
                    new PackageCompilationAssemblyResolver(),
                });

                this._loadContext = AssemblyLoadContext.GetLoadContext(this.Assembly);
                this._loadContext.Resolving += OnResolving;
            }
        }

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
                    _dotnetResolvers.Add(path, new ApplicationAssemblyResolver(path));
                }
            }
            return assembly;
        }

/*
        static public Assembly Resolve(List<string> depsJsonPaths, Assembly requestingAssembly, string qualifyAssemblyName)
        {

            foreach (var file in depsJsonPaths)
            {
                var dll = file.Replace("deps.json", "dll");
                if (!_dotnetResolvers.ContainsKey(dll))
                {
                    _dotnetResolvers.Add(dll, new ApplicationAssemblyResolver(dll));
                    //var defaultDepsFiles = (string)AppDomain.CurrentDomain.GetData("APP_CONTEXT_DEPS_FILES");
                    //if (!string.IsNullOrEmpty(defaultDepsFiles)) {
                    //    defaultDepsFiles += ";" + file;
                    //}
                    //AppDomain.CurrentDomain.SetData("APP_CONTEXT_DEPS_FILES", defaultDepsFiles);
                }
            }
            foreach(var resolver in _dotnetResolvers.Values)
            {
                var assembly = resolver.OnResolving(null, new AssemblyName(qualifyAssemblyName));
                if (assembly != null)
                {
                    return assembly;
                }
            }
            return null;
        }
*/
        public Assembly Assembly { get; }

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
                    _dependencyContext.Merge(DependencyContext.Default), 
                    _assemblyResolver, _loadContext, name);
                if (assembly != null)
                {
                    _resolvedAssemblies.Add(name.Name, assembly);
                }
                return assembly;
            }
        }

        static private Assembly ResolveReferenceAsselbly(DependencyContext dependencyContext, 
            ICompilationAssemblyResolver assemblyResolver,
            AssemblyLoadContext loadContext,
            AssemblyName name)
        {
            bool NamesMatch(RuntimeLibrary runtime)
            {
                return string.Equals(runtime.Name, name.Name, StringComparison.OrdinalIgnoreCase);
            }

            RuntimeLibrary library =
                dependencyContext.RuntimeLibraries.FirstOrDefault(NamesMatch);

/*            var ridGraph = dependencyContext.RuntimeGraph.Any()
                           ? dependencyContext.RuntimeGraph
                           : DependencyContext.Default.RuntimeGraph;

            var rid = Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment.GetRuntimeIdentifier();
            var fallbackRid = "win-x64";
            var fallbackGraph = ridGraph.FirstOrDefault(g => g.Runtime == rid)
                ?? ridGraph.FirstOrDefault(g => g.Runtime == fallbackRid)
                ?? new RuntimeFallbacks("any");

            var managed = dependencyContext.GetRuntimeNativeAssets(rid);
*/
            if (library == null)
            {
                //try to find the assembly by dll name instead of name self
                var assemblyName = name.Name + ".dll";
                library = dependencyContext.RuntimeLibraries.FirstOrDefault(
                    x=>x.RuntimeAssemblyGroups.Any(
                        y=>y.RuntimeFiles.Any(
                            z=>z.Path.Contains(assemblyName,StringComparison.OrdinalIgnoreCase)))
                    );
            }
            if (library != null)
            {
                if(library.RuntimeAssemblyGroups.Count==0 &&
                                    library.NativeLibraryGroups.Count==0 &&
                                   string.Equals(library.Type, "package", StringComparison.OrdinalIgnoreCase) &&
                                    library.Dependencies.Count>0)
                                {
                                    var dependencyName = library.Dependencies[0].Name;
                                    library =
                                        dependencyContext.RuntimeLibraries.FirstOrDefault(
                                            x=>string.Equals(x.Name, dependencyName,StringComparison.OrdinalIgnoreCase));
                                }

                                //var managed1 = dependencyContext.GetRuntimeNativeAssets(rid);
                var depAssemblies = library.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths).ToList();
                if (library.NativeLibraryGroups.Count > 0)
                {
                    //depAssemblies.AddRange(library.NativeLibraryGroups.SelectMany(x => x.AssetPaths));
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


                var assemblies = new List<string>();
                assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies);
                if (assemblies.Count > 0)
                {
                    return loadContext.LoadFromAssemblyPath(assemblies[0]);
                }
            }
            return null;
        }
    }
}
#endif