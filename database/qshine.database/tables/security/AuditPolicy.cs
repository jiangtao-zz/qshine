using System;
using System.Collections.Generic;
using System.Text;


namespace qshine.database.tables.Security
{
    /// <summary>
    /// Security Session activity audit policy table::
    /// The audit policy configure which level actions need be audited.  
    /// The detail audit information stored in different area.
    /// </summary>
    public class AuditPolicy : SqlDDLTable
    {
        /// <summary>
        /// Ctor::
        /// https://docs.microsoft.com/en-us/windows/security/threat-protection/auditing/basic-security-audit-policy-settings
        /// http://www.oag-bvg.gc.ca/internet/methodology/performance-audit/manual/1191.shtm
        /// https://www.netwrix.com/audit_policy_best_practice.html
        /// </summary>
        public AuditPolicy()
            : base("sec_audit_policy", "Security", "Security audit policy Table.", "secData", "secIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)

            //set module level audit. if only module enabled, it will audit given module actions.
            .AddColumn("module_id", System.Data.DbType.Int64, 0, isIndex: true,
            comments: "Set module level audit. If the value is blank, it audit all modules.")

            //set application level audit. if application enabled, it will audit all application actions.
            .AddColumn("application_id", System.Data.DbType.Int64, 0, isIndex: true,
            comments: "Set application level audit. If the value is blank, it audit all applications.")

            //set action level audit. if action enabled, it will audit given action.
            .AddColumn("action", System.Data.DbType.String, 256, isIndex: true,
            comments: "Set action level audit. If the value is blank, it audit all actions.")

            //set user level audit. If user id specified, only user specific actions will be audited.
            .AddColumn("user_id", System.Data.DbType.Int64, 0, isIndex: true,
            comments: "Set user level audit. If the value is blank, it audit all users actions.")

            //set audit retention period.
            .AddColumn("retention_period", System.Data.DbType.Int64, 0, defaultValue:7,
            comments: "Set audit retention period in year.")

            .AddColumn("enabled", System.Data.DbType.Boolean, 0,
            comments: "Indicates audit enabled.")

            ;
        }
    }
}