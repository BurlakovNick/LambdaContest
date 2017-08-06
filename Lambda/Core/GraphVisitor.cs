using System.Collections.Generic;
using System.Linq;
using Core.Objects;
using JetBrains.Annotations;

namespace Core
{
    public class GraphVisitor : IGraphVisitor
    {
        public PunterConnectedComponents GetConnectedComponents(Map map)
        {
            return GetConnectedComponents(map, false);
        }

        public PunterConnectedComponents GetConnectedByAvailableEdgesComponents(Map map)
        {
            return GetConnectedComponents(map, true);
        }

        private static PunterConnectedComponents GetConnectedComponents(Map map, bool withFreeEdges)
        {
            var connectedComponents = new PunterConnectedComponents();

            var punters = map
                .Edges
                .Select(x => x.Punter)
                .Where(x => x != null)
                .Select(x => x.Id)
                .Distinct()
                .Select(x => new Punter {Id = x})
                .ToArray();

            var queue = new Queue<Node>();
            foreach (var punter in punters)
            {
                var visitedNodeIds = new HashSet<int>();

                foreach (var node in map.Nodes)
                {
                    if (visitedNodeIds.Contains(node.Id))
                    {
                        continue;
                    }

                    var component = new List<Node>();
                    component.Add(AddNode(queue, visitedNodeIds, node));
                    Bfs(map, punter, queue, visitedNodeIds, component, withFreeEdges);

                    var nodeIds = component.Select(x => x.Id).ToArray();
                    connectedComponents.AddComponent(nodeIds, punter.Id);
                }
            }

            return connectedComponents;
        }

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

        private static void Bfs(Map map, Punter punter, Queue<Node> queue, HashSet<int> visitedNodeIds, List<Node> result, bool withFreeEdges = false)
        {
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                foreach ((var to, var edge) in map.GetEdges(node.Id))
                {
                    if (visitedNodeIds.Contains(to.Id))
                    {
                        continue;
                    }

                    if (withFreeEdges && edge.Punter == null ||
                        edge.Punter != null && edge.Punter.Id == punter.Id)
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