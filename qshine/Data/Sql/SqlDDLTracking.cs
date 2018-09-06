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
        ISqlDialect _nativeDatabaseSyntax;
		List<TrackingTable> _trackingTables;
		List<TrackingColumn> _trackingTableColumns;
		SqlDDLTable _ddlTrackingTable;
		SqlDDLTable _ddlTrackingColumnTable;

        DbClient _dbClient;

		public SqlDDLTracking(ISqlDialect databaseSyntax, DbClient dbClient)
		{
			_nativeDatabaseSyntax = databaseSyntax;
            _dbClient = dbClient;

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
						.AddColumn("column_size", DbType.Int32, 0)
                        .AddColumn("scale", DbType.Int32, 0)
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
            return _nativeDatabaseSyntax.ParameterPrefix + name;
        }

		/// <summary>
		/// Load table tracking information.
		/// </summary>
		public void Load()
		{
			var sql = string.Format(
@"select c.id, t.object_name, c.column_name, c.column_type, c.column_size, c.scale, c.default_value, c.allow_null,
c.reference, c.is_unique, c.is_pk, c.constraint_value, c.auto_increase, c.is_index, c.version, c.created_on, c.updated_on, c.comments 
from {0} c inner join {1} t on c.table_id=t.id
and t.object_type={2}", TrackingColumnTableName, TrackingTableName, ParameterName("p1"));

			_trackingTableColumns = _dbClient.Retrieve(
				(x) =>
			{
				return new TrackingColumn
				{
					Id = x.GetInt64(0),//id
					TableName = x.ReadString(1), //object_name
					ColumnName = x.ReadString(2),//column_name
					ColumnType = x.ReadString(3),//column_type
					Size = x.ReadInt32(4),//size
                    Scale = x.ReadInt32(5),//scale
                    DefaultValue = x.ReadString(6),//default_value
					AllowNull = x.ReadBoolean(7),//allow_null
					Reference = x.ReadString(8),//reference
					IsUnique = x.ReadBoolean(9),//is_unique
					IsPK = x.ReadBoolean(10),//is_pk
					CheckConstraint = x.ReadString(11),//constraint
					AutoIncrease = x.ReadBoolean(12),//auto_increase
					IsIndex = x.ReadBoolean(13),//is_index
					Version = x.GetInt32(14),
					CreatedOn = x.ReadDateTime(15),
					UpdatedOn = x.ReadDateTime(16),
					Comments = x.ReadString(17),
				};
			}, sql, DbParameters.New.Input("p1", (int)TrackingObjectType.Table, DbType.Int32));


			sql = string.Format(
@"select t.id, t.schema_name, t.object_type, t.object_name, t.object_hash, t.version, t.created_on, t.updated_on, t.comments, t.category 
from {0} t where t.object_type={1}", TrackingTableName, ParameterName("p1"));
			
			_trackingTables = _dbClient.Retrieve(
				(x) =>
			{
				var tableName = x.GetString(3);
				return new TrackingTable
				{
					Id = x.GetInt64(0),//id
					SchemaName = x.ReadString(1), //schema name
					ObjectType = (TrackingObjectType)x.GetInt32(2), //object type
					ObjectName = tableName,
					HashCode = x.GetInt64(4),
					Version = x.GetInt32(5),
					CreatedOn = x.ReadDateTime(6),
					UpdatedOn = x.ReadDateTime(7),
					Comments = x.ReadString(8),
					Category = x.ReadString(9),
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

            _dbClient.Sql(sql, DbParameters.New
                .Input("p1", table.SchemaName, System.Data.DbType.String)
                .Input("p2", (int)TrackingObjectType.Table)
                .Input("p3", table.TableName)
                .Input("p4", table.GetHashCode())
                .Input("p5", table.Version)
                .Input("p6", table.Comments)
                .Input("p7", table.Category)
										);
			var tableIdObject = _dbClient.SqlSelect(
                string.Format("select id from {0} where object_type={1} and object_name={2} and {3}",
                TrackingTableName, 
                ParameterName("p1"),
                ParameterName("p2"),
                _nativeDatabaseSyntax.ToSqlCondition("schema_name", "=", table.SchemaName)),
                DbParameters.New
                .Input("p1", (int)TrackingObjectType.Table)
                .Input("p2", table.TableName));

            if (tableIdObject != null)
			{
                long tableId = Convert.ToInt64(tableIdObject);

				//Create tracking table column records
				foreach (var column in table.Columns)
				{
					AddNewTrackingColumn(tableId, column);
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
					sql = string.Format(@"update {0} set column_name={1}, column_type={2}, column_size={3}, scale={4}, default_value={5}, allow_null={6}, 
reference={7}, is_unique={8}, is_pk={9}, constraint_value={10}, auto_increase={11}, is_index={12},
version={13}, updated_on={14}, comments={15}
where id={16}", 
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
                        ParameterName("p15"),
                        ParameterName("p16")
                        );

                    _dbClient.Sql(sql, DbParameters.New
                        .Input("p1", column.Name)
                        .Input("p2", column.DbType.ToString())
						.Input("p3", column.Size)
                        .Input("p4", column.Scale)
                        .Input("p5", Convert.ToString(column.DefaultValue))
						.Input("p6", column.AllowNull)
						.Input("p7", column.Reference)
						.Input("p8", column.IsUnique)
						.Input("p9", column.IsPK)
						.Input("p10", column.CheckConstraint)
						.Input("p11", column.AutoIncrease)
						.Input("p12", column.IsIndex)
						.Input("p13", column.Version)
						.Input("p14", DateTime.Now)
						.Input("p15", column.Comments)
						.Input("p16", trackingColumn.Id)
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

            _dbClient.Sql(sql, DbParameters.New
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

			var sql = string.Format(@"insert into {0} (table_id, column_name, column_type, column_size, scale, default_value, allow_null, reference, is_unique, is_pk, constraint_value, auto_increase, is_index, version, comments)
values({1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15})",
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
                ParameterName("p15")
                );

            _dbClient.Sql(sql, DbParameters.New
                .Input("p1", tableId)
				.Input("p2", column.Name)
				.Input("p3", column.DbType.ToString())
				.Input("p4", column.Size)
                .Input("p5", column.Scale)
                .Input("p6", Convert.ToString(column.DefaultValue))
				.Input("p7", column.AllowNull)
				.Input("p8", column.Reference)
				.Input("p9", column.IsUnique)
				.Input("p10", column.IsPK)
				.Input("p11", column.CheckConstraint)
				.Input("p12", column.AutoIncrease)
				.Input("p13", column.IsIndex)
				.Input("p14", column.Version)
				.Input("p15", column.Comments)
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
