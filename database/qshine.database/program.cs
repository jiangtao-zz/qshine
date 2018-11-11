using System;
using qshine.Configuration;
using qshine.database.common;
using qshine.database.common.language;
using qshine.database.idm;
//using qshine.LogInterceptor;

namespace qshine.database
{
	public class program
	{
		public static void Main()
		{
			ApplicationEnvironment.Build();
            //Interceptor.RegisterHandlerType(typeof(DbClientLog));

            using (var db = new SqlDDLBuilder("testDatabase"))
            {
                db
                    //Register Common tables
                    .RegisterTable(new Location())
                    .RegisterTable(new BuildingHour())
                    .RegisterTable(new Person())
                    .RegisterTable(new LookupType())
                    .RegisterTable(new Lookup())
                    .RegisterTable(new Module())
                    .RegisterTable(new Application())
                    .RegisterTable(new Language())
                    .RegisterTable(new Translation())

                    //Register Identity Manager
                    .RegisterTable(new User())
                    .RegisterTable(new Group())
                    .RegisterTable(new Role())
                    .RegisterTable(new Principal())
                    .RegisterTable(new RoleMember())
                    .RegisterTable(new GroupMember())

                    //Register Resources
                    .RegisterTable(new SecureResourceType())
                    .RegisterTable(new ResourceOperation())
                    .RegisterTable(new SecureResource())
                    ;

                var result = db.Build(true);
                if (result == true)
                {
                    Console.WriteLine("The database has been updated sucessfully.");
                }
                else
                {
                    Console.WriteLine("Failed to build database {0}. Last error is [{1}].", db.ConnectionStringName, db.LastErrorMessage);
                }
            }
		}
	}
}
