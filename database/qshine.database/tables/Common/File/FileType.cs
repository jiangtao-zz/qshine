using System;
namespace qshine.database.tables.common.file
{
    /// <summary>
    /// Define all file types the application supportted.
    /// </summary>
    public class FileType : SqlDDLTable
    {
        public FileType()
            : base("cm_file_type", "Common", "Defines document file types used by application.", "comData", "comIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)

                .AddColumn("mime_type", System.Data.DbType.String, 100, allowNull: false, 
                comments: "A MIME type/Internet Media Type")

                .AddColumn("extension", System.Data.DbType.String, 50, 
                comments: "File extension.")

                .AddColumn("description", System.Data.DbType.String, 250, 
                comments: "File type description.")

                .AddColumn("icon", System.Data.DbType.String, 256,
                comments: "Common file type icon uri")
                ;
        }
    }
}
