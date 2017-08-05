using System;
using Core;
using Core.Contracts;
using Core.Contracts.Converters;
using Core.Objects;
using Newtonsoft.Json;
using SimpleTCP;

namespace Client
{
	public class OnlineClient
	{
		private GameSession session;
		private readonly IPunter punter;
		private readonly Action<string> log;

		public OnlineClient(IPunter punter,
							ILog log)
		{
			this.punter = punter;
			this.log = log.Log;
		}

		public void Start()
		{
			session = new GameSession();

			var tcpClient = new SimpleTcpClient().Connect("localhost", 7777);
			log("tcp client connected to server");
			tcpClient.DataReceived += TcpClient_DataReceived;

			var handshakeCommand = new HandshakeCommand { me = $"player{new Random().Next(1000)}" };
			log($"Begin handshake as {handshakeCommand.me}");
			var reply = tcpClient.WriteAndGetReply(JsonConvert.SerializeObject(handshakeCommand), TimeSpan.MaxValue);
			var handshakeMessage = JsonConvert.DeserializeObject<HandshakeMessage>(reply.MessageString);
			if (handshakeMessage.you != handshakeCommand.me)
				throw new Exception($"me: {handshakeCommand.me}, you: {handshakeMessage.you}");
			session.Status = GameStatus.Setup;
		}

		private void TcpClient_DataReceived(object sender,
											Message e)
		{
			try
			{
				log($"Handling message in TcpClient_DataReceived. Message: {e.MessageString}");
				switch (session.Status)
				{
					case GameStatus.Setup:
						HandleSetup(e);
						break;
					case GameStatus.Gameplay:
						HandleGameplay(e);
						break;
					case GameStatus.Handshake:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			catch (Exception exception)
			{
				log("ERROR: " + exception);
				throw;
			}
		}

		private void HandleSetup(Message e)
		{
			log("HandleSetup");
			var setupMessage = JsonConvert.DeserializeObject<SetupMessage>(e.MessageString);
			session.setupMessage = setupMessage;
			session.MyId = setupMessage.punter;
			session.Map = Converter.Convert(setupMessage.map);

			punter.Init(session.Map, setupMessage.punters, new Punter { Id = setupMessage.punter });

			e.Reply(JsonConvert.SerializeObject(new SetupCommand
												{
													ready = setupMessage.punter
												}));

			session.Status = GameStatus.Gameplay;
		}

		private void HandleGameplay(Message e)
		{
			log("HandleGameplay");
			var moveMessage = JsonConvert.DeserializeObject<MoveMessage>(e.MessageString);
			if (moveMessage.IsStop)
			{
				ShowScores(moveMessage.stop.scores);
			}
			else
			{
				var gameState = new GameState
								{
									CurrentPunter = new Punter
													{
														Id = session.MyId
													},
									Map = session.Map
								};
				var edge = punter.Claim(gameState);
				var moveCommand = new MoveCommand
								  {
									  claim = new Claim
											  {
												  punter = session.MyId,
												  source = edge.Source.Id,
												  target = edge.Target.Id
											  }
								  };
				e.Reply(JsonConvert.SerializeObject(moveCommand));
			}
		}

		private void ShowScores(MoveMessage.Score[] scores)
		{
			log("scores: " + JsonConvert.SerializeObject(scores));
		}
	}
}