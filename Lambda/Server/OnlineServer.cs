using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Core;
using Core.Contracts;
using Core.Contracts.Converters;
using Core.Infrastructure;
using Core.Objects;
using Newtonsoft.Json;
using SimpleTCP;

namespace Server
{
    public class OnlineServer: IServer
    {
        private readonly IScorer scorer;
        private readonly string mapName;
        private readonly Action<string> log;
        private GameSession session;
        private SimpleTcpServer tcpServer;
        private readonly Serializer serializer = new Serializer();

        public OnlineServer(IScorer scorer,
                            ILog log,
                            string mapName)
        {
            this.scorer = scorer;
            this.mapName = mapName;
            this.log = log.Log;
        }

        public void Start(int playersCount)
        {
            ClearFile("moves.txt");
            ClearFile("map_command.txt");
            ClearFile("move_commands.txt");
            ClearFile("players_count.txt");
            ClearFile("scores.txt");
            ClearFile("who.txt");
            session = new GameSession(playersCount);
            tcpServer = new SimpleTcpServer().Start(7777);
            log("Tcp Server listening...");
            tcpServer.DataReceived += TcpServer_DataReceived;
        }

        private void TcpServer_DataReceived(object sender,
                                            Message e)
        {
            try
            {
                log($"Handling message in TcpServer_DataReceived. Message: {e.MessageString}");
                HandleHandshake(e);
            }
            catch (Exception exception)
            {
                log("ERROR " + exception);
                throw;
            }
        }

        private void HandleHandshake(Message message)
        {
            var handshakeCommand = serializer.Deserialize<HandshakeCommand>(message.MessageString);
            var handshakeMessage = new HandshakeMessage
                                   {
                                       you = handshakeCommand.me
                                   };
            log($"Reply to handshake from {handshakeCommand.me}");
            message.Reply(serializer.Serialize(handshakeMessage));

            var id = session.Clients.Count;
            var connection = new PlayerConnection(tcpClient: message.TcpClient,
                                                  name: handshakeCommand.me + " " + id,
                                                  id: id);
            session.Clients.Add(connection);

            log($"{session.Clients.Count}/{session.PlayersCount} players handshook");
            if (session.Clients.Count == session.PlayersCount)
            {
                tcpServer.DataReceived -= TcpServer_DataReceived;
                log("All players handshook. Gonna setup.");
                Setup();
            }
        }

        private void Setup()
        {
            var map = GetMap(mapName);
            LogMapToFile(map, session.PlayersCount);
            scorer.Init(Converter.Convert(map));
            foreach (var x in session.Clients)
            {
                var setupMessage = new SetupMessage
                                   {
                                       punter = x.Id,
                                       punters = session.PlayersCount,
                                       map = map
                                   };
                log($"Sending setup message to punter {x.Id}");
                var reply = x.Client.WriteAndGetReply(serializer.Serialize(setupMessage),
                                                      TimeSpan.FromSeconds(10));
                var setupCommand = serializer.Deserialize<SetupCommand>(reply.MessageString);
                log($"Punter {x.Id} is ready!");
                if (setupCommand.ready != x.Id)
                    throw new Exception("ready must be equal to player id");
            }

            var who = session.Clients.Select(x => new { x.Id, x.Name });
            File.WriteAllText("who.txt", JsonConvert.SerializeObject(who));

            Game(map);
        }

        private void Game(MapContract map)
        {
            log("Letsplay =)");

            var moveNumber = 1;
            var lastMoves = new MoveMessage
                            {
                                move = new MoveMessage.InternalMove
                                       {
                                           moves = session.Clients
                                                          .Select(x => new MoveCommand
                                                                       {
                                                                           pass = new Pass
                                                                                  {
                                                                                      punter = x.Id
                                                                                  }
                                                                       })
                                                          .ToList()
                                       }
                            };
            var moves = new List<MoveCommand>(lastMoves.move.moves);
            var claimSet = new HashSet<(int, int)>();

            while (moveNumber != map.rivers.Length)
            {
                foreach (var connection in session.Clients)
                {
                    log($"Move {moveNumber}/{map.rivers.Length} by punter {connection.Name} with id {connection.Id}");
                    var reply = connection.Client.WriteAndGetReply(serializer.Serialize(lastMoves), TimeSpan.FromSeconds(1));
                    if (reply == null)
                    {
                        log($"Punter {connection.Id} passed");
                        lastMoves.move.moves.Add(new MoveCommand { pass = new Pass { punter = connection.Id } });
                    }
                    else
                    {
                        var moveCommand = serializer.Deserialize<MoveCommand>(reply.MessageString);
                        LogClaimToFile(moveNumber, moveCommand);
                        lastMoves.move.moves.RemoveAt(0);
                        lastMoves.move.moves.Add(moveCommand);
                        moves.Add(moveCommand);
                        if (moveCommand.claim != null)
                        {
                            if (claimSet.Contains((moveCommand.claim.source, moveCommand.claim.target)))
                            {
                                log($"Punter {connection.Id} claimed what has already been claimed " +
                                    $"({moveCommand.claim.source}, {moveCommand.claim.target})");
                            }
                            claimSet.Add((moveCommand.claim.source, moveCommand.claim.target));
                            claimSet.Add((moveCommand.claim.target, moveCommand.claim.source));
                        }
                    }
                    if (moveNumber == map.rivers.Length)
                        break;
                    moveNumber++;

                    if (moveNumber % 100 == 0)
                        LogScores(map, moves);
                }
            }

            Scoring(map, lastMoves.move.moves, moves);
        }

        private static void ClearFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }

        private void Scoring(MapContract map,
                             List<MoveCommand> lastMoves,
                             List<MoveCommand> moves)
        {
            log("SCORING!");

            var scores = LogScores(map, moves);
            var serializedScores = JsonConvert.SerializeObject(scores.Select(x => new { Punter = x.Item1.Id, Score = x.Item2 }));
            File.WriteAllText("scores.txt", serializedScores);

            foreach (var connection in session.Clients)
            {
                var scoringMessage = new MoveMessage
                                     {
                                         stop = new MoveMessage.InternalStop
                                                {
                                                    moves = lastMoves,
                                                    scores = scores
                                                        .Select(x => new MoveMessage.Score
                                                                     {
                                                                         punter = x.Item1.Id,
                                                                         score = x.Item2
                                                                     })
                                                        .ToArray()
                                                }
                                     };
                connection.Client.Write(serializer.Serialize(scoringMessage));
            }

            Thread.Sleep(1000);
            Environment.Exit(0);
        }

        private static MapContract GetMap(string name)
        {
            var filePath = Path.Combine("Maps", $"{name}.json");
            var fileContent = File.ReadAllText(filePath);
            var map = JsonConvert.DeserializeObject<MapContract>(fileContent);
            return map;
        }

        private void LogMapToFile(MapContract map, int playersCount)
        {
            File.AppendAllLines("map_command.txt", new[] {JsonConvert.SerializeObject(map)});
            File.AppendAllLines("players_count.txt", new[] { JsonConvert.SerializeObject(playersCount) });
        }

        private static void LogClaimToFile(int i,
                                           MoveCommand x)
        {
            File.AppendAllLines("moves.txt", new[] { $"{i + 1}: {x.claim.punter} {x.claim.source}->{x.claim.target}" });

            File.AppendAllLines("move_commands.txt", new[] {JsonConvert.SerializeObject(x)});
        }

        private (Punter, int)[] LogScores(MapContract map,
                                          List<MoveCommand> moves)
        {
            var mapInfo = Converter.Convert(map, moves);

            var scores = session.Clients
                                .Select((x,
                                         i) => new GameState
                                               {
                                                   Map = mapInfo,
                                                   CurrentPunter = new Punter { Id = i }
                                               })
                                .Select(x => (x.CurrentPunter, scorer.Score(x)))
                                .ToArray();
            foreach (var (score, i) in scores.OrderByDescending(x => x.Item2)
                                             .Select((x,
                                                      i) => (x, i)))
                log($"#{i + 1} Punter {score.Item1.Id}, Score: {score.Item2}");

            return scores;
        }
    }
}