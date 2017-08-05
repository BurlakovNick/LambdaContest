using System.Collections.Generic;

namespace Core.Objects
{
    public class ScorerState
    {
        public Dictionary<(int, int), int> DistancesFromMines  { get; set; }
    }
}