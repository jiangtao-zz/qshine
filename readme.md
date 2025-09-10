# QShine Framework

QShine is a lightweight, pluggable framework for .NET applications. It helps developers easily use third-party components without creating code dependencies. Pluggable components are added to the application through environment configuration settings and file copying, requiring no changes to the application's code.

## Key Features

*   **Configuration and Setting**: Flexible and hierarchical configuration system that supports XML, JSON, and command-line arguments.
*   **Dependency Injection (DI) / Inversion of Control (IoC)**: A powerful IoC container for managing components and their dependencies.
*   **Database Access**: A database-agnostic data access layer with support for MySQL, Oracle, PostgreSQL, SQLite, and SQL Server.
*   **Caching**: A flexible caching mechanism with support for in-memory and distributed caches.
*   **Logging**: A simple logging facade that can be integrated with popular logging frameworks like NLog and Log4Net.
*   **Messaging**: A messaging system with support for CommandBus and EventBus patterns.
*   **Event Sourcing**: A component for implementing the event sourcing pattern.
*   **Audit Trail**: A component for tracking changes to entities.
*   **Pluggable Architecture**: A modular and extensible architecture that allows you to add new features and components without modifying the core framework.

## Getting Started

### Configuration

The QShine framework is configured using an XML-based configuration file (e.g., `app.config` or `web.config`). The configuration is divided into sections for environment settings, components, modules, and maps.

Here is an example of a simple configuration file:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <configSections>
        <!-- qshine environment config section -->
        <section name="appEnv" type="qshine.Configuration.EnvironmentSection, qshine" />
    </configSections>

    <!--environment setting -->
    <appEnv>
        <environments>
            <!-- global level default setting-->
            <environment name="global" path="c:/globalSetting/config" bin="commonDependency"/>
            <!-- app level common setting-->
            <environment name="app" path="config"/>
        </environments>

        <components>
            <component name="nlog" interface="qshine.ILoggerProvider"
                type="qshine.log.nlog.Provider, qshine.log.nlog"/>
        </components>
    </appEnv>
</configuration>
```

For more details on configuration, please refer to the [Application Environment Configuration](qshine/docs/applicationEnvironment.md) documentation.

### Usage

To use the QShine framework, you first need to build the application environment. You can do this by calling the `ApplicationEnvironment.Build()` method at the startup of your application.

Here is a simple example of how to use the QShine framework:

```c#
// Build application environment
ApplicationEnvironment
    .Build()
    .Startup<IBoot>();

// Get environment variable
var environmentVar1 = ApplicationEnvironment.Current["appVar1"];

// Get pluggable component instance
var repository = ApplicationEnvironment.Current.CreateComponent<IInvoiceRepository>();

// Get pluggable provider
var emailProvider = ApplicationEnvironment.Current.CreateProvider<IEmailProvider>();
emailProvider.Send(subject, body, to);

// Get mapped provider by bus name
var auditBus = ApplicationEnvironment.Current.CreateMappedProvider<IEventBus>("securityAuditBus");
auditBus.Publish(auditLog);
```

For more information on the application startup process, please refer to the [Application Startup](qshine/docs/applicationStartup.md) documentation.

## Pluggable Architecture

The QShine framework is designed to be modular and extensible. You can easily add new components to the application by dropping the component's assembly into the `bin` folder and adding a component definition to the configuration file.

For more information on how to create and use pluggable components, please refer to the documentation in the `qshine/docs` directory.

## Build project

To build the solution you need enable the Nuget package manager => Allow NuGet download missing package.
1. VS =>Tools=>Options=>Nuget Package Manager => General => Allow NuGet to download missing packages CHECKED.
2. re-build solution. If solution failed try to rebuild solution several times. Some projects depends on other projects and bin files from Nuget package.
3. Using default setting or change config setting for your own test.