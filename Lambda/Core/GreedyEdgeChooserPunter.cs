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

            var connectedComponents = graphVisitor.GetConnectedComponents(map);
            var punters = connectedComponents.GetPunters;
            var scoreByPunter = punters
                .ToDictionary(x => x,
                    x => scorer.Score(new GameState {Map = map, CurrentPunter = new Punter {Id = x}}));

            var reachableNodeIds = GetReachableNodesFromMines(map, punter);

            var bestIncreasingPathEdge = map
                .Edges
                .Where(x => x.Punter == null)
                .Where(x => reachableNodeIds.Contains(x.Source.Id) ||
                            reachableNodeIds.Contains(x.Target.Id) ||
                            x.Source.IsMine ||
                            x.Target.IsMine)
                .OrderByDescending(x => GetWeight(x, punter, connectedComponents))
                .ThenByDescending(x => CountFreeNeighborEdges(gameState, x))
                .FirstOrDefault();

            if (bestIncreasingPathEdge != null)
            {
                return bestIncreasingPathEdge;
            }

            var bestPunter = new Punter
            {
                Id = scoreByPunter.OrderByDescending(x => x.Value).First().Key
            };

            if (bestPunter.Id != punter.Id)
            {
                return map
                    .Edges
                    .Where(x => x.Punter == null)
                    .OrderByDescending(x => !connectedComponents.IsInSameComponent(x.Source.Id, x.Target.Id, punter.Id))
                    .ThenByDescending(x => HasNeighborPunterEdge(map, x, bestPunter))
                    .ThenBy(x => Guid.NewGuid())
                    .FirstOrDefault(x => x.Punter == null);
            }

            return map
                .Edges
                .Where(x => x.Punter == null)
                .OrderByDescending(x => CountFreeNeighborEdges(gameState, x))
                .ThenBy(x => Guid.NewGuid())
                .FirstOrDefault(x => x.Punter == null);
        }

        private static bool HasNeighborPunterEdge(Map map, Edge edge, Punter punter)
        {
            var from = edge.Source.Id;
            var to = edge.Target.Id;

            return map.GetPunterEdges(@from, punter).Count > 0 ||
                   map.GetPunterEdges(to, punter).Count > 0;
        }

        private int GetWeight(Edge claimEdge, Punter punter, PunterConnectedComponents punterConnectedComponents)
        {
            if (punterConnectedComponents.IsInSameComponent(claimEdge.Source.Id, claimEdge.Target.Id, punter.Id))
            {
                return 0;
            }

            claimEdge.Punter = punter;

            var leftComponent = punterConnectedComponents.GetComponent(punter.Id, claimEdge.Source.Id);
            var rightComponent = punterConnectedComponents.GetComponent(punter.Id, claimEdge.Target.Id);

            var scoreDelta = scorer.ScoreForUnitingComponents(leftComponent, rightComponent);

            claimEdge.Punter = null;
            return scoreDelta;
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