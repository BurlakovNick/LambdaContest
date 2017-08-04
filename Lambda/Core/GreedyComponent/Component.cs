using System.Collections.Generic;
using System.Linq;
using Core.Objects;

namespace Core.GreedyComponent
{
    public class Component
    {
        public List<Node> Nodes { get; set; }
        public List<Node> Mines { get; set; }
        public Dictionary<Node, int> Scores { get; set; }

        public Component(Node node, Node[] mines, IScorer scorer)
        {
            Nodes = new List<Node> {node};
            Mines = Nodes.Where(x => x.IsMine).ToList();
            foreach (var mine in mines)
            {
                var d = scorer.GetDistance(mine, node);
                Scores[mine] = d * d;
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
    }
}