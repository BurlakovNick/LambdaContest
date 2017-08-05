using System.Collections.Generic;

namespace Core.Contracts
{
    public class MoveMessage
    {
        //setup
        public int punter { get; set; }
        public int punters { get; set; }
        public MapContract map { get; set; }

        //move
        public InternalMove move { get; set; }

        //score
        public InternalStop stop { get; set; }

        public GameStateMessage state { get; set; }

        public bool IsSetup => map != null;
        public bool IsStop => stop != null;
        public bool IsGameplay => move != null;

        public class InternalMove
        {
            public List<MoveCommand> moves { get; set; }
        }

        public class InternalStop
        {
            public List<MoveCommand> moves { get; set; }
            public Score[] scores { get; set; }
        }

        public class Score
        {
            public int punter { get; set; }
            public int score { get; set; }
        }
    }
}