using System;
using Core;
using Core.GreedyComponent;
using Core.GreedyDesired;

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
                    return PunterFactory.Create(typeof(GreedyComponentPunter).Name);
                case "greedydesired":
                    return PunterFactory.Create(typeof(GreedyDesiredPunter).Name);
                case "first":
                    return PunterFactory.Create(typeof(AlwaysFirstPunter).Name);
                case "stupidgreedy":
                    return PunterFactory.Create(typeof(GreedyEdgeChooserPunter).Name);
                case "random":
                default:
                    return new RandomPunter();
            }
        }
    }
}