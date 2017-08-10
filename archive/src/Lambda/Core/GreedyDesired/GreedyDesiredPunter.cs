using Core.Components;
using Core.Objects;

namespace Core.GreedyDesired
{
    public class GreedyDesiredPunter : IPunter
    {
        private readonly IScorer scorer;
        private PunterState state;
        private int movesCount;
        private IComponentManager componentManager;

        public GreedyDesiredPunter(IScorer scorer)
        {
            this.scorer = scorer;
            componentManager = new ComponentManager(scorer);
            state = new PunterState();
        }

        public void Init(Map map, int puntersCount, Punter punter)
        {
            movesCount = (map.Edges.Length - punter.Id + puntersCount - 1) / puntersCount;
            scorer.Init(map);
            componentManager.InitComponents(map, punter);
        }

        public Edge Claim(GameState gameState)
        {
            componentManager.UpdateMap(gameState.Map);

            var component = componentManager.GetBestComponentByChart(movesCount / 2);
            --movesCount;

            var edge = componentManager.GetMostExpensiveFromComponentEdge(component);

            componentManager.ClaimEdge(edge.Source, edge.Target);
            return edge;
        }

        public PunterState State
        {
            get => state;
            set => state = value;
        }
    }
}
