using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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

        public ApplicationAssemblyResolver(string path)
        {
            this.Assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            this._dependencyContext = DependencyContext.Load(this.Assembly);
            if (this._dependencyContext != null)
            {
                //this._dependencyContext.Merge(DependencyContext.Default);

                this._assemblyResolver = new CompositeCompilationAssemblyResolver(new ICompilationAssemblyResolver[]
                {
                    new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(path)),
                    new ReferenceAssemblyPathResolver(),
                    new PackageCompilationAssemblyResolver()
                });

                this._loadContext = AssemblyLoadContext.GetLoadContext(this.Assembly);
                this._loadContext.Resolving += OnResolving;
            }
        }


        static public Assembly Resolve(List<string> depsJsonPaths, Assembly requestingAssembly, string qualifyAssemblyName)
        {
            foreach(var file in depsJsonPaths)
            {
                var dll = file.Replace("deps.json", "dll");

                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);
                var dependencyContext = DependencyContext.Load(assembly);
                var assemblyResolver = new CompositeCompilationAssemblyResolver(
                    new ICompilationAssemblyResolver[]
                    {
                        new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(dll)),
                        new ReferenceAssemblyPathResolver(),
                        new PackageCompilationAssemblyResolver()
                    });

                var loadContext = AssemblyLoadContext.GetLoadContext(assembly);
                var resolvedAssembly = 
                    ResolveReferenceAsselbly(dependencyContext, assemblyResolver, loadContext, new AssemblyName(qualifyAssemblyName));

                if (resolvedAssembly != null) return resolvedAssembly;
            }
            return null;
        }

        public Assembly Assembly { get; }

        public void Dispose()
        {
            this._loadContext.Resolving -= OnResolving;
        }

        private Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
        {
            return ResolveReferenceAsselbly(_dependencyContext, _assemblyResolver, _loadContext, name);
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
                var wrapper = new CompilationLibrary(
                    library.Type,
                    library.Name,
                    library.Version,
                    library.Hash,
                    library.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths),
                    library.Dependencies,
                    library.Serviceable);

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