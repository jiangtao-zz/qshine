# Application Environment Configuration
Application environment configuration are the system wide settings that affect application running in deployed environment. 

Environment settings could contain application config files and dependency components.
The config files and components could locate in different folder. The application will search all the configure folders by configure setting.

The environment config files could be shared by many applications running in same host. Different level config files (folders) form a hierarchy of application settings. 
The setting hierarchy overwrite the variable from higher to lower as default. The setting overwritten options can be specified by ApplicationEnvironmentInitializationOption.

The application environment library provide services for environment variable settings, dependency (pluggable) component settings, Startup and mapping service.

The ApplicationEnvironment configure file could be formatted differently. The common configure file formats are:

    .NET app config XML file format (default format)
    .NET Core JSON file format

## Environment config file format
The environment config is a typical .NET application config file which contains application configuration environment section.

```xml	
	<configSections>
		<section name="appEnv" type="qshine.Configuration.Environment, Qshine.Framework" />
	</configSections>

	<!--environment setting -->
	<appEnv>
		<environments>
			<!-- global level default setting-->
			<environment name="global" path="c:/globalSetting/config" bin="commonDependency"/>
			<!-- app level common setting-->
			<environment name="app" path="config"/>
			<!--QA environment specific setting. It is only available for specified QA server -->
			<environment name="qa" path="c:/globalSetting/qa/config" host="192.168.1.10"/>
			<!--UA environment specific setting. It is only available for specified UA server -->
			<environment name="ua" path="c:/globalSetting/qa/config" host="192.168.1.11"/>
			<!--PRODUCTION environment specific setting. It is only available for specified PRODUCTION server -->
			<environment name="production" path="c:/globalSetting/production/config" host="PRODUCTION_SERVER_NAME"/>
		</environments>
	</appEnv>
``` 

---
## Environment

The application environment is an infrastructure workspace to host an application with a collection of deployed components and dependency resources.
The host usually is a physcal machine, virtual machine, or a cloud system built with infrastructure.
Same host could have many applications running in different application environments.
In the application development lifcycle, a set of application environments are required for each stage of application deployment and testing.

The common application environments are:

- DEV environment
- QA environment
- UA environment
- STAGING/PRE-PRODUCTION environment
- PRODUCTION environment

An application environment configuration setting could contain many config files. Each file is located in a config folder.
One config file is responsible for one type of dependency service configuration settings. The dependecy binary components are located in same folder.

Example:

A folder mysql contains MySql database plugin

	database/mysql/plugin.config
	database/mysql/bin/net461/...
	database/mysql/bin/netcoreapp2.1/...
	database/mysql/bin/netcoreapp2.2/...

A folder nlog contains NLog logging plugin

	logger/nlog/plugin.config
	logger/nlog/bin/net461/...
	logger/nlog/bin/netcoreapp2.1/...
	logger/nlog/bin/netcoreapp2.2/...



A config file can contain all setting for different environment based on environment host information. 
Each environment element points to a setting folder and available for one environment host. 

```xml	
		<environments>
			<!--QA environment specific setting. It is only available for specified QA server -->
			<environment name="qa" path="c:/globalSetting/qa/config" host="192.168.1.10"/>
			<!--UA environment specific setting. It is only available for specified UA server -->
			<environment name="ua" path="c:/globalSetting/qa/config" host="192.168.1.11"/>
			<!--PRODUCTION environment specific setting. It is only available for specified PRODUCTION server -->
			<environment name="production" path="c:/globalSetting/production/config" host="PRODUCTION_SERVER_NAME"/>
		</environments>
``` 

An application environment configuration could contain all possible binary components for different running environment, 
such as target .NET framework version, cpu architecture, operating system and library versions. 

The application choose suitable running-time support libraries from binary configure folders automatically.

The binary components configure folders can have below structure.

- bin/[target framework name]/[target specific components]
- bin/[cpu architecture]/[cpu specific components]
- bin/[os]/[os specific components]
- bin/[qshine library versions]/[target specific components]
- bin/[one of above folder]/

Example of binary folder structure,

- bin/net461/
- bin/netcoreapp2.1/
- bin/x86/
- bin/x64/
- bin/win/
- bin/linux/
- bin/4/
- bin/4.5/
- bin/4.5.1/
- plugin/mysql/bin/win/x64/netcoreapp2.2/
- plugin/mysql/bin/linux/x64/netcoreapp2.2/4.5/

The application component version searching path start from nearest version number folder and the first found component win.
For example in below component folder structure, if the running qshine library is version 5.5.1 it will search version specific components from 5.5.1, 5.5 to 5 and only folder /5.5 components will be loaded.

- bin/4.2
- bin/5
- bin/5.5
- bin/6.5 

### environment element

Embedded next level environment config files and dependency components.

**name**: name of the config setting.

**path**: Specify a location path of the config files and components. The application will load all config files under given location. 
It also look up the "bin" folder to load pluggable components.

**bin**: Specify alternate bin folder location. If this value presents it will load components from this folder instead of "bin" folder.

**host**: Specify environment host tags that is used to match current environment. The host tags are semicolon separated tag list.
Each tag is used to match one of host environment arguments.

Host environment arguments:


|    Tag   | Description      | Example            |
|----------|------------------|--------------------|
|  ip      | host IP          | 10.3.3.102;        |
|  name    | host name        | QA-Server;         |
|  cpu     | CPU architecture | x86                |
|  os      | Operating System | win                |
|framework | Target framewrok | net461             |
|  version | library version  | 1.0.3              |

Example:

```xml
host="ip=10.3.3.120;hostName=QA-Server;cpu=x86;os=win;dotnet=net461;version=1.0"
or
host="10.3.3.120;QA-Server;x86;os=win;dotnet=net461;version=1.0"

```

---
## Environment Levels and Hierarchy
The highest level config file is the application config file. As a .NET application this config file is app.config or web.config (web application).
To keep configure setting structure simple, this config file environment section usually only contains next level environment config file paths, but not detail environment variables.
The detail environment variable will set in other level config files. 

The environment section could have more than one environment element. Each element contains a config attribute which points to next level environment config path. The chain of the next level environment config file build config hierarchy from higher to lower.
The higher level environment variable will overwrite lower level variable.
The same level environment elements settings are overwritten from top to down.

The common application environment config setting:

- highest level (app.config)
- application specific
- application module level
- application global level

For example,

The application myApp.exe deployed to folder programs/moduleA. The myApp.config is the highest environment config file

	c:\company\programs\moduleA\MyApp\myApp.exe
	c:\company\programs\moduleA\MyApp\myApp.config
myApp.config

```xml	
		<environments>
			<environment name="myApp" path="config"/>
			<environment name="moduleA" path="c:\company\programs\moduleA\config" />
			<environment name="global" path="c:\company\programs\config" />
		</environments>
``` 
	
The environment contain a special setting only for myApp
	c:\company\programs\moduleA\MyApp\config\
	c:\company\programs\moduleA\MyApp\config\special.config

special.config

```xml	
		<components>
			<component name="c1" ... />
		</components>
``` 


The environment contain gloabl setting for all the applications
	c:\company\programs\config\

The environment contain module level setting for moduleA
	c:\company\programs\moduleA\config\

---


## Environment variables

Environment variable is a key/value pair setting.

```xml

	<!--environment setting -->
	<appEnv>
		<appSettings>
			<add key="key0" value="value1" />
			<add key="key1" value="value2" />
			<add key="key2" value="value2" />
		</appSettings>
	</appEnv>

```

Get the environment variable in code:

```c#

	var key0 = ApplicationEnvironment.Current["key0"];

```

## Pluggable components

Add a pluggable component into application through application environment setting. 
A pluggable components packaged under a folder with an environment config file.

Example: Add NLog component into application.

folder nlog contains:

	nlog
    |
    |---plugin.config
    |---bin
    |   |--net461
    |   |   |--NLog.dll
    |   |   |--qshine.log.nlog.dll
    |   |
    |   |--netcoreapp2.1
    |   |   |--NLog.dll
    |   |   |--qshine.log.nlog.dll

plugin.config file

    <appEnv>
        <!--plug-in component -->
        <components>
            <component name="nlog" interface="qshine.ILoggerProvider" 
                type="qshine.log.nlog.Provider, qshine.log.nlog"/>
        </components>
    </appEnv>

The plugin config file contains a component element which is used to define plugin component. 

---
### component element
Represents a component to be plugin to the application.


**name**: Specify component name. The name can be used to locate component if the environment have more than one components with same interface type.

**interface**: Specify component interface type. The application access the component through the interface.

**type**: Specify component class type which implemented the given interface. 

**parameters**: a collection of parameters for component constructor.

**parameter**: Specify a constructor parameter by a name/value pair element.

            <component name="nservicebus" 
				interface="qshine.IEventBusProvider"
				type="qshine.extension.messaging.eventbus.nservicebus.eventProvider"
              scope="singleton">
				<parameters>
					<parameter name="path" value="//192.168.10.12/fileServer" /> 
					<parameter name="user" value="dev" /> 
					<parameter name="password" value="xxxxx" /> 
					<parameter name="domain" value="mydomain" /> 
				</parameters>
            </component>

**scope**: Specify a scope of component lifecycle. It could be a singlton, transient. The default is singleton.
The pluggable component usually is a factory component which can create particular object through code.

---

Consume default plugin object in application code.

```c#
    //publish event message by plugin event bus.
    var busProvider = ApplicationEnvironment.GetProvider<IEventBusProvider>();
    var bus = busProvider.Create(busName);
    bus.Publish(myEvent);

```

Consume named plugin object in application code.

```c#
    //publish event message by plugin event bus.
    var busProvider = ApplicationEnvironment.GetProvider<IEventBusProvider>("nservicebus");
    var bus = busProvider.Create(busName);
    bus.Publish(myEvent);

```

Consume mapped plugin object in application code.

```c#
    //publish event message by plugin event bus based on busname.
    //if the bus name is not in mapped list, a default bus will be selected.
    //see map section
    var busProvider = ApplicationEnvironment.GetProvider<IEventBusProvider>(busName);
    var bus = busProvider.Create(busName);
    bus.Publish(myEvent);

```

---
## Modules

Module is a self instantiated component loaded by application from config file. 
It is usually be used for components registeration, initialization or running as a background task.
The module do not take any parameter for component constructor.

**name**: module name.

**type**: component class type.

        <modules>
            <module name="iocRegistry"  type="myApp.ext.componentRegistry myApp.ext" />
        </modules>

---
## Maps

Map is a named collection of keys and values object in config file.
The keys are unique in all map elements in one named map collection.

A named map could be associated to one plugin component or application code looking up.

**maps name**: map collection name. The name usually is the provider interface type name of the pluggable components.

**maps default**: Specifies a default mapped value if map key is blank or map key is not in the map collection.

**add key**: map element key value. The map key value could be a regular expression or any text for mapping match.

**add value**: map element mapped value for particular key.

Examples: postcode maps

```xml
        <maps name="postcode" default="X1Y 0A0">
            <add key="1"  value="M1R 0E9" />
            <add key="2"  value="M3C 0C1" />
        </maps>
```

To define mapped plugin-components the maps name should be the plugin component interface name.

Below is an example of mapping config setting. The default event bus provider is dbBusProvider.
A named bus "apInvoiceBus" will use "rabbitMQProvider" provider.
The named bus "securityAuditBus" will use "kafkaProvider" provider.

```xml
        <maps name="qshine.IEventBusProvider" default="dbBusProvider">
            <add key="defaultBus"  value="dbBusProvider" />
            <add key="apInvoiceBus"  value="rabbitMQProvider" />
            <add key="securityAuditBus"  value="kafkaProvider" />
            <add key="*.Audit.*"  value="kafkaProvider" />
        </maps>
```

Example: select rabbitMQProvider provider as event bus

```c#
    var busName = "apInvoiceBus";
    var busProvider = ApplicationEnvironment.GetProvider<IEventBusProvider>(busName);
    var bus = busProvider.Create(busName);
    bus.Publish(myEvent);

```

Example: select default dbBusProvider provider as event bus

```c#
    var busProvider = ApplicationEnvironment.GetProvider<IEventBusProvider>();
    var bus = busProvider.Create("XYZ");
    bus.Publish(myEvent);

```

**Note:**: If the maps collection is not present for the plugin component, 
the name parameter of GetProvider(name) is used to specify a provider name instead of the map name.
If the name is blank then it will select first available provider.

Example: without maps collection below code is used to get dbBusProvider provider

```c#
    var busProvider = ApplicationEnvironment.GetProvider<IEventBusProvider>("dbBusProvider");
    var bus = busProvider.Create("XYZ");
    bus.Publish(myEvent);

```


If the map key is a regular expression, it can match to more than one  provider names for GetProvider(name).

Example: select plug-in components by partial name.

```xml
    <maps name="qshine.EventSourcing.IEventStoreProvider">
      <add key="AP.Invoice.C5" value="XEventstore" />
      <add key="AP.Invoice.*" value="kafkaEventstore" />
      <add key="*" value="eventstore" />
    </maps>
```

```c#
    var esProvider1 = ApplicationEnvironment.GetProvider<IEventStoreProvider>("AP.Invoice.C1");
    //esProvider1 is kafkaEventstore

    var esProvider2 = ApplicationEnvironment.GetProvider<IEventStoreProvider>("AP.Invoice.C5");
    //esProvider2 is XEventstore

    var esProvider3 = ApplicationEnvironment.GetProvider<IEventStoreProvider>("AnyName");
    //esProvider3 is eventstore

```






