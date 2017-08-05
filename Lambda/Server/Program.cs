using System;
using Core;

namespace Server
{
	class Program
	{
		static void Main(string[] args)
		{
			IServer server = new OnlineServer(new Scorer(new DistanceCalculator(), new GraphVisitor()),
										new ConsoleLog(),
										"sample");

			server.Start(playersCount: 2);

			Console.ReadLine();
		}
	}
}