using qshine.database.tables.organization;

namespace qshine.database.tables.common
{
    /// <summary>
    /// TagData contains tag values and digital content reference.
    /// The digital content could be referenced by URI or internal business object id.
    /// A web URL could be tagged with META tag. This TagData value can be applied to the web page.
    /// 
    /// </summary>
    public class TagData : SqlDDLTable
    {
        /// <summary>
        /// Ctor::
        /// </summary>
        public TagData()
            : base("cm_tagData", "Common", "Tag value and content reference table.", "comData", "comIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)

                //Tag value is a user assigned value to classified content.
                .AddColumn("tag_value", System.Data.DbType.String, 250,
                comments: "Tag value is a user assigned value to classified content.")

                //Apply to digital content. It refers to a business object data.
                .AddColumn("object_id", System.Data.DbType.Int64, 0,
                //reference: new Enterprise().PkColumn, 
                comments: "Business object id.")

                //Apply to digital content. It is an URI points to particular business object data (digital content)
                .AddColumn("uri", System.Data.DbType.String, 1000,
                comments: "URI of business object data (digital content)")
                ;

        }
    }
}
