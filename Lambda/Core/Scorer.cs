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
        private ScorerState state;

        public Scorer(
            IDistanceCalculator distanceCalculator,
            IGraphVisitor graphVisitor)
        {
            this.distanceCalculator = distanceCalculator;
            this.graphVisitor = graphVisitor;
            state = new ScorerState
            {
                DistancesFromMines = new Dictionary<(int, int), int>()
            };
        }

        public ScorerState State
        {
            get => state;
            set => state = value;
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

                    state.DistancesFromMines[(from, to)] = shortestDistance.Length;
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

        public int GetDistance(Node from, Node to)
        {
            if (state.DistancesFromMines.TryGetValue((from.Id, to.Id), out var dist))
            {
                return dist;
            }

            return 0;
        }
    }
}