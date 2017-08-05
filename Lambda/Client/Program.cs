using System;
using Core;
using Core.GreedyComponent;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var punter = GetPunter(args[0]);
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
                case "random":
                    return new RandomPunter();
                case "first":
                    return new AlwaysFirstPunter();
                default:
                    throw new Exception();
            }
        }
    }
}