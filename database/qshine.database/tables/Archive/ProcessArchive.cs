using System;
namespace qshine.database
{
    /// <summary>
    /// Process archiving contains all data processed by different task processor.
    /// All processed data could be archived in the lower cost storage.
    /// </summary>
    public class ProcessArchive : SqlDDLTable
    {
        public ProcessArchive()
            : base("ar_process_que", "ARCHIVE", "Task Process archiving table.", "arcData", "arcIndex",1,"ARCHIVE")
        {
            AddPKColumn("id", System.Data.DbType.Int64)
                .AddColumn("process_queue_name", System.Data.DbType.String, 50, isIndex:true, comments: "Task process queue name. It usually is a queue table name, such as cm_em_outq")
                .AddColumn("doc_ref_name", System.Data.DbType.String, 50, isIndex: true, comments: "Associated business document (entity) name. It is used to identify which business object the archiving data associated.")
                .AddColumn("doc_ref_id", System.Data.DbType.String, 50, isIndex: true, comments: "Identify a particular entity record the archiving data associated.")
                .AddColumn("data", System.Data.DbType.String, 0, comments: "Json formatted archiving data.")
                .AddColumn("module_id", System.Data.DbType.String, 50, comments: "Associated business module id/name.")
                .AddColumn("process_id", System.Data.DbType.String, 50, comments: "Working process id used to lock record for process.")
                .AddColumn("server_name", System.Data.DbType.String, 50, comments: "Working process server name.")
                .AddAuditColumn();
        }
    }
}
