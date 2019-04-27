using qshine.database.tables.organization;
using System;
namespace qshine.database.tables.common
{
	public class Lookup:SqlDDLTable
	{
		public Lookup()
			: base("cm_lookup", "Common", "Lookup and Reference data table.", "comData", "comIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
                //Specifies an organization
                .AddColumn("org_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true,
                reference: new OrganizationUnit().PkColumn, comments: "Organization id.")

				.AddColumn("lookup_type_id", System.Data.DbType.UInt64, 0, allowNull: false, reference:new LookupType().PkColumn,
                comments: "Defines a type of lookup that the code applied. ")

				.AddColumn("lookup_code", System.Data.DbType.String, 150, allowNull: false, 
                comments: "Defines a unique code or key for particular lookup type.")

				.AddColumn("lookup_value", System.Data.DbType.String, 250, 
                comments: "Lookup detail description. For multi-lingual support, other language description could be found in language 'lg_dictionary' table.")

				.AddColumn("order_position", System.Data.DbType.Int32, 0, 
                comments: "Order position of the lookup.")

				.AddColumn("is_default", System.Data.DbType.Boolean, 0, 
                comments: "Indicates a default lookup value used by the application.")

				.AddColumn("udf1", System.Data.DbType.String, 150, comments: "User defined value 1")
				.AddColumn("udf2", System.Data.DbType.String, 150, comments: "User defined value 2")
				.AddColumn("udf3", System.Data.DbType.String, 150, comments: "User defined value 3")
				.AddColumn("udf4", System.Data.DbType.String, 150, comments: "User defined value 4")
				.AddColumn("udf5", System.Data.DbType.String, 150, comments: "User defined value 5")
				.AddColumn("udf6", System.Data.DbType.String, 150, comments: "User defined value 6")
				.AddColumn("udf7", System.Data.DbType.String, 150, comments: "User defined value 7")
				.AddColumn("udf8", System.Data.DbType.String, 150, comments: "User defined value 8")
				.AddColumn("udf9", System.Data.DbType.String, 150, comments: "User defined value 9")
				.AddColumn("udf10", System.Data.DbType.String, 150, comments: "User defined value 10")

				.AddAuditColumn();
		}
	}
}
