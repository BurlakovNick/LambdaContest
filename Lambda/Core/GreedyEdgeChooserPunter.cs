using System;
using System.Collections.Generic;
using System.Linq;
using Core.Objects;

namespace Core
{
    public class GreedyEdgeChooserPunter : IPunter
    {
        private readonly IScorer scorer;
        private readonly IGraphVisitor graphVisitor;

        public GreedyEdgeChooserPunter(
            IScorer scorer,
            IGraphVisitor graphVisitor
            )
        {
            this.scorer = scorer;
            this.graphVisitor = graphVisitor;
        }

        public PunterState State { get; set; }

        public void Init(Map map, int puntersCount, Punter punter)
        {
            scorer.Init(map);
        }

        public Edge Claim(GameState gameState)
        {
            var map = gameState.Map;
            var punter = gameState.CurrentPunter;

            var reachableNodeIds = GetReachableNodesFromMines(map, punter);

            var bestIncreasingPathEdge = map
                .Edges
                .Where(x => x.Punter == null)
                .Where(x => reachableNodeIds.Contains(x.Source.Id) ||
                            reachableNodeIds.Contains(x.Target.Id) ||
                            x.Source.IsMine ||
                            x.Target.IsMine)
                .OrderByDescending(x => GetWeight(gameState, x, punter, reachableNodeIds))
                .ThenByDescending(x => CountFreeNeighborEdges(gameState, x))
                .FirstOrDefault();

            if (bestIncreasingPathEdge != null)
            {
                return bestIncreasingPathEdge;
            }

            return map
                .Edges
                .OrderBy(x => Guid.NewGuid())
                .FirstOrDefault(x => x.Punter == null);
        }

        private int GetWeight(GameState gameState, Edge claimEdge, Punter punter, HashSet<int> reachableNodeIds)
        {
            claimEdge.Punter = punter;

            if (GetReachableNodesFromMines(gameState.Map, punter).Count == reachableNodeIds.Count)
            {
                claimEdge.Punter = null;
                return 0;
            }

            var newScore = scorer.Score(gameState);
            claimEdge.Punter = null;
            return newScore;
        }

        private HashSet<int> GetReachableNodesFromMines(Map map, Punter punter)
        {
            var reachableNodes = graphVisitor.GetReachableNodesFromMinesForPunter(map, punter);
            return new HashSet<int>(reachableNodes.Select(x => x.Id));
        }

        private int CountFreeNeighborEdges(GameState gameState, Edge claimEdge)
        {
            return gameState.Map.GetFreeEdges(claimEdge.Source.Id).Count +
                   gameState.Map.GetFreeEdges(claimEdge.Target.Id).Count;
        }
    }
}