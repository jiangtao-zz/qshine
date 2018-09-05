using System;
namespace qshine.database.common
{
	/// <summary>
	/// Location table.
	/// Contains location address information
	/// </summary>
	public class Location : SqlDDLTable
	{
		public Location()
			: base("cm_location", "COMMON", "Location address table.", "comData", "comIndex")
		{
			AddPKColumn("id", System.Data.DbType.Int64)
				.AddColumn("enterprise_id", System.Data.DbType.Int32, 0, allowNull: false, defaultValue:0, comments: "Specifies an enterprise account id.")
				.AddColumn("name", System.Data.DbType.String, 150, comments: "Location human readable name. It is usually a business name.")
				.AddColumn("country", System.Data.DbType.String, 50, comments: "ISO 3166 2 letter Country code.")
				.AddColumn("province", System.Data.DbType.String, 150, allowNull: false, comments: "Province or State.")
				.AddColumn("region", System.Data.DbType.String, 150, allowNull: false, comments: "Region, above province.")
				.AddColumn("county", System.Data.DbType.String, 150, comments: "County, below province but above City")
				.AddColumn("city", System.Data.DbType.String, 150, comments: "City, town or lower level administrative division")
				.AddColumn("postal_code", System.Data.DbType.String, 50, comments: "postal code")
				.AddColumn("street_number", System.Data.DbType.String, 50, allowNull: false, comments: "street number")
				.AddColumn("route", System.Data.DbType.String, 150, allowNull: false, comments: "Street, Road, Avenue name")
				.AddColumn("unit", System.Data.DbType.String, 50, allowNull: false, comments: "building unit or room number")
				.AddColumn("floor", System.Data.DbType.String, 50, allowNull: false, comments: "building floor number")
				.AddColumn("latitude", System.Data.DbType.String, 50, comments: "Location latitude.")
				.AddColumn("longitude", System.Data.DbType.String, 50, comments: "Location longitude.")
				.AddColumn("northeast_lat", System.Data.DbType.String, 50, comments: "Location area bound northeast point latitude.")
				.AddColumn("northeast_lng", System.Data.DbType.String, 50, comments: "Location area bound northeast point longitude.")
				.AddColumn("southwest_lat", System.Data.DbType.String, 50, comments: "Location area bound southwest point latitude.")
				.AddColumn("southwest_lng", System.Data.DbType.String, 50, comments: "Location area bound southwest point longitude.")
				.AddColumn("utc_offset", System.Data.DbType.Int32, 0,  comments: "Timezone UTC offset. HHMM*100. Example, -500 means offset -5:00")
				.AddColumn("location_type", System.Data.DbType.String, 100, comments: "Location type. such as building, airport, park, hospital, restaurant, warehouse, school, university, campus")
				.AddColumn("formatted_address", System.Data.DbType.String, 200, comments: "formatted address based on location information. This field should be a readonly field.")
			.AddAuditColumn();
		}
	}
}
