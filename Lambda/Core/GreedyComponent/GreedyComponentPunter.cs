using Core.Components;
using Core.Objects;

namespace Core.GreedyComponent
{
    public class GreedyComponentPunter : IPunter
    {
        private readonly IScorer scorer;
        private DesireComponent desire;
        private int movesCount;
        private PunterState state;
        private IComponentManager componentManager;

        public GreedyComponentPunter(IScorer scorer)
        {
            this.scorer = scorer;
            componentManager = new ComponentManager(scorer);
            state = new PunterState();
        }

        public void Init(Map map, int puntersCount, Punter punter)
        {
            scorer.Init(map);
            movesCount = (map.Edges.Length - punter.Id + puntersCount - 1) / puntersCount;
            componentManager.InitComponents(map, punter);
            desire = componentManager.FindGreedyFullComponent(movesCount + 1);
        }

        public Edge Claim(GameState gameState)
        {
            componentManager.UpdateMap(gameState.Map);

            if (componentManager.IsConnected(desire))
            {
                desire = componentManager.FindGreedyFullComponent(movesCount + 1);
            }
            movesCount--;

            var edge = componentManager.GetFragileEdge(desire);
            componentManager.ClaimEdge(edge.Source, edge.Target, desire);
            return edge;
        }

        public PunterState State
        {
            get => state;
            set => state = value;
        }
    }
}