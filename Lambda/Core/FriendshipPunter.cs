using System;
using System.IO;
using Core.Components;
using Core.Objects;

namespace Core
{
    public class FriendshipPunter : IPunter
    {
        private readonly IScorer scorer;
        private IPunter nikitaPunter;
        private IGraphVisitor graphVisitor;
        private PunterState state;

        private IComponentManager componentManager;

        public FriendshipPunter(IScorer scorer)
        {
            this.scorer = scorer;
            graphVisitor = new GraphVisitor();
            componentManager = new ComponentManager(scorer);
            state = new PunterState
            {
                
            };
        }

        public PunterState State
        {
            get
            {
                state.ComponentManagerState = componentManager.State;
                return state;
            }
            set
            {
                state = value;
                componentManager.State = state.ComponentManagerState;
                if (state.desire != null)
                {
                    state.desire.UpdateComponents(componentManager.State.components);
                }
            }
        }

        public void Init(Map map, int puntersCount, Punter punter)
        {
            state.movesCount = (map.Edges.Length - punter.Id + puntersCount - 1) / puntersCount;
            state.puntersCount = puntersCount;
            scorer.Init(map);
            componentManager.InitComponents(map, punter);
            var mineCount = componentManager.GetMineComponents().Length;
            state.lambdasCount = Math.Max(Math.Min(state.movesCount / 20, 2 * mineCount), mineCount);
            state.desire = new DesireComponent();
        }

        public Edge Claim(GameState gameState)
        {
            if (state.puntersCount == 2)
            {
                return new BargeHauler3(scorer, graphVisitor).Claim(gameState);
            }

            componentManager.UpdateMap(gameState.Map);
            Edge edge;

            if (state.lambdasCount > 0)
            {
                edge = componentManager.GetMineEdge();
                --state.lambdasCount;
            }
            else
            {
                if (state.desire != null && (state.desire.Components.Count <= 1 || !componentManager.IsConnected(state.desire)))
                {
                    state.desire = GetDesire();
                }

                edge = state.desire == null
                    ? new BargeHauler3(scorer, graphVisitor).Claim(gameState)
                    : componentManager.GetFragileEdge(state.desire);
            }

            componentManager.ClaimEdge(edge.Source, edge.Target, state.desire);
            state.movesCount--;
            return edge;
        }

        private DesireComponent GetDesire()
        {
            var minLength = state.movesCount;
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
    }
}