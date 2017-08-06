using System;
using System.Diagnostics;
using System.Threading;
using SimpleTCP;

namespace Server
{
	public static class Ext
	{
		public static Message WriteAndGetReply(this SimpleTcpClient client,
											   string data,
											   TimeSpan timeout)
		{
			Message mReply = null;

			void onReceived(object s,
							Message e) => mReply = e;

			client.DataReceived += onReceived;
			client.Write(data);
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			while (mReply == null && stopwatch.Elapsed < timeout)
				Thread.Sleep(10);
			client.DataReceived -= onReceived;
			return mReply;
		}
	}
}