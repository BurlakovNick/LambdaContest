using Core.Objects;

namespace Core
{
    public interface IPunter
    {
        void Init(Map map, int puntersCount);
        Edge Claim(GameState gameState);
    }
}