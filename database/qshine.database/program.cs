using System;
using qshine.Configuration;
using qshine.database.tables.common.language;
//using qshine.LogInterceptor;

namespace qshine.database
{
	public class program
	{
		public static void Main()
		{
			ApplicationEnvironment.Build("app.config");

            var database = new MyDatabase(new Database("testDatabase"));

            using (var dbBuilder = new SqlDDLBuilder(database, null))
            {
                var error = BatchException.SkipException;
                var result = dbBuilder.Build(error, true);
                if (result == true)
                {
                    Console.WriteLine("The database has been updated sucessfully.");
                }
                else
                {
                    Console.WriteLine("Failed to build database {0}:", dbBuilder.ConnectionStringName);
                    int i = 0;
                    foreach (var e in error.Exceptions)
                    {
                        Console.WriteLine("Error {0}:{1}", ++i, e.Message);
                        if (e.Data["sql"] != null)
                        {
                            Console.WriteLine("Sql:{0}", e.Data["sql"]);
                        }
                    }
                }
            }
        }
	}
}
