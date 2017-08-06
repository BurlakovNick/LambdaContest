using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Objects
{
    public class PunterConnectedComponents
    {
        private readonly List<int[]> components;
        private readonly Dictionary<int, Dictionary<int, int>> componentId;
        private int componentsCount;

        public PunterConnectedComponents()
        {
            componentId = new Dictionary<int, Dictionary<int, int>>();
            components = new List<int[]>();
            componentsCount = 0;
        }

        public int[] GetPunters => componentId.Keys.ToArray();

        public void AddComponent(int[] nodeIds, int punterId)
        {
            if (!componentId.ContainsKey(punterId))
            {
                componentId[punterId] = new Dictionary<int, int>();
            }

            components.Add(nodeIds);
            foreach (var nodeId in nodeIds)
            {
                if (componentId[punterId].ContainsKey(nodeId))
                {
                    throw new Exception($"Node {nodeId} already in component {componentId[nodeId]}");
                }

                componentId[punterId][nodeId] = componentsCount;
            }

            componentsCount++;
        }

        public bool IsInSameComponent(int leftId, int rightId, int punterId)
        {
            var left = SafeGetComponent(punterId, leftId);
            var right = SafeGetComponent(punterId, rightId);

            if (left == -1 || right == -1)
            {
                return false;
            }

            return left == right;
        }

        public int[] GetComponent(int punterId, int nodeId)
        {
            var id = SafeGetComponent(punterId, nodeId);
            if (id == -1)
            {
                return new[] {nodeId};
            }

            return components[id];
        }

        private int SafeGetComponent(int punterId, int nodeId)
        {
            if (componentId.TryGetValue(punterId, out var componentIdsByNodeIds))
            {
                if (componentIdsByNodeIds.TryGetValue(nodeId, out var result))
                {
                    return result;
                }
            }

            return -1;
        }
    }
}