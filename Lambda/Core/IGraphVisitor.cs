using Core.Objects;

namespace Core
{
    public interface IGraphVisitor
    {
        PunterConnectedComponents GetConnectedComponents(Map map);
        PunterConnectedComponents GetConnectedByAvailableEdgesComponents(Map map);
        Node[] GetReachableNodesFromMinesForPunter(Map map, Punter punter);
        Node[] GetReachableNodesForPunter(Node from, Map map, Punter punter, bool withFreeEdges = false);
        Edge[] GetBridgesInAvailableEdges(Map map, Punter punter);
    }
}