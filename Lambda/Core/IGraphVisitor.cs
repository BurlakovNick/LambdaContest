using Core.Objects;

namespace Core
{
    public interface IGraphVisitor
    {
        Node[] GetReachableNodesForPunter(Node from, Map map, Punter punter);
    }
}