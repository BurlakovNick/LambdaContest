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
            var scorer = new Scorer(distanceCalculator, visitor);

            switch (name)
            {
                case "GreedyComponentPunter":
                    return new GreedyComponentPunter(scorer);
                case "GreedyDesiredPunter":
                    return new GreedyDesiredPunter(scorer);
                case "AlwaysFirstPunter":
                    return new AlwaysFirstPunter();
                case "GreedyEdgeChooserPunter":
                    return new GreedyEdgeChooserPunter(scorer, visitor);
                case "GreedyEdgeChooserPunterWithZergRush":
                    return new GreedyEdgeChooserPunterWithZergRush(scorer, visitor);
                case "GreedyEdgeChooserPunterWithStupidZergRush":
                    return new GreedyEdgeChooserPunterWithStupidZergRush(scorer, visitor);
                case "BargeHauler":
                    return new BargeHauler(scorer, visitor);
                case "BargeHauler2":
                    return new BargeHauler2(scorer, visitor);
                case "BargeHauler3":
                    return new BargeHauler3(scorer, visitor);
                case "BargeHauler4":
                    return new BargeHauler4(scorer, visitor);
                case "BargeHauler5":
                    return new BargeHauler5(scorer, visitor);
                case "BargeHauler6":
                    return new BargeHauler6(scorer, visitor);
                case "BargeHauler7":
                    return new BargeHauler7(scorer, visitor);
                case "MineConnecterPunter":
                    return new MineConnecterPunter(scorer);
                case "MineConnecterFullPunter":
                    return new MineConnecterFullPunter(scorer);
                case "FriendshipPunter":
                    return new FriendshipPunter(scorer);
                case "RandomPunter":
                    return new RandomPunter();
                default:
                    throw new Exception($"Запихни в этот свитч создание своего пунтера с именем {name}");
            }
        }
    }
}