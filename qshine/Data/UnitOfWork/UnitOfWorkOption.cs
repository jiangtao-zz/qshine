using System;
using System.Collections.Generic;
using System.Text;

namespace qshine
{
    /// <summary>
    /// Unit of Work option,
    /// The option is similar as System.Transactions.TransactionScopeOption.
    /// </summary>
    public enum UnitOfWorkOption
    {
        /// <summary>
        ///     A transaction is required by the UoW. It uses an ambient transaction if one
        ///     already exists. Otherwise, it creates a new transaction before entering the scope.
        ///     This is the default value.
        /// </summary>
        Required = 0,

        /// <summary>
        ///     A new transaction is always created for the UoW.
        /// </summary>
        RequiresNew = 1,

        /// <summary>
        ///     The ambient transaction context is suppressed when creating the UoW. All operations
        ///     within the UoW are done without an ambient transaction context.
        /// </summary>
        Suppress = 2
    }
}
