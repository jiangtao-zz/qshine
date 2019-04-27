using qshine.database.tables.organization;

namespace qshine.database.tables.job
{
    /// <summary>
    /// Job scheduler register an recurring job for different type of task.
    /// The scheduler trigger could be a timer or received event.
    /// 
    /// </summary>
    public class TaskJobScheduler : SqlDDLTable
    {
        /// <summary>
        /// Ctor:
        /// https://docs.microsoft.com/en-us/windows/desktop/taskschd/task-scheduler-start-page
        /// </summary>
		public TaskJobScheduler()
                    : base("cm_task_scheduler", "Common", "Register a job scheduler.", "comData", "comIndex")
        {
            AddPKColumn("id", System.Data.DbType.Int64)

                //Specifies an organization
                .AddColumn("org_id", System.Data.DbType.Int64, 0, allowNull: false, isIndex: true,
                reference: new OrganizationUnit().PkColumn, comments: "Organization id.")

                .AddColumn("name", System.Data.DbType.String, 250, allowNull: false,
                comments: "Job scheduler name.")

                //Schedule type
                .AddColumn("schedule_type", System.Data.DbType.String, 50, allowNull: false,
                comments: "Schedule types: Once, Immediate, Interval, Daily, Weekly, Monthly, Event,")

                //Start date and time in case schedule type is Once.
                .AddColumn("start_on", System.Data.DbType.Date, 0,
                comments: "Scheduler start on date and time.")

                .AddColumn("end_on", System.Data.DbType.Date, 0,
                comments: "Scheduler end on date and time")

                //Interval value by schedule type: 
                // Interval: 10 - every 10 seconds and ignore scheduled days and time
                // Daily: 10 - every 10 days
                // Weekly: 2 - every 2 week. it need combine with scheduled_days.
                // Once, Immediate, Monthly, Event: n/a.
                .AddColumn("interval_num", System.Data.DbType.Int32, 0,
                comments: "Interval number value based on schedule type, in second, day and week.")

                //Scheduled days is a comma separated list of scheduled days.
                //Once, Immediate, Interval, Daily, Event: n/a
                //Weekly: 1 to 7 (1: Monday, ...6: Saturday, 7: Sunday). Example: 6,7
                //Monthly: format is (MM-)DD. MM is 1 to 12 (1=Jan, 2=Feb, ...12=Dec). DD is 1 to 32 (1=1st, 2=2nd, 3=3rd, ... 31, 32= last day)
                //Ex: 1-1 (Jan-1),2-32 (last day of Feb), 5-1 (May 1), 1 (first day of the month), 32 (every month end)
                //The Monthly format also could be NN:WW. NN is week in month from 1 to 5. WW is weekday from 1-7 (1: Monday,...6: Saturday, 7: Sunday).
                //Ex: 1:1 (on first Monday), 5:7 (on last week Sunday) 
                .AddColumn("scheduled_days", System.Data.DbType.String, 250,
                comments: "Defines scheduled list of days based on schedule type.")
                
                //Scheduled time. example: 24:00 (or 00:00) = midnight, 12:00 = noon, 17:30
                .AddColumn("scheduled_time", System.Data.DbType.String, 6,
                comments: "Scheduled time in format of hh24:mm.")

                .AddColumn("event_bus", System.Data.DbType.String, 50,
                comments: "event bus name for Evnt type scheduler")

                .AddColumn("event_type_name", System.Data.DbType.String, 250,
                comments: "defines a type of event for Event type scheduler")

                //job action type such as CMD, C#Library, DB-Procedure, Event
                .AddColumn("action_type", System.Data.DbType.String, 50, 
                comments: "defines a job action type")

                .AddColumn("job_action", System.Data.DbType.String, 250, 
                comments: "Job action procedure name defined by action type. It could be a command action, C# Class Method, DB procedure, Event handler.")

                .AddColumn("action_arguments", System.Data.DbType.String, 250, 
                comments: "formatted comma separated action arguments recognized by job action.")

                .AddColumn("inactive_date", System.Data.DbType.Date, 0,
                comments: "Inactivated date")




                ;
            DataVersion = 1;
        }
    }
}
