1. download Oracle Database Express from http://www.oracle.com/technetwork/database/database-technologies/express-edition/downloads/index-083047.html
2. install Oracle Database 
3. Set SYSTEM password = royal1
4. Create a new user schema with necessary permission after installation done
	1. Run SQL Command line from "All Programs""
	2. connect system/royal1
	3. SQL> create user sampledb identified by royal1;
	4. SQL> grant CREATE SESSION, ALTER SESSION, CREATE DATABASE LINK, -
			CREATE MATERIALIZED VIEW, CREATE PROCEDURE, CREATE PUBLIC SYNONYM, -
			CREATE ROLE, CREATE SEQUENCE, CREATE SYNONYM, CREATE TABLE, - 
			CREATE TRIGGER, CREATE TYPE, CREATE VIEW, UNLIMITED TABLESPACE -
			to sampledb;
	5.SQL> exit;
5. If you use Oracle ManagedDataAccess you need Remove the section with oracle.manageddataaccess.client from your machine.config.