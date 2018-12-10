using System;
namespace qshine.database.common
{
	/// <summary>
	/// Location Building Hours table.
	/// Contains location Building Hours information
	/// </summary>
	public class BuildingHour : SqlDDLTable
	{
public BuildingHour()
			: base("cm_building_hour", "COMMON", "Location building hours table.", "comData", "comIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
				.AddColumn("location_id", System.Data.DbType.UInt64, 0, allowNull:false, comments: "Link to a location by id.", reference:new Location().PkColumn, isIndex:true)
				.AddColumn("start_1", System.Data.DbType.Int32, 0, comments: "Monday: Open Hours in format of HHMM*100. Example, 930 means 9:30AM. 0 means whole day closed")
				.AddColumn("start_2", System.Data.DbType.Int32, 0, comments: "Tuesday: Open Hours")
				.AddColumn("start_3", System.Data.DbType.Int32, 0, comments: "Wednesday: Open Hours")
				.AddColumn("start_4", System.Data.DbType.Int32, 0, comments: "Thursday: Open Hours")
				.AddColumn("start_5", System.Data.DbType.Int32, 0, comments: "Friday: Open Hours")
				.AddColumn("start_6", System.Data.DbType.Int32, 0, comments: "Saturday: Open Hours")
				.AddColumn("start_7", System.Data.DbType.Int32, 0, comments: "Sunday: Open Hours")
				.AddColumn("end_1", System.Data.DbType.Int32, 0, comments: "Monday: Closed Hours in format of HHMM*100. Example, 1730 means 7:30PM.")
				.AddColumn("end_2", System.Data.DbType.Int32, 0, comments: "Tuesday: Closed Hours")
				.AddColumn("end_3", System.Data.DbType.Int32, 0, comments: "Wednesday: Closed Hours")
				.AddColumn("end_4", System.Data.DbType.Int32, 0, comments: "Thursday: Closed Hours")
				.AddColumn("end_5", System.Data.DbType.Int32, 0, comments: "Friday: Closed Hours")
				.AddColumn("end_6", System.Data.DbType.Int32, 0, comments: "Saturday: Closed Hours")
				.AddColumn("end_7", System.Data.DbType.Int32, 0, comments: "Sunday: Closed Hours")
			.AddAuditColumn();
		}
	}
}
