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
ApplicationEnvironment.Build(string name, EnvironmentInitializationOption option)
```

## Build Parameters

**topMostConfigFile**: Specify topmost level application environment config file. If the parameter is omit, 
the default application config file (app.config or web.config) will be loaded as topmost application environment config file.

**name**: Specify application environment name to build named application environment. Named application environment hold a specific custom environment config setting based on build option.
Due to the nature of .NET application domain concept the named application environment will share the same domain assemblies with default application environment.

**option**: A build option to specify topmost config file and environment setting loading options.


## Application startup interface

The application environment library provides application startup interface IStartupInitializer.
The class implemented startup interface will be instantiated before building application environment.

The Start() method will be called after the application environment built completed.


    public interface IStartupInitializer
    {
        /// <summary>
        /// Start application after default application environment built completed
        /// </summary>
        /// <param name="name">application environment name. The default app environment has no name.</param>
        void Start(string name);
    }

