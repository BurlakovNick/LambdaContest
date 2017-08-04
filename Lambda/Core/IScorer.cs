using Core.Objects;

namespace Core
{
    public interface IScorer
    {
        int Score(GameState gameState, Punter punter);
    }
}