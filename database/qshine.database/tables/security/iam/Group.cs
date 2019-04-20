using qshine.database.organization;
using System;
namespace qshine.database.security.iam
{
	/// <summary>
	/// Group table.
    /// A group is a collection of users.
	/// </summary>
	public class Group : SqlDDLTable
	{
        /// <summary>
        /// Ctor::
        /// https://docs.aws.amazon.com/IAM/latest/UserGuide/id.html
        /// https://docs.microsoft.com/en-us/azure/governance/management-groups/index
        /// </summary>
		public Group()
			: base("im_group", "Security", "User group table.", "secData", "secIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
                
                //Specifies an organization
                .AddColumn("enterprise_id", System.Data.DbType.Int64, 0, allowNull: false,
                isIndex: true, reference: new Enterprise().PkColumn,
                comments: "Enterprise organization id. For a service group, teh value could be -1.")

                //group name::The name should be immutable.
                .AddColumn("group_name", System.Data.DbType.String, 150, allowNull: false, 
                comments: "group name", isUnique: true, isIndex: true)

                //group description
				.AddColumn("description", System.Data.DbType.String, 500, 
                comments: "group detail description")

				.AddAuditColumn();
		}
	}
}
