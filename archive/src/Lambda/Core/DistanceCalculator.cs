using System;
using System.Collections.Generic;
using Core.Objects;

namespace Core
{
    public class DistanceCalculator : IDistanceCalculator
    {
        public ShortestDistance[] GetShortest(Node from, Map map)
        {
            var result = new List<ShortestDistance>();
            var queue = new Queue<(Node, int)>();
            var visitedNodeIds = new HashSet<int>();

            result.Add(AddNode(queue, visitedNodeIds, from, from, 0));
            while (queue.Count > 0)
            {
                (var node, var dist) = queue.Dequeue();

                foreach ((var to, var _) in map.GetEdges(node.Id))
                {
                    if (!visitedNodeIds.Contains(to.Id))
                    {
                        result.Add(AddNode(queue, visitedNodeIds, from, to, dist + 1));
                    }
                }
            }

            return result.ToArray();
        }

        private static ShortestDistance AddNode(Queue<(Node, int)> queue, HashSet<int> visitedNodeIds, Node from, Node to, int distance)
        {
            queue.Enqueue((to, distance));
            visitedNodeIds.Add(to.Id);
            return new ShortestDistance {From = from, To = to, Length = distance};
        }
    }
}