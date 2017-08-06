using System;
using System.Collections.Generic;
using System.Linq;
using Core.Objects;

namespace Core.Components
{
    public class Component : IComparable
    {
        public List<Node> Nodes { get; set; }
        public List<Node> Mines { get; set; }
        public Dictionary<int, long> Scores { get; set; } = new Dictionary<int, long>();
        public long SelfScore { get; set; }
        public int SubtreeSize { get; set; }

        public Component(Node node, Node[] mines, IScorer scorer)
        {
            Nodes = new List<Node> {node};
            Mines = Nodes.Where(x => x.IsMine).ToList();
            SelfScore = Mines.Sum(x => Scores[x.Id]);
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
            SelfScore = Mines.Sum(x => Scores[x.Id]);
        }

        public int CompareTo(object obj)
        {
            if (SubtreeSize != ((Component) obj).SubtreeSize)
            {
                return ((Component) obj).SubtreeSize - SubtreeSize;
            }
            return GetHashCode() - obj.GetHashCode();
        }
    }
}