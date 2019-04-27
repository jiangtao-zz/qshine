using qshine.database.tables.security.iam;

namespace qshine.database
{
    /// <summary>
    /// Define a smart entity. 
    /// Smart entity represents a full business entity or a set of additional proeprties for particular business entity (UDF).
    /// Configure smart entity data entry in database will enable application perform entity CRUD action automatically without or minimal additional programming. 
    /// This table defines CRUD specification for smart entity.
    /// </summary>
    public class SmartEntity : SqlDDLTable
    {
        public SmartEntity()
            : base("cm_sm_entity", "Common", "Entity Lookup table.", "comData", "comIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)

                .AddColumn("entity", System.Data.DbType.String, 50, allowNull: false, isIndex: true, 
                comments: "Entity unique name. It usually is a smart entity table name.")

                .AddColumn("table_name", System.Data.DbType.String, 50, allowNull: false, isIndex: true, 
                comments: "Entity data table name.")

                .AddColumn("table_type", System.Data.DbType.String, 50, 
                comments: "Entity data table type. Two types table available for the smart entity. 1: single record. 2: multi-records. 3: json")

                .AddColumn("display_name", System.Data.DbType.String, 250, allowNull: false, 
                comments: "Smart entity display title.")

                .AddColumn("pk_name", System.Data.DbType.String, 50, 
                comments: "Primary key name.")

                .AddColumn("control_keyColumn", System.Data.DbType.String, 50, 
                comments: "A column used to control section behavior.")

                .AddColumn("control_type", System.Data.DbType.String, 50, 
                comments: "Section behavior control type. By column value or by regular expre over the value.")

                .AddColumn("search_template", System.Data.DbType.String, 250, 
                comments: "Entity search template page. using default page if the value is null.")

                .AddColumn("new_template", System.Data.DbType.String, 250, 
                comments: "Create a new entity template page. using default page if the value is null.")

                .AddColumn("edit_template", System.Data.DbType.String, 250, 
                comments: "Read or Update entity template page. using default page if the value is null.")

                .AddColumn("delete_template", System.Data.DbType.String, 250, 
                comments: "Delete entity template page. using default page if the value is null.")

                .AddColumn("app_id", System.Data.DbType.Int64, 0, reference:new Application().PkColumn,
                comments: "Application Id is refer to application detail definition for security and site navigation.")

                .AddAuditColumn();
        }
    }
}