# Application Startup

When an application is launched by system runner (web or desktop), first activity is to build application environment.

```c
///Build application environment with default options
ApplicationEnvironment.Build()
or
///Build application environment from a specific config file.
ApplicationEnvironment.Build(string topMostConfigFile)
or
///Build a named application environment with custom options.
ApplicationEnvironment.Build(EnvironmentInitializationOption option,string name)
```

## Build Parameters

**topMostConfigFile**: Specify topmost level application environment config file. If the parameter is omit, 
the default application config file (app.config or web.config) will be loaded as topmost application environment config file.

**name**: Specify application environment name to build named application environment. Named application environment hold a specific custom environment config setting based on build option.
Due to the nature of .NET application domain concept the named application environment will share the same domain assemblies with default application environment.

**option**: A build option to specify topmost config file and environment setting loading options. 
The options may receive builder initialization result for non-fatal error.



## Application startup interface

The Application environment builder can specify which interface class could be used as start up calsses.

The starup class constructor can take one type of ApplicationEnvironment argument or default constructor.

Call Startup() method to instantiate the all startup classes. The Startup class could be in pluggable assembly.

```c#
    public interface IMyStartup
    {
    }

    public class MyStartup:IMyStartup
    {
        public MyStartup(ApplicationEnvironment env)
        {
            ...
        }
    }

    ApplicationEnvironment
        .Build()
        .Startup<IMyStartup>();
    
```







