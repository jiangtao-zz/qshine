using System;
namespace qshine.database
{
    /// <summary>
    /// Email template specification.
    /// A email template contains email information with business variable placeholder used to compose a email message.
    /// The email recipients TO and CC column could contain actual email address or email recipient placeholder variable which will be placed with
    /// actual email address from business entity in email compose time.
    /// The email subject and body contains actual email data with placeholder variables. The placeholder variable is a business field value assocaited with the template.
    /// The placeholder variable could be a attachment variable which will generate an attachment from business data.
    /// </summary>
    public class EmailTemplate : SqlDDLTable
    {
        public EmailTemplate()
            : base("cm_em_template", "COMMON", "Lookup and Reference data table.", "comData", "comIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)
                .AddColumn("org_id", System.Data.DbType.Int32, 0, allowNull: false, defaultValue: 0, comments: "Specifies an organization id. It is a system lookup if the value is 0.")
                .AddColumn("subject", System.Data.DbType.String, 250, comments: "Email subject. It could contain placeholder variables defined by each usage.")
                .AddColumn("body", System.Data.DbType.String, 0, comments: "Email body. It could contain placeholder variables defined by each usage.")
                .AddColumn("from_address", System.Data.DbType.String, 250, comments: "Semicolon separated email address list of email FROM.")
                .AddColumn("to_address", System.Data.DbType.String, 250, comments: "Semicolon separated email address list of email TO.")
                .AddColumn("cc_address", System.Data.DbType.String, 250, comments: "Semicolon separated email address list of email CC.")
                .AddColumn("to_address", System.Data.DbType.String, 250, comments: "Semicolon separated email address list of email send to.")
                .AddColumn("format_type", System.Data.DbType.String, 50, comments: "Email format: Html or Plain")
                .AddColumn("doc_ref_type", System.Data.DbType.String, 50, comments: "Associated to business document (entity) name.")
                .AddAuditColumn();
        }
    }
}
