using System.Collections.Generic;

namespace Core.Contracts
{
	public class MoveMessage
	{
		public Internal move { get; set; }

		public class Internal
		{
			public List<MoveCommand> moves { get; set; }
		}
	}
}