using System.Collections.Generic;

namespace Core.Contracts
{
	public class ScoringMessage
	{
		public Internal stop { get; set; }

		public class Internal
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