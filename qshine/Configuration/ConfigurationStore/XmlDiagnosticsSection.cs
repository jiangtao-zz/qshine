using qshine.Logger;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Text;

namespace qshine.Configuration.ConfigurationStore
{
    /// <summary>
    /// configure system.diagnostics section parser for default TraceLogger provider
    /// </summary>
    public class XmlDiagnosticsSection
    {
        string _rawXml;
        /// <summary>
        /// Ctor::
        /// </summary>
        /// <param name="rawXml"></param>
        public XmlDiagnosticsSection(string rawXml)
        {
            _rawXml = rawXml;
        }

        /// <summary>
        /// Parse system.diagnostics section
        /// </summary>
        public void Parse()
        {
            var section = new XmlSection(_rawXml);
            if (section.Items.Count > 0)
            {
                dynamic setting = new ExpandoObject();
                setting.Sources = null;
                setting.Switches = null;
                setting.Autoflush = false;

                for (int i=0;i< section.Items.Count;i++)
                {
                    switch(section.Items[i].Name)
                    {
                        case "sources":
                            {
                                var items = section.Items[i].Items;
                                setting.Sources = new dynamic[items.Count];
                                for (int j = 0; j < items.Count; j++)
                                {
                                    setting.Sources[j]
                                        = new
                                        {
                                            Name = items[j]["name"],
                                            SwitchName = items[j]["switchName"],
                                            SwitchType = items[j]["switchType"],
                                            SwitchValue = items[j]["switchValue"]
                                        };
                                }
                            }
                            break;
                        case "switches":
                            {
                                var items = section.Items[i].Items;
                                setting.Switches = new dynamic[items.Count];
                                for (int j = 0; j < items.Count; j++)
                                {
                                    if (items[j].Name == "add")
                                    {
                                        string name = items[j]["name"];
                                        if (string.IsNullOrEmpty(name)) continue;

                                        setting.Switches[j]
                                            = new
                                            {
                                                Name = name,
                                                Value = items[j]["value"],
                                            };
                                    }
                                }
                            }
                            break;
                        case "trace":
                            {
                                var items = section.Items[i];
                                var autoflush = items["autoflush"];
                                if (!string.IsNullOrEmpty(autoflush))
                                {
                                    setting.Autoflush = bool.Parse(autoflush);
                                }
                            }
                            break;
                    }
                }

                if (setting.Sources != null)
                {
                    foreach (var source in setting.Sources)
                    {
                        if(!TraceLogger.TraceSourceSettings.ContainsKey(source.Name))
                        {
                            var traceSource = new TraceSource(source.Name);
                            var switchName = source.SwitchName;
                            if (!string.IsNullOrEmpty(switchName) && setting.Switches!=null)
                            {
                                foreach (var switchObject in setting.Switches)
                                {
                                    if(switchObject.Name == switchName)
                                    {
                                        traceSource.Switch = new SourceSwitch(switchName, switchObject.Value);
                                        break;
                                    }
                                }
                            }

                            //Set source switch in same element
                            if (traceSource.Switch == null && string.IsNullOrEmpty(switchName) && !string.IsNullOrEmpty(source.SwitchValue))
                            {
                                traceSource.Switch = new SourceSwitch(source.Name, source.SwitchValue);
                            }

                            TraceLogger.TraceSourceSettings.Add(source.Name, traceSource);
                        }
                    }
                }
            }
        }
    }
}
