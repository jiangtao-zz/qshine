﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace qshine
{
    /// <summary>
    /// Sql statement
    /// </summary>
    public class DbSqlStatement
    {
        #region Ctro.
        /// <summary>
        /// Ctro.
        /// </summary>
        /// <param name="sql"></param>
        public DbSqlStatement(string sql)
            :this(sql, null)
        {}
        /// <summary>
        /// Ctro.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public DbSqlStatement(string sql, DbParameters parameters)
            :this(CommandType.Text, sql, parameters)
        {}
        /// <summary>
        /// Ctro.
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public DbSqlStatement(CommandType commandType, string sql, DbParameters parameters)
        {
            CommandType = commandType;
            Sql = sql;
            Parameters = parameters;
        }
        #endregion

        /// <summary>
        /// Statement command type
        /// </summary>
        public CommandType CommandType { get; private set; }

        /// <summary>
        /// Sql statement or store procedure/function
        /// </summary>
        public string Sql { get; private set; }

        /// <summary>
        /// Parameters
        /// </summary>
        public DbParameters Parameters { get; private set; }

        /// <summary>
        /// Statement result
        /// </summary>
        public object Result { get; set; }
    }
}
