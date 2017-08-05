using System;
using Core;
using Core.GreedyComponent;

namespace Client
{
	class Program
	{
		static void Main(string[] args)
		{
			//var punter = new GreedyComponentPunter(new Scorer(new DistanceCalculator(), new GraphVisitor()));
			var punter = new RandomPunter();
			var log = new ConsoleLog();
			var client = new OnlineClient(punter, log);
			client.Start();
			Console.ReadLine();
		}
	}
}