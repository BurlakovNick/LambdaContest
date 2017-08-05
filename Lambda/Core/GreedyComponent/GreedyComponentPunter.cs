using System.Collections.Generic;
using System.Linq;
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

            var edgePower = new Dictionary<Edge, int>();
            var grey = new List<(Component, Edge)>();
            visited.Clear();
            calculatePower(first, edgePower, grey, gameState, visited, nodeToComponent);

            return edgePower.OrderBy(x => x.Value).First().Key;
        }

        private void calculatePower(Component v, Dictionary<Edge, int> edgePower, List<(Component, Edge)> grey, GameState state, HashSet<Component> visited, Dictionary<int, Component> nodeToComponent)
        {
            visited.Add(v);
            var neighbours = v.Nodes
                .SelectMany(n => state.Map.GetAvaliableEdges(n.Id, state.CurrentPunter))
                .GroupBy(n => nodeToComponent[n.Item1.Id])
                .Where(n => n.Key != v && desiredComponent.Contains(n.Key)).ToArray();

            foreach (var neighbour in neighbours)
            {
                var power = neighbour.Count();
                if (!visited.Contains(neighbour.Key))
                {
                    var edge = neighbour.First().Item2;
                    grey.Add((neighbour.Key, edge));
                    edgePower[edge] = power;
                    calculatePower(neighbour.Key, edgePower, grey, state, visited, nodeToComponent);
                }
                else
                {
                    for (int i = grey.Count - 1; i >= 0; --i)
                    {
                        if (grey[i].Item1 == neighbour.Key)
                        {
                            break;
                        }
                        edgePower[grey[i].Item2] += power;
                    }
                }
            }

            grey.RemoveAt(grey.Count - 1);
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
            var S = new SortedSet<KeyValuePair<int, Component>>();
            foreach (var c in first.Nodes.SelectMany(n => map.GetAvaliableEdges(n.Id, punter).Select(e => nodeToComponent[e.Item1.Id])).Distinct())
            {
                S.Add(new KeyValuePair<int, Component>(first.Mines.Union(c.Mines).Sum(x => c.Scores[x.Id] + desiredScores[x.Id]), c));
            }

            for (int i = 0; i < movesCount; ++i)
            {
                while (S.Count != 0 && desiredComponent.Contains(S.Max.Value))
                {
                    S.Remove(S.Max);
                }
                if (S.Count == 0)
                {
                    break;
                }
                var newComponent = S.Max.Value;
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
                        S.Add(new KeyValuePair<int, Component>(desiredMines.Union(c.Mines).Sum(x => c.Scores[x.Id] + desiredScores[x.Id]), c));
                    }
                }
                else
                {
                    foreach (var c in newComponent.Nodes.SelectMany(n => map.GetAvaliableEdges(n.Id, punter).Select(e => nodeToComponent[e.Item1.Id])).Distinct())
                    {
                        S.Add(new KeyValuePair<int, Component>(desiredMines.Union(c.Mines).Sum(x => c.Scores[x.Id] + desiredScores[x.Id]), c));
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