using System;
using System.Collections.Generic;
using System.Linq;
using Core.Objects;

namespace Core
{
    public class BargeHauler3 : IPunter
    {
        private readonly IScorer scorer;
        private readonly IGraphVisitor graphVisitor;
        private int maxScore;
        private int bridgeMaxScore;

        public BargeHauler3(
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

            var strictComponents = graphVisitor.GetConnectedComponents(map);

            var punters = strictComponents.GetPunters;
            var scoreByPunter = punters
                .ToDictionary(x => x,
                    x => scorer.Score(new GameState {Map = map, CurrentPunter = new Punter {Id = x}}));

            var reachableNodeIds = GetReachableNodesFromMines(map, punter);

            var rushingEdges = map
                .Edges
                .Where(x => x.Source.IsMine || x.Target.IsMine)
                .Where(x => x.Punter == null)
                .ToArray();

            maxScore = rushingEdges.Length > 0 ? rushingEdges.Max(x => GetWeight(x, punter, strictComponents)) : 1;

            var bestRushingPathEdge = rushingEdges
                .OrderByDescending(x => CapturedMinesCount(gameState, x))
                .ThenByDescending(x => GetWeight(x, punter, strictComponents))
                .ThenBy(x => CountMyNeighborEdges(gameState, x))
                .ThenBy(x => GetShortestDistanceToMineInOtherComponent(x, strictComponents, punter))
                .ThenByDescending(x => CountFreeNeighborEdges(gameState, x))
                .FirstOrDefault();

            if (bestRushingPathEdge != null)
            {
                return bestRushingPathEdge;
            }

            var bridgeEdges = graphVisitor
                .GetBridgesInAvailableEdges(map, punter)
                .Where(x => x.Punter == null)
                .ToArray();

            var goodBridges = bridgeEdges
                .Select(x => new { edge = x, weight = GetWeightForBridge(map, x, punter, reachableNodeIds)})
                .Where(x => x.weight > 0)
                .ToArray();

            bridgeMaxScore = goodBridges.Length > 0 ? goodBridges.Max(x => x.weight) : 1;
            maxScore = goodBridges.Length > 0 ? goodBridges.Max(x => GetWeight(x.edge, punter, strictComponents)) : 1;

            var bestBridge = goodBridges
                .Select(x => x.edge)
                .OrderByDescending(x => GetWeightForBridge(map, x, punter, reachableNodeIds))
                .ThenByDescending(x => GetWeight(x, punter, strictComponents))
                .ThenBy(x => GetShortestDistanceToMineInOtherComponent(x, strictComponents, punter))
                .ThenByDescending(x => CountFreeNeighborEdges(gameState, x))
                .FirstOrDefault();

            if (bestBridge != null)
            {
                return bestBridge;
            }

            var increasingEdges = map
                .Edges
                .Where(x => x.Punter == null)
                .Where(x => reachableNodeIds.Contains(x.Source.Id) ||
                            reachableNodeIds.Contains(x.Target.Id) ||
                            x.Source.IsMine ||
                            x.Target.IsMine)
                .ToArray();

            maxScore = increasingEdges.Length > 0
                ? increasingEdges.Max(x => GetWeight(x, punter, strictComponents))
                : 1;

            var bestIncreasingPathEdge = increasingEdges
                .OrderByDescending(x => GetWeight(x, punter, strictComponents))
                .ThenBy(x => GetShortestDistanceToMineInOtherComponent(x, strictComponents, punter))
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
                var enemyBridges = graphVisitor.GetBridgesInAvailableEdges(map, bestPunter).Where(x => x.Punter == null).ToArray();

                if (enemyBridges.Any())
                {
                    return enemyBridges
                        .OrderByDescending(x => !strictComponents.IsInSameComponent(x.Source.Id, x.Target.Id, punter.Id))
                        .ThenByDescending(x => HasNeighborPunterEdge(map, x, bestPunter))
                        .ThenBy(x => Guid.NewGuid())
                        .FirstOrDefault(x => x.Punter == null);
                }

                return map
                    .Edges
                    .Where(x => x.Punter == null)
                    .OrderByDescending(x => !strictComponents.IsInSameComponent(x.Source.Id, x.Target.Id, punter.Id))
                    .ThenByDescending(x => HasNeighborPunterEdge(map, x, bestPunter))
                    .ThenBy(x => Guid.NewGuid())
                    .FirstOrDefault(x => x.Punter == null);
            }

            if (bridgeEdges.Any())
            {
                return bridgeEdges
                    .Where(x => x.Punter == null)
                    .OrderBy(x => GetShortestDistanceToMineInOtherComponent(x, strictComponents, punter))
                    .ThenByDescending(x => CountFreeNeighborEdges(gameState, x))
                    .ThenBy(x => Guid.NewGuid())
                    .FirstOrDefault(x => x.Punter == null);
            }

            return map
                .Edges
                .Where(x => x.Punter == null)
                .OrderBy(x => GetShortestDistanceToMineInOtherComponent(x, strictComponents, punter))
                .ThenByDescending(x => CountFreeNeighborEdges(gameState, x))
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

            var scalingFactor = Math.Max(10, maxScore / 10);
            var scoreDelta = (scorer.ScoreForUnitingComponents(leftComponent, rightComponent) + scalingFactor - 1) /
                             scalingFactor;

            claimEdge.Punter = null;
            return scoreDelta;
        }

        private int GetWeightForBridge(Map map, Edge claimEdge, Punter punter, HashSet<int> reachableNodeIds)
        {
            claimEdge.Punter = new Punter {Id = -1};

            var leftComponent = graphVisitor.GetReachableNodesForPunter(claimEdge.Source, map, punter, true).Where(x => reachableNodeIds.Contains(x.Id)).ToArray();
            var rightComponent = graphVisitor.GetReachableNodesForPunter(claimEdge.Target, map, punter, true).Where(x => reachableNodeIds.Contains(x.Id)).ToArray();

            claimEdge.Punter = null;

            var left = leftComponent.Length;
            var leftMines = leftComponent.Count(x => x.IsMine);
            var right = rightComponent.Length;
            var rightMines = rightComponent.Count(x => x.IsMine);

            if (left == 0 || right == 0)
            {
                return 0;
            }

            var scalingFactor = Math.Max(10, bridgeMaxScore / 10);
            var scoreDelta = ((left - leftMines) * rightMines + (right - rightMines) * leftMines - 1 + scalingFactor) / scalingFactor;

            return scoreDelta;
        }

        private int GetShortestDistanceToMineInOtherComponent(Edge edge,
            PunterConnectedComponents punterConnectedComponents, Punter punter)
        {
            return Math.Min(
                GetShortestDistanceToMineInOtherComponent(edge.Source, punterConnectedComponents, punter),
                GetShortestDistanceToMineInOtherComponent(edge.Target, punterConnectedComponents, punter)
            );
        }

        private int GetShortestDistanceToMineInOtherComponent(Node node,
            PunterConnectedComponents punterConnectedComponents, Punter punter)
        {
            var minesInOtherComponents = scorer.State.Mines
                .Where(mine => !punterConnectedComponents.IsInSameComponent(mine.Id, node.Id, punter.Id))
                .ToArray();
            if (minesInOtherComponents.Length == 0)
            {
                return 1000 * 1000 * 1000;
            }

            return minesInOtherComponents.Min(mine => scorer.GetDistance(mine, node));
        }

        private HashSet<int> GetReachableNodesFromMines(Map map, Punter punter)
        {
            var reachableNodes = graphVisitor.GetReachableNodesFromMinesForPunter(map, punter);
            return new HashSet<int>(reachableNodes.Select(x => x.Id));
        }

        private int CapturedMinesCount(GameState gameState, Edge claimEdge)
        {
            return (IsNotCapturedMine(gameState, claimEdge.Source) ? 1 : 0) +
                   (IsNotCapturedMine(gameState, claimEdge.Target) ? 1 : 0);
        }

        private bool IsNotCapturedMine(GameState gameState, Node node)
        {
            return node.IsMine &&
                   gameState.Map.GetPunterEdges(node.Id, gameState.CurrentPunter).Count == 0;
        }

        private int CountFreeNeighborEdges(GameState gameState, Edge claimEdge)
        {
            return gameState.Map.GetFreeEdges(claimEdge.Source.Id).Count +
                   gameState.Map.GetFreeEdges(claimEdge.Target.Id).Count;
        }

        private int CountMyNeighborEdges(GameState gameState, Edge claimEdge)
        {
            return gameState.Map.GetPunterEdges(claimEdge.Source.Id, gameState.CurrentPunter).Count +
                   gameState.Map.GetPunterEdges(claimEdge.Target.Id, gameState.CurrentPunter).Count;
        }
    }
}