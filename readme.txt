To build the solution you need enable the Nuget package manager => Allow NuGet download missing package.
1. VS =>Tools=>Options=>Nuget Package Manager => General => Allow NuGet to download missing packages CHECKED.
2. re-build solution. If solution failed try to rebuild solution several times. Some projects depends on other projects and bin files from Nuget package.
3. run config.bat command in solution folder and current folder to copy necessary assembly files to config folder.
4. Using default setting or change config setting for your own test.