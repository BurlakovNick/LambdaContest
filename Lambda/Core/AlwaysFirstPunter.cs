using System.Linq;
using Core.Objects;

namespace Core
{
    public class AlwaysFirstPunter: IPunter
    {
        public void Init(Map map,
                         int puntersCount,
                         Punter punter)
        {
        }

        public Edge Claim(GameState gameState)
        {
            return gameState.Map.Edges.FirstOrDefault(x => x.Punter == null);
        }
    }
}