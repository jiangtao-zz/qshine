using System;

namespace qshine.Utility
{
    /// <summary>
    /// Name mapper is a utility to help mapping a given object to particular text.
    /// You can create a named mapper for one specific usage. The named mapper can be overwritten by new mapper register.
    /// 
    /// For example, the entity audit service uses named mapper (CommonMapperName.AuditEntityNameMapper) 
    /// to map the entity type to type name as audit entity name.
    /// If the application want to translate the entity type to different name, the applciation can register a new mapper for CommonMapperName.AuditEntityNameMapper.
    /// 
    /// The common usuage could be language translation, text terminology mapping for different tenant,
    /// or convert a standard name to a particular name.
    /// 
    /// Usage:
    ///
    ///     var MyEntityAuditName = NamedMapper.MapTo(CommonMapperName.AuditEntityNameMapper, typeof(MyEntity));
    ///     
    /// </summary>
    public class NamedMapper
    {
        /// <summary>
        /// Map an object to other object using a register mapper.
        /// If named mapper is not found, it returns mapObject directly.
        /// </summary>
        /// <param name="mapperName">a mapper register name.</param>
        /// <param name="mapObject">map object</param>
        /// <returns>mapped object</returns>
        public static object MapTo(string mapperName, object mapObject)
        {
            if (_mappers.ContainsKey(mapperName))
            {
                return _mappers[mapperName](mapObject);
            }
            return mapObject;
        }

        /// <summary>
        /// Register a named mapper for particular usage.
        /// </summary>
        /// <param name="mapperName">name of the mapper</param>
        /// <param name="mapper">A mapper delegate which map one text to other.</param>
        public static void Register(string mapperName, Func<object, string> mapper)
        {
            if(_mappers.ContainsKey(mapperName))
            {
                _mappers[mapperName] = mapper;
            }
            else
            {
                _mappers.Add(mapperName, mapper);
            }
        }

        static Utility.SafeDictionary<string, Func<object, object>> _mappers =
            new Utility.SafeDictionary<string, Func<object, object>>();

    }

    /// <summary>
    /// Common mapper names
    /// </summary>
    public enum CommonMapperName
    {
        /// <summary>
        /// Audit entity name mapper.
        /// </summary>
        AuditEntityNameMapper,

    }
}
