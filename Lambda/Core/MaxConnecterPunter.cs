using System;
using Core.Components;
using Core.Objects;

namespace Core
{
    public class MaxConnecterPunter : IPunter
    {
        private readonly IScorer scorer;
        private PunterState state;
        private int movesCount;
        private int lambdasCount;
        private IComponentManager componentManager;
        private DesireComponent desire;

        public MaxConnecterPunter(IScorer scorer)
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
            long maxScore = 0;
            DesireComponent result = null;
            Component a = null, b = null;
            var mineComponents = componentManager.GetMineComponents();
            for (var i = 0; i < mineComponents.Length; ++i)
            {
                for (var j = 0; j < i; ++j)
                {
                    var length = componentManager.FindShortestPathLength(mineComponents[i], mineComponents[j]);
                    if (length < movesCount)
                    {
                        long score;
                        var desireComponent = componentManager.FindShortestPath(mineComponents[i], mineComponents[j], out score);
                        if (score > maxScore)
                        {
                            maxScore = score;
                            result = desireComponent;
                            a = mineComponents[i];
                            b = mineComponents[j];
                        }
                    }
                }
            }

            if (a != null)
            {
                result.Root = a.Score.SelfScore > b.Score.SelfScore ? a : b;
            }
            return result;
        }

        public PunterState State
        {
            get => state;
            set => state = value;
        }
    }
}