using System;
using System.Collections.Generic;
using System.Linq;
using Core.Objects;

namespace Core.GreedyComponent
{
    public class Component : IComparable
    {
        public List<Node> Nodes { get; set; }
        public List<Node> Mines { get; set; }
        public Dictionary<int, int> Scores { get; set; } = new Dictionary<int, int>();

        public Component(Node node, Node[] mines, IScorer scorer)
        {
            Nodes = new List<Node> {node};
            Mines = Nodes.Where(x => x.IsMine).ToList();
            foreach (var mine in mines)
            {
                var d = scorer.GetDistance(mine, node);
                Scores.Add(mine.Id, d * d);
            }
        }

        public void Union(Component other)
        {
            Nodes.AddRange(other.Nodes);
            Mines.AddRange(other.Mines);
            foreach (var score in other.Scores)
            {
                Scores[score.Key] += score.Value;
            }
        }

        public int CompareTo(object obj)
        {
            return GetHashCode() - obj.GetHashCode();
        }
    }
}