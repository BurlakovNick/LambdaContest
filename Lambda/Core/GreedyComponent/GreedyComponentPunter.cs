using System.Collections.Generic;
using System.Linq;
using Core.Objects;

namespace Core.GreedyComponent
{
    public class GreedyComponentPunter : IPunter
    {
        private readonly IScorer scorer;
        private Node[] mines;
        private List<Component> components;
        private HashSet<Component> desiredComponent;
        private int movesCount;

        public GreedyComponentPunter(IScorer scorer)
        {
            this.scorer = scorer;
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
            return null;
        }

        private void FindDesiredComponent(Map map, Punter punter)
        {
            var top = movesCount / 2;
            var mineToScore = mines.ToDictionary(m => m, m => components.Select(n => n.Scores[m]).OrderByDescending(s => s).Take(top).Sum());
            var first = components.OrderByDescending(n => n.Mines.Sum(x => mineToScore[x])).First();

            var desiredScores = first.Scores.ToDictionary(x => x.Key, x => x.Value);
            var desiredMines = first.Mines.Select(x => x).ToList();

            var nodeToComponent = components
                .SelectMany(c => c.Nodes.Select(n => new {Key = n, Value = c}))
                .ToDictionary(x => x.Key, x => x.Value);

            desiredComponent = new HashSet<Component> {first};
            var S = new SortedSet<KeyValuePair<int, Component>>();
            foreach (var c in first.Nodes.SelectMany(n => map.GetAvaliableEdges(n.Id, punter).Select(e => nodeToComponent[e.Item1])).Distinct())
            {
                S.Add(new KeyValuePair<int, Component>(first.Mines.Union(c.Mines).Sum(x => c.Scores[x] + desiredScores[x]), c));
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
                        S.Add(new KeyValuePair<int, Component>(desiredMines.Union(c.Mines).Sum(x => c.Scores[x] + desiredScores[x]), c));
                    }
                }
                else
                {
                    foreach (var c in newComponent.Nodes.SelectMany(n => map.GetAvaliableEdges(n.Id, punter).Select(e => nodeToComponent[e.Item1])).Distinct())
                    {
                        S.Add(new KeyValuePair<int, Component>(desiredMines.Union(c.Mines).Sum(x => c.Scores[x] + desiredScores[x]), c));
                    }
                }
            }
        }
    }
}