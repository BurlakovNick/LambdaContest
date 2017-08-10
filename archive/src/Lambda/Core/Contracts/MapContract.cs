namespace Core.Contracts
{
	public class MapContract
	{
		public SiteContract[] sites { get; set; }
		public RiverContract[] rivers { get; set; }
		public int[] mines { get; set; }
	}
}