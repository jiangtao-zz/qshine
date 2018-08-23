using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace qshine
{
	public class NetCommandBus:CommandBusBase
	{
		private readonly IPEndPoint _endpoint;
		const int defaulPort = 11123;

		public NetCommandBus()
		{
			var ip = IPAddress.Parse("127.0.0.1");
			_endpoint = new IPEndPoint(ip, defaulPort);
		}
		
		public NetCommandBus(string ipAddress, int port)
		{
			var ip = IPAddress.Parse(ipAddress);
			_endpoint = new IPEndPoint(ip, port);
		}

		public override void SendAsync<T>(T command)
		{
			using (var connect = new TcpClient())
			{
				var task = connect.ConnectAsync(_endpoint.Address, _endpoint.Port);
				task.Wait(TimeSpan.FromMinutes(1));

				var envelope = new MessageEnvelope(command);
				var msg = envelope.Serialize();
				var buffer = Encoding.Unicode.GetBytes(msg);
				using (var networkStream = connect.GetStream())
				{
					using (var bufferedStream = new BufferedStream(networkStream))
					{
						bufferedStream.WriteAsync(buffer, 0, buffer.Length);
					}
				}
			}
		}
	}
}
