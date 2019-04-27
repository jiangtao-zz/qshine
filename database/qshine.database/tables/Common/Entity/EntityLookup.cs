namespace qshine.database.tables.common.entity

{
    /// <summary>
    /// Entity lookup table. 
    /// Entity lookup data structure provides a specification for application to search business entity data by designed rules.
    /// Application can build a generic data search or lookup mechanism for the system without individual implementation.
    /// 
    /// It also allows application to build custom search function if default implementation is not sufficient.
    /// </summary>
    public class EntityLookup : SqlDDLTable
    {
        public EntityLookup()
            : base("cm_entity_lookup", "Common", "Entity Lookup table.", "comData", "comIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)

                .AddColumn("entity", System.Data.DbType.String, 50, allowNull: false,isIndex:true, 
                comments: "Entity unique name. It usually is a root entity table name.")

                .AddColumn("display_name", System.Data.DbType.String, 250, allowNull: false, 
                comments: "Display title in entity lookup screen.")

                .AddColumn("query_clause", System.Data.DbType.String, 2000, 
                comments: "Search query sql clause without select clause. It could contain placeholder parameters:{orgid}, {lngid}")

                .AddColumn("pk_condition", System.Data.DbType.String, 300, 
                comments: "sql condition to find one entity by primary key. The primary key placeholder parameters name is {pk}")

                .AddColumn("is_single", System.Data.DbType.Boolean, 0, 
                comments: "Indicates a single or multi-choice lookup")

                .AddColumn("template", System.Data.DbType.String, 250, 
                comments: "Entity lookup template page. using default page if the value is null.")

                .AddColumn("app_id", System.Data.DbType.String, 50, 
                comments: "Scope the lookup for a particular application.")

                .AddAuditColumn();
        }
    }
}