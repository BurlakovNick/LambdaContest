using Core.Objects;

namespace Core
{
    public interface IGraphVisitor
    {
        Node[] GetReachableNodesFromMinesForPunter(Map map, Punter punter);
        Node[] GetReachableNodesForPunter(Node from, Map map, Punter punter);
    }
}