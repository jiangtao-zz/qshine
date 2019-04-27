using qshine.database.tables.organization;
using System;
namespace qshine.database.tables.common.file
{
    /// <summary>
    /// Define all file location used by application
    /// It usually store application uploaded document. The document could be uploaded into different media. 
    /// Each particular file storage media service provider is responsible for file storage operation. 
    /// The provider could be network share folder provider, colud storage service provider or other document management system.
    /// See IFileStorageProvider for detail implementation and usage.
    /// </summary>
    public class File : SqlDDLTable
    {
        public File()
            : base("cm_file", "Common", "Defines document files used by the application.", "comData", "comIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)

                //Specifies an organization
                .AddColumn("org_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true,
                reference: new OrganizationUnit().PkColumn, comments: "Organization id.")

                .AddColumn("file_name", System.Data.DbType.String, 256, allowNull: false, 
                comments: "File readable name")

                .AddColumn("file_size", System.Data.DbType.Int64, 0, 
                comments: "File size in byte.")

                .AddColumn("description", System.Data.DbType.String, 250, 
                comments: "Document short description.")

                .AddColumn("version", System.Data.DbType.String, 20, 
                comments: "Document version number")

                .AddColumn("storage_provider", System.Data.DbType.String, 50, 
                comments: "File storage provider used to perform file read/write activity.")

                .AddColumn("file_path", System.Data.DbType.String, 500, 
                comments: "Relative file storage path or URL interprated to the provider.")

                .AddColumn("object_name", System.Data.DbType.String, 50, 
                comments: "Associated business document (entity) name. It is used to identify a business object.")

                .AddColumn("object_id", System.Data.DbType.Int64, 0, 
                comments: "Identify a particular business entity record.")

            .AddAuditColumn();

            AddIndex("object_name, object_id", "cm_file_cinx_1");
        }
    }
}
