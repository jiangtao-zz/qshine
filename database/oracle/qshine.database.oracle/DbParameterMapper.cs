using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qshine.database.oracle
{
    public class DbParameterMapper : IDbTypeMapper
    {
        //any provider named with Oracle.
        public string SupportedProviderNames => "Oracle";

        /// <summary>
        /// Mapping Oracle specific type
        /// </summary>
        /// <param name="native"></param>
        /// <param name="common"></param>
        /// <returns></returns>
        public bool MapFromNative(IDbDataParameter native, IDbDataParameter common)
        {
            //Some database do not support certain data type
            //hack native data convert
            if (common.DbType == DbType.Boolean)
            {
                bool booleanValue = false;
                if (native.Value != null)
                {
                    var value = native.Value.ToString().ToLower();
                    if ("1,y,t".Contains(value))
                    {
                        booleanValue = true;
                    }
                }
                common.Value = booleanValue;
            }
            else if (common.DbType == DbType.Guid)
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
            if (common.Value is Boolean)
            {
                native.DbType = DbType.Int16;
                native.Value = Convert.ToInt16(common.Value);
                return true;
            }
            else if (common.DbType == DbType.Guid || common.Value is Guid)
            {
                native.DbType = DbType.AnsiString;
                native.Size = 40;
                native.Value = common.Value is Guid ? ((Guid)(common.Value)).ToString("N") : common.Value.ToString();
                return true;
            }
            return false;
        }
    }
}
