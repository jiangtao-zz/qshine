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
        //
        // Summary:
        //     A transaction is required by the UoW. It uses an ambient transaction if one
        //     already exists. Otherwise, it creates a new transaction before entering the scope.
        //     This is the default value.
        Required = 0,
        //
        // Summary:
        //     A new transaction is always created for the UoW.
        RequiresNew = 1,
        //
        // Summary:
        //     The ambient transaction context is suppressed when creating the UoW. All operations
        //     within the UoW are done without an ambient transaction context.
        Suppress = 2
    }
}
