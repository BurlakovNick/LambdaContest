using System.Collections.Generic;
using System.Linq;
using Core.Objects;
using JetBrains.Annotations;

namespace Core
{
    public class GraphVisitor : IGraphVisitor
    {
        public Node[] GetReachableNodesFromMinesForPunter(Map map, Punter punter)
        {
            var result = new List<Node>();
            var queue = new Queue<Node>();
            var visitedNodeIds = new HashSet<int>();

            foreach (var node in map.Nodes.Where(x => x.IsMine))
            {
                result.Add(AddNode(queue, visitedNodeIds, node));
            }
            Bfs(map, punter, queue, visitedNodeIds, result);

            return result.ToArray();
        }

        public Node[] GetReachableNodesForPunter([NotNull] Node from, [NotNull] Map map, [NotNull] Punter punter)
        {
            var result = new List<Node>();
            var queue = new Queue<Node>();
            var visitedNodeIds = new HashSet<int>();

            result.Add(AddNode(queue, visitedNodeIds, from));
            Bfs(map, punter, queue, visitedNodeIds, result);

            return result.ToArray();
        }

        private static void Bfs(Map map, Punter punter, Queue<Node> queue, HashSet<int> visitedNodeIds, List<Node> result)
        {
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
        }

        private static Node AddNode(Queue<Node> queue, HashSet<int> visitedNodeIds, Node node)
        {
            queue.Enqueue(node);
            visitedNodeIds.Add(node.Id);
            return node;
        }
    }
}