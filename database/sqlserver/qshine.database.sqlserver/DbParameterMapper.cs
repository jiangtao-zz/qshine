using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qshine.database.sqlserver
{
    public class DbParameterMapper : IDbTypeMapper
    {
        //any provider named with Oracle.
        public string SupportedProviderNames => "SqlClient";

        /// <summary>
        /// Mapping Sql Server specific type
        /// </summary>
        /// <param name="native">A native parameter value to be converted</param>
        /// <param name="common">Hold a converted value</param>
        /// <returns>True if the parameter mapped</returns>
        public bool MapFromNative(IDbDataParameter native, IDbDataParameter common)
        {
            //Some database do not support certain data type
            //hack native data convert
            if (common.DbType == DbType.Guid)
            {
                common.Value = new Guid(native.Value.ToString());
                return true;
            }

            return false;
        }

        public bool MapToNative(IDbDataParameter common, IDbDataParameter native)
        {
            //Some database do not support certain data type
            //hack native data convert
            if (common.DbType == DbType.Guid || common.Value is Guid)
            {
                native.DbType = DbType.Guid;
                native.Size = 40;
                native.Value = common.Value;// is Guid ? ((Guid)(common.Value)).ToString("N") : common.Value.ToString();
                return true;
            }
            return false;
        }
    }
}
