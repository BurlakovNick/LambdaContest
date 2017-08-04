using System.Net.Sockets;
using SimpleTCP;

namespace Server
{
	public class PlayerConnection
	{
		public PlayerConnection(TcpClient tcpClient,
								string name)
		{
			Client = new SimpleTcpClient().Connect(tcpClient);
			Name = name;
		}

		public SimpleTcpClient Client { get; }
		public string Name { get; }
	}
}