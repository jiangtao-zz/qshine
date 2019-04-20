using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.database.tables.Security
{
    /// <summary>
    /// Security (Login) Session logs table::
    /// Audit logs of user session. Set security policy to enable session audit log.
    /// </summary>
    public class SessionLog: SqlDDLTable
    {
        public SessionLog()
            : base("sec_session_log", "Security", "Session Logs Table.", "sec_Data", "sec_Index")
        {
            AddColumn("id", System.Data.DbType.Int64, 0, 
                comments: "id backup from session table")

                .AddColumn("session_token", System.Data.DbType.String, 256, isIndex:true,
                comments: "Session Id or token or encrypted session id for the long-running session.")

                .AddColumn("user_id", System.Data.DbType.Int64, 0, isIndex: true, reference: new security.iam.User().PkColumn, 
                comments: "user id")

                .AddColumn("expiration", System.Data.DbType.DateTime, 0,
                comments: "Session expiration time in UTC")

                .AddColumn("created_on", System.Data.DbType.DateTime, 0, isIndex: true,
                comments: "Session creation time in UTC")

                .AddColumn("updated_on", System.Data.DbType.DateTime, 0,
                comments: "Session last activity time in UTC")

                .AddColumn("client_ip", System.Data.DbType.String, 50,
                comments: "User remote IP address or other evidence.")

                .AddColumn("latitude", System.Data.DbType.Double, 0,
                comments: "User GPS geolocation latitude.")

                .AddColumn("longitude", System.Data.DbType.Double, 0,
                comments: "User GPS geolocation longitude.")

                .AddColumn("server_ip", System.Data.DbType.String, 50,
                comments: "Session management server IP address.")

                .AddColumn("session_type", System.Data.DbType.String, 50,
                comments: "Session type and scope. It indicates the usage and security scope of the session.")
                
                .AddColumn("data", System.Data.DbType.String, 5000,
                comments: "Session private data.")
                    ;

        }
    }
}
