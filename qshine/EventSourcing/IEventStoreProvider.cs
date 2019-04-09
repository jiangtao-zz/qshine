namespace qshine.EventSourcing
{
    /// <summary>
    /// event store provider
    /// </summary>
    public interface IEventStoreProvider:IProvider
    {
        /// <summary>
        /// Create a event store instance for given aggregate
        /// </summary>
        /// <param name="aggregateName">aggregate name. It is usually the name of the aggregate type.
        /// The aggregate name can be used to select mapped event store provider by application environment setting.
        /// </param>
        /// <returns>event store instance created by the provider</returns>
        IEventStore Create(string aggregateName);
    }
}
