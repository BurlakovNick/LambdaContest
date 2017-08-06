using System;
using System.Collections.Generic;
using Core.Objects;

namespace Core.Components
{

    public class Component : IComparable
    {
        public List<Node> Nodes { get; set; }
        public ComponentScore Score { get; set; }
        public int SubtreeSize { get; set; }

        public Component(Node node, Node[] mines, IScorer scorer)
        {
            Nodes = new List<Node> { node };
            Score = new ComponentScore(node, mines, scorer);
        }

        public void Union(Component other)
        {
            Nodes.AddRange(other.Nodes);
            Score.Add(other.Score);
        }

        public int CompareTo(object obj)
        {
            if (SubtreeSize != ((Component)obj).SubtreeSize)
            {
                return ((Component)obj).SubtreeSize - SubtreeSize;
            }
            return GetHashCode() - obj.GetHashCode();
        }
    }
}