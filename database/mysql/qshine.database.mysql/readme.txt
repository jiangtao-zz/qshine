MySQL server Sql DDL Provider

Solution 1:
1. Install MySQL Server.
Download Wamp from https://sourceforge.net/projects/wampserver/ and install MySql  /WampServer 3/WampServer 3.0.0/Addons/Mysql/wampserver3_x86_addon_mysql5.7.23.exe
2. Install MySql Connector Net8.0.12
3. connect to MySQL server using default user name root, password blank.
	mysql>use mysql
	mysql>update user set authentication_string=password('royal1') where user='root';
	mysql>FLUSH PRIVILEGES;
	mysql>GRANT ALL PRIVILEGES ON *.* TO 'dev'@'localhost' IDENTIFIED BY 'royal1';
	mysql>quit 



4. ConnectionString = "server=localhost;user=dev;database=testDB;port=3306;password=royal1;"
==================================

Solution 2:
1. Download Mysql server from https://dev.mysql.com/downloads/file/?id=480823 (No thanks, just start my download.)
2. Install MySql server and Workbench
3. set root password to royal
4. create user dev/royal1
5. create testDB connection



Plugin location:

	<qshine>
		<environments>
			<environment name="mysql" path="config/component/database/mysql"/>
		</environments>   
	</qshine>