using System;

namespace Server
{
	class Program
	{
		static void Main(string[] args)
		{
			IServer server = null;

			server.Start(playersCount: 2);

			Console.ReadLine();
		}
	}
}