namespace qshine.Configuration.Setting
{
    /// <summary>
    /// Module element collection
    /// </summary>
    public class ModuleElementCollection : ConfigurationElementCollection<NamedTypeElement>
	{
        /// <summary>
        /// Ctro.
        /// </summary>
		public ModuleElementCollection()
			: base("module")
		{
		}
	}
}
