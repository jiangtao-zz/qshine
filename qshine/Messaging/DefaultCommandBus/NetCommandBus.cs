using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace qshine
{
    /// <summary>
    /// Tcp/ip based command bus. 
    /// </summary>
	public class NetCommandBus:CommandBusBase
	{
		private readonly IPEndPoint _endpoint;
        /// <summary>
        /// default command bus port.
        /// </summary>
		const int defaulPort = 11123;
        const string defaultIp = "127.0.0.1";

        /// <summary>
        /// Ctor::
        /// </summary>
		public NetCommandBus()
            :this(defaultIp, defaulPort)
		{
		}
		
        /// <summary>
        /// Ctor:: by given ip and port
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
		public NetCommandBus(string ipAddress, int port)
		{
			var ip = IPAddress.Parse(ipAddress);
			_endpoint = new IPEndPoint(ip, port);
		}

        /// <summary>
        /// Send command in async. 
        /// </summary>
        /// <typeparam name="T">Command type</typeparam>
        /// <param name="command">command message</param>
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
