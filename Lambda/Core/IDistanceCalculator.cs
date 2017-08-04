using Core.Objects;

namespace Core
{
    public interface IDistanceCalculator
    {
        ShortestDistance[] GetShortest(Node from, Map map);
    }
}