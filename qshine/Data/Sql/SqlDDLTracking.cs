using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace qshine.database
{
	/// <summary>
	/// Tracking DDL execution
	/// </summary>
	public class SqlDDLTracking
	{
		ISqlDatabase _nativeDatabase;
		List<TrackingTable> _trackingTables;
		List<TrackingColumn> _trackingTableColumns;
		SqlDDLTable _ddlTrackingTable;
		SqlDDLTable _ddlTrackingColumnTable;

		public SqlDDLTracking(ISqlDatabase database)
		{
			_nativeDatabase = database;
		}

		/// <summary>
		/// The name of the tracking table.
		/// </summary>
		public const string TrackingTableName = "sys_ddl_object";
		/// <summary>
		/// The name of the tracking column table.
		/// </summary>
		public const string TrackingColumnTableName = "sys_ddl_column";

		/// <summary>
		/// Gets the tracking table structure.
		/// </summary>
		/// <value>The tracking table.</value>
		public SqlDDLTable TrackingTable
		{
			get
			{
				if (_ddlTrackingTable == null)
				{
					_ddlTrackingTable = new SqlDDLTable(TrackingTableName, "System ddl object tracking table", "sysData", "sysIndex");
					_ddlTrackingTable
						.AddPKColumn("id", DbType.Int64)
						.AddColumn("schema_name", DbType.String, 100)
						.AddColumn("object_type", DbType.Int32, 0)
						.AddColumn("comments", DbType.String, 1000)
						.AddColumn("category", DbType.String, 150)
						.AddColumn("object_name", DbType.String, 100, isIndex:true)
						.AddColumn("object_hash", DbType.Int64, 0)
						.AddColumn("version", DbType.Int32, 0, defaultValue: 1)
						.AddColumn("created_on", DbType.DateTime, 0, defaultValue: SqlReservedWord.SysDate)
						.AddColumn("updated_on", DbType.DateTime, 0, defaultValue: SqlReservedWord.SysDate);

				}
				return _ddlTrackingTable;
			}
		}

		/// <summary>
		/// Gets the tracking column table structure.
		/// </summary>
		/// <value>The tracking column table.</value>
		public SqlDDLTable TrackingColumnTable
		{
			get
			{
				if (_ddlTrackingColumnTable == null)
				{
					_ddlTrackingColumnTable = new SqlDDLTable(TrackingColumnTableName, "System ddl table column tracking table", "sysData", "sysIndex");
					_ddlTrackingColumnTable
						.AddPKColumn("id", DbType.Int64)
						.AddColumn("table_id", DbType.Int64,0, reference:TrackingTableName+":id")
						.AddColumn("column_name", DbType.String, 100)
						.AddColumn("comments", DbType.String, 1000)
						.AddColumn("column_type", DbType.String, 100)
						.AddColumn("size", DbType.Int32, 0)
						.AddColumn("default_value", DbType.String, 250)
						.AddColumn("allow_null", DbType.Boolean, 0)
						.AddColumn("reference", DbType.String, 100)
						.AddColumn("is_unique", DbType.Boolean, 0)
						.AddColumn("is_pk", DbType.Boolean, 0)
						.AddColumn("constraint_value", DbType.String, 250)
						.AddColumn("auto_increase", DbType.Boolean, 0)
						.AddColumn("is_index", DbType.Boolean, 0)
						.AddColumn("version", DbType.Int32, 0, defaultValue: 1)
						.AddColumn("created_on", DbType.DateTime, 0, defaultValue: SqlReservedWord.SysDate)
						.AddColumn("updated_on", DbType.DateTime, 0, defaultValue: SqlReservedWord.SysDate);
				}
				return _ddlTrackingColumnTable;
			}
		}

        string ParameterName(string name)
        {
            return _nativeDatabase.ParameterPrefix + name;
        }

		/// <summary>
		/// Load table tracking information.
		/// </summary>
		public void Load()
		{
			var sql = string.Format(
@"select c.id, t.object_name, c.column_name, c.column_type, c.size, c.default_value, c.allow_null,
c.reference, c.is_unique, c.is_pk, c.constraint_value, c.auto_increase, c.is_index, c.version, c.created_on, c.updated_on, c.comments 
from {0} c inner join {1} t on c.table_id=t.id
and t.object_type={2}", TrackingColumnTableName, TrackingTableName, ParameterName("p1"));

			_trackingTableColumns = _nativeDatabase.DBClient.Retrieve(
				(x) =>
			{
				return new TrackingColumn
				{
					Id = x.GetInt64(0),//id
					TableName = x.GetString(1), //object_name
					ColumnName = x.GetString(2),//column_name
					ColumnType = x.GetString(3),//column_type
					Size = x.ReadInt32(4),//size
					DefaultValue = x.ReadString(5),//default_value
					AllowNull = x.GetBoolean(6),//allow_null
					Reference = x.ReadString(7),//reference
					IsUnique = x.GetBoolean(8),//is_unique
					IsPK = x.GetBoolean(9),//is_pk
					CheckConstraint = x.ReadString(10),//constraint
					AutoIncrease = x.GetBoolean(11),//auto_increase
					IsIndex = x.GetBoolean(12),//is_index
					Version = x.GetInt32(13),
					CreatedOn = x.ReadDateTime(14),
					UpdatedOn = x.ReadDateTime(15),
					Comments = x.GetString(16),
				};
			}, sql, DbParameters.New.Input("p1", (int)TrackingObjectType.Table, DbType.Int32));


			sql = string.Format(
@"select t.id, t.schema_name, t.object_type, t.object_name, t.object_hash, t.version, t.created_on, t.updated_on, t.comments, t.category 
from {0} t where t.object_type={1}", TrackingTableName, ParameterName("p1"));
			
			_trackingTables = _nativeDatabase.DBClient.Retrieve(
				(x) =>
			{
				var tableName = x.GetString(3);
				return new TrackingTable
				{
					Id = x.GetInt64(0),//id
					SchemaName = x.GetString(1), //schema name
					ObjectType = (TrackingObjectType)x.GetInt32(2), //object type
					ObjectName = tableName,
					HashCode = x.GetInt64(4),
					Version = x.GetInt32(5),
					CreatedOn = x.ReadDateTime(6),
					UpdatedOn = x.ReadDateTime(7),
					Comments = x.GetString(8),
					Category = x.GetString(9),
					Columns = _trackingTableColumns.FindAll(y=>y.TableName==tableName)
				};
			}, sql, DbParameters.New.Input("p1",  (int)TrackingObjectType.Table, DbType.Int32));

		}

		/// <summary>
		/// Adds to tracking history.
		/// </summary>
		/// <param name="table">Table.</param>
		public void AddToTrackingTable(SqlDDLTable table)
		{
			//Create a tracking table record
			var sql = string.Format("insert into {0} (schema_name, object_type,object_name,object_hash,version, comments, category) values ({1},{2},{3},{4},{5},{6},{7})",
                TrackingTableName,
                ParameterName("p1"),
                ParameterName("p2"),
                ParameterName("p3"),
                ParameterName("p4"),
                ParameterName("p5"),
                ParameterName("p6"),
                ParameterName("p7"));

			_nativeDatabase.DBClient.Sql(sql, DbParameters.New
                .Input("p1", table.SchemaName, System.Data.DbType.String)
                .Input("p2", (int)TrackingObjectType.Table)
                .Input("p3", table.TableName)
                .Input("p4", table.GetHashCode())
                .Input("p5", table.Version)
                .Input("p6", table.Comments)
                .Input("p7", table.Category)
										);
			var tableId = _nativeDatabase.DBClient.SqlSelect(
                string.Format("select id from {0} where schema_name={1} and object_type={2} and object_name={3}",
                TrackingTableName, 
                ParameterName("p1"),
                ParameterName("p2"),
                ParameterName("p3")),
                DbParameters.New
                .Input("p1", table.SchemaName)
                .Input("p2", (int)TrackingObjectType.Table)
                .Input("p3", table.TableName));

            if (tableId != null)
			{
				//Create tracking table column records
				foreach (var column in table.Columns)
				{
					AddNewTrackingColumn((long)tableId, column);
				}
			}
			else
			{
				throw new Exception(string.Format("Unexpected error: couldn't find tracking table Id for table {0}", table.TableName));
			}
		}

		/// <summary>
		/// Updates the tracking table columns if any column structure changed.
		/// </summary>
		/// <param name="trackingTable">Tracking table.</param>
		/// <param name="table">the table to be updated.</param>
		public void UpdateTrackingTableColumns(TrackingTable trackingTable, SqlDDLTable table)
		{
			string sql;

			//Create tracking table column records
			foreach (var column in table.Columns)
			{
				bool columnChanged = false;
				var trackingColumn = trackingTable.Columns.SingleOrDefault(x => x.ColumnName == column.Name);

				if (trackingColumn != null && column.Version > trackingColumn.Version)
				{
					//column name changed
					columnChanged = true;
				}
				else if (trackingColumn == null)
				{
					if (column.ColumnNameHistory != null && column.ColumnNameHistory.Contains(trackingColumn.ColumnName))
					{
						//column property changed
						columnChanged = true;
					}
					else
					{
						//new column
						AddNewTrackingColumn(trackingTable.Id, column);
					}
				}

				if (columnChanged)
				{
					//Column attribute changed
					sql = string.Format(@"update {0} set column_name={1}, column_type={2}, size={3}, default_value={4}, allow_null={5}, 
reference={6}, is_unique={7}, is_pk={8}, constraint_value={9}, auto_increase={10}, is_index={11},
version={12}, updated_on={13}, comments={14}
where id={15}", 
                        TrackingColumnTableName,
                        ParameterName("p1"),
                        ParameterName("p2"),
                        ParameterName("p3"),
                        ParameterName("p4"),
                        ParameterName("p5"),
                        ParameterName("p6"),
                        ParameterName("p7"),
                        ParameterName("p8"),
                        ParameterName("p9"),
                        ParameterName("p10"),
                        ParameterName("p11"),
                        ParameterName("p12"),
                        ParameterName("p13"),
                        ParameterName("p14"),
                        ParameterName("p15"));

                    _nativeDatabase.DBClient.Sql(sql, DbParameters.New
                        .Input("p1", column.Name)
                        .Input("p2", column.DbType.ToString())
						.Input("p3", column.Size)
						.Input("p4", column.DefaultValue)
						.Input("p5", column.AllowNull)
						.Input("p6", column.Reference)
						.Input("p7", column.IsUnique)
						.Input("p8", column.IsPK)
						.Input("p9", column.CheckConstraint)
						.Input("p10", column.AutoIncrease)
						.Input("p11", column.IsIndex)
						.Input("p12", column.Version)
						.Input("p13", DateTime.Now)
						.Input("p14", column.Comments)
						.Input("p15", trackingColumn.Id)
					);
				}
			}
		}


		/// <summary>
		/// Updates the tracking table if table name change.
		/// </summary>
		/// <param name="trackingTable">Tracking table.</param>
		/// <param name="table">Table.</param>
		public void UpdateTrackingTable(TrackingTable trackingTable, SqlDDLTable table)
		{
			var sql = string.Format(
                "update {0} set schema_name={1}, object_name={2}, object_hash={3}, version={4}, comments={5}, category={6} where id={7}",
                TrackingTableName,
                ParameterName("p1"),
                ParameterName("p2"),
                ParameterName("p3"),
                ParameterName("p4"),
                ParameterName("p5"),
                ParameterName("p6"),
                ParameterName("p7")
                );

			_nativeDatabase.DBClient.Sql(sql, DbParameters.New
				.Input("p1", table.SchemaName, System.Data.DbType.String)
				.Input("p2", table.TableName)
				.Input("p3", table.GetHashCode())
				.Input("p4", table.Version)
				.Input("p5", table.Comments)
				.Input("p6", table.Category)
				.Input("p7", trackingTable.Id)
                );
		}

		/// <summary>
		/// Adds the new tracking column.
		/// </summary>
		/// <param name="tableId">Table identifier.</param>
		/// <param name="column">Column.</param>
		void AddNewTrackingColumn(long tableId, SqlDDLColumn column)
		{

			var sql = string.Format(@"insert into {0} (table_id, column_name, column_type, size, default_value, allow_null, reference, is_unique, is_pk, constraint_value, auto_increase, is_index, version, comments)
values({1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14})",
                TrackingColumnTableName,
                ParameterName("p1"),
                ParameterName("p2"),
                ParameterName("p3"),
                ParameterName("p4"),
                ParameterName("p5"),
                ParameterName("p6"),
                ParameterName("p7"),
                ParameterName("p8"),
                ParameterName("p9"),
                ParameterName("p10"),
                ParameterName("p11"),
                ParameterName("p12"),
                ParameterName("p13"),
                ParameterName("p14")
                );

			_nativeDatabase.DBClient.Sql(sql, DbParameters.New
                .Input("p1", tableId)
				.Input("p2", column.Name)
				.Input("p3", column.DbType.ToString())
				.Input("p4", column.Size)
				.Input("p5", column.DefaultValue)
				.Input("p6", column.AllowNull)
				.Input("p7", column.Reference)
				.Input("p8", column.IsUnique)
				.Input("p9", column.IsPK)
				.Input("p10", column.CheckConstraint)
				.Input("p11", column.AutoIncrease)
				.Input("p12", column.IsIndex)
				.Input("p13", column.Version)
				.Input("p14", column.Comments)
                );
        }



		/// <summary>
		/// Gets the tracking table.
		/// </summary>
		/// <returns>The tracking table.</returns>
		/// <param name="tableName">Table name.</param>
		public TrackingTable GetTrackingTable(string tableName)
		{
			if (_trackingTables != null)
			{
				return _trackingTables.SingleOrDefault(x => x.ObjectName == tableName && x.ObjectType == TrackingObjectType.Table);
			}
			return null;
		}
	}
}
