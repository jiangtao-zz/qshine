using qshine.database.tables.organization;
using System;
namespace qshine.database.tables.common
{
	/// <summary>
	/// Person table.
	/// Contains person base information
	/// </summary>
	public class Person : SqlDDLTable
	{
		public Person()
			: base("cm_person", "Common", "Person table.", "comData", "comIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)

                //Specifies an organization
                .AddColumn("org_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true,
                reference: new OrganizationUnit().PkColumn, comments: "Organization id.")

                .AddColumn("title", System.Data.DbType.String, 100, isIndex: true,
                comments: "Title")

                .AddColumn("first_name", System.Data.DbType.String, 150, isIndex: true,
                comments: "First name")

				.AddColumn("middle_name", System.Data.DbType.String, 50, 
                comments: "Middle name")

				.AddColumn("last_name", System.Data.DbType.String, 150, allowNull: false, 
                comments: "Last name", isIndex: true)

				.AddColumn("full_name", System.Data.DbType.String, 350, 
                comments: "Display full name")

				.AddColumn("gender", System.Data.DbType.String, 10, 
                comments: "Gender")

				.AddColumn("birth_date", System.Data.DbType.Date, 0, 
                comments: "Birth_date")

				.AddColumn("email", System.Data.DbType.String, 100, allowNull: false, isIndex: true,
                comments: "email address")

                .AddColumn("alt_email", System.Data.DbType.String, 100, 
                comments: "alternate email address")

                .AddColumn("phone", System.Data.DbType.String, 50, 
                comments: "Primary phone number")

				.AddColumn("cell_phone", System.Data.DbType.String, 50, 
                comments: "Cell phone number")

                .AddColumn("second_phone", System.Data.DbType.String, 50,
                comments: "Second phone or after hours phone number")

                .AddColumn("home_location_id", System.Data.DbType.UInt64, 0, reference: new Location().PkColumn,
                comments: "Home location id")

				.AddColumn("work_location_id", System.Data.DbType.UInt64, 0, reference: new Location().PkColumn,
                comments: "Workplace location id")

			.AddAuditColumn();
		}
	}
}
