using Core.Objects;

namespace Core
{
    public interface IPunter
    {
        void Init(Map map, int puntersCount, Punter punter);
        Edge Claim(GameState gameState);
    }
}