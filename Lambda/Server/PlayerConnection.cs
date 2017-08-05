using System.Net.Sockets;
using SimpleTCP;

namespace Server
{
	public class PlayerConnection
	{
		public PlayerConnection(TcpClient tcpClient,
								string name,
								int id)
		{
			Client = new SimpleTcpClient().Connect(tcpClient);
			Name = name;
			Id = id;
		}

		public SimpleTcpClient Client { get; }
		public string Name { get; }
		public int Id { get; }
	}
}