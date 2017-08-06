using Core.Objects;

namespace Core
{
    public interface IScorer
    {
        void Init(Map map);
        int Score(GameState gameState);
        int GetDistance(Node mine, Node node);
        ScorerState State { get; set; }
        int ScoreForUnitingComponents(int[] leftComponent, int[] rightComponent);
    }
}