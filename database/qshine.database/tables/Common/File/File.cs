using System;
namespace qshine.database.common
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
            : base("cm_file", "COMMON", "Defines document files used by application.", "comData", "comIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)
                .AddColumn("file_name", System.Data.DbType.String, 250, allowNull: false, comments: "File readable name")
                .AddColumn("file_size", System.Data.DbType.Int64, 0, comments: "File size in byte.")
                .AddColumn("description", System.Data.DbType.String, 250, comments: "Document short description.")
                .AddColumn("version", System.Data.DbType.String, 20, comments: "Document version number")
                .AddColumn("storage_provider", System.Data.DbType.String, 50, comments: "File storage provider used to perform file strage activity.")
                .AddColumn("file_path", System.Data.DbType.String, 500, comments: "Relative file storage path or URL interprated to the provider.")
                .AddColumn("doc_ref_name", System.Data.DbType.String, 50, comments: "Associated business document (entity) name. It is used to identify a business object.")
                .AddColumn("doc_ref_id", System.Data.DbType.String, 50, comments: "Identify a particular business entity record.")
            .AddAuditColumn();

            AddIndex("doc_ref_name,doc_ref_id", "cm_file_cinx_1");
        }
    }
}
