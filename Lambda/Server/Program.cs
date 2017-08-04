using System;
using Core;

namespace Server
{
	class Program
	{
		static void Main(string[] args)
		{
			IServer server = new Server(new Scorer(new DistanceCalculator(), new GraphVisitor()));

			server.Start(playersCount: 2);

			Console.ReadLine();
		}
	}
}