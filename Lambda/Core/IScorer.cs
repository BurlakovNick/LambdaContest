using Core.Objects;

namespace Core
{
    public interface IScorer
    {
        void Init(Map map);
        int Score(GameState gameState);
    }
}