using Core.Objects;

namespace Core
{
    public interface IPunter
    {
        void Init(Map map);
        Edge Claim(GameState gameState);
    }
}