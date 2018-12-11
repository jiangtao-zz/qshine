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
    public class MyDatabase:SqlDDLDatabase
    {
        public MyDatabase(Database database)
            :base(database)
        {
            //Register Common tables
            AddTable(new Location());
            AddTable(new BuildingHour());
            AddTable(new Person());
            AddTable(new LookupType());
            AddTable(new Lookup());
            AddTable(new Module());
            AddTable(new Application());
            AddTable(new Language());
            AddTable(new Translation());

            //Register Identity Manager
            AddTable(new User());
            AddTable(new Group());
            AddTable(new Role());
            AddTable(new Principal());
            AddTable(new RoleMember());
            AddTable(new GroupMember());

            //Register Resources
            AddTable(new SecureResourceType());
            AddTable(new ResourceOperation());
            AddTable(new SecureResource());
        }
    }
}
