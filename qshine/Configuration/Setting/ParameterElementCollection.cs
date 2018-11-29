using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace qshine.Configuration.Setting
{
    /// <summary>
    /// Parameter element collection.
    /// <parameter name="name1" value="value1" type="type1"/>
    /// 
    /// </summary>
    public class ParameterElementCollection : ConfigurationElementCollection<NamedValueElement>
    {
        public ParameterElementCollection()
            : base("parameter")
        {
        }
    }
}
