using qshine.database.organization;
using System;
namespace qshine.database.security.iam
{
    /// <summary>
    /// Business module and application configuration/setting table.
    /// It stores business application/module setting data for each organization
    /// </summary>
    public class ApplicationSetting : SqlDDLTable
    {
        public ApplicationSetting()
            : base("im_app_cfg", "Security", "Defines application varaibles for particular organization.", "secData", "secIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)
                //apply to specific organization
                .AddColumn("enterprise_id", System.Data.DbType.Int64, 0, reference: new Enterprise().PkColumn,
                comments: "Enterprise organization id. It could be null which indicates the page applies to all organization accounts")
                //apply to specific module
                .AddColumn("module_id", System.Data.DbType.Int64, 0, reference: new Module().PkColumn, 
                comments: "Refer to a business module.")
                //aply to specific application
                .AddColumn("app_id", System.Data.DbType.Int64, 0, reference: new Application().PkColumn, 
                comments: "Refer to business application.")

                //configuration category
                .AddColumn("category", System.Data.DbType.String, 50, 
                comments: "setting category")

                //configuration name key
                .AddColumn("name", System.Data.DbType.String, 50, allowNull: false,
                comments: "setting name or key.")

                //configuration value
                .AddColumn("value", System.Data.DbType.String, 250, 
                comments: "setting value.")

            .AddAuditColumn();
        }
    }
}
