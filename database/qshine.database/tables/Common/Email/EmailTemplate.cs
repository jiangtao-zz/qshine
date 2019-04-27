
using qshine.database.tables.organization;

namespace qshine.database.tables.common.email
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
            : base("cm_em_template", "Common", "Lookup and Reference data table.", "comData", "comIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)

                //Specifies an organization
                .AddColumn("org_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true,
                reference: new OrganizationUnit().PkColumn, comments: "Organization id.")

                .AddColumn("subject", System.Data.DbType.String, 256, 
                comments: "Email subject. It could contain placeholder variables defined by each usage.")

                .AddColumn("body", System.Data.DbType.String, 0, 
                comments: "Email body. It could contain placeholder variables defined by each usage.")

                .AddColumn("from_recipient", System.Data.DbType.String, 256,
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

                .AddAuditColumn();
        }
    }
}
