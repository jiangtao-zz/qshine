using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace qshine
{

    public interface IDbTypeMapper
    {
        /// <summary>
        /// List all supportted database provider names. 
        /// Using wildcard to match partail provider name.(*)
        /// </summary>
        string SupportedProviderNames { get;}
        
        /// <summary>
        /// Map common data type and value to supported provider specific native dbtype and value
        /// </summary>
        /// <param name="common">common parameter</param>
        /// <param name="native">native parameter</param>
        /// <returns>returns true if the parameter get mapped.</returns>
        bool MapToNative(IDbDataParameter common, IDbDataParameter native);

        /// <summary>
        /// Map supported provider specific native dbtype and value to common data type and value.
        /// </summary>
        /// <param name="native">native parameter</param>
        /// <param name="common">common parameter</param>
        /// <returns>returns true if the parameter get mapped.</returns>
        bool MapFromNative(IDbDataParameter native, IDbDataParameter common);

    }
    /// <summary>
    /// It provides a custom database data type mapper between C# type, common DbType and native DbType for parameterize Sql.
    /// User can define data type mapper if a data type behavior is not match teh default behavior.
    /// </summary>
    public class DbTypeMapper
    {
        Dictionary<Type, Func<Type, DbType>> _mapperHandlers = new Dictionary<Type, Func<Type, DbType>>();
        /// <summary>
        /// Construct DbTypeMapper for particualr database provider.
        /// Note: data mapper is provider specific. All the mapper defined for particular database provider will be combine togather.
        /// </summary>
        /// <param name="dbProviderName">common data provider name from DbProviderFactory</param>
        public DbTypeMapper(string dbProviderName)
        {

        }

        /// <summary>
        /// Map a C# data type to common DbType
        /// </summary>
        /// <param name="dataType">an object data type to be mapped to DbType.</param>
        /// <param name="mapper">A custom map handler to convert data type to DbType </param>
        public void MapDataType(Type dataType, Func<Type, DbType> mapper)
        {
            if (!_mapperHandlers.ContainsKey(dataType))
            {
                _mapperHandlers.Add(dataType, mapper);
            }
        }

        /// <summary>
        /// Map Common DbType to native DbType from datap rovider
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="mapper"></param>
        public void MapDataType(CommonDbParameter parameter, Func<Type, DbType> mapper)
        {

        }
    }
}
