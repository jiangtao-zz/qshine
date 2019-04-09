using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;


namespace qshine
{
    /// <summary>
    /// Defines a configuration XML Section.
    /// Work with XmlHelper to load config section from config file
    /// <![CDATA[
    ///     <system.data>
    ///        <!-- !!!!!   -->
    ///        <DbProviderFactories>
    ///            <!--Sqlite Data provider-->
    ///            <remove invariant="System.Data.SQLite.EF6"/>
    ///            <add name = "SQLite Data Provider (Entity Framework 6)" 
    ///                  invariant = "System.Data.SQLite.EF6"
    ///                  description = ".NET Framework Data Provider for SQLite (Entity Framework 6)"
    ///                  type = "System.Data.SQLite.EF6.SQLiteProviderFactory, System.Data.SQLite.EF6" />
    ///           <remove invariant = "System.Data.SQLite" />
    ///           <add name = "SQLite Data Provider" invariant = "System.Data.SQLite"
    ///                  type = "System.Data.SQLite.SQLiteFactory, System.Data.SQLite" />
    ///           <!--MySQL Data provider-->Text Value
    ///        </DbProviderFactories >
    ///     </system.data >
    /// ]]>
    /// </summary>
    public class XmlSection
    {
        /// <summary>
        /// Ctor::
        /// </summary>
        public XmlSection() { }

        /// <summary>
        /// Ctor::
        /// </summary>
        /// <param name="rawXml"></param>
        public XmlSection(string rawXml)
        {
            var node = XDocument.Parse(rawXml);
            XElement topNode = node.Elements().First();
            Parse(this, topNode);
        }
        private bool Parse(XmlSection parent, XElement node)
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

        /// <summary>
        /// Get/Set name attribute
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Get/set value attribute
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Get/Set items
        /// </summary>
        public List<XmlSection> Items { get; set; }

        // The inner dictionary.
        Dictionary<string, string> dictionary
            = new Dictionary<string, string>();

        /// <summary>
        /// This property returns the number of elements in the inner dictionary.
        /// </summary>
        public int Count
        {
            get
            {
                return dictionary.Count;
            }
        }

        /// <summary>
        /// Get value by the name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string this[string name]
        {
            get
            {
                if (dictionary.TryGetValue(name, out string result))
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
}
