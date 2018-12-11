namespace qshine.database
{
    /// <summary>
    /// Entity lookup columns table. 
    /// Defines columns to be search for entity lookup (search).
    /// </summary>
    public class EntityLookupColumn : SqlDDLTable
    {
        public EntityLookupColumn()
            : base("cm_entity_lookup_c", "COMMON", "Entity Lookup table.", "comData", "comIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)
                .AddColumn("lookup_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true, reference: new EntityLookup().PkColumn, comments: "Entity lookup id.")
                .AddColumn("column_query", System.Data.DbType.String, 250, allowNull: false, comments: "column query.")
                .AddColumn("column_type", System.Data.DbType.Int32, 0, allowNull: false, comments: "column type. 0: text, 1: date, 2: primary key column,")
                .AddColumn("display_name", System.Data.DbType.String, 50, allowNull: false, comments: "Column display name.")
                .AddColumn("size", System.Data.DbType.Int32, 0, comments: "Size of the column to be display. Do not show column if the size is -1")
                .AddColumn("position", System.Data.DbType.Int32, 0, comments: "Column display order.")
                .AddColumn("can_sort", System.Data.DbType.Boolean, 0, comments: "Indicates a sortable column.")
                ;
        }
    }
}