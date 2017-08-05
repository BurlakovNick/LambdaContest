using System;
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
        private readonly Action<string> log;
        private readonly Serializer serializer = new Serializer();

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
            var reply = tcpClient.WriteAndGetReply(serializer.Serialize(handshakeCommand), TimeSpan.MaxValue);
            var handshakeMessage = serializer.Deserialize<HandshakeMessage>(reply.MessageString);
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
            var setupMessage = serializer.Deserialize<SetupMessage>(e.MessageString);
            session.setupMessage = setupMessage;
            session.MyId = setupMessage.punter;
            session.Map = Converter.Convert(setupMessage.map);

            punter.Init(session.Map, setupMessage.punters, new Punter { Id = setupMessage.punter });

            e.Reply(serializer.Serialize(new SetupCommand
                                         {
                                             ready = setupMessage.punter
                                         }));

            session.Status = GameStatus.Gameplay;
        }

        private void HandleGameplay(Message e)
        {
            log("HandleGameplay");
            var moveMessage = serializer.Deserialize<MoveMessage>(e.MessageString);
            if (moveMessage.IsStop)
            {
                ShowScores(moveMessage.stop.scores);
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
            log("scores: " + serializer.Serialize(scores));
        }
    }
}