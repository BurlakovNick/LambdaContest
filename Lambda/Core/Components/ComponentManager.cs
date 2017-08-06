using System.Collections.Generic;
using System.Linq;
using Core.Objects;

namespace Core.Components
{
    public class ComponentManager : IComponentManager
    {
        private readonly IScorer scorer;
        private Node[] mines;
        private Map map;
        private Punter punter;
        private List<Component> components;
        private Dictionary<int, Component> nodeToComponent;

        public ComponentManager(IScorer scorer)
        {
            this.scorer = scorer;
        }

        public void InitComponents(Map map, Punter punter)
        {
            this.map = map;
            this.punter = punter;
            mines = map.Nodes.Where(n => n.IsMine).ToArray();
            components = map.Nodes.Select(n => new Component(n, mines, scorer)).ToList();
            nodeToComponent = components
                .SelectMany(c => c.Nodes.Select(n => new { Key = n, Value = c }))
                .ToDictionary(x => x.Key.Id, x => x.Value);
        }

        public void UpdateMap(Map map)
        {
            this.map = map;
        }

        public void ClaimEdge(Node a, Node b)
        {
            var sourceComponent = nodeToComponent[a.Id];
            var targetComponent = nodeToComponent[b.Id];
            if (sourceComponent != targetComponent)
            {
                sourceComponent.Union(targetComponent);
                components.Remove(targetComponent);
                foreach (var node in targetComponent.Nodes)
                {
                    nodeToComponent[node.Id] = sourceComponent;
                }
            }
        }

        public int FindShortestPathLength(Component a, Component b)
        {
            throw new System.NotImplementedException();
        }

        public DesireComponent FindShortestPath(Component a, Component b)
        {
            throw new System.NotImplementedException();
        }

        private (Component, int)[] GetEdges(Component from)
        {
            return from.Nodes.SelectMany(n => map.GetFreeEdges(n.Id))
                .GroupBy(n => nodeToComponent[n.Item1.Id])
                .Select(x => (x.Key, x.Count()))
                .ToArray();
        }

    }
}