using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Mapper
{
    /// <summary>
    /// Name mapper is a utility to help mapping a given object to somthing else.
    /// You can create mapper for different usage.
    /// 
    /// The common usuage could be language translation, text terminology mapping for different tenant,
    /// or convert a standard name a particular name
    /// </summary>
    public class NameMapper
    {
        public string MapTo(object mapObject)
        {
            if (mapObject == null) return "";

            return mapObject.ToString();
        }

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

        public static Func<object, object> Mapper(string mapperName)
        {
            if (_mappers.ContainsKey(mapperName)) return _mappers[mapperName];
            return null;
        }

        static Utility.SafeDictionary<string, Func<object, object>> _mappers =
            new Utility.SafeDictionary<string, Func<object, object>>();

        static NameMapper()
        {

            //register default common name mapper
            NameMapper.Register(CommonMapperName.AuditEntityNameMapper.ToString()
                , (t)=>
                {
                    Type type = t as Type;
                    if (t == null) return "unknowAuditEntityName";
                    return type.FullName;
                });
        }

        static Func<object, object> AuditEntityNameMapper
        {
            get
            {
                return _mappers[CommonMapperName.AuditEntityNameMapper.ToString()];
            }
        }

        internal static string GetAuditEntityName(Type entityType)
        {
            return (string)AuditEntityNameMapper(entityType);
        }

    }

    public enum CommonMapperName
    {
        AuditEntityNameMapper,

    }
}
