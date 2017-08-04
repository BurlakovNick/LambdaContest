using System.Collections.Generic;

namespace Core.Contracts
{
	public class MoveMessage
	{
		public InternalMove move { get; set; }
		public InternalStop stop { get; set; }

		public bool IsStop => stop != null;

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