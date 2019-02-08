# Application Environment Configuration
Application environment configuration are the system wide settings that affect application running in deployed environment. 

Environment settings could contain many application config files. The config files could be located in different folders and different environment levels. All levels could contain the same environment variables that have the same affect for the application.

The environment config files could be shared by many applications running in same server or server cluster. Different level config files (folders) form a hierarchy of application settings.

The library provides common application setting service such as environment variable settings, add pluggable service components, instatiate task services, associate add-on component for particular usage and etc.

## Environment config file format
The environment config is a typical .NET application config file which should contain application configuration environment section.

```xml	
	<configSections>
		<section name="appEnv" type="qshine.Configuration.Environment, Qshine.Framework" />
	</configSections>

	<!--environment setting -->
	<appEnv>
		<environments>
			<!-- global level default setting-->
			<environment name="global" config="c:/globalSetting/config"/>
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

The application environment is a workspace to host an application with a collection of deployed components connected to many different resources. 
The application host usually is a physcal machine, virtual machine, or a cloud system. Many application could be located in same host.
In application development lifcycle, a set of environments is required for each stage of deployment. 

The common environments are:

- DEV environment
- QA environment
- UA environment
- STAGING/PRE-PRODUCTION environment
- PRODUCTION environment

A environment config file can contain all those environment setting files. The application will choose different environment setting file based on running host.

## Environment Levels and Hierarchy
The highest level config file is the application config file. As a .NET application this config file is app.config or web.config (web application).
To keep configure setting structure simple, this config file environment section usually only contains next level environment config file paths, but not detail environment variables.
The detail environment variable will set in other level config files. 

The environment section could have more than one environment element. Each element contains a config attribute which points to next level environment config path. The chain of the next level environment config file build config hierarchy from higher to lower.
The higher level environment variable will overwrite lower level variable.

The common application environment config levels are:

- highest level (app.config)
- application global level
- application module level
- application specific


## Environment variables

