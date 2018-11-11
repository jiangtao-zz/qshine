using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;


namespace qshine
{
    public class XmlSection
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public List<XmlSection> Items { get; set; }

        // The inner dictionary.
        Dictionary<string, string> dictionary
            = new Dictionary<string, string>();

        // This property returns the number of elements
        // in the inner dictionary.
        public int Count
        {
            get
            {
                return dictionary.Count;
            }
        }

        public string this[string name]
        {
            get
            {
                string result;
                if(dictionary.TryGetValue(name, out result))
                {
                    return result;
                }
                return null;
            }
            set
            {
                dictionary[name] = value;
            }
        }
    }
    public class XmlHelper
    {
        XmlSection _dynamicObject;

        public XmlHelper(string rawXml)
        {
            var node = XDocument.Parse(rawXml);
            _dynamicObject = new XmlSection();
            XElement topNode = node.Elements().First();
            Parse(_dynamicObject, topNode);
        }

        public XmlSection XmlSection
        {
            get
            {
                return _dynamicObject;
            }
        }
        public static bool Parse(XmlSection parent, XElement node)
        {
            parent.Name = node.Name.LocalName;
            parent.Value = node.Value;
            if (node.HasAttributes)
            {
                foreach (var attribute in node.Attributes())
                {
                    parent[attribute.Name.LocalName] = attribute.Value;
                }
            }

            if (node.HasElements)
            {
                parent.Items = new List<XmlSection>();
                foreach (XElement childNode in node.Elements())
                {
                    var data = new XmlSection();
                    if (Parse(data, childNode))
                    {
                        parent.Items.Add(data);
                    }
                }
            }
            return true;
        }
    }
}
