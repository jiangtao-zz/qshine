using System;
namespace qshine.database
{
	public class LookupType : SqlDDLTable
	{
		public LookupType()
			: base("cm_lookup_type", "COMMON", "Lookup type table.", "comData", "comIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
				.AddColumn("enterprise_id", System.Data.DbType.Int32, 0, false, defaultValue:0, comments: "Specifies an enterprise account id. It is a system lookup if the value is 0.")
				.AddColumn("org_id", System.Data.DbType.Int32, 0, false, defaultValue:0, comments: "Specifies an organization id. It is a system lookup if the value is 0.")
				.AddColumn("lookup_type", System.Data.DbType.String, 150, false, comments: "Defines a type of lookup.")
				.AddColumn("scope", System.Data.DbType.String, 50, true, comments: "Defines a scope of the lookup. the scope could be system or customized scope.")
				.AddColumn("app_id", System.Data.DbType.String, 50, true, comments: "Scope the lookup for a particular application.")
				.AddColumn("module_id", System.Data.DbType.String, 50, true, comments: "Business owner of the lookup code. Module is a business logical area.")
				.AddAuditColumn();
			AddIndex("enterprise_id,org_id,lookup_type", "cm_lookup_type_inx1");
		}
	}
}
