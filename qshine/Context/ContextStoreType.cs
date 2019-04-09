using System;
using System.Collections.Generic;
using System.Text;

namespace qshine
{
    /// <summary>
    /// Context store type
    /// </summary>
    public enum ContextStoreType:int
    {
        /// <summary>
        /// Static context across entire application.
        /// It is same as static variable
        /// </summary>
        Static,
        /// <summary>
        /// Local thread static context within same thread.
        /// Each thread maintain its own context.
        /// It is same as ThreadStatic variable.
        /// </summary>
        ThreadLocal,
        /// <summary>
        /// Call static context within same call execution path through its child threads.
        /// The context value set in parent thread will be shared by child thread.
        /// 
        /// </summary>
        CallContext
    }
}
