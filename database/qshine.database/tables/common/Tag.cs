using qshine.database.tables.organization;

namespace qshine.database.tables.common
{
    /// <summary>
    /// Tags are a form of metadata (Taxonomy keywords) that can be applied to a form of electronic content 
    /// (business objects such as location, files, web pages, people, video, music).
    /// It helps user find those contents by their own view of data classification.
    /// Metadata tags should be based on the standard taxonomy defined for the organization.
    /// 
    /// A tag system contains three parts:
    ///     Tag part- Specifies a metadata of a tag that can be applied to a type of document
    ///     Tag value part- User assign a value to the tag
    ///     Content (business object) part- Apply a particular tag value to this digital content.
    /// 
    /// 
    /// Example: Tag a "Recreation Centre" on a location object.
    ///     Tag scope = Location
    ///     Tag name: Recreation Centre
    ///     Tag value:  Central Eglinton Community Centre
    ///     Tag content (Location): 160 Eglinton Ave E, Toronto, ON M4P 3B5
    ///     
    ///     Tag -- TagMap -- TagData -- Business object
    ///     
    /// Search by tag name
    /// Search by tag name associated value
    /// 
    /// Tag could be used to define UDF (User defined field) or keyworks.
    /// 

    /// </summary>
    public class Tag : SqlDDLTable
    {
        /// <summary>
        /// Ctor::
        /// https://en.wikipedia.org/wiki/Tag_(metadata)
        /// https://merlinone.com/what-is-metadata-tagging/
        /// https://en.wikipedia.org/wiki/Taxonomy
        /// https://www.ibm.com/support/knowledgecenter/en/SSYJ99_8.5.0/wcm/wcm_config_wcmviewer_seo.html
        /// </summary>
        public Tag()
            : base("cm_tag", "Common", "Tag table.", "comData", "comIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)

                //Specifies an organization
                .AddColumn("org_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true,
                reference: new OrganizationUnit().PkColumn, comments: "Organization id.")

                //Unique tag name within one particular business scope.
                .AddColumn("name", System.Data.DbType.String, 256, allowNull: false, 
                comments: "Tag name.")

                //Unique tag name within one particular business scope.
                .AddColumn("data_type", System.Data.DbType.Int16, 50, allowNull: false, defaultValue:0,
                comments: "Defines a tag metadat type. Valid values: 0 - Free Text, 1 - Lookup, 2 - Number, 3 - Date, 4 - Yes/No ")

                //Tag element specification defined in the system.
                .AddColumn("tag_spec", System.Data.DbType.String, 1000,
                comments: "Tag element specification defined in the system.")

                //Scope the tags to a particular digital content (business object type).
                //It could be "WebSite", "User", "Location" or any business object.
                .AddColumn("scope", System.Data.DbType.String, 50, allowNull: false, 
                comments: "Business scope of a particular digital content.")

                ;
        }
    }
}
