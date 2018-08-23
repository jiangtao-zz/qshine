namespace qshine
{
	/// <summary>
	/// Interface of IoC registration module
	/// </summary>
    public interface IIocModule
    {
		/// <summary>
		/// Load IoC types registration from IoC module
		/// </summary>
		/// <param name="container">IoC Container.</param>
        void Load(IIocContainer container);
    }
}
