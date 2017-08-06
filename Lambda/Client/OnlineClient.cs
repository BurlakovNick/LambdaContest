using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Core;
using Core.Contracts;
using Core.Contracts.Converters;
using Core.Infrastructure;
using Core.Objects;
using SimpleTCP;

namespace Client
{
    public class OnlineClient
    {
        private GameSession session;
        private readonly IPunter punter;
        private readonly Serializer serializer = new Serializer();

        public OnlineClient(IPunter punter)
        {
            this.punter = punter;
        }

        public void Start(string server, string port)
        {
            try
            {
                session = new GameSession();

                var tcpClient = new SimpleTcpClient().Connect(server, int.Parse(port));
                Log("tcp client connected to server");

                var handshakeCommand = new HandshakeCommand { me = $"player{new Random().Next(1000)} {punter.GetType().Name}" };
                Log($"Begin handshake as {handshakeCommand.me}");
                var reply = tcpClient.WriteAndGetReply(serializer.Serialize(handshakeCommand), TimeSpan.MaxValue);
                session.Status = GameStatus.Setup;
                tcpClient.DataReceived += TcpClient_DataReceived;
                var handshakeMessage = serializer.Deserialize<HandshakeMessage>(reply.MessageString);
                if (handshakeMessage.you != handshakeCommand.me)
                    throw new Exception($"me: {handshakeCommand.me}, you: {handshakeMessage.you}");
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
        }

	    private void TcpClient_DataReceived(object sender,
                                            Message e)
        {
            try
            {
                Log($"Handling message in TcpClient_DataReceived. Message: {e.MessageString}, Status: {session.Status}");
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
                Log("ERROR: " + exception);
                throw;
            }
        }

	    private void HandleSetup(Message e)
        {
            Log("HandleSetup");
            var setupMessage = serializer.Deserialize<SetupMessage>(e.MessageString);
            session.setupMessage = setupMessage;
            session.MyId = setupMessage.punter;
            session.Map = Converter.Convert(setupMessage.map);

            Log($"##############################My Id is {session.MyId}");

            punter.Init(session.Map, setupMessage.punters, new Punter { Id = setupMessage.punter });

            e.Reply(serializer.Serialize(new SetupCommand
                                         {
                                             ready = setupMessage.punter
                                         }));

            session.Status = GameStatus.Gameplay;
        }

	    private void HandleGameplay(Message e)
        {
            Log("HandleGameplay");
            var moveMessage = serializer.Deserialize<MoveMessage>(e.MessageString);
            if (moveMessage.IsStop)
            {
                ShowScores(moveMessage.stop.scores);
                Environment.Exit(0);
            }
            else
            {
                var claims = moveMessage.move.moves.Where(x => x.claim != null);
                foreach (var move in claims)
                    session.Map.Claim(source: move.claim.source,
                                      target: move.claim.target,
                                      punterId: move.claim.punter);

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
                e.Reply(serializer.Serialize(moveCommand));
            }
        }

	    private void ShowScores(MoveMessage.Score[] scores)
        {
            Log("scores: " + serializer.Serialize(scores));
        }

	    private static void Log(string text)
	    {
		    Console.Out.WriteLine(text);
		    File.AppendAllLines($"client{Process.GetCurrentProcess().Id}Log.txt", new[] { text });
	    }
    }
}