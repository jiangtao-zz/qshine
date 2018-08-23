Application environment manager.
It manages your application lifecycle.

1. Call EnvironmentManager.Boot() in the begin of the application to initialize application. 
   It will load all configure files, dlls and initialize plugable components (include providers).
   It also call assembly bootstrap loader from each assembly available for the application. 

2. Get provider by provider type

	var myProvider = EnvironmentManager.GetProvider<providerInterface>();

	Each provider interface must inherted from IProvider.

3. Get environment variable

	var value = EnvironmentManager["abc"]

	The environment variable come from configure files. The closest level vailable overwrite lower level one.

4. Find config file by name.  The closest level file overwrite lower level one.

	var fullName = EnvironmentManager.GetConfigFilePathIfAny(configFileName);