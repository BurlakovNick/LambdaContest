using System.Collections.Generic;
using System.Linq;
using Core.Components;
using Core.Objects;

namespace Core.GreedyComponent
{
    public partial class GreedyComponentPunter : IPunter
    {
        private readonly IScorer scorer;
        private Node[] mines;
        private List<Component> components;
        private HashSet<Component> desiredComponent;
        private int movesCount;
        private Component first;
        private PunterState state;

        public GreedyComponentPunter(IScorer scorer)
        {
            this.scorer = scorer;
            state = new PunterState();
        }

        public void Init(Map map, int puntersCount, Punter punter)
        {
            scorer.Init(map);
            movesCount = (map.Edges.Length - punter.Id + puntersCount - 1) / puntersCount;
            mines = map.Nodes.Where(n => n.IsMine).ToArray();
            components = map.Nodes.Select(n => new Component(n, mines, scorer)).ToList();
            FindDesiredComponent(map, punter);
        }

        public Edge Claim(GameState gameState)
        {
            var nodeToComponent = BuildNodeToComponent();
            var visited = new HashSet<Component>();
            dfs(first, gameState, visited, nodeToComponent);

            if (visited.Count != desiredComponent.Count)
            {
                FindDesiredComponent(gameState.Map, gameState.CurrentPunter);
            }
            movesCount--;

            var edgePower = new Dictionary<Edge, (int, Component)>();
            var grey = new List<(Component, Edge)>();
            var color = new Dictionary<Component, int>();
            calculatePower(first, edgePower, grey, gameState, color, nodeToComponent);

            var claimedEdge = !edgePower.Any() 
                ? gameState.Map.Edges.First(e => e.Punter == null) 
                : edgePower.OrderBy(x => x.Value).First().Key;
            var sourceComponent = nodeToComponent[claimedEdge.Source.Id];
            var targetComponent = nodeToComponent[claimedEdge.Target.Id];
            if (sourceComponent != targetComponent)
            {
                sourceComponent.Union(targetComponent);
                components.Remove(targetComponent);
                desiredComponent.Remove(targetComponent);
                if (first == targetComponent)
                {
                    first = sourceComponent;
                }
            }
            return claimedEdge;
        }

        private void calculatePower(Component v, Dictionary<Edge, (int, Component)> edgePower, List<(Component, Edge)> grey, GameState state, Dictionary<Component, int> color, Dictionary<int, Component> nodeToComponent)
        {
            color[v] = 1;
            v.SubtreeSize = 1;
            var neighbours = v.Nodes
                .SelectMany(n => state.Map.GetAvaliableEdges(n.Id, state.CurrentPunter))
                .GroupBy(n => nodeToComponent[n.Item1.Id])
                .Where(n => n.Key != v && desiredComponent.Contains(n.Key)).ToArray();

            foreach (var neighbour in neighbours)
            {
                var power = neighbour.Count();
                if (!color.ContainsKey(neighbour.Key))
                {
                    var edge = neighbour.First().Item2;
                    grey.Add((neighbour.Key, edge));
                    edgePower[edge] = (0, neighbour.Key);
                    calculatePower(neighbour.Key, edgePower, grey, state, color, nodeToComponent);
                    v.SubtreeSize += neighbour.Key.SubtreeSize;
                }
                else
                if (color[neighbour.Key] == 1)
                {
                    for (int i = grey.Count - 1; i >= 0; --i)
                    {
                        if (grey[i].Item1 == neighbour.Key)
                        {
                            break;
                        }
                        var cur = edgePower[grey[i].Item2];
                        edgePower[grey[i].Item2] = (cur.Item1 + power, cur.Item2);
                    }
                }
            }

            color[v] = 2;
            if (grey.Any())
            {
                grey.RemoveAt(grey.Count - 1);
            }
        }

        private void dfs(Component v, GameState state, HashSet<Component> visited, Dictionary<int, Component> nodeToComponent)
        {
            visited.Add(v);
            var neighbours = v.Nodes.SelectMany(n => state.Map.GetAvaliableEdges(n.Id, state.CurrentPunter)
                .Select(e => nodeToComponent[e.Item1.Id])).Distinct();

            foreach (var neighbour in neighbours)
            {
                if (!visited.Contains(neighbour) && desiredComponent.Contains(neighbour))
                {
                    dfs(neighbour, state, visited, nodeToComponent);
                }
            }
        }

        public PunterState State
        {
            get => state;
            set => state = value;
        }

        private void FindDesiredComponent(Map map, Punter punter)
        {
            var top = movesCount / 2;
            var mineToScore = mines.ToDictionary(m => m.Id, m => components.Select(n => n.Scores[m.Id]).OrderByDescending(s => s).Take(top).Sum());
            first = components.OrderByDescending(n => n.Mines.Sum(x => mineToScore[x.Id])).First();

            var desiredScores = first.Scores.ToDictionary(x => x.Key, x => x.Value);
            var desiredMines = first.Mines.Select(x => x).ToList();

            var nodeToComponent = BuildNodeToComponent();

            desiredComponent = new HashSet<Component> {first};
            var S = new SortedSet<(long, Component)>();
            foreach (var c in first.Nodes.SelectMany(n => map.GetAvaliableEdges(n.Id, punter).Select(e => nodeToComponent[e.Item1.Id])).Distinct())
            {
                S.Add((first.Mines.Union(c.Mines).Sum(x => c.Scores[x.Id] + desiredScores[x.Id]), c));
            }

            for (int i = 0; i < movesCount; ++i)
            {
                while (S.Count != 0 && desiredComponent.Contains(S.Max.Item2))
                {
                    S.Remove(S.Max);
                }
                if (S.Count == 0)
                {
                    break;
                }
                var newComponent = S.Max.Item2;
                desiredComponent.Add(newComponent);
                S.Remove(S.Max);

                foreach (var score in newComponent.Scores)
                {
                    desiredScores[score.Key] += score.Value;
                }
                desiredMines.AddRange(newComponent.Mines);
                if (newComponent.Mines.Any())
                {
                    foreach (var c in components.Where(c => !desiredComponent.Contains(c)))
                    {
                        S.Add((desiredMines.Union(c.Mines).Sum(x => c.Scores[x.Id] + desiredScores[x.Id]) - desiredMines.Sum(x => desiredScores[x.Id]), c));
                    }
                }
                else
                {
                    foreach (var c in newComponent.Nodes.SelectMany(n => map.GetAvaliableEdges(n.Id, punter).Select(e => nodeToComponent[e.Item1.Id])).Distinct().Where(x => !desiredComponent.Contains(x)))
                    {
                        S.Add((desiredMines.Union(c.Mines).Sum(x => c.Scores[x.Id] + desiredScores[x.Id]) - desiredMines.Sum(x => desiredScores[x.Id]), c));
                    }
                }
            }
        }

        private Dictionary<int, Component> BuildNodeToComponent()
        {
            return components
                .SelectMany(c => c.Nodes.Select(n => new {Key = n, Value = c}))
                .ToDictionary(x => x.Key.Id, x => x.Value);
        }
    }
}