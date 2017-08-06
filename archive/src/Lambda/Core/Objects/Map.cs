using System.Collections.Generic;
using System.Linq;

namespace Core.Objects
{
    public class Map
    {
        private readonly Dictionary<int, List<(Node, Edge)>> nodeEdges;
        private readonly Dictionary<(int, int), Edge> sourceTargetToEdge = new Dictionary<(int, int), Edge>();

        public Map(Node[] nodes,
                   Edge[] edges)
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
                sourceTargetToEdge.Add((edge.Source.Id, edge.Target.Id), edge);
                sourceTargetToEdge.Add((edge.Target.Id, edge.Source.Id), edge);
            }
        }

        public Node[] Nodes { get; }
        public Edge[] Edges { get; }

        public List<(Node, Edge)> GetEdges(int fromNodeId) => nodeEdges[fromNodeId];

        public List<(Node, Edge)> GetAvaliableEdges(int fromNodeId,
                                                    Punter punter) =>
            nodeEdges[fromNodeId]
                .Where(e => e.Item2.Punter == null || e.Item2.Punter == punter)
                .ToList();

        public List<(Node, Edge)> GetFreeEdges(int fromNodeId) =>
            nodeEdges[fromNodeId]
                .Where(e => e.Item2.Punter == null)
                .ToList();

        public void Claim(int source,
                          int target,
                          int punterId)
        {
            sourceTargetToEdge[(source, target)].Punter = new Punter { Id = punterId };
        }
    }
}