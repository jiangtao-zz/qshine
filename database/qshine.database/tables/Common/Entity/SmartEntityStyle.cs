﻿namespace qshine.database.tables.common.entity
{
    /// <summary>
    /// Smart entity additional information.
    /// Defines entity, section or column level additional specification for customization or additional control.
    /// The additional data could be used for data import/export, defines special type of column data
    /// Note: This is an optional record.
    /// </summary>
    public class SmartEntityStyle : SqlDDLTable
    {
        public SmartEntityStyle()
            : base("cm_sm_entity_style", "Common", "Smart Entity additional style and control table.", "comData", "comIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)

                .AddColumn("entity_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true, reference: new SmartEntity().PkColumn, 
                comments: "FK:Smart entity id.")

                .AddColumn("section_id", System.Data.DbType.Int64, 0, reference: new SmartEntitySection().PkColumn, 
                comments: "FK:Smart entity section id.")

                .AddColumn("column_id", System.Data.DbType.Int64, 0, reference: new SmartEntityColumn().PkColumn, 
                comments: "FK:Smart entity column id.")

                .AddColumn("name", System.Data.DbType.String, 250, allowNull: false, 
                comments: "style name to identify a particular usage.")

                .AddColumn("value", System.Data.DbType.String, 0, 
                comments: "CLOB: Additional style and control data.")
                ;
        }
    }
}