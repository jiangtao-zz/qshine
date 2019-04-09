# Logging

Logging service provide an unified interface to log application activities into logging system.
The Log service is a static class which is responsible to pass a specific logging category message to a proper logging framewrok provider.
Logging framework provider is a pluggable logging component which can be added into the application through application environment configure setting.
The Map configuration provides ability to choose default logging framewrok provider and map logging category to specific logging framework provider.

The common logging framework:
  - Log4net - https://logging.apache.org/log4net/
  - nlog - https://nlog-project.org/
  - elmah - https://elmah.github.io/

```c#
    var logger = Log.GetLogger("MyApp.Service");
    logger.Trace("UserService ctor");
```
The actual logging services are provided by individual logging framework component. 
Follow the logging framework specification to configure the logging output. 


## Application Logging

Log service help application to capture useful data for application health monitoring, problem troubleshooting/debugging and activity analysis.

### Log category

Logging category is used to categorize a area of application logging message to be logged in a target storage. 
The logging category name is an arbitrary string that usually is an application class type name, namespace or anything else. 
Most logging frameworks can configure logging setting separately by logging category and logging priority level.


**Common Categories:**

    - General
    - Database
    - Security
    - Network
    - MailServer

    - App.Invoice
    - App.Vendor

### Logging priority levels

Typically there are 6 levels of logging message.

    - Fatal: A crital error that cause application/service termination.
    - Error: A functional failure or unexpected error (exception). Usually capture in try/catch block.
    - Warn: Anything potentially cause problem, but could be recovered later.
    - Info: General useful information to tell function/service current state.
    - Debug: More detail information to help IT diagnose problems.
    - Trace: A simple entry/exits log for function profiling or activity analysis.


### Application logging separation 

The application logging is accomplished through a Logger instance created from a Logging framework system (implemented ILoggerProvider).
A Logger object is built for particular logging category. It could produce logging data in share or separate logging environment which depends on logging configuration setting.
A logging configure file could be shared by many applications (web or non-web) in host server. It also can be configured per application.
The granularity of logging setting is in logging category and priority level.

Use application environment configure to plug a logging framework. Detail logging configure setting provided by Logging framework component.


## Log Service

Log is a service class for application logging.

### 1. Plugin logging framework systems

A logging framework system is a pluggable component which contains a plugin configure file and logging components binary files folder. 

Sample of NLog logging framework system

**plugin.config:**

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
      <component name="nlog" interface="qshine.ILoggerProvider" type="qshine.log.nlog.Provider, qshine.log.nlog"/>
      <component name="default" interface="qshine.ILoggerProvider" type="qshine.TraceLoggerProvider, qshine"/>
    </components>

    <!--map logging framework system -->
    <maps name="qshine.ILoggerProvider"  default="nlog">
        <map key="System" value="default" />
    </maps>

  </qshine>
</configuration>
```

**component binary files folder:**

    - NLog.dll
    - qshine.log.nlog.dll

The plugin config file need be added in application environment configure element.
An application environment can plugin more than one Logging framework environments.

Using environment configure Maps setting to setup default Logger provider, and map of logging category.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>
    <!-- qshine environment config section -->
    <section name="qshine" type="qshine.Configuration.EnvironmentSection, qshine" />
  </configSections>

  <qshine>
    <!--set system logging -->
    <components>
      <component name="default" interface="qshine.ILoggerProvider" type="qshine.TraceLoggerProvider, qshine"/>
    </components>

    <!--map logging framework system -->
    <maps name="qshine.ILoggerProvider"  default="nlog">
        <map key="System" value="default" />
    </maps>

  </qshine>
</configuration>
```



### 2. Get ILogger instance

ILogger instance perform logging function per category. 

**Get logger for a specified category**


```c#
    var logger = Log.GetLogger("database");
    logger.Trace("Begin create database {0}", databaseName);
    ...
    logger.Trace("End ceate database {0}", databaseName);

```

**Specifies a class type as category**

    var logger = Log.GetLogger<InvoiceService>();


**Get a generic logger**

    var logger = Log.GetLogger();

### 3. Perform Logging

ILogger provides logging function per logging level.

    logger.Fatal(message, properties);
    logger.Fatal(ex, message, properties);

    logger.Error(message, properties);
    logger.Error(ex, message, properties);

    logger.Warn(message, properties);
    logger.Warn(ex, message, properties);

    logger.Info(message, properties);
    logger.Info(ex, message, properties);

    logger.Debug(message, properties);
    logger.Debug(ex, message, properties);

    logger.Trace(message, properties);
    logger.Trace(ex, message, properties);

 
### 4. DevDebug()

Log DEBUG level message for developer DEBUG purpose. The DevDebug() code is only available in DEBUG version. The code will be removed from Release version by compiler.

The DevDebug() method only logs "General" category logging message.

```c#

    Log.DevDebug(format, args);

```

### 5. SysLogger and SysLoggerProvider

SysLogger perform library system logger. Change SysLoggerProvider to do logging using different logging framework.

    Log.SysLogger.Exception(ex);



## Log provider and logging category Map

The Log service can support many logging framework providers in same application. 
Logger with specific logging category can be built by a specific Log provider through logging category Map.
The Map usually is defined in application envieonment config file. It also can be configured by code.

The logging Map name is "qshine.ILoggerProvider" which is Log provider interface name.

A default Log provider will be selected if the logging category is not associated with a Log provider in Map.
The default Log provider can be defined in configure file. 

**maps name**: qshine.ILoggerProvider

**map key**: logging category

**map value**: logging provider component name.

```xml

  <qshine>
    ...
    <!--map logging framework system -->
    <maps name="qshine.ILoggerProvider"  default="nlog">
        <map key="System" value="default" />
    </maps>

  </qshine>
</configuration>
```

