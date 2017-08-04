using System.Collections.Generic;

namespace Core.Objects
{
    public class Map
    {
        private readonly Dictionary<int, List<(Node, Edge)>> nodeEdges;

        private Map()
        {
        }

        public Map(Node[] nodes, Edge[] edges)
        {
            Nodes = nodes;
            Edges = edges;

            nodeEdges = new Dictionary<int, List<(Node, Edge)>>();
            foreach (var node in nodes)
            {
                nodeEdges[node.Id] = new List<(Node, Edge)>();
            }

            foreach (var edge in edges)
            {
                nodeEdges[edge.Source.Id].Add((edge.Target, edge));
                nodeEdges[edge.Target.Id].Add((edge.Source, edge));
            }
        }

        public Node[] Nodes { get; }
        public Edge[] Edges { get; }

        public List<(Node, Edge)> GetEdges(int fromNodeId) => nodeEdges[fromNodeId];
    }
}