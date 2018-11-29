Sqlite server Sql DDL Provider

The Sqlite is a serverless database. No installation and configuration for Sqlite component. 
But you need download Nuget package "System.Data.Sqlite" for qshine.database.sqlite project.
The project already has this nuget package referenced.

The "System.Data.Sqlite" package contains a native sqlite component in nuget package. It cannot be loaded directly from plugin folder.
Three options to allow application loading qshine.database.sqlite with sqlite components from plugin folder.

Option 1: Add "qshine.database.sqlite" project reference to the application.
Option 2: Change the "qshine.database.sqlite" project "Sqlite" reference from nuget package reference to assembly reference.
(see project file and uncomment the project related reference.)
Option 3: Add the nuget package "System.Data.Sqlite" in application project or manually update the application .deps.json file by
merging "System.Data.Sqlite" related dependence data. (See AddThisDepsJson.deps.json file)

==================================
Plugin location:

	<qshine>
		<environments>
			<environment name="sqlite" path="config/component/database/sqlite"/>
		</environments>   
	</qshine>
