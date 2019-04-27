using qshine.database.tables.organization;
using qshine.database.tables.security.iam;

namespace qshine.database.tables.Security
{
    /// <summary>
    /// Security user account policy table::
    /// The account policy apply to enterprise and group level.
    /// </summary>
    public class AccountPolicy : SqlDDLTable
    {
        /// <summary>
        /// Ctor::
        /// MS::https://docs.microsoft.com/en-us/windows/security/threat-protection/security-policy-settings/account-policies
        /// AWS::https://docs.aws.amazon.com/IAM/latest/UserGuide/id_credentials_passwords_account-policy.html
        /// LENOVO::https://flexsystem.lenovofiles.com/help/index.jsp?topic=%2Fcom.ibm.acc.cmm.doc%2Fcmm_password_policy_settings.html
        /// STF::https://uit.stanford.edu/security/hipaa/info-access-policy
        /// </summary>
        public AccountPolicy()
            : base("sec_account_policy", "Security", "Account Policy Table.", "secData", "secIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)
                //Specifies an organization
                .AddColumn("org_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true,
                reference: new OrganizationUnit().PkColumn, comments: "Organization id.")


                //apply policy to specific group
                .AddColumn("group_id", System.Data.DbType.Int64, 0,
                isIndex: true, reference: new Group().PkColumn, 
                comments: "Group id. It could be null which indicates the policy apply to all group accounts")

                //enforce password history
                .AddColumn("pwd_history", System.Data.DbType.Int16, 0, defaultValue:24,
                comments: "Enforce password history. The valid value is from 0 to 24. 0 means no enforcement.")

                //Maximun password age
                .AddColumn("pwd_age", System.Data.DbType.Int16, 0, defaultValue: 60,
                comments: "Set maximum password age (in day) to expire passwords. 0 means password will never expire.")

                //Minimum password length
                .AddColumn("pwd_length", System.Data.DbType.Int16, 0, defaultValue: 8,
                comments: "Set minimum password length.")

                //Complexity password required
                .AddColumn("pwd_length", System.Data.DbType.Int16, 0, defaultValue: 1,
                comments: "Enable complexity password requirement. 0: Disabled, 1: Enabled, other: Not defined (for future)")

                //Password change on first access
                .AddColumn("pwd_chg_first", System.Data.DbType.Int16, 0,
                comments: "Change the password when user access system in first time. 1: true, others: ignore.")

                //Account lockout threshold
                .AddColumn("lko_threshold", System.Data.DbType.Int32, 0, defaultValue:10,
                    comments: "Set account lockout threshold. The account will be locked out after the retry exceeds the threshold.")

                //Account lockout duration
                .AddColumn("lko_duration", System.Data.DbType.Int32, 0, defaultValue: 15,
                comments: "Set account lockout duraction (in minute). The locked account will be unlock automatically after lockout duration.")

                //Session time-out
                .AddColumn("session_timeout", System.Data.DbType.Int32, 0, defaultValue: 30,
                comments: "Set session time-out duration (in minute).")

                //User login authentication method.
                //SSO, OAUTH2, SAML, BASIC, CERTIFICATE, JWT.
                //For SSO, authentication provider name must be present
                .AddColumn("auth_method", System.Data.DbType.String, 50,
                comments: "Authentication method.")

                //Security authentication provider
                .AddColumn("auth_provider", System.Data.DbType.String, 250,
                comments: "The name of authentication provider. It usually is a mapping name point to user authentication provider.")

                //Security account provider
                //The provider is required if the user stored in out-of-box system, such as AD, Security product, OAUTH2 userinfo provider.
                .AddColumn("account_provider", System.Data.DbType.String, 250,
                comments: "The name of user account provider. It usually is a mapping name point to a user account provider.")

                //audit
                .AddAuditColumn()
                    ;

        }
    }
}
