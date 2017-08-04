using Core.Contracts;
using Core.Objects;

namespace Client
{
	public class GameSession
	{
		public int MyId { get; set; }
		public Map Map { get; set; }
		public GameStatus Status { get; set; }
		public SetupMessage setupMessage { get; set; }
	}
}