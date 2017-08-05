using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Contracts;
using Core.Contracts.Converters;
using Core.Objects;
using Newtonsoft.Json;

namespace OfflinePlayer
{
    public class OfflinePlayer
    {
        private const string PlayerName = "BargeHauler";
        private readonly Transport transport;
        private readonly IScorer scorer;

        private readonly IPunter punter;

        private static void Log(string message)
        {
            Console.Error.WriteLine(message);
        }

        public OfflinePlayer(IPunter randomPunter)
        {
            try
            {
                Log("Init player");

                transport = new Transport();
                scorer = new Scorer(new DistanceCalculator(), new GraphVisitor());
                punter = randomPunter;

                Log("Player initialized");
            }
            catch (Exception e)
            {
                Log(e.Message);
                Log(e.StackTrace);
                throw;
            }
        }

        public void Play()
        {
            try
            {
                Handshake();

                var moveMessage = transport.Recieve<MoveMessage>();
                if (moveMessage.IsSetup)
                {
                    Setup(moveMessage);
                }
                else
                if (moveMessage.IsGameplay)
                {
                    Gameplay(moveMessage);
                }
                else
                if (moveMessage.IsStop)
                {
                    Score(moveMessage);
                }
                else
                {
                    throw new Exception($"Unknown message. {JsonConvert.SerializeObject(moveMessage, Formatting.Indented)}");
                }

                Log("Jobs done");
            }
            catch (Exception e)
            {
                Log(e.Message);
                Log(e.StackTrace);
                throw;
            }
        }

        private void Handshake()
        {
            Log("Let's handshake");

            transport.Send(new HandshakeCommand {me = PlayerName});
            var handshakeMessage = transport.Recieve<HandshakeMessage>();
            if (handshakeMessage.you != PlayerName)
            {
                throw new Exception($"You: {handshakeMessage.you}, me: {PlayerName}");
            }
        }

        private void Setup(MoveMessage setupMessage)
        {
            Log("Let's setup");

            var map = Converter.Convert(setupMessage.map);
            var myPunter = setupMessage.punter;

            scorer.Init(map);
            punter.Init(map, setupMessage.punters, new Punter {Id = myPunter});

            var gameStateMessage = new GameStateMessage
            {
                MapContract = setupMessage.map,
                Moves = new List<MoveCommand>(),
                Punters = setupMessage.punters,
                MyPunter = myPunter,
                ScorerState = scorer.State,
                PunterState = punter.State
            };

            transport.Send(new SetupCommand { ready = myPunter, state = gameStateMessage });
        }

        private void Gameplay(MoveMessage gameplayMessage)
        {
            Log("Let's gameplay");

            var move = gameplayMessage.move;
            var state = gameplayMessage.state;

            var gameState = InitGameState(state, move.moves);

            var edge = punter.Claim(gameState);
            state.PunterState = punter.State;

            transport.Send(new MoveCommand
            {
                claim = new Claim
                {
                    punter = state.MyPunter,
                    source = edge.Source.Id,
                    target = edge.Target.Id,
                },
                state = state
            });
        }

        private void Score(MoveMessage scoringMessage)
        {
            Log("Let's score");
            //Не приходит GameState, пока так
            return;

            var moves = scoringMessage.stop.moves;
            var scores = scoringMessage.stop.scores;
            var state = scoringMessage.state;

            var gameState = InitGameState(state, moves);

            var myScore = scorer.Score(gameState);
            var serverScore = scores.First(x => x.punter == state.MyPunter);
            if (myScore != serverScore.score)
            {
                throw new Exception($"My score: {myScore}, server score: {serverScore}");
            }
        }

        private GameState InitGameState(GameStateMessage state, List<MoveCommand> moves)
        {
            state.Moves.AddRange(moves);

            var map = Converter.Convert(state.MapContract, state.Moves);
            scorer.State = state.ScorerState;
            punter.State = state.PunterState;

            var gameState = new GameState {Map = map, CurrentPunter = new Punter {Id = state.MyPunter}};
            return gameState;
        }
    }
}