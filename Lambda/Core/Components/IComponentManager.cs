using Core.Objects;

namespace Core.Components
{
    public interface IComponentManager
    {
        void InitComponents(Map map, Punter punter);
        void UpdateMap(Map map);
        void ClaimEdge(Node a, Node b);

        int FindShortestPathLength(Component a, Component b);
        DesireComponent FindShortestPath(Component a, Component b);
    }
}