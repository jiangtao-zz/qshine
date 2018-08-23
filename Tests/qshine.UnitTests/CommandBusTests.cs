using NUnit.Framework;
using qshine.Configuration;
using qshine;
using System;
namespace qshine.UnitTests
{
	[TestFixture]
	public class CommandBusTests
	{
		[Test()]
		public void CommandBus_Send_Message_And_Handler()
		{
			var command = new SampleCommandMessage
			{
				MessageId = 1,
				MessageP1 = "M1"
			};
			var bus = new CommandBus();
			bus.Send(command);
			Assert.AreEqual("1:1,M1", command.MessageP2, "message send and handler");
			command.MessageP1 = "M2";
			bus.Send(command);
			Assert.AreEqual("1:1,M2", command.MessageP2, "message2 send and handler");
		}

		[Test()]
		public void CommandBus_Send_Message_Separated_Handler()
		{
			var command = new SampleCommandMessage2
			{
				MessageId = 1,
				MessageP1 = "M2"
			};
			var bus = new CommandBus();
			bus.Send(command);

			Assert.AreEqual("2:1,M2", command.MessageP2, "message send and handler");
		}
	}

	public class SampleCommandMessage : ICommandMessage, ICommandHandler<SampleCommandMessage>
	{
		public int MessageId { get; set; }
		public string MessageP1 { get; set; }
		public string MessageP2 { get; set; }

		/// <summary>
		/// Handle the specified commandMessage.
		/// </summary>
		/// <returns>The handle.</returns>
		/// <param name="commandMessage">Command message.</param>
		public void Handle(SampleCommandMessage commandMessage)
		{
			MessageP2 = "1:"+commandMessage.MessageId.ToString() + "," + commandMessage.MessageP1;
		}
	}

	public class SampleCommandMessage2 : ICommandMessage
	{
		public int MessageId { get; set; }
		public string MessageP1 { get; set; }
		public string MessageP2 { get; set; }
	}

	public class SampleCommandMessage2Handler : ICommandHandler<SampleCommandMessage2>
	{
		public void Handle(SampleCommandMessage2 commandMessage)
		{
			commandMessage.MessageP2 = "2:"+commandMessage.MessageId.ToString() + "," + commandMessage.MessageP1;
		}
	}
}
