using System;
using Core.Contracts;
using Core.Contracts.Converters;
using Newtonsoft.Json;
using SimpleTCP;

namespace Client
{
	public class Client
	{
		private GameSession session;

		public void Start(int playersCount)
		{
			session = new GameSession();
			var tcpClient = new SimpleTcpClient().Connect("localhost", 7777);
			tcpClient.DataReceived += TcpClient_DataReceived;

			var handshakeCommand = new HandshakeCommand { me = $"player{new Random().Next(1000)}" };
			var reply = tcpClient.WriteLineAndGetReply(JsonConvert.SerializeObject(handshakeCommand), TimeSpan.MaxValue);
			var handshakeMessage = JsonConvert.DeserializeObject<HandshakeMessage>(reply.MessageString);
			if (handshakeMessage.you != handshakeCommand.me)
				throw new Exception($"me: {handshakeCommand.me}, you: {handshakeMessage.you}");
		}

		private void TcpClient_DataReceived(object sender,
											Message e)
		{
			switch (session.Status)
			{
				case GameStatus.Setup:
					HandleSetup(e);
					break;
				case GameStatus.Gameplay:
					HandleGameplay(e);
					break;
				case GameStatus.Scoring:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void HandleSetup(Message e)
		{
			var setupMessage = JsonConvert.DeserializeObject<SetupMessage>(e.MessageString);
			session.setupMessage = setupMessage;
			session.MyId = setupMessage.punter;
			session.Map = Converter.Convert(setupMessage.map);

			e.Reply(JsonConvert.SerializeObject(new SetupCommand
												{
													ready = setupMessage.punter
												}));

			session.Status = GameStatus.Gameplay;
		}

		private void HandleGameplay(Message e)
		{
		}
	}
}