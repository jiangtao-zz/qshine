using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Configuration.ConfigurationStore
{
    /// <summary>
    /// EnvironmentConfigure extension to Load XML formatted configure files
    /// </summary>
    public static class CommandlineConfigurationLoader
    {
        /// <summary>
        /// Load XML formatted application configure files
        /// </summary>
        /// <param name="config">environment configure instance</param>
        /// <param name="args">commandline args</param>
        public static void AddCommandline(this EnvironmentConfigure config, string[] args)
        {
            var store = new CommandlineConfigurationSetting(config);
            store.Load(args);
        }

    }

    /// <summary>
    /// Commandline arguments setting
    /// </summary>
    public class CommandlineConfigurationSetting
    {
        EnvironmentConfigure _configure;
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="configure"></param>
        public CommandlineConfigurationSetting(EnvironmentConfigure configure)
        {
            _configure = configure;
        }

        /// <summary>
        /// Load environment variables
        /// key1=value1 -key2=value2 /key3=value3 -key4 value4 /key5 value5 --key6=value6 --key7 value7
        /// </summary>
        /// <param name="args"></param>
        public void Load(string[] args)
        {
            bool hasKey = false;
            string key="";
            string value;
            foreach(var arg in args)
            {
                if (!hasKey)
                {
                    if (arg.Contains("="))
                    {
                        var a = arg.Split(new char[] {'='});
                        key = ParseKey(a[0]);
                        value = ParseValue(a[1]);
                        AddAppSetting(key, value);
                        hasKey = false;
                    }
                    else if(arg.StartsWith("/") || arg.StartsWith("-"))
                    {
                        hasKey = true;
                        key = ParseKey(arg);
                    }
                }
                else
                {
                    value = ParseValue(arg);
                    AddAppSetting(key, value);
                    hasKey = false;
                }
            }
        }

        string ParseKey(string key)
        {
            return key.TrimStart('/', '-');
        }

        string ParseValue(string value)
        {
            return value;
        }

        void AddAppSetting(string key, string value)
        {
            if (!_configure.AppSettings.ContainsKey(key))
            {
                _configure.AppSettings.Add(key, value);
            }
            else
            {
                _configure.AppSettings[key]=value;
            }
        }
    }
}
