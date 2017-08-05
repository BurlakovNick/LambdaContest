using System;

namespace Core
{
	public class ConsoleLog: ILog
	{
		public void Log(string message)
		{
			Console.Out.WriteLine(message);
			Console.Out.WriteLine();
		}
	}
}