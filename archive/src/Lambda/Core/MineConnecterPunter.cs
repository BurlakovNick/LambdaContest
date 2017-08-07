using System;
using Core.Components;
using Core.Objects;

namespace Core
{
    public class MineConnecterPunter : IPunter
    {
        private readonly IScorer scorer;
        private PunterState state;
        private int movesCount;
        private int lambdasCount;
        private IComponentManager componentManager;
        private DesireComponent desire;

        public MineConnecterPunter(IScorer scorer)
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
            var mineCount = componentManager.GetMineComponents().Length;
            lambdasCount = Math.Max(Math.Min(movesCount / 20, 2 * mineCount), mineCount);
            desire = new DesireComponent();
        }

        public Edge Claim(GameState gameState)
        {
            componentManager.UpdateMap(gameState.Map);
            Edge edge;

            if (lambdasCount > 0)
            {
                edge = componentManager.GetMineEdge();
                --lambdasCount;
            }
            else
            {
                if (desire != null && (desire.Components.Count <= 1 || !componentManager.IsConnected(desire)))
                {
                    desire = GetDesire();
                }
                edge = desire == null
                    ? componentManager.GetMostExpensiveEdge()
                    : componentManager.GetFragileEdge(desire);
            }

            componentManager.ClaimEdge(edge.Source, edge.Target, desire);
            movesCount--;
            return edge;
        }

        private DesireComponent GetDesire()
        {
            var minLength = movesCount;
            Component a = null, b = null;
            var mineComponents = componentManager.GetMineComponents();
            for (var i = 0; i < mineComponents.Length; ++i)
            {
                for (var j = 0; j < i; ++j)
                {
                    var length = componentManager.FindShortestPathLength(mineComponents[i], mineComponents[j]);
                    if (length < minLength)
                    {
                        minLength = length;
                        a = mineComponents[i];
                        b = mineComponents[j];
                    }
                }
            }

            if (a == null)
            {
                return null;
            }
            long temp;
            var result = componentManager.FindShortestPath(a, b, out temp);
            result.Root = a.Score.SelfScore > b.Score.SelfScore ? a : b;
            return result;
        }

        public PunterState State
        {
            get => state;
            set => state = value;
        }
    }
}