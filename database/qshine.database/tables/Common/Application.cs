﻿using System;
namespace qshine.database.common
{
	/// <summary>
	/// Module application table.
	/// Contains all applications available for the system.
	/// </summary>
	public class Application : SqlDDLTable
	{
public Application()
			: base("cm_application", "COMMON", "Defines business application.", "comData", "comIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
				.AddColumn("module_id", System.Data.DbType.Int64, 0, false, comments: "module id", reference:"cm_module:id")
				.AddColumn("code", System.Data.DbType.String, 50, false, comments: "application unique code")
				.AddColumn("name", System.Data.DbType.String, 150, false, comments: "application name")
				.AddColumn("description", System.Data.DbType.String, 250, true, comments: "application description")
				.AddColumn("version", System.Data.DbType.String, 20, true, comments: "application version number. major.minor[build[revision]]")
			.AddAuditColumn();
		}
	}
}