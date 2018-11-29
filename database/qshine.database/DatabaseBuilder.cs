using qshine.database.common;
using qshine.database.common.language;
using qshine.database.idm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qshine.database
{
    public class DatabaseBuilder
    {
        string _databaseName;
        public DatabaseBuilder(string databaseName)
        {
            _databaseName = databaseName;
        }

        public bool Build()
        {
            using (var dbBuilder = new SqlDDLBuilder(_databaseName))
            {
                dbBuilder
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

                var result = dbBuilder.Build(true);
                if (result == true)
                {
                    Console.WriteLine("The database has been updated sucessfully.");
                }
                else
                {
                    Console.WriteLine("Failed to build database {0}. Last error is [{1}].", dbBuilder.ConnectionStringName, dbBuilder.LastErrorMessage);
                }
                return result;
            }
        }


    }
}
