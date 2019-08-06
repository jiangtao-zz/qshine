using System;
using qshine.Configuration;
using qshine.database.tables.common.language;
using qshine.Specification;
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
                var validator = new Validator();

                var result = dbBuilder.Build(validator, true);
                if (result == true)
                {
                    Console.WriteLine("The database has been updated sucessfully.");
                }
                else
                {
                    Console.WriteLine("Failed to build database {0}:", dbBuilder.ConnectionStringName);
                    int i = 0;
                    foreach (var e in validator.ValidationResults)
                    {
                        Console.WriteLine("Error {0}:{1}", ++i, e.Error.Message);
                        if (e.Error.Data["sql"] != null)
                        {
                            Console.WriteLine("Sql:{0}", e.Error.Data["sql"]);
                        }
                    }
                }
            }
        }
	}
}
