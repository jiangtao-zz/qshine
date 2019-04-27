
using qshine.database.tables.organization;

namespace qshine.database.tables.security.iam
{
    /// <summary>
    /// application Module table.
    /// A module usually is a group of application functional areas. Such as:
    /// Finance & Accounting, Human Resources, Project management in ERP system.
    /// It could be a business module, functional module or technical module.
    /// </summary>
    public class Module : SqlDDLTable
    {
        /// <summary>
        /// Ctor::
        /// https://www.erp-information.com/erp-modules.html
        /// 
        /// </summary>
		public Module()
            : base("im_module", "Security", "A group of business application functional areas.", "secData", "secIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)

                //a unique code to identify a business module.
                .AddColumn("code", System.Data.DbType.String, 250, allowNull: false, 
                comments: "Module unique code")

                //Module name
                .AddColumn("name", System.Data.DbType.String, 250, allowNull: false, 
                comments: "Module name")

                //Module description
                .AddColumn("description", System.Data.DbType.String, 250, 
                comments: "Module description")

                //Module version
                .AddColumn("version", System.Data.DbType.String, 20, 
                comments: "Module version number")

            .AddAuditColumn();

            DataVersion = 1; //set system data version

            //Set a technical module
            SetData(1000, "Security", "Security", "Security module", 1);
        }

	}
}
