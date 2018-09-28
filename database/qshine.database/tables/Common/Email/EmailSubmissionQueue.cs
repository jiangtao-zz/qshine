using System;
namespace qshine.database
{
    /// <summary>
    /// Email Submission Queue contains message waiting to be sent to email server.
    /// The purpose of the queue is to tracking all emails out from the business system. 
    /// It allows application tracking each sent email associated to specific business entity or application. 
    /// </summary>
    public class EmailSubmissionQueue : SqlDDLTable
    {
        public EmailSubmissionQueue()
            : base("cm_em_outq", "COMMON", "Email submission queue.", "comData", "comIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)
                .AddColumn("subject", System.Data.DbType.String, 250, comments: "Email subject.")
                .AddColumn("body", System.Data.DbType.String, 0, comments: "Email body.")
                .AddColumn("from_address", System.Data.DbType.String, 250, comments: "Semicolon separated email address list of email FROM.")
                .AddColumn("to_address", System.Data.DbType.String, 250, comments: "Semicolon separated email address list of email TO.")
                .AddColumn("cc_address", System.Data.DbType.String, 250, comments: "Semicolon separated email address list of email CC.")
                .AddColumn("to_address", System.Data.DbType.String, 250, comments: "Semicolon separated email address list of email send to.")
                .AddColumn("format_type", System.Data.DbType.String, 50, comments: "Email format: Html or Plain")
                .AddColumn("attachment", System.Data.DbType.String, 250, comments: "Email attachment file id list. All files need be uploaded into system file server.")
                .AddColumn("doc_ref_type", System.Data.DbType.String, 50, comments: "Associated to business document (entity) name.")
                .AddColumn("doc_ref_id", System.Data.DbType.String, 50, comments: "Associated to business document id.")
                .AddColumn("module_id", System.Data.DbType.String, 50, comments: "Associated business module id/name.")
                .AddColumn("process_id", System.Data.DbType.String, 50, comments: "Working process id used to lock record for process.")
                .AddColumn("server_name", System.Data.DbType.String, 50, comments: "Working process server name.")
                .AddColumn("Retries", System.Data.DbType.Int32, 0, comments: "Working process retries number.")
                .AddAuditColumn();
        }
    }
}
