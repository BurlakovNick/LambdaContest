﻿namespace Core.Contracts
{
	public class SetupCommand
	{
		public int ready { get; set; }
	    public GameStateMessage state { get; set; }
	}
}