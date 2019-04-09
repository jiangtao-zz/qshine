# Dependency Injection and Invension of Control

The Dependency Injection (DI) service can pluggin different external DI library for the application. 
It provides a unified service interface to accomplish IoC using well-known external DI components.

The common external DI components are:

 - [autofac](https://autofac.org/)
 - [ninject](http://www.ninject.org/)
 - [structureMap](http://structuremap.github.io/)
 - unity, spring.NET, 
 
Each external DI component has it's own class interafce which makes it difficult to build cross plateform pluggable application.

This DI service provides an extra layer to easily pluggin any external DI component into application without change business applciation code.

The DI service is a critical pluggable component for the application. The component should be added in the top level pluggable component.

Plugin DI into application environment through configure file:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>
    <!-- qshine environment config section -->
    <section name="qshine" type="qshine.Configuration.EnvironmentSection, qshine" />
  </configSections>

  <qshine>
    <!--plug-in component -->
    <components>
      <component name="autofac" interface="qshine.IIocProvider" type="qshine.ioc.autofac.Provider, qshine.ioc.autofac"/>
    </components>
  </qshine>
</configuration>

```

Another critical pluggable component is the [logging](logging.md) component which need be loaded in top level configure.

Note: A default [tinyIoC](https://github.com/grumpydev/TinyIoC) component will be used if the external DI plugin component is not added.


## DI Service

DI service and DI container.

```c#
var container = IoC.CreateContainer();
container.Register<IBar, Bar>();
...

var bar = container.Resolve<IBar>();
```

or,

```c# 
IoC.Register<IBar, Bar>();
...
var bar = IoC.Resolve<IBar>();
```

The IoC static service will use current DI container to service registry and resolve.

```c#
var currentContainer = IoC.Current;
```

## Register Service

All IoC services need be registered once when the application startup.

```c#

    /// <summary>
    /// Register a (named) scope within a given scope
    /// </summary>
    /// <typeparam name="IT">service interface</typeparam>
    /// <typeparam name="T">service implementation class</typeparam>
    /// <param name="instanceScope">service instance scope</param>
    /// <param name="name">Specifies the service name</param>
    /// <returns>current container</returns>
    IoC.RegisterType<IT, T>(string name, IocInstanceScope instanceScope)
    IoC.RegisterType<IT, T>(IocInstanceScope instanceScope)
    IoC.RegisterType<IT, T>(string name)
    IoC.RegisterType<IT, T>()

    /// <summary>
    /// Register a transient/singleton/scoped (named) service
    /// </summary>
    IoC.AddTransient<IT, T>()
    IoC.AddTransient<IT, T>(string name)
    IoC.AddSingleton<IT, T>()
    IoC.AddSingleton<IT, T>(string name)
    IoC.AddScoped<IT, T>()
    IoC.AddScoped<IT, T>(string name)

```

A default lifetime scope set by IoC.DefaultInstanceScope property.
Use Addxxx() to register a given scope service.

### Dependency Lifetime scope

   **Transient** – Created a service instance every time requested.

   **Singleton** – Created a service instance within current container.

   **Scoped** – Created a service instance per context scope. 
The context scope could be a web request, service request, call context 
or any context scope that implemented IContextStore. The context scoped service lifetime are within context bind() and unbind().

## Get Service 

You always can use IoC.Resolve<IT>(name) to resolve a service registered in current container.
The name can be omitted.

Based on the DI design pattern, the DI service usually be injected into class through the class constructor or class property.

```c#

    class Foo{
        private readonly IBar _bar;
        public Foo(IBar bar) {
            _bar = bar;
        }
    }
```

The IoC.Resolve() should only be placed in a single location as close as possible to the applciation' entry point
which usually is in MVC controller factory, service or program startup method. 
Most external DI component implemented IoC in framewrok. Select suitable pluggable DI component for the application.



 ## Register service from external pluggable component

The external component services can be registered through application environment module.

    <appEnv>
        <modules>
            <!-- plugin myService component-->
            <module name="myServices"  type="myServices.Startup myService" />
        </modules>
    </appEnv>

In myService project
```c#
    
    public class Startup()
    {
        IoC.AddTransient<IServiceA, ServiceAClass>();
        IoC.AddTransient<IServiceB, ServiceBClass>();
    }

```


## Bind and Unbind scoped service

The context scoped service instance is only valid within context scope.
When the context is out of the scope the life of the service should be end. 

In ASP.NET Core the scoped service mechanism are built-in the framework. 
In other framework plateform we need explicitly bind and unbind the scoped service from DI container.

For example, 

in ASP.NET, the bind and unbind can be added in httpModule BeginRequest and EndRequest.

```C#

    void OnBeginRequest(...)
    {
        IoC.Bind();
    }

    void OnEndRequest(...)
    {
        IoC.Unbind();
    }
```

in ASP.NET MVC you also can bind and unbind through ActionFilter.

Most DI framewrok implemented the extension to allow you binding and unbinding the scoped service.

  
