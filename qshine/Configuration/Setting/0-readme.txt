QShine environment contains many level configuration and binary folders. lower level configuration overwrite higher level configure.
Each level binary folder may contain different .NET framework version assembly dlls. Only the closest versionable folder dlls will be loaded 
from each level. The common folder dlls always be loaded.

The configure file may contain environment element which points to another level config.
The application config file is a level zero file that will be loaded first. Others will be loaded later if the config element not exists.

Qshine environment configuration structured in below format:

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
                  |--4/ 								//1 level version 4.XX bin folder.
                  |   |---version 4 dlls
                  |
                  |--5.7/								//1 level version 5.7.XX bin folder.
                  |   |---version 5.71 dlls 

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
