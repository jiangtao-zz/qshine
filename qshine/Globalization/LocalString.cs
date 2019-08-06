using qshine.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Globalization
{
    /// <summary>
    /// Lable translation object.
    /// Transalte an English phase to other environment language
    /// 
    /// <![CDATA[
    ///     var localizer = new Localizer(string name);
    ///     //var localizer = new Localizer<ResourceType>();
    ///     //var localizer = new Localizer(Type ResourceType);
    /// 
    ///     var nativeText = localizer.GetString("English Message");
    ///     
    /// ]]>
    /// </summary>
    public static class LocalString
    {
        //using default local string
        static Localizer _qshineLocalizer = new Localizer(null, "qshine.Globalization");
        /// <summary>
        /// Translate to native language
        /// </summary>
        /// <param name="format">English phase with format</param>
        /// <param name="args">arguments.</param>
        /// <returns></returns>
        internal static string _G(this string format, params object[] args)
        {
            return _qshineLocalizer.GetString(format,args);
        }
    }

    /// <summary>
    /// Localization (l10n).
    /// The localizer provides service to translate a Neutral resource language text to local language text 
    /// based on current locale.
    /// 
    /// The language resource categorized by the resource name.
    /// The resource name is a dot noation component which contains 1 to many fields separated with the period(.) symbol.
    /// When searching for a text resource, it will search any levels combination of the fields.
    /// Example: 
    ///     Resource Name: qshine.Resources.LevelOne.LevelTwo
    /// The localizer will search below resources in order
    ///     "qshine.LevelOne.LevelTwo"
    ///     "qshine.LevelOne"
    ///     "qshine"
    ///     
    /// If the resources file (ex:resources.resx) is under one of the project namespace (ex: qshine, qshine.LevelOne), the locale resource could be in
    ///     "qshine.LevelOne.LevelTwo.Resources.fr-CA.resx"
    ///     "qshine.LevelOne.LevelTwo.Resources.fr.resx"
    ///     "qshine.LevelOne.Resources.fr-CA.resx"
    ///     "qshine.LevelOne.Resources.fr.resx"
    ///     "qshine.Resources.fr-CA.resx"
    ///     "qshine.Resources.fr.resx"
    /// 
    /// </summary>
    public class Localizer
    {
        ILocalStringProvider _provider;
        ILocalString _localString;
        /// <summary>
        /// Create a string localizer for particular resource category.
        /// The resource name is used to categorize application local string resources.
        /// </summary>
        /// <param name="resourceName">resource name</param>
        public Localizer(string resourceName)
        {
            Initialize(ApplicationEnvironment.Default.Services.GetProvider<ILocalStringProvider>(resourceName), resourceName);
        }

        /// <summary>
        /// Create a string localizer for particular resource type.
        /// The resource type is used to categorize an application resource.
        /// </summary>
        /// <param name="resourceType">Resource type</param>
        public Localizer(Type resourceType)
            :this(resourceType.FullName)
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="resourceName"></param>
        public Localizer(ILocalStringProvider provider, string resourceName)
        {
            Initialize(provider, resourceName);
        }


        private void Initialize(ILocalStringProvider provider , string resourceName)
        {
            _provider = provider;
            if (_provider == null)
            {
                _provider = new ResourceStringProvider();
            }

            try
            {
                _localString = _provider.Create(resourceName);
            }
            catch (Exception ex)
            {
                Logger.Log.SysLogger.Warn(ex, "Failed to load Local String resource {0}. ", resourceName);
            }
        }

        /// <summary>
        /// Get localized string
        /// </summary>
        /// <param name="format">The format string in neutral language.
        /// The format string need be converted to local text by the local string provider.
        /// </param>
        /// <param name="args">The format arguments</param>
        /// <returns></returns>
        public string GetString(string format, params object[] args)
        {
            if (_localString != null)
            {
                try
                {
                    return _localString.GetString(format, args);
                }
                catch (Exception)
                {
                    return string.Format(format, args);
                }
            }

            return string.Format(format, args);
        }
    }


    /// <summary>
    /// Localization (l10n).
    /// </summary>
    /// <typeparam name="T">A resource class type to categorize application resource.</typeparam>
    public class Localizer<T>:Localizer
        where T:class
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public Localizer()
            :base(typeof(T))
        {
        }
    }

}
