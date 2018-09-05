1. install Sql Server express localdb (usually, it come with VS)
2. create and start a database server instance
	c:\> sqllocaldb create sampleinstance
	c:\> sqllocaldb start sampleinstance
	c:\> sqllocaldb info sampleinstance
		Name:               sampledbInstance
		Version:            13.1.4001.0
		Shared name:
		Owner:              domain\myname
		Auto-create:        No
		State:              Running
		Last start time:    9/2/2018 12:16:36 AM
		Instance pipe name: np:\\.\pipe\LOCALDB#B7DDEAC4\tsql\query
3. Create a sample database.
	C:\>"C:\Program Files\Microsoft SQL Server\110\Tools\Binn\sqlcmd.exe" -S "np:\\.\pipe\LOCALDB#B7DDEAC4\tsql\query"
		1> create database sampledb;
		2> go
		1> exit

4. Get the connectionstring
	Data Source=(LocalDb)\sampledbinstance;Initial Catalog=sampledb;Integrated Security=True


