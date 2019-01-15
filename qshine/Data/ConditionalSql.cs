using System;
using System.Collections.Generic;
using System.Text;

namespace qshine
{
    /// <summary>
    /// Define a conditional sql statement to be executed based on other selection sql result
    /// </summary>
    /// <example>
    /// var conditionalSql = new ConditionalSql {
    ///     Sql = "create sequence A",
    ///     ConditionSql = "select 1 from user_sequence where sequence_name = 'A'",
    ///     Condition = (result) => result!="1"
    /// };
    /// dbClient.Sql(conditionalSql);
    /// 
    /// It will create a sequence A if it doesn't exist.
    /// 
    /// </example>
    public class ConditionalSql
    {
        public ConditionalSql(string sql)
            :this(new List<DbSqlStatement>(){ new DbSqlStatement(sql) }, null, null)
        {
        }

        public ConditionalSql(string sql, DbParameters parameters)
            :this(new List<DbSqlStatement>() { new DbSqlStatement(sql, parameters) }, null, null)
        {
        }

        public ConditionalSql(string sql, string conditionalSql, Func<string, bool> condition)
            : this(new List<DbSqlStatement>() { new DbSqlStatement(sql, null)}, 
                  new DbSqlStatement(conditionalSql), condition)
        {
        }

        public ConditionalSql(DbSqlStatement sql, DbSqlStatement conditionalSql, Func<string, bool> condition)
            :this(new List<DbSqlStatement> { sql }, conditionalSql, condition)
        {
        }

        public ConditionalSql(List<DbSqlStatement> sqls, DbSqlStatement conditionalSql, Func<string, bool> condition)
        {
            Check.HaveValue(sqls);

            Sqls = sqls;
            ConditionSql = conditionalSql;
            Condition = condition;
        }

        /// <summary>
        /// Execution Sql
        /// </summary>
        public List<DbSqlStatement> Sqls { get; private set; }

        /// <summary>
        /// Single result selection SQL statement. 
        /// The result will be used to determine whether a Sql to be executed.
        /// </summary>
        public DbSqlStatement ConditionSql { get; private set; }

        /// <summary>
        /// A condition to be evaluated to against ConditionalSql result.
        /// </summary>
        public Func<string, bool> Condition { get; set; }


    }
}
