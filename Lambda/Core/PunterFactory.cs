using System;
using Core.GreedyComponent;
using Core.GreedyDesired;

namespace Core
{
    public static class PunterFactory
    {
        public static IPunter Create(string name)
        {
            var visitor = new GraphVisitor();
            var distanceCalculator = new DistanceCalculator();

            switch (name)
            {
                case "GreedyComponentPunter":
                    return new GreedyComponentPunter(new Scorer(distanceCalculator, visitor));
                case "GreedyDesiredPunter":
                    return new GreedyDesiredPunter(new Scorer(distanceCalculator, visitor));
                case "AlwaysFirstPunter":
                    return new AlwaysFirstPunter();
                case "GreedyEdgeChooserPunter":
                    return new GreedyEdgeChooserPunter(new Scorer(distanceCalculator, visitor), visitor);
                case "GreedyEdgeChooserPunterWithZergRush":
                    return new GreedyEdgeChooserPunterWithZergRush(new Scorer(distanceCalculator, visitor), visitor);
                case "GreedyEdgeChooserPunterWithStupidZergRush":
                    return new GreedyEdgeChooserPunterWithStupidZergRush(new Scorer(distanceCalculator, visitor), visitor);
                case "RandomPunter":
                    return new RandomPunter();
                default:
                    throw new Exception($"Запихни в этот свитч создание своего пунтера с именем {name}");
            }
        }
    }
}