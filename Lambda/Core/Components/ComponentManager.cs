using System;
using System.Collections.Generic;
using System.Linq;
using Core.Objects;

namespace Core.Components
{
    public class ComponentManager : IComponentManager
    {
        private readonly IScorer scorer;
        private Node[] mines;
        private Map map;
        private Punter punter;
        private List<Component> components;
        private Dictionary<int, Component> nodeToComponent;

        public ComponentManager(IScorer scorer)
        {
            this.scorer = scorer;
        }

        public void InitComponents(Map map, Punter punter)
        {
            this.map = map;
            this.punter = punter;
            mines = map.Nodes.Where(n => n.IsMine).ToArray();
            components = map.Nodes.Select(n => new Component(n, mines, scorer)).ToList();
            nodeToComponent = components
                .SelectMany(c => c.Nodes.Select(n => new { Key = n, Value = c }))
                .ToDictionary(x => x.Key.Id, x => x.Value);
        }

        public void UpdateMap(Map map)
        {
            this.map = map;
        }

        public void ClaimEdge(Node a, Node b)
        {
            //todo: придумать что делать с desireComponent при слиянии
            var sourceComponent = nodeToComponent[a.Id];
            var targetComponent = nodeToComponent[b.Id];
            if (sourceComponent != targetComponent)
            {
                sourceComponent.Union(targetComponent);
                components.Remove(targetComponent);
                foreach (var node in targetComponent.Nodes)
                {
                    nodeToComponent[node.Id] = sourceComponent;
                }
            }
        }

        public int FindShortestPathLength(Component a, Component b)
        {
            var distance = Bfs(a, b);
            return distance.ContainsKey(b) ? distance[b] : int.MaxValue;
        }

        public DesireComponent FindShortestPath(Component a, Component b)
        {
            var distanceFromA = Bfs(a, b);
            var distanceFromB = Bfs(b, a);

            var length = distanceFromA[b];

            var nodes = distanceFromA
                .Where(x => distanceFromB.ContainsKey(x.Key) && x.Value + distanceFromB[x.Key] == length)
                .OrderBy(x => x.Value)
                .Select(x => x.Key)
                .ToArray();

            var dp = new Dictionary<Component, ComponentScore>();
            var parent = new Dictionary<Component, Component>();

            dp[a] = a.Score;
            foreach (var node in nodes.Skip(1))
            {
                var prevs = GetEdges(node)
                    .Select(x => x.Item1)
                    .Where(x => dp.ContainsKey(x) && distanceFromA[x] == distanceFromA[node] + 1);

                foreach (var prev in prevs)
                {
                    var score = dp[prev].Clone().Add(node.Score).Add(b.Score);
                    if (!dp.ContainsKey(node) || dp[node].SelfScore < score.SelfScore)
                    {
                        dp[node] = score;
                        parent[node] = prev;
                    }
                }
            }

            var result = new DesireComponent();
            result.Components.Add(a);
            while (a != b)
            {
                result.Components.Add(b);
                b = parent[b];
            }

            return result;
        }

        public DesireComponent FindGreedyFullComponent(int size)
        {
            var result = new DesireComponent();

            result.Root = GetBestComponentByChart(size / 2);
            result.Components.Add(result.Root);

            var desireScore = result.Root.Score.Clone();

            var S = new SortedSet<(long, Component)>();

            foreach (var c in GetEdges(result.Root).Select(x => x.Item1))
            {
                S.Add((desireScore.Clone().Add(c.Score).SelfScore, c));
            }

            for (int i = 0; i < size - 1; ++i)
            {
                while (S.Count != 0 && result.Components.Contains(S.Max.Item2))
                {
                    S.Remove(S.Max);
                }
                if (S.Count == 0)
                {
                    break;
                }
                var newComponent = S.Max.Item2;
                result.Components.Add(newComponent);
                desireScore.Add(newComponent.Score);
                S.Remove(S.Max);

                if (newComponent.Score.Mines.Any())
                {
                    foreach (var c in components.Where(c => !result.Components.Contains(c)))
                    {
                        S.Add((desireScore.Clone().Add(c.Score).SelfScore - desireScore.SelfScore, c));
                    }
                }
                else
                {
                    foreach (var c in newComponent.Nodes.SelectMany(n => map.GetAvaliableEdges(n.Id, punter).Select(e => nodeToComponent[e.Item1.Id])).Distinct().Where(x => !result.Components.Contains(x)))
                    {
                        S.Add((desireScore.Clone().Add(c.Score).SelfScore - desireScore.SelfScore, c));
                    }
                }
            }

            return result;
        }

        public Component GetBestComponentByChart(int chartSize)
        {
            var mineToScore = mines.ToDictionary(m => m.Id, m => components.Select(n => n.Score.Scores[m.Id]).OrderByDescending(s => s).Take(chartSize).Sum());
            return components.OrderByDescending(n => n.Score.Mines.Sum(x => mineToScore[x.Id])).First();
        }

        private Dictionary<Component, int> Bfs(Component a, Component b)
        {
            var queue = new Queue<Component>();
            queue.Enqueue(a);
            var distance = new Dictionary<Component, int>();
            distance[a] = 0;

            while (queue.Any())
            {
                var current = queue.Dequeue();
                var dist = distance[current];
                foreach (var neighbour in GetEdges(current))
                {
                    if (!distance.ContainsKey(neighbour.Item1))
                    {
                        distance[neighbour.Item1] = dist + 1;
                        queue.Enqueue(neighbour.Item1);
                    }
                    if (neighbour.Item1 == b)
                    {
                        return distance;
                    }
                }
            }
            return distance;
        }

        public Edge GetMineEdge()
        {
            throw new NotImplementedException();
        }

        public Edge GetMostExpensiveEdge()
        {
            return map.Edges
                .Where(x => x.Punter == null)
                .GroupBy(x => (nodeToComponent[x.Source.Id], nodeToComponent[x.Target.Id]))
                .OrderByDescending(x => x.Key.Item1.Score.Clone().Add(x.Key.Item2.Score).SelfScore
                                        - x.Key.Item1.Score.SelfScore - x.Key.Item2.Score.SelfScore)
                .First()
                .First();
        }

        public Edge GetMostExpensiveFromComponentEdge(Component from)
        {
            var neighbours = GetEdges(from).Select(x => x.Item1).ToArray();

            Component best = null;
            long maxScore = -1;
            foreach (var neighbour in neighbours)
            {
                var score = from.Score.Clone().Add(neighbour.Score).SelfScore;
                if (score > maxScore)
                {
                    maxScore = score;
                    best = neighbour;
                }
            }

            return best == null ? GetMostExpensiveEdge() : GetFreeEdge(from, best);
        }

        public Edge GetMaxSubtreeEdge(DesireComponent desire)
        {
            throw new NotImplementedException();
        }

        public Edge GetFragileEdge(DesireComponent desire)
        {
            throw new NotImplementedException();
        }

        private (Component, int)[] GetEdges(Component from)
        {
            return from.Nodes.SelectMany(n => map.GetFreeEdges(n.Id))
                .GroupBy(n => nodeToComponent[n.Item1.Id])
                .Select(x => (x.Key, x.Count()))
                .Where(x => x.Item1 != from)
                .ToArray();
        }

        private Edge GetFreeEdge(Component a, Component b)
        {
            return a.Nodes
                .SelectMany(n => map.GetFreeEdges(n.Id))
                .First(n => nodeToComponent[n.Item1.Id] == b)
                .Item2;
        }
    }
}