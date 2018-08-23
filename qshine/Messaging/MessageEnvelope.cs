using System;
using System.Runtime.Serialization;
namespace qshine
{
	[DataContract]
	public class MessageEnvelope
	{
		public MessageEnvelope(object message)
		{
			AssemblyQualifiedName = message.GetType().AssemblyQualifiedName;
			_messageBody = message.Serialize();
			_source = EnvironmentEx.MachineIp;
		}

		private string _assemblyQualifiedName;

		/// <summary>
		/// assembly qualified name
		/// </summary>
		[DataMember]
		public string AssemblyQualifiedName
		{
			get { return _assemblyQualifiedName; }
			set { _assemblyQualifiedName = value; }
		}

		private string _messageBody;

		/// <summary>
		/// message body
		/// </summary>
		[DataMember]
		public string Body
		{
			get { return _messageBody; }
			set { _messageBody = value; }
		}

		private string _source;

		/// <summary>
		/// sender application
		/// </summary>
		[DataMember]
		public string Source
		{
			get { return _source; }
			set { _source = value; }
		}

		private DateTime _sendTime;

		/// <summary>
		/// When the message send out
		/// </summary>
		[DataMember]
		public DateTime SendTime
		{
			get { return _sendTime; }
			set { _sendTime = value; }
		}
	}
}
