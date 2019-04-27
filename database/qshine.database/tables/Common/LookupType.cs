using qshine.database.tables.organization;
using System;
namespace qshine.database.tables.common
{
	public class LookupType : SqlDDLTable
	{
		public LookupType()
			: base("cm_lookup_type", "Common", "Lookup type table.", "comData", "comIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
                //Specifies an organization
                .AddColumn("org_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true,
                reference: new OrganizationUnit().PkColumn, comments: "Organization id.")

                .AddColumn("lookup_type", System.Data.DbType.String, 150, allowNull: false, 
                comments: "Defines a type of lookup.")

				.AddColumn("scope", System.Data.DbType.String, 50, 
                comments: "Defines a scope of the lookup. the scope could be system or customized scope.")

				.AddColumn("app_id", System.Data.DbType.String, 50, 
                comments: "Scope the lookup for a particular application.")

				.AddAuditColumn();

			AddIndex("org_id, lookup_type", "cm_lookup_type_inx1");
		}
	}
}
