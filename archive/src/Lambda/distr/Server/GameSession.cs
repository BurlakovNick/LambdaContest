using System.Collections.Generic;

namespace Server
{
	public class GameSession
	{
		public GameSession(int playersCount)
		{
			PlayersCount = playersCount;
		}

		public int PlayersCount { get; }
		public List<PlayerConnection> Clients { get; } = new List<PlayerConnection>();
	}
}