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
                .OrderByDescending(x => CountEnemyNeighborEdges(gameState, x))
                .ThenBy(x => Guid.NewGuid())
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

        private int CountEnemyNeighborEdges(GameState gameState, Edge claimEdge)
        {
            return (gameState.Map.GetEdges(claimEdge.Source.Id).Count - gameState.Map.GetAvaliableEdges(claimEdge.Source.Id, gameState.CurrentPunter).Count) +
                   (gameState.Map.GetEdges(claimEdge.Target.Id).Count - gameState.Map.GetAvaliableEdges(claimEdge.Target.Id, gameState.CurrentPunter).Count);
        }

        private int CountFreeNeighborEdges(GameState gameState, Edge claimEdge)
        {
            return gameState.Map.GetFreeEdges(claimEdge.Source.Id).Count +
                   gameState.Map.GetFreeEdges(claimEdge.Target.Id).Count;
        }
    }
}