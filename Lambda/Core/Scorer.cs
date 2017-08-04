using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Core.Objects;

namespace Core
{
    public class Scorer : IScorer
    {
        private readonly IDistanceCalculator distanceCalculator;
        private readonly IGraphVisitor graphVisitor;
        private readonly Dictionary<(int, int), int> distancesFromMines;

        public Scorer(
            IDistanceCalculator distanceCalculator,
            IGraphVisitor graphVisitor
            )
        {
            this.distanceCalculator = distanceCalculator;
            this.graphVisitor = graphVisitor;
            distancesFromMines = new Dictionary<(int, int), int>();
        }

        public void Init(Map map)
        {
            var mines = map.Nodes.Where(x => x.IsMine);
            foreach (var mine in mines)
            {
                var shortestDistances = distanceCalculator.GetShortest(mine, map);
                foreach (var shortestDistance in shortestDistances)
                {
                    var from = shortestDistance.From.Id;
                    var to = shortestDistance.To.Id;

                    distancesFromMines[(from, to)] = shortestDistance.Length;
                }
            }
        }

        public int Score(GameState gameState)
        {
            var map = gameState.Map;
            var score = 0;

            foreach (var mine in map.Nodes.Where(x => x.IsMine))
            {
                var reachableNodes = graphVisitor.GetReachableNodesForPunter(mine, map, gameState.CurrentPunter);
                foreach (var node in reachableNodes)
                {
                    var distance = GetDistance(mine, node);
                    score += distance * distance;
                }
            }

            return score;
        }

        private int GetDistance(Node from, Node to)
        {
            if (distancesFromMines.TryGetValue((from.Id, to.Id), out var dist))
            {
                return dist;
            }

            return 0;
        }
    }
}