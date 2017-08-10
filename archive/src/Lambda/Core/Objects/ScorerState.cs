using System.Collections.Generic;

namespace Core.Objects
{
    public class ScorerState
    {
        public Node[] Mines { get; set; }
        public Dictionary<int, Dictionary<int, int>> DistancesFromMines { get; set; }
    }
}