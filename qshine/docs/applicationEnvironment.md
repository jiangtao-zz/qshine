# Application Environment Configuration
Application environment configuration are the system wide settings that affect application running in deployed environment. 

Environment settings could contain application config files and dependency components.
The config files could be located in different folder and different environment levels.
Each level could contain the same environment variable that will have the same affect for the application.

The environment config files could be shared by many applications running in same host. Different level config files (folders) form a hierarchy of application settings. 
The setting hierarchy overwrite the variable from higher to lower.

The environment config files and dependency (pluggable) components are structured in the same folder.

The application environment setting service provider environment variable settings, dependency (pluggable) component settings, bootstrapper and mapping service.

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
			<environment name="global" config="c:/globalSetting/config" bin="commonDependency"/>
			<!-- app level common setting-->
			<environment name="app" config="config"/>
			<!--QA environment specific setting. It is only available for specified QA server -->
			<environment name="qa" config="c:/globalSetting/qa/config" host="192.168.1.10"/>
			<!--UA environment specific setting. It is only available for specified UA server -->
			<environment name="ua" config="c:/globalSetting/qa/config" host="192.168.1.11"/>
			<!--PRODUCTION environment specific setting. It is only available for specified PRODUCTION server -->
			<environment name="production" config="c:/globalSetting/production/config" host="PRODUCTION_SERVER_NAME"/>
		</environments>
	</appEnv>
``` 

## Environment

The application environment is an infrastructure workspace to host an application with a collection of deployed components and dependency resources. 
The host usually is a physcal machine, virtual machine, or a cloud system built with infrastructure. Same host could have many applications running in different application environment.
In the application development lifcycle, a set of application environments are required for each stage of application deployment and testing. 

The common application environments are:

- DEV environment
- QA environment
- UA environment
- STAGING/PRE-PRODUCTION environment
- PRODUCTION environment

An application environment configuration setting could contain many config files and each can be located in different config folder. 
One config file is responsible for one type of dependency service configuration settings. The dependecy components are located in same folder.

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
			<environment name="qa" config="c:/globalSetting/qa/config" host="192.168.1.10"/>
			<!--UA environment specific setting. It is only available for specified UA server -->
			<environment name="ua" config="c:/globalSetting/qa/config" host="192.168.1.11"/>
			<!--PRODUCTION environment specific setting. It is only available for specified PRODUCTION server -->
			<environment name="production" config="c:/globalSetting/production/config" host="PRODUCTION_SERVER_NAME"/>
		</environments>
``` 


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
			<environment name="myApp" config="config"/>
			<environment name="moduleA" config="c:\company\programs\moduleA\config" />
			<environment name="global" config="c:\company\programs\config" />
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



## Environment variables

Environment variable is a key/value pair setting.

	<!--environment setting -->
	<appEnv>
		<appSettings>
			<add key="key0" value="value1" />
			<add key="key1" value="value2" />
			<add key="key2" value="value2" />
		</appSettings>
	</appEnv>

Get the environment variable in code:

```c#

	var key0 = ApplicationEnvironment.Current["key0"];

```

## Pluggable components