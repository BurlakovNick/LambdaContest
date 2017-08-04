using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Core.Contracts;
using Newtonsoft.Json;
using SimpleTCP;

namespace Server
{
	public class Server: IServer
	{
		private GameSession session;
		private SimpleTcpServer tcpServer;

		public void Start(int playersCount)
		{
			session = new GameSession(playersCount);
			tcpServer = new SimpleTcpServer().Start(7777);
			Console.WriteLine("Tcp Server listening...");
			tcpServer.DataReceived += TcpServer_DataReceived;
		}

		private void TcpServer_DataReceived(object sender,
											Message e)
		{
			HandleHandshake(e);
		}

		private void HandleHandshake(Message message)
		{
			var handshakeCommand = JsonConvert.DeserializeObject<HandshakeCommand>(message.MessageString);
			var handshakeMessage = new HandshakeMessage
								   {
									   you = handshakeCommand.me
								   };
			message.Reply(JsonConvert.SerializeObject(handshakeMessage));

			var connection = new PlayerConnection(message.TcpClient, handshakeCommand.me);
			session.Clients.Add(connection);

			if (session.Clients.Count == session.PlayersCount)
			{
				tcpServer.DataReceived -= TcpServer_DataReceived;
				Setup();
			}
		}

		private void Setup()
		{
			var map = GetMap("sample");
			foreach (var (x, i) in session.Clients.Select((x,
														   i) => (x, i)))
			{
				var setupMessage = new SetupMessage
								   {
									   punter = i,
									   punters = session.PlayersCount,
									   map = map
								   };
				var reply = x.Client.WriteAndGetReply(JsonConvert.SerializeObject(setupMessage),
													  TimeSpan.FromSeconds(10));
				var setupCommand = JsonConvert.DeserializeObject<SetupCommand>(reply.MessageString);
				if (setupCommand.ready != i)
					throw new Exception("ready must be equal to player id");
			}

			Game(map);
		}

		private void Game(MapContract map)
		{
			var moveNumber = 0;
			var moves = new MoveMessage
						{
							move = new MoveMessage.Internal
								   {
									   moves = session.Clients
													  .Select((x,
															   i) => new MoveCommand
																	 {
																		 pass = new Pass
																				{
																					punter = i
																				}
																	 })
													  .ToList()
								   }
						};
			while (moveNumber != map.rivers.Length)
			{
				foreach (var connection in session.Clients)
				{
					var reply = connection.Client.WriteAndGetReply(JsonConvert.SerializeObject(moves), TimeSpan.FromSeconds(1));
					var moveCommand = JsonConvert.DeserializeObject<MoveCommand>(reply.MessageString);
					moves.move.moves.RemoveAt(0);
					moves.move.moves.Add(moveCommand);
					moveNumber++;
					if (moveNumber == map.rivers.Length)
						break;
				}
			}

			Scoring(moves);
		}

		private void Scoring(MoveMessage moves)
		{
			foreach (var connection in session.Clients)
			{
				var scoringMessage = new ScoringMessage
									 {
										 stop = new ScoringMessage.Internal
												{
													moves = moves.move.moves,
													scores = new ScoringMessage.Score[0]
												}
									 };
				connection.Client.Write(JsonConvert.SerializeObject(scoringMessage));
			}
		}

		private static MapContract GetMap(string name)
		{
			var filePath = Path.Combine("Maps", $"{name}.json");
			var fileContent = File.ReadAllText(filePath);
			var map = JsonConvert.DeserializeObject<MapContract>(fileContent);
			return map;
		}
	}

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