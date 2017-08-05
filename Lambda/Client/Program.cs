using System;
using System.Linq;
using Core;
using Core.GreedyComponent;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var punterName = args[0];
            
            var punter = GetPunter(punterName);
            var log = new ConsoleLog();
            var client = new OnlineClient(punter, log);
            client.Start();
            Console.ReadLine();
        }

        private static IPunter GetPunter(string name)
        {
            switch (name)
            {
                case "greedy":
                    return new GreedyComponentPunter(new Scorer(new DistanceCalculator(), new GraphVisitor()));
                case "first":
                    return new AlwaysFirstPunter();
                case "random":
                default:
                    return new RandomPunter();
            }
        }
    }
}