using Core.Objects;

namespace Core.Components
{
    public interface IComponentManager
    {
        void InitComponents(Map map, Punter punter);
        void UpdateMap(Map map);
        void ClaimEdge(Node a, Node b, DesireComponent desire = null);

        int FindShortestPathLength(Component a, Component b);
        DesireComponent FindShortestPath(Component a, Component b);
        DesireComponent FindGreedyFullComponent(int size);

        Component GetBestComponentByChart(int chartSize);
        bool IsConnected(DesireComponent component);

        Edge GetMineEdge();
        Edge GetMostExpensiveEdge();
        Edge GetMostExpensiveFromComponentEdge(Component from);
        Edge GetMaxSubtreeEdge(DesireComponent desire);
        Edge GetFragileEdge(DesireComponent desire);
    }
}