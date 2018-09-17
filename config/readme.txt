This "config" folder contains a typical application environment setting and plug-in components.

a typical config folder structure:
  ---   main.config			<< Common application config >>
  ---   ioc.config			<< IoC config file >>
  ---   logger.config			<< Logging config file >>
  ---   database.config			<< database config file >>
  ---   saple.config			<< application sample config file >>

  ---   component/			<< Common plug-in components folder>>

        component/ioc/			<< IoC plug-in components folder >>
        component/ioc/autofac/		<< Autofac plug-in component folder >>

        component/logger/		<< Logging plug-in components folder >>
        component/logger/nlog/		<< nlog plug-in component folder>>

        component/database/		<< Database plug-in folder>>
	component/database/sqlite/	<< Sqlite plug-in >>
	component/database/postgresql/	<< postgresql plug-in >>
	component/database/mysql/	<< mysql plug-in >>
	component/database/oracle/	<< oracle plug-in >>
	component/database/sqlserver/	<< sqlserver plug-in >>

