using System;
using System.Linq;
using Core.Objects;

namespace Core
{
    public class RandomPunter: IPunter
    {
        private PunterState state;
        private readonly Random random = new Random();

        public RandomPunter()
        {
            state = new PunterState();
        }

        public void Init(Map map, int puntersCount, Punter punter)
        {
        }

        public Edge Claim(GameState gameState)
        {
            return gameState.Map.Edges
                            .OrderBy(x => random.Next())
                            .FirstOrDefault(x => x.Punter == null);
        }

        public PunterState State
        {
            get => state;
            set => state = value;
        }
    }
}