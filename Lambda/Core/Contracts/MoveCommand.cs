namespace Core.Contracts
{
	public class MoveCommand
	{
		public Claim claim { get; set; }
		public Pass pass { get; set; }

		public override string ToString() =>
			pass != null
				? $"Punter {pass.punter}. Pass"
				: $"Punter {claim.punter}. {claim.source}->{claim.target}";
	}
}