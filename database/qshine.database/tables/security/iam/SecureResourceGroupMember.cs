using System;
namespace qshine.database.security.iam
{
    /// <summary>
    /// Secure resource group member table.
    /// The group member could be a secure resource or resource group.
    /// Note: The member is also used to build a resource hierarchy.
    /// </summary>
    public class SecureResourceGroupMember : SqlDDLTable
    {
        /// <summary>
        /// Ctor::
        /// 
        /// </summary>
		public SecureResourceGroupMember()
            : base("im_res_group_member", "Security", "Resource group member table.", "secData", "secIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)

                .AddColumn("enterprise_id", System.Data.DbType.Int32, 0, allowNull: false, defaultValue: 0,
                comments: "Specifies an enterprise account id.")

                //Refer to an role
                .AddColumn("group_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true, reference: new SecureResourceGroup().PkColumn,
                comments: "resource group id")

                //refer to a valid role member
                .AddColumn("member_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true
                , comments: "member id. It could be resource id or other resource group id.")

                //resource group member type
                .AddColumn("member_type", System.Data.DbType.Int64, 0, allowNull: false, defaultValue: 0,
                comments: "Resource group member type. 0: resource, 1: resource group")

                .AddAuditColumn();
        }
    }
}
