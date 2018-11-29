The application environment could contain many levels configuration setting files and plugable binary files in different folders(and sub-folders). 
The lower level configuration setting could overload higher level setting if the name are same.
Each level binary folder could contain different qshine version dlls or target .NET framework version dlls.

The loader will search for component start from current bin folder and then qshine version folder (version match start from major, minor then build). 
It only load closest version folder dlls if multiple version sub-folders found.
Some binary sub-folders contains components target to specific dotnet framework version. 

Only the environment targeted framework version folder component will be loaded if such folder found.
The full qualified environment target framework version is identified by Microsoft Target Framework Moniker (TFM).  
You can find full list of TFM from https://docs.microsoft.com/en-us/dotnet/standard/frameworks.



The configure file may contain environment element which can point to another level configuration setting.
As a default, the application configuration file is a level zero environment configure setting that will be loaded first. 
Others will be loaded based on environment element setting.

Restriction:
The binary files under environment configuration folder should be .NET managed assembly. 
The unmanaged assembly may not be located by plugin assembly resolver.
You can copy the unmanaged assembly files into application base folder to resolve the issue.

For dotnet core application, you can resolve same issue by merge related section from plugin dll deps.json to application deps.json.

Below is a sample application environment configuration structure:

applicationWorkingFolder (console/form/web):
      |
      |--- app.config (or web.config)					//application level configure file. (0 level configure file, not necessary)
      |---config/ 										//1 level configuration folder (default configure folder)
            |----config11.config						//1 level configuration file. it may contain level 2 config folder
            |----config12.config						//1 level configuration file
            |----singleConfig.config					//1 level configuration file only load once
            |----bin/									//1 level bin folder
                  |--commonDll1.dll						//1 level common dll file
                  |--commonDll2.dll						//1 level common dll file
                  |--4/ 								//1 level qshine version 4.XX bin folder.
                  |   |---version 4 dlls
                  |
                  |--5.7/								//1 level qshine version 5.7.XX bin folder.
                  |   |---version 5.71 dlls
                  |
                  |--net461/ 							//1 level target framework version 4.6.1 bin folder.
                  |   |---build for dotnet target framework 4.6.1 dlls

level2ConfigFolder/										//2 level configuration folder
            |----config21.config						//2 level configuration file. It may contain level 3 config folder
            |----config22.config						//2 level configuration file
            |----singleConfig.config					//2 level configuration file will not be loaded if same named configure file already loaded from previous level
            |----bin/									//2 level bin folder
                  |--commonDll1.dll						//2 level common dll file
                  |--commonDll2.dll						//2 level common dll file
                  |--4/ 								//2 level version 4.XX bin folder. 
                  |   |---version 4 dlls
                  |
                  |--5.7/								//2 level version 5.7.XX bin folder.
                  |   |---version 5.7.1 dlls 

level3ConfigFolder/										//3 level configuration folder
            |----config31.config						//3 level configuration file
            |----config32.config						//3 level configuration file
            |----singleConfig.config					//3 level configuration file will not be loaded if same named configure file already loaded from previous level
            |----bin/									//3 level bin folder
                  |--commonDll1.dll						//3 level common dll file
                  |--commonDll2.dll						//3 level common dll file
                  |--4/ 								//3 level version 4.XX bin folder. qshine.framework version
                  |   |---version 4 dlls
                  |
                  |--5.7/								//3 level version 5.7.XX bin folder. qshine.framework version
                  |   |---version 5.7.1 dlls 



===Configure file could contains below objects:

1. environments: contains one or more environment element. Each environment points to one environment config folder
2. components: contains one or more component element. Each component describle one interface andinterface implementation. 
          The component could be a provider interface or others. The provider interface will be constructed before other components.
3. modules: contains one or more component element. The module will be loaded into system. An bootstrap method will be called after the module loaded.
4. appSettings: contains application setting variables
