﻿using System;
namespace qshine.database.common
{
	/// <summary>
	/// Person table.
	/// Contains person base information
	/// </summary>
	public class Person : SqlDDLTable
	{
		public Person()
			: base("cm_person", "COMMON", "Person table.", "comData", "comIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
				.AddColumn("enterprise_id", System.Data.DbType.Int32, 0, false, defaultValue:0, comments: "Specifies an enterprise account id.")
				.AddColumn("first_name", System.Data.DbType.String, 150, true, comments: "First name", isIndex: true)
				.AddColumn("middle_name", System.Data.DbType.String, 50, true, comments: "Middle name")
				.AddColumn("last_name", System.Data.DbType.String, 150, false, comments: "Last name", isIndex: true)
				.AddColumn("full_name", System.Data.DbType.String, 350, true, comments: "Display full name")
				.AddColumn("gender", System.Data.DbType.String, 10, true, comments: "Gender")
				.AddColumn("birth_date", System.Data.DbType.Date, 0, true, comments: "Birth_date")
				.AddColumn("email", System.Data.DbType.String, 100, false, comments: "email address", isIndex: true)
				.AddColumn("phone", System.Data.DbType.String, 50, true, comments: "Primary phone number")
				.AddColumn("phone1", System.Data.DbType.String, 50, true, comments: "Second phone number or cell phone number")
				.AddColumn("home_location_id", System.Data.DbType.UInt64, 0, true, comments: "Home location id", reference:"cm_location:id")
				.AddColumn("work_location_id", System.Data.DbType.UInt64, 0, true, comments: "Workplace location id", reference:"cm_location:id")
			.AddAuditColumn();
		}
	}
}