using qshine.database.tables.organization;

namespace qshine.database.tables.job
{
    /// <summary>
    /// Task job log
    /// 
    /// </summary>
    public class TaskJobLog : SqlDDLTable
    {
        /// <summary>
        /// Ctor:
        /// </summary>
		public TaskJobLog()
                    : base("cm_jobLog", "Common", "Task job logs.", "comData", "comIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64, autoIncrease:false)

                //Specifies an organization
                .AddColumn("org_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true,
                reference: new OrganizationUnit().PkColumn, comments: "Organization id.")

                .AddColumn("name", System.Data.DbType.String, 250, allowNull: false,
                comments: "Task name.")

                //Time to run the task action
                .AddColumn("action_time", System.Data.DbType.Date, 0,
                comments: "Time to run the task action.")

                //action name
                .AddColumn("job_action", System.Data.DbType.String, 250,
                comments: "Job action procedure name defined by action type. It could be a command action, C# Class Method, DB procedure, Event handler.")

                //action arguments
                .AddColumn("action_arguments", System.Data.DbType.String, 250,
                comments: "The job process arguments. It is a comma separated text arguments recognized by the job action.")

                //action status
                .AddColumn("status_id", System.Data.DbType.Int16, 0, reference: new ProcessStatus().PkColumn,
                comments: "Job process status.")

                //document
                .AddColumn("document", System.Data.DbType.String, 50,
                comments: "a document associated to the job process.")

                //error/warning message
                .AddColumn("message", System.Data.DbType.String, 256,
                comments: "Error or warning message returned from the process.")

                //Process instance name.
                .AddColumn("process_instance", System.Data.DbType.String, 256,
                comments: "The process instance name.")

                //Process host name.
                .AddColumn("host_server", System.Data.DbType.String, 256,
                comments: "The process host server name (may contain machine name and IP).")

                //Process start date.
                .AddColumn("process_start_date", System.Data.DbType.Date, 0,
                comments: "The date and time the Process started.")

                //Process end date.
                .AddColumn("process_end_date", System.Data.DbType.Date, 0,
                comments: "The date and time the Process ended.")

                //Submit date.
                .AddColumn("submit_date", System.Data.DbType.Date, 0,
                comments: "The date and time the job was submitted.")

                //User name
                .AddColumn("user_name", System.Data.DbType.String, 50,
                comments: "The user who submitted the Job.")
                ;
        }
    }
}
