namespace qshine.database
{
    /// <summary>
    /// Smart entity columns.
    /// Defines column specification of smart entity record.
    /// The smart entity column fields data could be stored in single record or multiple records or just stored in a single field (as json data).
    /// 
    /// Note: The section is not mandatory in the data structure.
    /// </summary>
    public class SmartEntityColumn : SqlDDLTable
    {
        public SmartEntityColumn()
            : base("cm_sm_entity_col", "Common", "Smart Entity columns table.", "comData", "comIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)
                .AddColumn("entity_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true, reference: new SmartEntity().PkColumn, comments: "FK:Smart entity id.")
                .AddColumn("section_id", System.Data.DbType.Int64, 0, reference: new SmartEntitySection().PkColumn, comments: "FK:Smart entity section id.")
                .AddColumn("column_name", System.Data.DbType.String, 250, allowNull: false, comments: "smart entity column name.")
                .AddColumn("display_name", System.Data.DbType.String, 50, allowNull: false, comments: "Column display name.")
                .AddColumn("position", System.Data.DbType.Int32, 0, comments: "Column position, start from 0.")
                .AddColumn("column_type", System.Data.DbType.Int32, 0, allowNull: false, comments: "column data type. 0: text, 1: date, 2: Number,...")
                .AddColumn("display_type", System.Data.DbType.Int32, 0, allowNull: false, comments: "column display type: 0: text, 1: date, 2: Number, 3: lookup, 4: multi-choice ")
                .AddColumn("mandatory", System.Data.DbType.Boolean, 0, comments: "Indicates a mandatory field.")
                .AddColumn("readonly", System.Data.DbType.Boolean, 0, comments: "Indicates a read-only field.")
                .AddColumn("reg_exp", System.Data.DbType.String, 250, comments: "Data format validation regular expression. It could check number, email, date, time and any data format. A link column placeholder {cN} could be used to validate multiple columns. 'N' is column position number.")
                .AddColumn("max_value", System.Data.DbType.Int32, 0, comments: "Max size of text column or max value of the number.")
                .AddColumn("min_value", System.Data.DbType.Int32, 0, comments: "Min number of the value.")
                .AddColumn("validation_message", System.Data.DbType.String, 500, comments: "Column validation custom message.")
                .AddColumn("lookup_name", System.Data.DbType.String, 50, comments: "Lookup type, entity lookup name.")
                .AddColumn("display_size", System.Data.DbType.Int32, 0, comments: "Column display size.")
                .AddColumn("can_sort", System.Data.DbType.Boolean, 0, comments: "Indicates a sortable column.")
                .AddColumn("can_filter", System.Data.DbType.Boolean, 0, comments: "Indicates a searchable column.")
                ;
        }
    }
}