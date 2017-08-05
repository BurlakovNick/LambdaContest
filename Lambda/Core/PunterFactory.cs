using System;
using Core.GreedyComponent;
using Core.GreedyDesired;

namespace Core
{
    public static class PunterFactory
    {
        public static IPunter Create(string name)
        {
            switch (name)
            {
                case "GreedyComponentPunter":
                    return new GreedyComponentPunter(new Scorer(new DistanceCalculator(), new GraphVisitor()));
                case "GreedyDesiredPunter":
                    return new GreedyDesiredPunter(new Scorer(new DistanceCalculator(), new GraphVisitor()));
                case "AlwaysFirstPunter":
                    return new AlwaysFirstPunter();
                case "GraphVisitor":
                    var visitor = new GraphVisitor();
                    return new GreedyEdgeChooserPunter(new Scorer(new DistanceCalculator(), visitor), visitor);
                case "RandomPunter":
                    return new RandomPunter();
                default:
                    throw new Exception($"Запихни в этот свитч создание своего пунтера с именем {name}");
            }
        }
    }
}