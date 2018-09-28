using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qshine.database.idm
{ 

    /// <summary>
    /// User preference table.
    /// It contains all user preference data. 
    /// </summary>
    public class UserPreference : SqlDDLTable
    {
        public UserPreference()
            : base("im_user_pref", "IDM", "User preference table.", "idmData", "idmIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)
                .AddColumn("user_id", System.Data.DbType.Int64, 0, allowNull: false, comments: "User Id. reference to user table", reference:"im_user:id")
                .AddColumn("name", System.Data.DbType.String, 50, allowNull: false, comments: "User preference data name or type")
                .AddColumn("value", System.Data.DbType.String, 50, allowNull: false, comments: "User preference data value")
                .AddColumn("value2", System.Data.DbType.String, 1000, allowNull: false, comments: "Additional user preference data value.")
                .AddAuditColumn();
        }
    }
}
