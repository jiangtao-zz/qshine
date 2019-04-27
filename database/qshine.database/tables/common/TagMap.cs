using qshine.database.tables.organization;

namespace qshine.database.tables.common
{
    /// <summary>
    /// TagMap is a cross association table between Tags and business object.
    /// </summary>
    public class TagMap : SqlDDLTable
    {
        /// <summary>
        /// Ctor::
        /// http://howto.philippkeller.com/2005/04/24/Tags-Database-schemas/
        /// </summary>
        public TagMap()
            : base("cm_tagMap", "Common", "Tag map table.", "comData", "comIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)

                //Refer to tag Id
                .AddColumn("tag_id", System.Data.DbType.Int64, 0,
                reference: new Tag().PkColumn,
                comments: "Tag id.")

                //Refer to a tag value and business object data
                .AddColumn("value_id", System.Data.DbType.Int64, 0, 
                reference: new TagData().PkColumn, 
                comments: "Refer to a tag value and business object data.")

                ;

        }
    }
}
