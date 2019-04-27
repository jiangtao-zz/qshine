using qshine.database.tables.organization;

namespace qshine.database.tables.common.email
{
    /// <summary>
    /// Archive business email messages.
    /// </summary>
    public class EmailSentLog : SqlDDLTable
    {
        public EmailSentLog()
            : base("cm_em_sent_log", "Common", "Email sent message.", "comData", "comIndex")
        {
            AddColumn("id", System.Data.DbType.Int64, 0,
                comments: "Email message id.")

                //Specifies an organization
                .AddColumn("org_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true,
                reference: new OrganizationUnit().PkColumn, comments: "Organization id.")

                .AddColumn("subject", System.Data.DbType.String, 256,
                comments: "Email subject.")

                .AddColumn("body", System.Data.DbType.String, 0,
                comments: "Email body.")

                .AddColumn("from_address", System.Data.DbType.String, 256,
                comments: "Email FROM address.")

                .AddColumn("to_recipients", System.Data.DbType.String, 0,
                comments: "Semicolon separated email address list of email TO.")

                .AddColumn("cc_recipients", System.Data.DbType.String, 0,
                comments: "Semicolon separated email address list of email CC.")

                .AddColumn("bcc_recipients", System.Data.DbType.String, 0,
                comments: "Semicolon separated email address list of email BCC.")

                .AddColumn("format_type", System.Data.DbType.String, 50,
                comments: "Email format: Html or Plain")

                .AddColumn("importance", System.Data.DbType.Int16, 0, defaultValue: 0,
                comments: "Email importance. The valid values: 0-Normal, 1-High, 2-Low")

                .AddColumn("attachments", System.Data.DbType.String, 250,
                comments: "Email attachment file id list. All files need be uploaded into system file server.")

                //Email associated to a business module.
                .AddColumn("module_id", System.Data.DbType.String, 50,
                comments: "Associated business module id/name.")

                .AddColumn("doc_ref_type", System.Data.DbType.String, 50,
                comments: "Associated to business document (entity) name.")

                .AddColumn("doc_ref_id", System.Data.DbType.String, 50,
                comments: "Associated to business document id.")

                //Email sender process and result
                .AddColumn("process_id", System.Data.DbType.String, 250,
                comments: "Working process id used to lock record for process.")

                .AddColumn("process_time", System.Data.DbType.DateTime, 0,
                comments: "Process date and time.")

                .AddColumn("status", System.Data.DbType.Int16, 0,
                comments: "Working process status. valid values: 0=Initialized, 1=Ready, 2=waitingProcess, 5=running, 7=terminated_error, 8=terminated_warning, 9=terminated_success")

                .AddColumn("server_name", System.Data.DbType.String, 50,
                comments: "Working process server name.")

                .AddColumn("Retries", System.Data.DbType.Int32, 0,
                comments: "Working process retries number.")

                .AddAuditColumn();
        }
    }
}
