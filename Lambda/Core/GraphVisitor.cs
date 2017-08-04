using System.Collections.Generic;
using Core.Objects;
using JetBrains.Annotations;

namespace Core
{
    public class GraphVisitor : IGraphVisitor
    {
        public Node[] GetReachableNodesForPunter([NotNull] Node from, [NotNull] Map map, [NotNull] Punter punter)
        {
            var result = new List<Node>();
            var queue = new Queue<Node>();
            var visitedNodeIds = new HashSet<int>();

            result.Add(AddNode(queue, visitedNodeIds, from));
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                foreach ((var to, var edge) in map.GetEdges(node.Id))
                {
                    if (edge.Punter != null &&
                        edge.Punter.Id == punter.Id &&
                        !visitedNodeIds.Contains(to.Id))
                    {
                        result.Add(AddNode(queue, visitedNodeIds, to));
                    }
                }
            }

            return result.ToArray();
        }

        private static Node AddNode(Queue<Node> queue, HashSet<int> visitedNodeIds, Node node)
        {
            queue.Enqueue(node);
            visitedNodeIds.Add(node.Id);
            return node;
        }
    }
}