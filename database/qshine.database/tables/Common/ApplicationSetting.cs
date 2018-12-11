using System;
namespace qshine.database.common
{
    /// <summary>
    /// Application module configuration/setting.
    /// It stores business application/module setting data for each organization
    /// </summary>
    public class ApplicationSetting : SqlDDLTable
    {
        public ApplicationSetting()
            : base("cm_app_cfg", "COMMON", "Defines application varaibles for particular organization.", "comData", "comIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)
                .AddColumn("org_id", System.Data.DbType.Int64, 0, allowNull: false, comments: "Multi-tenant: organization id")
                .AddColumn("name", System.Data.DbType.String, 50, allowNull: false, comments: "Application setting variable name.")
                .AddColumn("category", System.Data.DbType.String, 50, comments: "Variable category")
                .AddColumn("value", System.Data.DbType.String, 250, comments: "Application setting variable value")
                .AddColumn("module_id", System.Data.DbType.Int64, 0, reference:new Module().PkColumn, comments: "Refer to business module.")
                .AddColumn("app_id", System.Data.DbType.Int64, 0, reference: new Application().PkColumn, comments: "Refer to business application.")
            .AddAuditColumn();
        }
    }
}
