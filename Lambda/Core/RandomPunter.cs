using System;
using System.Linq;
using Core.Objects;

namespace Core
{
    public class RandomPunter: IPunter
    {
        private readonly Random random = new Random();

        public void Init(Map map,
                         int puntersCount,
                         Punter punter)
        {
        }

        public Edge Claim(GameState gameState)
        {
            return gameState.Map.Edges
                            .OrderBy(x => random.Next())
                            .FirstOrDefault(x => x.Punter == null);
        }
    }
}