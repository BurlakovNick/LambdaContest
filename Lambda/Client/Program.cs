﻿using System;
using System.Linq;
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
                    return new GreedyComponentPunter(new Scorer(new DistanceCalculator(), new GraphVisitor()));
                case "greedydesired":
                    return new GreedyDesiredPunter(new Scorer(new DistanceCalculator(), new GraphVisitor()));
                case "first":
                    return new AlwaysFirstPunter();
                case "stupidgreedy":
                    var visitor = new GraphVisitor();
                    return new GreedyEdgeChooserPunter(new Scorer(new DistanceCalculator(), visitor), visitor);
                case "random":
                default:
                    return new RandomPunter();
            }
        }
    }
}