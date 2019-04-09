using System;
namespace qshine
{
    /// <summary>
    /// event message interface
    /// </summary>
	public interface IEventMessage:IMessage
	{
        /// <summary>
        /// unique event message Id
        /// </summary>
        Guid Id { get; set; }
    }
}
