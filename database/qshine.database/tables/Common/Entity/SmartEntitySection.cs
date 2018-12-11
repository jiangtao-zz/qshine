namespace qshine.database
{
    /// <summary>
    /// Smart entity columns section. A columns section is a logic collection of data fields for one particular entity record.
    /// From UI prespective a section could be a UI group, section, tab or single page (or step). 
    /// From data prespective a section data could related to one particular key field value outside the section.
    /// This table defines a section specification for different usage.
    /// 
    /// Note: It could have a control key column defined to associate the section data to a particular type of record. 
    /// If the control type is by column value then the section data is only applied to matched column value record.
    /// The control type could be a regular expression, in this case, it uses a regular expression to evaluate the control value.
    /// 
    /// The section is not mandatory in smart entity data structure.
    /// </summary>
    public class SmartEntitySection : SqlDDLTable
    {
        public SmartEntitySection()
            : base("cm_sm_entity_sec", "COMMON", "Smart Entity section table.", "comData", "comIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)
                .AddColumn("org_id", System.Data.DbType.Int32, 0, allowNull: false, defaultValue: 0, comments: "Specifies an organization id apply to this section. It is a system record if the value is 0.")
                .AddColumn("entity_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true, reference: new SmartEntity().PkColumn, comments: "FK:Smart entity id.")
                .AddColumn("name", System.Data.DbType.String, 250, allowNull: false, comments: "smart entity section name.")
                .AddColumn("display_name", System.Data.DbType.String, 50, allowNull: false, comments: "Section display name.")
                .AddColumn("position", System.Data.DbType.Int32, 0, comments: "Section position, start from 0.")
                .AddColumn("control_keyValue", System.Data.DbType.String, 50, comments: "A control column value used to drive section data behavior. The control column name is defined in smatrt entity table.")
                .AddAuditColumn();
        }
    }
}