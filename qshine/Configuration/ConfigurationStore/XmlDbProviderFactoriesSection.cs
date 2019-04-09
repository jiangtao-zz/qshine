using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Configuration.ConfigurationStore
{
    /// <summary>
    /// Parse system.data section for DbProviderFactories
    /// </summary>
    public class XmlDbProviderFactoriesSection
    {
        string _rawXml;
        /// <summary>
        /// Ctor::
        /// </summary>
        /// <param name="rawXml"></param>
        public XmlDbProviderFactoriesSection(string rawXml)
        {
            _rawXml = rawXml;
        }

        /// <summary>
        /// Build DbProviderFactories
        /// </summary>
        public void Parse()
        {
            var factories = new XmlSection(_rawXml);
            if (factories.Items.Count > 0 && factories.Items[0].Name == "DbProviderFactories"
                && factories.Items[0].Items.Count > 0)
            {
                foreach (var item in factories.Items[0].Items)
                {
                    if (item.Name == "remove")
                    {
                        UnRegisterDbProviderFactory(item["invariant"]);
                    }
                    else if (item.Name == "add")
                    {
                        RegisterDbProviderFactory(item["invariant"], item["type"]);
                    }
                }
            }
        }

        /// <summary>
        /// Register DbProviderFactories for .NET CORE.
        /// .NET CORE doesn't implement the factory register.
        /// We have to add piece of code here to make it compatiable with >NET framework
        /// </summary>
        void RegisterDbProviderFactory(string invariantName, string assemblyQualifiedName)
        {
#if NETCORE
            System.Data.Common.DbProviderFactories.RegisterFactory(invariantName, assemblyQualifiedName);
#endif
            //var table = System.Data.Common.DbProviderFactories.GetFactoryClasses();
            //if (table != null)
            //{
            //    foreach (DataRow row in table.Rows)
            //    {
            //        if (row[2].ToString() == invariantName)
            //        {
            //            string name = (string)row[0];
            //            string description = (string)row[1];
            //            string invariant = (string)row[2];
            //            string assemblyType = (string)row[3];
            //            if (assemblyType != assemblyQualifiedName)
            //            {
            //                table.Rows.Remove(row);
            //                table.Rows.Add(name, description, invariant, assemblyQualifiedName);
            //                break;
            //            }
            //        }
            //    }
            //}
        }

        void UnRegisterDbProviderFactory(string invariantName)
        {
#if NETCORE
            System.Data.Common.DbProviderFactories.UnregisterFactory(invariantName);
#endif
        }
    }
}
