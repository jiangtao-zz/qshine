Wrap dependency injection (DI)/inversion of control(IoC) function in IoC component

1. Select a plugable IoC component for the application (ex: autofac)
	The plugable IoC component could be any stable component which implement qshine.IoC.IoCProvider.

2. Plug selected IoC component through environment configure file.

		<!--providers setting-->
		<components>
			<component name="ioc" interface="qshine.IoC.IIoCProvider" type="qshine.ioc.autofac.Provider, qshine.ioc.autofac"/>
		</components>

3. Register plugable components through configure file

		<!--Register component one by one -->
		<components>
			<component name="mainFileServer" 
				interface="qshine.IFileManagerProvider"
				type="qshine.FileManager" scope="singleton">
				<parameters>
					<parameter name="path" value="//192.168.10.12/fileServer" /> 
					<parameter name="user" value="dev" /> 
					<parameter name="password" value="password" /> 
					<parameter name="domain" value="mydomain" /> 
				</parameters>
			</component>
		</components>

		<!--Register all repository components through single module-->
		<modules>
			<module name="repositoryModule" type="qshine.NHibernateRepository.BootStraper, qshine.NHibernateRepository"/>
		</modules>

3.1 Or, register component from code
	IoC.Register<IMyinterface, MyClass>();

	var container = IoC.CreateContainer();
	container.Register(typeof(IT1), typeof(T1));


4. Add application init code in begin of the application.
	qshine.EnvironmentManager.Boot();

5. Bind IoC container to the context in begin of the request to control IoC lifetime scope.
	protected void Application_BeginRequest(object sender,EventArgs e)
	{
		IoC.Bind();
	}

	protected void Application_EndRequest(object sender, EventArgs e)
	{
		IoC.Unbind();
	}


6. Resolve component 

	IoC.Resolve<T>();

