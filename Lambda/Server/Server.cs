using System;
using System.IO;
using System.Linq;
using Core;
using Core.Contracts;
using Core.Contracts.Converters;
using Core.Objects;
using Newtonsoft.Json;
using SimpleTCP;

namespace Server
{
	public class Server: IServer
	{
		private readonly IScorer scorer;
		private GameSession session;
		private SimpleTcpServer tcpServer;

		public Server(IScorer scorer)
		{
			this.scorer = scorer;
		}

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
			scorer.Init(Converter.Convert(map));
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
			var lastMoves = new MoveMessage
							{
								move = new MoveMessage.InternalMove
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
			var moves = lastMoves;

			while (moveNumber != map.rivers.Length)
			{
				foreach (var connection in session.Clients)
				{
					var reply = connection.Client.WriteAndGetReply(JsonConvert.SerializeObject(lastMoves), TimeSpan.FromSeconds(1));
					var moveCommand = JsonConvert.DeserializeObject<MoveCommand>(reply.MessageString);
					lastMoves.move.moves.RemoveAt(0);
					lastMoves.move.moves.Add(moveCommand);
					moves.move.moves.Add(moveCommand);
					moveNumber++;
					if (moveNumber == map.rivers.Length)
						break;
				}
			}

			Scoring(map, lastMoves, moves);
		}

		private void Scoring(MapContract map,
							 MoveMessage lastMoves,
							 MoveMessage moves)
		{
			var mapInfo = Converter.Convert(map, moves.move.moves);

			var scores = session.Clients
								.Select((x,
										 i) => new GameState
											   {
												   Map = mapInfo,
												   CurrentPunter = new Punter { Id = i }
											   })
								.Select(x => (x.CurrentPunter, scorer.Score(x)))
								.ToArray();

			foreach (var connection in session.Clients)
			{
				var scoringMessage = new MoveMessage
									 {
										 stop = new MoveMessage.InternalStop
												{
													moves = lastMoves.move.moves,
													scores = scores
														.Select(x => new MoveMessage.Score
																	 {
																		 punter = x.Item1.Id,
																		 score = x.Item2
																	 })
														.ToArray()
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
}