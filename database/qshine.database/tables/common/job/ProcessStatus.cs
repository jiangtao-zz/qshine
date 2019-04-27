using qshine.database.tables.organization;

namespace qshine.database.tables.job
{
    /// <summary>
    /// Job process status metadata
    /// 
    /// </summary>
    public class ProcessStatus : SqlDDLTable
    {
        /// <summary>
        /// Ctor:
        /// 
        /// </summary>
		public ProcessStatus()
                    : base("cm_job_status", "Common", "Defines job status.", "comData", "comIndex")
        {
            AddPKColumn("status_id", System.Data.DbType.Int16, autoIncrease:false)

                .AddColumn("status", System.Data.DbType.String, 50, allowNull: false,
                comments: "Job status.")
                ;
            DataVersion = 1;

            //0 = Initialized, 1 = Ready, 2 = WaitingProcess, 5 = Running, 7 = Failed, 8 = Warning, 9 = Success
            SetData(0, "Initialized");
            SetData(1, "Ready"); //ready for job process
            SetData(2, "Booked");
            SetData(5, "Running"); //in process
            SetData(6, "Running-1"); //in process step 1
            SetData(7, "Running-2"); //in process step 2
            SetData(8, "Running-3"); //in process step 3
            SetData(30, "Success");//completed sucessfully
            SetData(31, "Warning");//completed with warning
            SetData(32, "Failed"); //completed with error
            SetData(33, "Cancelled"); //Job cancelled by user
            SetData(34, "OnHold"); //Job is on-hold
        }
    }
}
