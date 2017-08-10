using System.Collections.Generic;
using Core.Objects;

namespace Core.Contracts
{
    public class GameStateMessage
    {
        public MapContract MapContract { get; set; }
        public List<MoveCommand> Moves { get; set; }
        public int Punters { get; set; }
        public int MyPunter { get; set; }
        public ScorerState ScorerState { get; set; }
        public PunterState PunterState { get; set; }
    }
}