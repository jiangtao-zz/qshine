namespace qshine.database.tables.Security
{
    /// <summary>
    /// Security Session activity audit logs table::
    /// Audit log for user security activities.  
    /// Set security audit policy to enable detail session activity audit log.
    /// </summary>
    public class SessionAuditLog: SqlDDLTable
    {
        public SessionAuditLog()
            : base("sec_session_audit", "Log", "Session Logs Table.", "log_Data", "log_Index")
        {
            AddColumn("id", System.Data.DbType.Int64, 0,
                comments: "id refer to log_session_activity.id")

            .AddColumn("module_id", System.Data.DbType.Int64, 0, isIndex: true,
            comments: "Indicates module.")

            .AddColumn("application_id", System.Data.DbType.Int64, 0, isIndex: true,
            comments: "Indicates application.")

            .AddColumn("action", System.Data.DbType.String, 256, isIndex: true,
            comments: "Action name.")

            .AddColumn("result", System.Data.DbType.String, 2000,
            comments: "action result or error message.")

            .AddColumn("action_time", System.Data.DbType.DateTime, 0, isIndex: true,
            comments: "action time in UTC")
            ;
        }
    }
}
