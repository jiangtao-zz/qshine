Wrap application context in ContextManager component

1. Select a Context store based on type of application.
	Console application can choose CallContextLocalStore or CallContextLogicStore
	Web application can choose HttpContextStore
	WCF application can choose WcfContextStore

2. Plug context in the application when application start
	ContextManager.Current = new qshine.web.HttpContextStore();

2.1. Or, plug context store through provider

		<component name="context" interface="qshine.IContextStore" type="qshine.web.HttpContextStore, qshine.web"/>

2.2. Or, Use default CallContextLocalStore if no any plug context added on. 

