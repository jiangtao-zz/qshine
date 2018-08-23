using System;
using qshine;
namespace sqliteSample
{
	public class DatabaseScripts
	{
		
		public void CreateDatabase(string databaseName)
		{
			var sqliteDb = new Database("System.Data.SQLite", @"Data Source=sample.db");

			using (var db = new DbClient(sqliteDb))
			{
				db.Sql(
@"create table sample (
	number_field number,
	text_field varchar(250),
	date_field date
);");
			}
		}

	}
}
