using System.Collections.Generic;
using System.Linq;
using Core.Objects;

namespace Core.Contracts.Converters
{
	public static class Converter
	{
		public static Map Convert(MapContract mapContract, List<MoveCommand> moves = null)
		{
			var mines = new HashSet<int>(mapContract.mines);
			var nodes = mapContract.sites.Select(x => CreateNode(x.id, mines)).ToArray();
			var edgeToPunters = moves
				.Where(x => x.claim != null)
				.SelectMany(x => new[]
								 {
									 (x.claim.source, x.claim.target, x.claim.punter),
									 (x.claim.target, x.claim.source, x.claim.punter)
								 })
				.ToDictionary(x => (x.Item1, x.Item2), x => x.Item3);

			var edges = mapContract
				.rivers
				.Select(x =>
						{
							var punter = edgeToPunters.TryGetValue((x.source, x.target), out var punterId)
											 ? new Punter { Id = punterId }
											 : null;

							return new Edge
								   {
									   Punter = punter,
									   Source = CreateNode(x.source, mines),
									   Target = CreateNode(x.target, mines)
								   };
						})
				.ToArray();

			return new Map(nodes, edges);
		}

		private static Node CreateNode(int id,
									   HashSet<int> mines)
		{
			return new Node{ Id = id, IsMine = mines.Contains(id)};
		}
	}
}